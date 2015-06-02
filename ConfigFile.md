## Introduction ##

The config file is an optional xml file in the same directory as smokey.exe. It has the same name as the smokey assembly but with ".config" appended. So, it's typically named "smokey.exe.config".


## Settings ##

Individual settings consist of a key and a value. All the settings are optional. The settings are
| Name | Default | Allowed Values |
|:-----|:--------|:---------------|
| consoleWidth | 80      | positive int (used when writing text reports) |
| custom | ---     | colon separated list of paths to assemblies with custom rules |
| dictionary | ""      | colon separated list of paths to files with a whitespace separated list of words to ignore when spell-checking |
| ignoreList | ""      | path to a file with a whitespace separated list of words to ignore when spell-checking (deprecated) |
| logfile | null    | stdout, stderr, or a path |
| maxBoxes | 3       | non-negative int (used by the AvoidBoxing rule) |
| maxBranches | 40      | used by TooComplexRule |
| maxNamespace | 40      | used by LargeNamespaceRule |
| maxUnboxes | 1       | non-negative int (used by the AvoidUnboxing rule) |
| naming | mono    | mono, net, jurassic (jurassic is .NET except protected and private fields start with `m_, m, ms_, s_, or ms`) |
| topic | ---     | log topic (see below) |

Settings in the config file can be overwritten using the -set command line switch.


## Logging ##

The logger is a hierarchical logger which leverages the C# type system. By default System.Object and all subclasses log at the Warning level. You can change the default for Object or for a sub or base class using the topic settings. Currently the topics are:

  * System.Object
    * Smokey.Analyze
      * Smokey.AnalyzeAssembly
      * Smokey.Rule
        * Smokey.DontDestroyStackTraceRule
        * Smokey.EqualsCantCastRule
        * ...
    * Smokey.Report
    * Smokey.SymbolTable
    * Smokey.ViolationData
    * Smokey.Visitor
      * Smokey.LinearVisitor

## Example ##

```
<?xml version = "1.0" encoding = "utf-8" ?>
<configuration>
    <appSettings>
        <add key = "logfile" value = "/Users/jessejones/Source/Smokey/smokey.log"/>
		<add key = "topic:System.Object" value = "Info"/>	<!-- may be off, Error, Warning, Info, Trace, or Debug -->
        <add key = "topic:Smokey.DisposeNativeResourcesRule" value = "Debug"/>	<!-- get more detailed logging for this rule -->
        <add key = "consoleWidth" value = "80"/> 
    </appSettings>
</configuration>
```