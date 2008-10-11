# -----------------------------------------------------------------------------
# Public variables
CSC ?= gmcs
NUNIT ?= nunit-console2
export MONO ?= $(shell which mono)

export LOG_PATH ?= /tmp/smokey.log
export INSTALL_DIR ?= /usr/local/bin
	
ifdef RELEASE
	CSC_FLAGS ?= -checked+ -nowarn:1591 -optimize+
else
	CSC_FLAGS ?= -checked+ -debug+ -nowarn:1591 -define:DEBUG
endif

APP_CSC_FLAGS ?= $(CSC_FLAGS) -warn:4 -warnaserror+
ifdef PROFILE
	APP_CSC_FLAGS := $(APP_CSC_FLAGS) -define:PROFILE
endif

# -----------------------------------------------------------------------------
# Internal variables
comma := ,
cur_dir := $(shell pwd)

dummy1 := $(shell mkdir bin 2> /dev/null)			
dummy2 := $(shell if [[ "$(CSC_FLAGS)" != `cat bin/csc_flags 2> /dev/null` ]]; then echo "$(CSC_FLAGS)" > bin/csc_flags; fi)
dummy3 := $(shell if [[ "$(APP_CSC_FLAGS)" != `cat bin/app_flags 2> /dev/null` ]]; then echo "$(APP_CSC_FLAGS)" > bin/app_flags; fi)

app_files := $(strip $(shell find source/app -name "*.cs" -print))
framework_files := $(strip $(shell find source/framework -name "*.cs" -print))
internal_files := $(strip $(shell find source/internal -name "*.cs" -print))

rules_files := $(strip $(shell find source/internal/rules -name "*.cs" -print))
test_files := $(strip $(shell find source/tests -name "*.cs" -print))
extra_test_files := source/internal/AssertTraceListener.cs source/internal/Break.cs source/internal/GetOptions.cs source/internal/Ignore.cs source/internal/Unused.cs source/internal/Reformat.cs

xml_files := $(strip $(shell find source/internal/rules/xml -name "*.xml" -print))

base_version := 1.3.xxx.0										# major.minor.build.revision
version := $(shell ./get_version.sh $(base_version) build_num)	# this will increment the build number stored in build_num
version := $(strip $(version))

# -----------------------------------------------------------------------------
# Primary targets
all: bin/smokey.exe bin/tests.dll bin/smokey.exe.config bin/tests.dll.config

app: bin/smokey.exe bin/smokey.exe.config

check: bin/tests.dll bin/tests.dll.config
	$(NUNIT) -nologo -config=bin/tests.dll.config bin/tests.dll

check1: bin/tests.dll bin/tests.dll.config
	$(NUNIT) -nologo -config=bin/tests.dll.config -fixture=Smokey.Tests.DisposeNativeResourcesTest bin/tests.dll

ftest_asms := bin/evildoer.dll,bin/NoSecurity.exe,bin/APTCA.dll,bin/APTCA2.dll,bin/APTCA3.dll
fcheck: bin/smokey.exe $(subst $(comma), ,$(ftest_asms)) bin/FullTrust.dll bin/functest.exe
	$(MONO) --debug bin/functest.exe -exe:bin/smokey.exe -asm:$(ftest_asms)
	
ftest_asm := bin/evildoer.dll
fcheck1: bin/smokey.exe $(ftest_asm) bin/functest.exe
	$(MONO) --debug bin/functest.exe -exe:bin/smokey.exe -asm:$(ftest_asm)
	
# -----------------------------------------------------------------------------
# Generated targets

# smokey.exe
exe_files := $(app_files) $(framework_files) $(internal_files)
bin/exe_files: $(exe_files)
	@echo "$(exe_files)" > bin/exe_files
	
bin/exe_resources: $(xml_files) IgnoreList.txt SysIgnore.txt
	@echo "-resource:IgnoreList.txt -resource:SysIgnore.txt " > bin/exe_resources
	@echo $(xml_files) | sed "s/source/-resource:source/g" >> bin/exe_resources

bin/exe_references:
	@echo "-reference:Mono.CompilerServices.SymbolWriter.dll,Mono.Cecil.dll,System.Configuration.dll" > bin/exe_references

bin/smokey.exe: keys bin/app_flags bin/exe_references bin/exe_resources bin/exe_files
	@./gen_version.sh $(version) source/internal/AssemblyVersion.cs
	$(CSC) -out:bin/smokey.exe $(APP_CSC_FLAGS) -keyfile:keys -target:exe -doc:docs.xml @bin/exe_references @bin/exe_resources @bin/exe_files

# tests.dll
tests_files := $(extra_test_files) $(framework_files) $(rules_files) $(test_files)
bin/tests_files: $(tests_files)
	@echo "$(tests_files)" > bin/tests_files

bin/tests_references: 
	@echo "-reference:Mono.CompilerServices.SymbolWriter.dll,Mono.Cecil.dll,System.Configuration.dll,System.Data.dll,System.Windows.Forms.dll" > bin/tests_references

bin/tests.dll: bin/csc_flags bin/tests_references bin/tests_files 
	$(CSC) -out:bin/tests.dll $(CSC_FLAGS) -define:TEST -pkg:mono-nunit -target:library @bin/tests_references @bin/tests_files 
		
# functest.exe
functest_files := extras/functest/*.cs source/framework/DBC.cs source/framework/DisableRuleAttribute.cs source/internal/GetOptions.cs
bin/functest_files: $(functest_files)
	@echo "$(functest_files)" > bin/functest_files

functest_resources := extras/functest/*.xml
bin/functest_resources: $(functest_resources)
	@echo $(functest_resources) | sed "s/extras/-resource:extras/g" > bin/functest_resources

bin/functest.exe: bin/csc_flags bin/functest_resources bin/functest_files
	@./gen_version.sh $(version) extras/functest/AssemblyVersion.cs
	$(CSC) -out:bin/functest.exe $(CSC_FLAGS) -target:exe -reference:System.Windows.Forms.dll @bin/functest_resources @bin/functest_files

# functest assemblies
bin/evildoer.dll: bin/csc_flags extras/evildoer/Expected.xml extras/evildoer/*.cs
	@./gen_version.sh $(version) extras/evildoer/AssemblyVersion.cs
	$(CSC) -out:bin/evildoer.dll $(CSC_FLAGS) -target:library -reference:System.Windows.Forms.dll -resource:extras/evildoer/Expected.xml extras/evildoer/*.cs 

bin/NoSecurity.exe: bin/csc_flags extras/miscevil/NoSecurity.xml extras/miscevil/NoSecurity.cs 
	@./gen_version.sh $(version) extras/miscevil/AssemblyVersion.cs
	$(CSC) -out:bin/NoSecurity.exe $(CSC_FLAGS) -target:exe -reference:System.Windows.Forms.dll -resource:extras/miscevil/NoSecurity.xml extras/miscevil/AssemblyVersion.cs extras/miscevil/NoSecurity.cs 

bin/FullTrust.dll: keys bin/csc_flags extras/miscevil/FullTrust.cs 
	@./gen_version.sh $(version) extras/miscevil/AssemblyVersion.cs
	$(CSC) -out:bin/FullTrust.dll $(CSC_FLAGS) -keyfile:keys -target:library -reference:System.Windows.Forms.dll extras/miscevil/AssemblyVersion.cs extras/miscevil/FullTrust.cs 

bin/APTCA.dll: bin/csc_flags bin/FullTrust.dll extras/miscevil/APTCA.xml extras/miscevil/APTCA.cs 
	@./gen_version.sh $(version) extras/miscevil/AssemblyVersion.cs
	$(CSC) -out:bin/APTCA.dll $(CSC_FLAGS) -target:library -reference:System.Windows.Forms.dll,bin/FullTrust.dll -resource:extras/miscevil/APTCA.xml extras/miscevil/AssemblyVersion.cs extras/miscevil/APTCA.cs 

bin/APTCA2.dll: bin/csc_flags bin/FullTrust.dll extras/miscevil/APTCA2.xml extras/miscevil/APTCA2.cs 
	@./gen_version.sh $(version) extras/miscevil/AssemblyVersion.cs
	$(CSC) -out:bin/APTCA2.dll $(CSC_FLAGS) -target:library -reference:System.Windows.Forms.dll,bin/FullTrust.dll -resource:extras/miscevil/APTCA2.xml extras/miscevil/AssemblyVersion.cs extras/miscevil/APTCA2.cs 

# Note that this deliberately does not use CSC_FLAGS.
bin/APTCA3.dll: bin/FullTrust.dll extras/miscevil/APTCA3.xml extras/miscevil/APTCA3.cs 
	@./gen_version.sh $(version) extras/miscevil/AssemblyVersion.cs
	$(CSC) -out:bin/APTCA3.dll -target:library -reference:System.Windows.Forms.dll,bin/FullTrust.dll -resource:extras/miscevil/APTCA3.xml extras/miscevil/AssemblyVersion.cs extras/miscevil/APTCA3.cs 

# config files
bin/smokey.exe.config:
	cp extras/Makefile bin
	cd bin && make smokey.exe.config
	rm bin/Makefile

bin/tests.dll.config:
	@echo "generating bin/tests.dll.config"
	@echo "<?xml version = \"1.0\" encoding = \"utf-8\" ?>" > bin/tests.dll.config
	@echo "<configuration>" >> bin/tests.dll.config
	@echo "	<appSettings>" >> bin/tests.dll.config
	@echo "		<add key = \"logfile\" value = \"$(LOG_PATH)\"/>" >> bin/tests.dll.config
	@echo "		<add key = \"topic:System.Object\" value = \"Info\"/>	<!-- may be off, Error, Warning, Info, Trace, or Debug -->" >> bin/tests.dll.config
	@echo "		<add key = \"topic:Smokey.Internal.Rules.AttributePropertiesRule\" value = \"Debug\"/>" >> bin/tests.dll.config
	@echo "		<add key = \"consoleWidth\" value = \"80\"/>			<!-- TextReport breaks lines so that that they aren't longer than this -->" >> bin/tests.dll.config
	@echo "	</appSettings>" >> bin/tests.dll.config
	@echo "</configuration>" >> bin/tests.dll.config

# -----------------------------------------------------------------------------
# Other targets
keys: 
	sn -k keys
		
smokey_flags := --not-localized -set:naming:jurassic -set:ignoreList:IgnoreList.txt
smokey_flags += -exclude-check:D1015	# ExceptionConstructors
smokey_flags += -exclude-check:D1024	# SerializeException
smokey_flags += -exclude-check:D1031	# PublicType
smokey_flags += -exclude-check:P1005	# StringConcat
smokey_flags += -exclude-check:P1022	# PropertyReturnsCollection
smoke: bin/smokey.exe
	@-$(MONO) --debug bin/smokey.exe $(smokey_flags) bin/smokey.exe
	
help:
	@echo "smokey version $(version)"
	@echo " "
	@echo "The primary targets are:"
	@echo "app       - build smokey exe"
	@echo "check     - run the unit tests"
	@echo "fcheck    - run the functional test"
	@echo "install   - install the exe and a simple smoke script"
	@echo "uninstall - remove the exe and the smoke script"
	@echo " "
	@echo "Variables include:"
	@echo "RELEASE - define to enable release builds, defaults to not defined"
	@echo "LOG_PATH - file smokey writes its logs to, defaults to $(LOG_PATH)"
	@echo "INSTALL_DIR - where to put the exe, defaults to $(INSTALL_DIR)"
	@echo " "
	@echo "Here's an example:"	
	@echo "sudo make LOG_PATH=~/smokey.log install"	

install: bin/smokey.exe
	cp extras/Makefile bin
	cd bin && make install
	rm bin/Makefile
	
uninstall:
	cp extras/Makefile bin
	cd bin && make uninstall
	rm bin/Makefile

clean:
	-rm bin/TestResult.xml
	-rm bin/*.exe
	-rm bin/*.dll
	-rm bin/*.mdb
	-rm bin/exe_files bin/exe_references bin/exe_resources 
	-rm bin/tests_files bin/tests_references bin/functest_files bin/functest_resources

tar_binary: bin/smokey.exe bin/smokey.exe.config
	cp extras/Makefile bin
	tar --create --compress --file=smokey_bin-$(version).tar.gz bin/smokey.exe CHANGES CHANGE_LOG README bin/Makefile
	rm bin/Makefile
	
tar_source: 
	tar --create --compress --file=smokey_src-$(version).tar.gz AUTHORS CHANGES CHANGE_LOG IgnoreList.txt MIT.X11 Makefile extras/Makefile README SysIgnore.txt gen_docs.sh gen_match.py gen_version.sh get_version.sh custom extras source

tar: tar_binary tar_source
