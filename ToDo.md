## Introduction ##

As far as I can tell Smokey is pretty much feature complete so most of the changes I envision involve new rules and bug fixes.


## Known Bugs ##

  * A few rules want to know if an instruction is executing within a loop. The current code to detect loops fails for do/while loops. One place this is seen is with RandomUsedOnceRule which will return false positives if random numbers are generated in a do/while.

  * The functional test still doesn't work quite right with release builds.

  * IdenticalMethodsRule is not deterministic and sometimes reports one or the other identical method. This causes the functional test to sometimes fail.


## Medium Term ##

More rules, in particular I'd like to add more FxCop rules (Smokey probably has 70-80% of the FxCop already). I may also add some SQL related rules.

Better support on Windows: support for file/line numbers and some sort of install.bat script.


## Long Term ##

These are things I'd like to implement, but probably won't get around to any time soon:

  * I haven't looked at Smokey memory usage at all. I suspect there's not a lot of room for improvement, but it would be nice to look more closely at it.
  * It'd be nice to be able to provide more hints to direct Smokey. For example, we might be able to do extended type checking by annotating variables with type attributes.
  * We automagically ensure that the code samples in the XML compile, but it would be nice to smoke them as well.
  * It should be possible to get a substantial speedup on a multicore machine by running rules in parallel.
  * Localization is only partially supported: all of the rule text can be localized but the TextReport has hardcoded strings.