# -----------------------------------------------------------------------------
# Public variables
CSC ?= gmcs
NUNIT ?= nunit-console2
MONO ?= $(shell which mono)

LOG_PATH ?= /tmp/smokey.log
INSTALL_DIR ?= /usr/local/bin
	
# TODO: should probably default to building release
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
bin_path := bin
app_path := $(bin_path)/smokey.exe
tests_path := $(bin_path)/tests.dll
ftest_path := $(bin_path)/functest.exe

app_files := $(strip $(shell find source/app -name "*.cs" -print))
framework_files := $(strip $(shell find source/framework -name "*.cs" -print))
internal_files := $(strip $(shell find source/internal -name "*.cs" -print))

rules_files := $(strip $(shell find source/internal/rules -name "*.cs" -print))
test_files := $(strip $(shell find source/tests -name "*.cs" -print))
extra_test_files := source/internal/AssertTraceListener.cs source/internal/Break.cs source/internal/GetOptions.cs source/internal/Ignore.cs source/internal/Unused.cs source/internal/Reformat.cs

xml_files := $(strip $(shell find source/internal/rules/xml -name "*.xml" -print))
xml_resources := $(shell echo $(xml_files) | sed "s/source/-resource:source/g")

base_version := 1.2.xxx.0										# major.minor.build.revision
version := $(shell ./get_version.sh $(base_version) build_num)	# this will increment the build number stored in build_num
version := $(strip $(version))

# -----------------------------------------------------------------------------
# Primary targets
all: bin $(app_path) $(tests_path) $(app_path).config $(tests_path).config

app: $(app_path) $(app_path).config

check: $(tests_path) $(tests_path).config
	$(NUNIT) -nologo -config=$(tests_path).config $(tests_path)

check1: $(tests_path) $(tests_path).config
	$(NUNIT) -nologo -config=$(tests_path).config -fixture=Smokey.Tests.BoolMarshalingTest $(tests_path)

ftest_asms := $(bin_path)/evildoer.dll,$(bin_path)/NoSecurity.exe,$(bin_path)/APTCA.dll,$(bin_path)/APTCA2.dll,$(bin_path)/APTCA3.dll
fcheck: $(app_path) $(subst $(comma), ,$(ftest_asms)) $(bin_path)/FullTrust.dll $(ftest_path)
	$(MONO) --debug $(ftest_path) -exe:$(app_path) -exclude-check:C1030 -asm:$(ftest_asms)
	
ftest_asm := $(bin_path)/evildoer.dll
fcheck1: $(app_path) $(ftest_asm) $(ftest_path)
	$(MONO) --debug $(ftest_path) -exe:$(app_path) -exclude-check:C1030 -asm:$(ftest_asm)
	
# -----------------------------------------------------------------------------
# Generated targets
exe_files := $(app_files) $(framework_files) $(internal_files)
$(app_path):keys $(exe_files) IgnoreList.txt SysIgnore.txt $(xml_files)
	@echo "building $(app_path)"
	@./gen_version.sh $(version) source/internal/AssemblyVersion.cs
	@$(CSC) $(APP_CSC_FLAGS) -keyfile:keys -target:exe -doc:docs.xml -out:$(app_path)				\
		-reference:Mono.CompilerServices.SymbolWriter.dll,Mono.Cecil.dll,System.Configuration.dll	\
		-resource:IgnoreList.txt -resource:SysIgnore.txt $(xml_resources)							\
		$(exe_files)

tests_files := $(extra_test_files) $(framework_files) $(rules_files) $(test_files)
$(tests_path): $(tests_files) 
	@echo "building $(tests_path)"
	@$(CSC) $(CSC_FLAGS) -define:TEST -pkg:mono-nunit -target:library -out:$(tests_path)	\
		-reference:Mono.CompilerServices.SymbolWriter.dll,Mono.Cecil.dll					\
		-reference:System.Configuration.dll,System.Data.dll,System.Windows.Forms.dll		\
		$(tests_files) 
		
$(bin_path)/evildoer.dll: extras/evildoer/Expected.xml extras/evildoer/*.cs
	@echo "building $(bin_path)/evildoer.dll"
	@./gen_version.sh $(version) extras/evildoer/AssemblyVersion.cs
	@$(CSC) $(CSC_FLAGS) -target:library -out:$(bin_path)/evildoer.dll 		\
		-reference:System.Windows.Forms.dll									\
		-resource:extras/evildoer/Expected.xml								\
		extras/evildoer/*.cs 

$(bin_path)/NoSecurity.exe: extras/miscevil/NoSecurity.xml extras/miscevil/NoSecurity.cs 
	@echo "building $(bin_path)/NoSecurity.exe"
	@./gen_version.sh $(version) extras/miscevil/AssemblyVersion.cs
	@$(CSC) $(CSC_FLAGS) -target:exe -out:$(bin_path)/NoSecurity.exe 		\
		-reference:System.Windows.Forms.dll									\
		-resource:extras/miscevil/NoSecurity.xml							\
		extras/miscevil/AssemblyVersion.cs extras/miscevil/NoSecurity.cs 

$(bin_path)/FullTrust.dll: keys extras/miscevil/FullTrust.cs 
	@echo "building $(bin_path)/FullTrust.dll"
	@./gen_version.sh $(version) extras/miscevil/AssemblyVersion.cs
	@$(CSC) $(CSC_FLAGS) -keyfile:keys -target:library -out:$(bin_path)/FullTrust.dll 		\
		-reference:System.Windows.Forms.dll													\
		extras/miscevil/AssemblyVersion.cs extras/miscevil/FullTrust.cs 

$(bin_path)/APTCA.dll: $(bin_path)/FullTrust.dll extras/miscevil/APTCA.xml extras/miscevil/APTCA.cs 
	@echo "building $(bin_path)/APTCA.dll"
	@./gen_version.sh $(version) extras/miscevil/AssemblyVersion.cs
	@$(CSC) $(CSC_FLAGS) -target:library -out:$(bin_path)/APTCA.dll 		\
		-reference:System.Windows.Forms.dll,$(bin_path)/FullTrust.dll		\
		-resource:extras/miscevil/APTCA.xml									\
		extras/miscevil/AssemblyVersion.cs extras/miscevil/APTCA.cs 

$(bin_path)/APTCA2.dll: $(bin_path)/FullTrust.dll extras/miscevil/APTCA2.xml extras/miscevil/APTCA2.cs 
	@echo "building $(bin_path)/APTCA2.dll"
	@./gen_version.sh $(version) extras/miscevil/AssemblyVersion.cs
	@$(CSC) $(CSC_FLAGS) -target:library -out:$(bin_path)/APTCA2.dll 		\
		-reference:System.Windows.Forms.dll,$(bin_path)/FullTrust.dll		\
		-resource:extras/miscevil/APTCA2.xml								\
		extras/miscevil/AssemblyVersion.cs extras/miscevil/APTCA2.cs 

# Note that this deliberately does not use CSC_FLAGS.
$(bin_path)/APTCA3.dll: $(bin_path)/FullTrust.dll extras/miscevil/APTCA3.xml extras/miscevil/APTCA3.cs 
	@echo "building $(bin_path)/APTCA3.dll"
	@./gen_version.sh $(version) extras/miscevil/AssemblyVersion.cs
	@$(CSC) -target:library -out:$(bin_path)/APTCA3.dll 					\
		-reference:System.Windows.Forms.dll,$(bin_path)/FullTrust.dll		\
		-resource:extras/miscevil/APTCA3.xml								\
		extras/miscevil/AssemblyVersion.cs extras/miscevil/APTCA3.cs 

$(ftest_path): extras/functest/*.xml source/framework/DBC.cs source/framework/DisableRuleAttribute.cs source/internal/GetOptions.cs extras/functest/*.cs
	@echo "building $(ftest_path)"
	@./gen_version.sh $(version) extras/functest/AssemblyVersion.cs
	@$(CSC) $(CSC_FLAGS) -target:exe -out:$(ftest_path) 												\
		-reference:System.Windows.Forms.dll																\
		-resource:extras/functest/Expected.schema.xml -resource:extras/functest/Smoke.schema.xml		\
		source/framework/DBC.cs source/framework/DisableRuleAttribute.cs source/internal/GetOptions.cs	\
		extras/functest/*.cs

$(app_path).config:
	@echo "generating $(app_path).config"
	@echo "<?xml version = \"1.0\" encoding = \"utf-8\" ?>" > $(app_path).config
	@echo "<configuration>" >> $(app_path).config
	@echo "	<appSettings>" >> $(app_path).config
	@echo "		<add key = \"logfile\" value = \"$(LOG_PATH)\"/>" >> $(app_path).config
	@echo "		<add key = \"topic:System.Object\" value = \"Info\"/>	<!-- may be off, Error, Warning, Info, Trace, or Debug -->" >> $(app_path).config
	@echo "		<add key = \"topic:Smokey.Internal.Rules.AttributePropertiesRule\" value = \"Debug\"/>" >> $(app_path).config
	@echo "		<add key = \"consoleWidth\" value = \"80\"/>			<!-- TextReport breaks lines so that that they aren't longer than this -->" >> $(app_path).config
	@echo "	</appSettings>" >> $(app_path).config
	@echo "</configuration>" >> $(app_path).config

$(tests_path).config:
	@echo "generating $(tests_path).config"
	@echo "<?xml version = \"1.0\" encoding = \"utf-8\" ?>" > $(tests_path).config
	@echo "<configuration>" >> $(tests_path).config
	@echo "	<appSettings>" >> $(tests_path).config
	@echo "		<add key = \"logfile\" value = \"$(LOG_PATH)\"/>" >> $(tests_path).config
	@echo "		<add key = \"topic:System.Object\" value = \"Info\"/>	<!-- may be off, Error, Warning, Info, Trace, or Debug -->" >> $(tests_path).config
	@echo "		<add key = \"topic:Smokey.Internal.Rules.AttributePropertiesRule\" value = \"Debug\"/>" >> $(tests_path).config
	@echo "		<add key = \"consoleWidth\" value = \"80\"/>			<!-- TextReport breaks lines so that that they aren't longer than this -->" >> $(tests_path).config
	@echo "	</appSettings>" >> $(tests_path).config
	@echo "</configuration>" >> $(tests_path).config

# -----------------------------------------------------------------------------
# Other targets
keys: 
	sn -k keys
	
bin:
	-mkdir $(bin_path)
	
smokey_flags := --not-localized -set:naming:jurassic -set:ignoreList:IgnoreList.txt
smokey_flags += -exclude-check:D1015	# ExceptionConstructors
smokey_flags += -exclude-check:D1024	# SerializeException
smokey_flags += -exclude-check:D1031	# PublicType
smokey_flags += -exclude-check:P1005	# StringConcat
smoke: $(app_path)
	@-$(MONO) --debug $(app_path) $(smokey_flags) $(app_path)
	
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

install: bin $(app_path) $(app_path).config
	cp $(app_path) $(INSTALL_DIR)
	chmod -x $(INSTALL_DIR)/smokey.exe
	cp $(app_path).config $(INSTALL_DIR)
	echo "#!/bin/sh" > $(INSTALL_DIR)/smoke
	echo "exec -a smokey.exe $MONO $(INSTALL_DIR)/smokey.exe \$@" >> $(INSTALL_DIR)/smoke
	chmod +x $(INSTALL_DIR)/smoke
	
uninstall:
	-rm $(INSTALL_DIR)/smokey.exe
	-rm $(INSTALL_DIR)/smokey.exe.config
	-rm $(INSTALL_DIR)/smoke
	-rm $(LOG_PATH)

clean:
	-rm  $(bin_path)/TestResult.xml
	-rm  $(bin_path)/*.exe
	-rm  $(bin_path)/*.dll
	-rm  $(bin_path)/*.mdb
	
tar_binary: $(app_path)
	tar --create --compress --file=smokey_bin-$(version).tar.gz $(app_path) CHANGES CHANGE_LOG README
	
tar_source: 
	tar --create --compress --file=smokey_src-$(version).tar.gz AUTHORS CHANGES CHANGE_LOG IgnoreList.txt MIT.X11 Makefile README SysIgnore.txt gen_docs.sh gen_match.py gen_version.sh get_version.sh custom extras source

tar: tar_binary tar_source
