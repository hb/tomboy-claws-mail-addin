all:
	gmcs -debug -out:ClawsMail.dll -target:library -pkg:tomboy-addins -r:Mono.Posix ClawsMailNoteAddin.cs `pkg-config --libs gmime-sharp-2.2` -resource:ClawsMail.addin.xml -resource:mail.png

install:
	mkdir -p $(HOME)/.config/tomboy/addins
	cp ClawsMail.dll $(HOME)/.config/tomboy/addins

uninstall:
	rm -f $(HOME)/.config/tomboy/addins/ClawsMail.dll

clean:
	rm -f *.dll *.mdb
