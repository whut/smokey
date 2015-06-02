# Building #

To build use `make app`, or `make check`, etc. Note that `make help` will give you a more complete list.

As of mono 1.0 Cecil is included in the gac but not in the lib directory so the compiler won't find it. The easiest fix for this is to copy Mono.Cecil.dll from the gac to $prefix/lib/mono/2.0.


## Tests ##

To build and run the unit tests: `make check`. All of the unit tests should pass. Note that you can also run these in release. If you just want to run one test you can use the check1 target although you will need to edit the test1 target in the make file to include the test you wish to test.

To build and run the functional tests: `make fcheck`. The functional tests should all pass. You can run the functional tests in release, but at the moment, there will be few false positives because release assemblies are built without mdb files.


## Smoking ##

To run smokey on itself: `make smoke`. No errors or warnings should be reported.