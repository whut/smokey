## Installation ##

Currently there's no installer on Windows, but for now you can simply drop the smokey.exe assembly into the C:\WINDOWS directory. You will also need to ensure Mono.Cecil.dll is in the same directory as smokey.exe. You can download Cecil [here](https://home.comcast.net/~jesse98/public/Mono.Cecil.dll).


## Running ##

If you put smokey.exe into C:\WINDOWS then you can smoke an assembly by executing `smokey Cool.dll` from the cmd tool. If not you can use `C:\path\to\smokey.exe Cool.dll`. This will write a report to stdout listing all of the problems that were found with the file. You can also exclude types and methods from being tested. See `smokey -usage` for more details on this.

You can change the default behavior of Smokey with the ConfigFile. You can also include SmokeyAttributes within your assembly to affect the way Smokey processes your types and methods.

Note that, for now, you will not get file and line numbers when running Smokey with Microsoft's .NET.