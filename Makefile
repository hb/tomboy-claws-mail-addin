TOMBOY_DIR=$(HOME)/.config/tomboy/addins

ClawsMailNoteAddin.dll: ClawsMailNoteAddin.cs ClawsMail.addin.xml
	mcs -debug -out:ClawsMail.dll -target:library -pkg:gtk-sharp-2.0 -pkg:tomboy-addins -r:Mono.Posix ClawsMailNoteAddin.cs `pkg-config --libs gmime-sharp-2.6` -resource:ClawsMail.addin.xml -resource:mail.png

install: ClawsMailNoteAddin.dll
	mkdir -p $(TOMBOY_DIR)
	cp ClawsMail.dll $(TOMBOY_DIR)

uninstall:
	rm -f $(TOMBOY_DIR)/ClawsMail.dll

clean:
	rm -f ClawsMail.dll ClawsMail.mdb
