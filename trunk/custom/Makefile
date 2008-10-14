# -----------------------------------------------------------------------------
# Public variables
CSC ?= gmcs
NUNIT ?= nunit-console2
MONO ?= $(shell which mono)
	
ifdef RELEASE
	CSC_FLAGS ?= -checked+ -nowarn:1591 -optimize+
else
	CSC_FLAGS ?= -checked+ -debug+ -warn:4 -warnaserror+ -nowarn:1591 -define:DEBUG
endif

# -----------------------------------------------------------------------------
# Internal variables
dummy := $(shell if [[ "$(CSC_FLAGS)" != `cat ../bin/csc_flags 2> /dev/null` ]]; then echo "$(CSC_FLAGS)" > ../bin/csc_flags; fi)

# -----------------------------------------------------------------------------
# Primary targets
all: ../bin/custom.dll ../bin/customtests.dll

lib: ../bin/custom.dll

check: ../bin/customtests.dll
	$(NUNIT) -nologo -config=../bin/tests.dll.config ../bin/customtests.dll
	
# -----------------------------------------------------------------------------
# Generated targets
../bin/custom.dll: ../bin/csc_flags ../bin/smokey.exe source/rules/Rules.xml source/*.cs source/rules/*.cs
	$(CSC) -out:../bin/custom.dll $(CSC_FLAGS) -target:library -r:../bin/smokey.exe,Mono.Cecil.dll -res:source/rules/Rules.xml source/*.cs  source/rules/*.cs

../bin/customtests.dll: ../bin/csc_flags ../bin/tests.dll source/rules/Rules.xml source/rules/*.cs source/tests/*.cs
	$(CSC) -out:../bin/customtests.dll $(CSC_FLAGS) -define:TEST -pkg:mono-nunit -target:library -r:../bin/tests.dll,Mono.Cecil.dll -res:source/rules/Rules.xml source/rules/*.cs source/tests/*.cs

# -----------------------------------------------------------------------------
# Other targets
smokey_flags := --not-localized -set:naming:jurassic -set:dictionary:../IgnoreList.txt
smokey_flags += -exclude-check:D1004	# UseStrongName
smoke: ../bin/custom.dll
	@-$(MONO) --debug ../bin/smokey.exe $(smokey_flags) ../bin/custom.dll
	
help:
	@echo "The primary targets are:"
	@echo "lib       - build the custom library (into ../bin)"
	@echo "check     - run the unit tests"
	@echo "clean     - remove the files we generated"
	@echo " "
	@echo "Variables include:"
	@echo "RELEASE - define to enable release builds, defaults to not defined"

clean:
	-rm ../bin/TestResult.xml
	-rm ../bin/custom.dll
	-rm ../bin/customtests.dll
