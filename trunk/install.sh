#!/bin/sh -x
# Installs smokey.exe and a simple smoke script into /usr/local/bin.

BIN_DIR=/usr/local/bin
LOG_FILE=/tmp/smokey.log
MONO=`which mono`

cp bin/smokey.exe "$BIN_DIR"
chmod -x "$BIN_DIR/smokey.exe"

echo "<?xml version = \"1.0\" encoding = \"utf-8\" ?>" > "$BIN_DIR/smokey.exe.config"
echo "<configuration>" >> "$BIN_DIR/smokey.exe.config"
echo "	<appSettings>" >> "$BIN_DIR/smokey.exe.config"
echo "		<add key = \"logfile\" value = \"$LOG_FILE\"/>" >> "$BIN_DIR/smokey.exe.config"
echo "		<add key = \"topic:System.Object\" value = \"Info\"/>	<!-- may be off, Error, Warning, Info, Trace, or Debug -->" >> "$BIN_DIR/smokey.exe.config"
echo "		<add key = \"consoleWidth\" value = \"80\"/>			<!-- TextReport breaks lines so that that they aren't longer than this -->" >> "$BIN_DIR/smokey.exe.config"
echo "		<add key = \"topic:System.Object\" value = \"Warning\"/>	" >> "$BIN_DIR/smokey.exe.config"
echo "	</appSettings>" >> "$BIN_DIR/smokey.exe.config"
echo "</configuration>" >> "$BIN_DIR/smokey.exe.config"

echo "#!/bin/sh" > "$BIN_DIR/smoke"
echo "exec -a smokey.exe $MONO $BIN_DIR/smokey.exe \$@" >> "$BIN_DIR/smoke"
chmod +x "$BIN_DIR/smoke"