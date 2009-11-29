// Heavily influenced by the Evolution addin

using System;
using Mono.Unix;
using Gtk;
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
					mail_icon = GuiUtils.GetIcon (System.Reflection.Assembly.GetExecutingAssembly(), "mail", 16);
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
				return (string) Attributes ["uri"];
			}
			set {
				Attributes ["uri"] = value;
			}
		}        
        
    }
    
    public class ClawsMailNoteAddin : NoteAddin
    {
		static ClawsMailNoteAddin()
		{
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

		[GLib.ConnectBefore]
		void OnDragDataReceived(object sender, Gtk.DragDataReceivedArgs args)
		{
			if(args.SelectionData.Length < 0)
				return;
            
            Logger.Info("ClawsMail: OnDragDataReceived");
		}
                
    }
}
