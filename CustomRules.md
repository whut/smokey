## Introduction ##

Before writing a custom rule you should be familiar with SmokeyDesign and ConfigFile.


## Building ##

Rules can be built with your choice of method: nant, make files, scons, whatever. If you use make then you can use the provided Makefile in the custom directory as an example. Note that it's a little bit odd because we drop the binaries into our parent directories bin directory.

For now you'll need to add references to smokey.exe and tests.dll. In the future I may create a framework.dll and a test\_stub.dll which would theoretically provide more stable version numbers.


## Testing ##

It's usually a good idea to write a stubbed out version of your rule and then the test. Then you can gradually implement your rule until it passes all of the tests. The tests themselves are very easy to write: the base classes in tests.dll do all of the heavy lifting.

Once you're satisfied with your rule it's a good idea to try it out on a large assembly such as one of the system dlls.


## Writing ##

The first step is to create a Rules.xml file. Then you can implement the rule itself. Rules are written using a simple visitor design implemented by RuleDispatcher. There are a lot of different types you can visit including every instruction in a method. Failures are reported using the !IReportViolations interface.

Note that whenever possible you should let RuleDispatcher take care of iterating over the instructions. This allows Smokey to run faster and scale better because the rules are processed in parallel and data locality is maximized. Also try to write rules so that they can do quick preflighting and return before they do anything too expensive.


## Running ##

You can run the tests by hand using something like `nunit-console2 bin/custom_d.dll`. Or you can do like I did and have a nant target ("test") which runs the unit tests.

To run Smokey with your rules you need to edit the smokey.exe.config file and make sure it has a custom setting pointing to your assembly. Something like this:

```
<?xml version = "1.0" encoding = "utf-8" ?>
<configuration>
    <appSettings>
        <add key = "custom" value = "/Users/jessejones/Source/Smokey/bin/custom_d.dll"/>    <!-- colon separated -->

        <add key = "logfile" value = "/Users/jessejones/Source/Smokey/smokey.log"/>
        <add key = "topic:System.Object" value = "Info"/>    <!-- may be off, Error, Warning, Info, Trace, or Debug -->
        <add key = "topic:Smokey.Internal.Rules.LargeNamespaceRule" value = "Info"/>    
        <add key = "topic:Custom.PrivateNeedsDoRule" value = "Debug"/>    
        <add key = "consoleWidth" value = "80"/>            <!-- TextReport breaks lines so that that they aren't longer than this -->
    </appSettings>
</configuration>
```

You can do something similar with your tests dll to get logging working with your unit testing.


## Example ##

The custom directory in the source distribution contains a complete example of an assembly implementing custom rules. I've tried to comment the custom rules well so they should be a good starting point for writing new rules. You can also examine the code in source/internal/ rules of course. The custom rules are:

  * LogStartsWithString - visits call instructions, and if it's a call to one of the Smokey Log methods, that the first argument is not a string.
  * PrivateNeedsDo - a simple rule that checks a MethodDefinition to see if it's private and that the name starts with "Do".
  * PublicStartsWithDo - a simple rule that checks a MethodDefinition to see if it's public and that the name does not start with "Do".