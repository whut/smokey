#!/bin/bash
FILE="$1"

if [ -f "$FILE" ]
then
	PATTERN="[0-9]+\\.[0-9]+\\.[0-9]+\\.[0-9]+"
	VERSION=`grep -o -E "$PATTERN" $FILE`
	
	MAJOR_VERSION=`echo $VERSION | cut -d. -f1`
	MINOR_REVISION=`echo $VERSION | cut -d. -f2`
	BUILD_NUMBER=`echo $VERSION | cut -d. -f3`
	REVISION=`echo $VERSION | cut -d. -f4`
	
	((BUILD_NUMBER = $BUILD_NUMBER + 1))
	VERSION="$MAJOR_VERSION.$MINOR_REVISION.$BUILD_NUMBER.$REVISION"
	
	echo "// Machine generated on `date`" > $FILE
	echo "using System.Reflection;" >> $FILE
	echo " " >> $FILE
	echo "[assembly: AssemblyVersion(\"$VERSION\")]" >> $FILE
	
	touch -t 0012221500 $FILE		# don't regenerate assemblies if only the build number changed
else
	echo "// Machine generated on `date`" > $FILE
	echo "using System.Reflection;" >> $FILE
	echo " " >> $FILE
	echo "[assembly: AssemblyVersion(\"0.1.0.0\")]" >> $FILE
fi
