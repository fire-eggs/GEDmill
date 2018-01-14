On this branch, I am modifying GEDmill to read GEDCOM using my library [SharpGedParser](https://github.com/fire-eggs/YAGP).

So far, this has been a useful exercise, in that:
* I found and fixed some bugs in my library.
* I have been motivated to fix several not-yet-implemented features in my library.
* I always learn something by viewing GED processing from a different application.

Using SharpGedParser has introduced a few limitations. This version of GEDmill does not:
1. support ANSEL characters
1. support carriage-return (MAC) GED files
1. support embedded media files
1. support CHAN.TIME
1. the setting "Preserve trailing spaces in GEDCOM" no longer has any effect

I have not made any changes to GEDmill functionality (except for those due to SharpGedParser).


# GEDmill
GEDmill is an easy-to-use program that lets you create webpages from your family history data. It accepts output from most family history programs and uses it to build HTML webpages.

## Features
GEDmill's main features are:

* Creates a separate page for each individual, listing key events in their life, alongside a photograph (if available) and a family tree diagram.
* Provides references to all sources. Lets you see at a glance where a piece of information has come from, with images of certificates, official documents etc. where available.
* Option to hide certain individuals, for example to keep information about living relatives or others out of the website.
* Accepts family history data in GEDCOM format. All genealogy applications should have the option to export data in this format. This version of GEDmill can accept GEDCOM in 5.5 and 5.5.1 formats.
* Creates standards compliant HTML web pages. All webpages are W3C XHTML 1.0 compliant.
* Generate files for a self-playing CD-ROM, to share your family history with others without the need for a website.

## System Requirements

This version of GEDmill requires the .NET framework Version 4.5 or later. On Windows 8 and earlier, it may be necessary to 
install the framework. On Windows 10, Gedmill should work "out of the box".
