Claws Mail addin for Tomboy
===========================

This addin provides Claws Mail integration for Tomboy.
When emails are dragged from the summary view in Claws Mail and dropped
into a Tomboy note, an email link is created in the note. Clicking the
link will open the note again in Claws Mail.

Installation
------------

Dependencies are the mcs compiler, gmime-sharp, Tomboy and Claws Mail 3.7.4 or later.

For Debian: `apt-get install gtk-sharp2 libgmime-2.6-dev mono-mcs libgmime2.6-cil-dev libglib2.0-cil-dev`

* clone this repository and build as usual
```
$ git clone https://github.com/hb/tomboy-claws-mail-addin
$ cd tomboy-claws-mail-addin
$ make && make install
```
* Activate the addin in the Tomboy addin dialog, "Desktop Integration" section. To install it system wide, copy the file ClawsMail.dll to the system wide addin folder (e.g. /usr/lib/tomboy/addins)

------------------------------------------------------------------------
Copyright 2009 Holger Berndt <berndth@gmx.de>

The addin is heavily influenced by the Evolution addin that is shipped
with Tomboy copyrighted by the Tomboy team.
