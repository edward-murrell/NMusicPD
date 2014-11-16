#!/usr/bin/env python

compiler = "mcs"
additional_parameters = " -g"
out = " /target:library /out:NMusicPD.dll"

print "WARNING: This is not the best way of doing things."
print "WARNING: Please give NAnt a try before using it, as this *is* buggy."
print "WARNING: Using \"%s\" compiler (to change it, modify the header of this file)" % compiler

from glob import glob
from os import path
from os import system


def glade():
	dir = path.join(path.curdir, "glade")
	if (not path.isdir(dir)):
		return "";
	files = glob(path.join(dir, "*.glade"))
	if (len(files) < 1):
		return "";
	strings = [" /resource:%s,%s" % (x, x[8:]) for x in files]
	return (" /pkg:glade-sharp /pkg:gtk-sharp" + "".join(strings))

def csfiles():
	dir = path.join(path.curdir, "src")
	if (not path.isdir(dir)):
		return "";
	files = glob(path.join(dir, "*/*.cs"))
	if (len(files) < 1):
		return "";
	return (" " + " ".join(files))

def images():
	dir = path.join(path.curdir, "images")
	if (not path.isdir(dir)):
		return "";
	files = glob(path.join(dir, "*.png"))
	if (len(files) < 1):
		return "";
	return "".join([" /resource:%s,%s" % (x, x[9:]) for x in files])

command = compiler + out + additional_parameters + images() + glade() + csfiles()
print
print command
system(command)
