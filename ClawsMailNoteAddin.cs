// Heavily influenced by the Evolution addin
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

using Mono.Unix;
using Mono.Unix.Native;

using Tomboy;

namespace Tomboy.ClawsMail
{
    public class EmailLink : DynamicNoteTag
    {
        static Gdk.Pixbuf mail_icon = null;
        
        static Gdk.Pixbuf MailIcon
        {
			get {
				if(mail_icon == null)
					mail_icon = GuiUtils.GetIcon(System.Reflection.Assembly.GetExecutingAssembly(), "mail", 16);
				return mail_icon;
			}
        }
        
		public EmailLink() : base()
		{
		}

		public override void Initialize(string element_name)
		{
			base.Initialize(element_name);

			Underline = Pango.Underline.Single;
			Foreground = "blue";
			CanActivate = true;

			Image = MailIcon;
		}     

		protected override bool OnActivate (NoteEditor editor,
		                                    Gtk.TextIter start,
		                                    Gtk.TextIter end)
		{
            return true;
        }        
    }
    
    public class ClawsMailNoteAddin : NoteAddin
    {
        List<string> subject_list;
        
		static ClawsMailNoteAddin()
		{
            GMime.Global.Init();
		}
        
        public override void Initialize()
        {
            if(!Note.TagTable.IsDynamicTagRegistered("link:cm-mail"))
                Note.TagTable.RegisterDynamicTag("link:cm-mail", typeof(EmailLink));
        }

		public override void Shutdown()
		{
		}

		public override void OnNoteOpened()
		{
            Window.Editor.DragDataReceived += OnDragDataReceived;
		}

		[DllImport("libgobject-2.0.so.0")]
		static extern void g_signal_stop_emission_by_name (IntPtr raw, string name);

		[GLib.ConnectBefore]
		void OnDragDataReceived(object sender, Gtk.DragDataReceivedArgs args)
		{
            bool stop_emission = false;
            
			if(args.SelectionData.Length < 0)
				return;
            
            if(args.Info == 1) {
                foreach (Gdk.Atom atom in args.Context.Targets) {
                    if(atom.Name == "text/uri-list") {
                        DropEmailUriList(args);
                        stop_emission = true;
                        InsertMailLinks(args.X, args.Y, subject_list);
                    }
                }
            }
            
			if(stop_emission) 
				g_signal_stop_emission_by_name(Window.Editor.Handle, "drag-data-received");
		}
        
        void DropEmailUriList(Gtk.DragDataReceivedArgs args)
        {
			string uri_string = Encoding.UTF8.GetString(args.SelectionData.Data);

			subject_list = new List<string>();

			UriList uri_list = new UriList(uri_string);

			foreach(Uri uri in uri_list) {
                Logger.Info("Claws Mail: Dropped URI: {0}", uri.LocalPath);
                int mail_fd = Syscall.open(uri.LocalPath, OpenFlags.O_RDONLY);
				if(mail_fd == -1)
					continue;
                    
                GMime.Stream stream = new GMime.StreamFs(mail_fd);
                GMime.Parser parser = new GMime.Parser(stream);

                parser.ScanFrom = false;
				while(!parser.Eos()) {
					GMime.Message message = parser.ConstructMessage();
					if(message == null)
						break;
					
					Logger.Info("Claws Mail: Message Subject: {0}", message.Subject);
					subject_list.Add(message.Subject);
					message.Dispose();
				};

				parser.Dispose();
				stream.Close();
				stream.Dispose();
            }
        }
        
		void InsertMailLinks (int x, int y, List<string> subject_list)
		{
			// Place the cursor in the position where the uri was
			// dropped, adjusting x,y by the TextView's VisibleRect.
			Gdk.Rectangle rect = Window.Editor.VisibleRect;
			x = x + rect.X;
			y = y + rect.Y;
			Gtk.TextIter cursor = Window.Editor.GetIterAtLocation (x, y);
			Buffer.PlaceCursor (cursor);

            bool more_than_one = false;
			foreach (string subject in subject_list) {
                int start_offset;
                
                if(more_than_one) {
                    cursor = Buffer.GetIterAtMark (Buffer.InsertMark);

                    if(cursor.LineOffset == 0)
                        Buffer.Insert(ref cursor, "\n");
                    else
                        Buffer.Insert(ref cursor, ", ");
				}

                
                EmailLink link_tag;
                link_tag = (EmailLink) Note.TagTable.CreateDynamicTag("link:cm-mail");
                cursor = Buffer.GetIterAtMark(Buffer.InsertMark);
                start_offset = cursor.Offset;
                Buffer.Insert(ref cursor, subject);
				Gtk.TextIter start = Buffer.GetIterAtOffset (start_offset);
				Gtk.TextIter end = Buffer.GetIterAtMark (Buffer.InsertMark);
				Buffer.ApplyTag (link_tag, start, end);                
                
                more_than_one = true;
            }
        }

    } // ClawsMailNoteAddin
}
