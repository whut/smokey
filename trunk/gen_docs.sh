#!/bin/bash
nant -nologo -targetframework:mono-2.0 -D:debug=true app &&
monodocer -pretty -importslashdoc:docs.xml -assembly:bin/smokey_d.exe -path:docs/ &&
monodocs2html --source docs/ --dest docs.html
