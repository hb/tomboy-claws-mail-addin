all:
	gmcs -debug -out:ClawsMail.dll -target:library -pkg:tomboy-addins -r:Mono.Posix ClawsMailNoteAddin.cs `pkg-config --libs gmime-sharp-2.2` -resource:ClawsMail.addin.xml -resource:mail.png

install:
	cp ClawsMail.dll $(HOME)/.config/tomboy/addins

clean:
	rm -f *.dll *.mdb
