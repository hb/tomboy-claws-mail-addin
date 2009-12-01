// Heavily influenced by the Evolution addin
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;

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

		public string EmailUri
		{
			get {
				return (string) Attributes["uri"];
			}
			set {
				Attributes["uri"] = value;
			}
		}
        
        protected override bool OnActivate(NoteEditor editor,
                                           Gtk.TextIter start,
                                           Gtk.TextIter end)
        {
			Process p = new Process();
			p.StartInfo.FileName = "claws-mail";
			p.StartInfo.Arguments = "--select '" + EmailUri + "'";
			p.StartInfo.UseShellExecute = false;

			try {
				p.Start();
			} catch(Exception ee) {
				string message = String.Format("Error running Claws Mail: {0}", ee.Message);
				Logger.Error(message);
				HIGMessageDialog dialog = new HIGMessageDialog(editor.Toplevel as Gtk.Window,
				                              Gtk.DialogFlags.DestroyWithParent,
				                              Gtk.MessageType.Info,
				                              Gtk.ButtonsType.Ok,
				                              Catalog.GetString("Cannot open email"),
				                              message);
				dialog.Run();
				dialog.Destroy();
			}
            return true;
        }        
    }
    
    public class ClawsMailNoteAddin : NoteAddin
    {
        string uri_list_string = null;
        string cm_path_string = null;
        
        static ClawsMailNoteAddin()
        {
            GMime.Global.Init();
        }
        
        public override void Initialize()
        {
            if(!Note.TagTable.IsDynamicTagRegistered("link:cm-mail"))
                Note.TagTable.RegisterDynamicTag("link:cm-mail", typeof(EmailLink));
        }

        Gtk.TargetList TargetList
        {
            get {
                return Gtk.Drag.DestGetTargetList(Window.Editor);
            }
        }

        public override void Shutdown()
        {
            if(HasWindow)
                TargetList.Remove(Gdk.Atom.Intern("claws-mail/msg-path-list", false));
        }

        public override void OnNoteOpened()
        {
            TargetList.Add(Gdk.Atom.Intern("claws-mail/msg-path-list", false), 0, 50);
            Window.Editor.DragDataReceived += OnDragDataReceived;
        }

        [DllImport("libgobject-2.0.so.0")]
        static extern void g_signal_stop_emission_by_name(IntPtr raw, string name);

        [GLib.ConnectBefore]
        void OnDragDataReceived(object sender, Gtk.DragDataReceivedArgs args)
        {
            bool stop_emission = false;
            
            if(args.SelectionData.Length < 0)
                return;
                        
            if(args.Info == 1) {
                foreach(Gdk.Atom atom in args.Context.Targets) {
                    if(atom.Name == "claws-mail/msg-path-list") {
                        uri_list_string = Encoding.UTF8.GetString(args.SelectionData.Data);
                        Gtk.Drag.GetData(Window.Editor, args.Context, Gdk.Atom.Intern("claws-mail/msg-path-list", false), args.Time);
                        Gdk.Drag.Status(args.Context, Gdk.DragAction.Link, args.Time);
                        stop_emission = true;
                    }
                }
            }
            else if(args.Info == 50) {
                cm_path_string = Encoding.UTF8.GetString(args.SelectionData.Data);
                Gtk.Drag.Finish(args.Context, true, false, args.Time);
                stop_emission = true;
                HandleDrops(args.X, args.Y);
            }
            
            if(stop_emission) 
                g_signal_stop_emission_by_name(Window.Editor.Handle, "drag-data-received");
        }

        void HandleDrops(int xx, int yy)
        {
            UriList uri_list = new UriList(uri_list_string);
            string [] cm_path_list = cm_path_string.Split('\n');

            uri_list_string = null;
            cm_path_string = null;
            
            if(uri_list.Count != (cm_path_list.Length-1)) {
                Logger.Error("Error parsing drop input");
                return;
            }
            
            string cm_path_folderitem = cm_path_list[0];
            bool first = true;
            for(int ii = 1; ii < cm_path_list.Length; ii++) {
                Uri uri = uri_list[ii-1];
                string msgid = cm_path_list[ii];
                HandleDrop(xx, yy, first, uri, cm_path_folderitem, msgid);
                first = false;
            }
        }
        
        void HandleDrop(int xx, int yy, bool first, Uri uri, string cm_path_folderitem, string msgid)
        {
            // get subject
            string subject = "<unknown>";
            int mail_fd = Syscall.open(uri.LocalPath, OpenFlags.O_RDONLY);
            if(mail_fd != -1) {
                GMime.Stream stream = new GMime.StreamFs(mail_fd);
                GMime.Parser parser = new GMime.Parser(stream);
                
                parser.ScanFrom = false;
                while(!parser.Eos()) {
                    GMime.Message message = parser.ConstructMessage();
                    if(message == null)
                        break;
                    subject = message.Subject;
                    message.Dispose();
                }
                parser.Dispose();
                stream.Close();
                stream.Dispose();
            }
            
            // Place the cursor in the position where the uri was
            // dropped, adjusting x,y by the TextView's VisibleRect.
            Gdk.Rectangle rect = Window.Editor.VisibleRect;
            xx += rect.X;
            yy += rect.Y;
            Gtk.TextIter cursor = Window.Editor.GetIterAtLocation(xx, yy);
            Buffer.PlaceCursor(cursor);
            
            int start_offset;
                
            if(!first) {
                cursor = Buffer.GetIterAtMark(Buffer.InsertMark);

                if(cursor.LineOffset == 0)
                    Buffer.Insert(ref cursor, "\n");
                else
                    Buffer.Insert(ref cursor, ", ");
            }
            
            EmailLink link_tag;
            link_tag = (EmailLink) Note.TagTable.CreateDynamicTag("link:cm-mail");
            link_tag.EmailUri = cm_path_folderitem + "/<" + msgid + ">";
            
            cursor = Buffer.GetIterAtMark(Buffer.InsertMark);
            start_offset = cursor.Offset;
            Buffer.Insert(ref cursor, subject);
            Gtk.TextIter start = Buffer.GetIterAtOffset(start_offset);
            Gtk.TextIter end = Buffer.GetIterAtMark(Buffer.InsertMark);
            Buffer.ApplyTag(link_tag, start, end);
        }

    } // ClawsMailNoteAddin
}
