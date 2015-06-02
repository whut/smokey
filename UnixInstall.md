## Installation ##

You can run Smokey without doing any installation at all, but it's usually simpler to install it. To install using the binary distro navigate to the smokey/bin directory and type `sudo make install`. To install from the source you can do the same with the top level make file.

The install target will copy smokey.exe to /usr/local/bin, make a config file if one does not already exist, and create a little script to launch smokey. If you want to install to a different location you can set the INSTALL\_DIR environment variable.


## Running ##

If you installed you can smoke an assembly by executing `smoke Cool.dll` from a shell. If not you can use `mono /path/to/smokey.exe Cool.dll`. This will write a report to stdout listing all of the problems that were found with the file. You can also exclude types and methods from being tested. See `smoke -usage` for more details on this.

You can change the default behavior of Smokey with the ConfigFile. You can also include SmokeyAttributes within your assembly to affect the way Smokey processes your types and methods.


## Uninstall ##

The make files include an uninstall target which will remove smokey.exe, the smoke script, and smokey's log file. To run it execute `sudo make uninstall` from a shell.