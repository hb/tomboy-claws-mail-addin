all:
	gmcs -debug -out:ClawsMail.dll -target:library -pkg:tomboy-addins -r:Mono.Posix ClawsMailNoteAddin.cs -resource:ClawsMail.addin.xml

install:
	cp ClawsMail.dll $(HOME)/.config/tomboy/addins

clean:
	rm -f *.dll *.mdb
