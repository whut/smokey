# -----------------------------------------------------------------------------
# Public variables
MONO ?= $(shell which mono)

LOG_PATH ?= /tmp/smokey.log
INSTALL_DIR ?= /usr/local/bin
	
# -----------------------------------------------------------------------------
# Targets
smokey.exe.config:
	@echo "generating smokey.exe.config"
	@echo "<?xml version = \"1.0\" encoding = \"utf-8\" ?>" > smokey.exe.config
	@echo "<configuration>" >> smokey.exe.config
	@echo "	<appSettings>" >> smokey.exe.config
	@echo "		<add key = \"logfile\" value = \"$(LOG_PATH)\"/>" >> smokey.exe.config
	@echo "		<add key = \"topic:System.Object\" value = \"Info\"/>	<!-- may be off, Error, Warning, Info, Trace, or Debug -->" >> smokey.exe.config
	@echo "		<add key = \"topic:Smokey.Internal.Rules.AttributePropertiesRule\" value = \"Debug\"/>" >> smokey.exe.config
	@echo "		<add key = \"consoleWidth\" value = \"80\"/>			<!-- TextReport breaks lines so that that they aren't longer than this -->" >> smokey.exe.config
	@echo "	</appSettings>" >> smokey.exe.config
	@echo "</configuration>" >> smokey.exe.config

install: smokey.exe.config
	cp smokey.exe $(INSTALL_DIR)
	chmod -x $(INSTALL_DIR)/smokey.exe
	if [[ ! -e $(INSTALL_DIR)/smokey.exe.config ]]; then cp smokey.exe.config $(INSTALL_DIR); fi
	echo "#!/bin/sh" > $(INSTALL_DIR)/smoke
	echo "exec -a smokey.exe $(MONO) $(INSTALL_DIR)/smokey.exe \x24@" >> $(INSTALL_DIR)/smoke
	chmod +x $(INSTALL_DIR)/smoke
	
uninstall:
	-rm $(INSTALL_DIR)/smokey.exe
#	-rm $(INSTALL_DIR)/smokey.exe.config
	-rm $(INSTALL_DIR)/smoke
	-rm $(LOG_PATH)
	
help:
	@echo "The targets are:"
	@echo "install   - install the exe and a simple smoke script"
	@echo "uninstall - remove the exe and the smoke script"
	@echo " "
	@echo "Variables are:"
	@echo "MONO - the mono the smoke script will use, defaults to $(MONO)"
	@echo "LOG_PATH - file smokey writes its logs to, defaults to $(LOG_PATH)"
	@echo "INSTALL_DIR - where to put the exe, defaults to $(INSTALL_DIR)"
	@echo " "
	@echo "Here's an example:"	
	@echo "sudo make LOG_PATH=~/smokey.log install"	

