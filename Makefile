# -----------------------------------------------------------------------------
# Public variables
export CSC ?= gmcs
export NUNIT ?= nunit-console2

# TODO: need PROFILE var

ifdef RELEASE
	CSC_FLAGS ?= -checked+ -warn:4 -nowarn:1591 -optimize+
else
	CSC_FLAGS ?= -checked+ -debug+ -warn:4 -nowarn:1591 -define:DEBUG
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
extra_test_files := source/internal/AssertTraceListener.cs source/internal/Break.cs source/internal/GetOptions.cs source/internal/Ignore.cs source/internal/Reformat.cs

xml_files := $(strip $(shell find source/internal/rules/xml -name "*.xml" -print))
xml_resources := $(shell echo $(xml_files) | sed "s/source/-resource:source/g")

base_version := 1.3.xxx.0										# major.minor.build.revision
version := $(shell ./get_version.sh $(base_version) build_num)	# this will increment the build number stored in build_num

# -----------------------------------------------------------------------------
# Primary targets
all: bin $(app_path) $(tests_path) $(app_path).config $(tests_path).config

app: $(app_path) $(app_path).config

check: $(tests_path) $(tests_path).config
	$(NUNIT) -nologo -config=$(tests_path).config $(tests_path)

check1: $(tests_path) $(tests_path).config
	$(NUNIT) -nologo -config=$(tests_path).config -fixture=Smokey.Tests.AvoidBoxingTest $(tests_path)

ftest_asms := $(bin_path)/evildoer.dll,$(bin_path)/NoSecurity.exe,$(bin_path)/APTCA.dll,$(bin_path)/APTCA2.dll,$(bin_path)/APTCA3.dll
fcheck: $(app_path) $(subst $(comma), ,$(ftest_asms)) $(bin_path)/FullTrust.dll $(ftest_path)
	mono --debug $(ftest_path) -exe:$(app_path) -asm:$(ftest_asms)
	
# -----------------------------------------------------------------------------
# Generated targets
exe_files := $(app_files) $(framework_files) $(internal_files)
$(app_path):keys $(exe_files) IgnoreList.txt SysIgnore.txt $(xml_files)
	@echo "building $(app_path)"
	@./gen_version.sh $(version) source/internal/AssemblyVersion.cs
	@$(CSC) $(CSC_FLAGS) -keyfile:keys -target:exe -doc:docs.xml -out:$(app_path) 					\
		-reference:Mono.CompilerServices.SymbolWriter.dll,Mono.Cecil.dll,System.Configuration.dll	\
		-resource:IgnoreList.txt -resource:SysIgnore.txt $(xml_resources)							\
		$(exe_files)

tests_files := $(extra_test_files) $(framework_files) $(rules_files) $(test_files)
$(tests_path): $(tests_files) 
	@echo "building $(tests_path)"
	@$(CSC) $(CSC_FLAGS) -define:TEST -target:library -out:$(tests_path) 							\
		-reference:nunit.framework.dll,Mono.CompilerServices.SymbolWriter.dll,Mono.Cecil.dll		\
		-reference:System.Configuration.dll,System.Data.dll,System.Windows.Forms.dll				\
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
	@echo "		<add key = \"logfile\" value = \"$(cur_dir)/smokey.log\"/>" >> $(app_path).config
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
	@echo "		<add key = \"logfile\" value = \"$(cur_dir)/smokey.log\"/>" >> $(tests_path).config
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
	-mkdir "$(bin_path)"
	
clean:
	-rm  $(bin_path)/TestResult.xml
	-rm  $(bin_path)/*.exe
	-rm  $(bin_path)/*.dll
	-rm  $(bin_path)/*.mdb
