TOMBOY_DIR=$(HOME)/.config/tomboy/addins

ClawsMailNoteAddin.dll: ClawsMailNoteAddin.cs ClawsMail.addin.xml
	gmcs -debug -out:ClawsMail.dll -target:library -pkg:tomboy-addins -r:Mono.Posix ClawsMailNoteAddin.cs `pkg-config --libs gmime-sharp-2.2` -resource:ClawsMail.addin.xml -resource:mail.png

install: ClawsMailNoteAddin.dll
	mkdir -p $(TOMBOY_DIR)
	cp ClawsMail.dll $(TOMBOY_DIR)

uninstall:
	rm -f $(TOMBOY_DIR)/ClawsMail.dll

clean:
	rm -f ClawsMail.dll ClawsMail.mdb
