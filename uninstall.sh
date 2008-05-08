#!/bin/sh -x
# Uninstalls Smokey and associated files from /usr/local/bin.

BIN_DIR=/usr/local/bin
LOG_FILE=/tmp/smokey.log

rm -f "$BIN_DIR/smokey.exe"
rm -f "$BIN_DIR/smokey.exe.config"
rm -f "$BIN_DIR/smoke"
rm -f "$LOG_FILE"
