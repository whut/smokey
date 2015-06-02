## Introduction ##

Smokey is a open source command line tool used to analyze .NET or Mono assemblies for problems. It's similar to tools like FxCop and Gendarme. Currently Smokey runs on Mono 2.0 or above and on .NET 3.5 and has 233 rules. Descriptions of all of the rules provided by the currently released version can be found [here](https://home.comcast.net/~jesse98/public/Smokey/violations/frames.html).

Smokey can generate reports of problems found in html, text, or xml formats. [Html reports](https://home.comcast.net/~jesse98/public/Smokey/report.html) can be generated with commands like `smoke -profile=system -html -out:report.html mono-1.9/mcs/class/lib/default/System.Drawing.dll`.

## Related Tools ##

[Smokey Add-in](http://code.google.com/p/smokeyaddin/) is an addon that integrates Smokey into [MonoDevelop](http://www.monodevelop.com/Main_Page).

[Gendarme](http://www.mono-project.com/Gendarme) is the Mono assembly checker. It has improved a great deal since I originally started Smokey and I am now helping with its development instead of Smokey.

FxCop is Microsoft's tool. It has a lot of [rules](http://msdn2.microsoft.com/en-us/library/ee1hzekz(VS.80).aspx) and is a mature product. But it only runs on Windows, is not open source, and is missing some cool rules such as [ConsistentEquality](https://home.comcast.net/~jesse98/public/Smokey/violations/Reliability.html#ConsistentEquality), [CtorCallsVirtual](https://home.comcast.net/~jesse98/public/Smokey/violations/Reliability.html#CtorCallsVirtual), [NullDeref](https://home.comcast.net/~jesse98/public/Smokey/violations/Correctness.html#NullDeref), [StaticSetter](https://home.comcast.net/~jesse98/public/Smokey/violations/Reliability.html#StaticSetter), and [ValueHashCode](https://home.comcast.net/~jesse98/public/Smokey/violations/Performance.html#ValueHashCode).

[MoonWalker](http://wwwhome.cs.utwente.nl/~ruys/moonwalker/) is a model checker for Mono. It exhaustively simulates execution of assemblies and looks for problems like deadlocks and invalid assertions.

[HeapBuddy](http://www.mono-project.com/HeapBuddy) and [HeapShot](http://www.mono-project.com/HeapShot) are Mono tools which can be used to profile your application's memory usage to discover problems like objects living longer than they should.

The [CLR Profiler](http://msdn.microsoft.com/en-us/library/ms979205.aspx) can be used with .NET to investigate memory issues.

[FindBugs](http://findbugs.sourceforge.net/) is a Java tool. Unlike FxCop it focuses more on outright bugs than design violations.

[mono-api-info](http://www.mono-project.com/Generating_class_status_pages) is used internally by the Mono team to ensure that their public APIs do not change between releases.