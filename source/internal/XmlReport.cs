// Copyright (C) 2007 Jesse Jones
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.IO;
using System.Xml;
using Smokey.App;
using Smokey.Framework;

namespace Smokey.Internal
{	
//	internal sealed class Report {}
	
	// Reports rule violations as xml. This is useful for tools such as
	// daily build systems.
	internal static class XmlReport 
	{
		public static void Report(string assemblyPath, string outPath, Error[] errors)
		{
			Log.InfoLine(true, "generating an xml report for {0} violations", errors.Length);
			
			XmlWriterSettings settings = new XmlWriterSettings();
			settings.Indent       = true;
			settings.IndentChars  = "\t";
			settings.NewLineChars = Environment.NewLine;
			
			Stream stream = outPath == "stdout" ? Console.OpenStandardOutput() : new FileStream(outPath, FileMode.Create, FileAccess.Write);
			using (XmlWriter writer = XmlWriter.Create(stream, settings)) 
			{
				writer.WriteStartElement("Errors");
				writer.WriteAttributeString("assembly", assemblyPath);

				foreach (Error error in errors)
					if (error.Violation.Severity == Severity.Error)
						DoError(writer, error.Violation, error.Location);

				foreach (Error error in errors)
					if (error.Violation.Severity == Severity.Warning)
						DoError(writer, error.Violation, error.Location);

				foreach (Error error in errors)
					if (error.Violation.Severity == Severity.Nitpick)
						DoError(writer, error.Violation, error.Location);

				writer.WriteEndElement();
			}
		}
				
		#region Private Methods -----------------------------------------------
		private static void DoError(XmlWriter writer, Violation violation, Location location)
		{
			writer.WriteStartElement("Error");
			
			DoLocation(writer, location);
			DoViolation(writer, violation);

			writer.WriteEndElement();
		}
		
		private static void DoLocation(XmlWriter writer, Location location)
		{
			writer.WriteStartElement("Location");
			
			writer.WriteAttributeString("name", location.Name);
			writer.WriteAttributeString("details", location.Details);
			
			if (location.File != null)
			{
				writer.WriteAttributeString("file", location.File);
				
				if (location.Line >= 0)
					writer.WriteAttributeString("line", location.Line.ToString());

				if (location.Offset >= 0)
					writer.WriteAttributeString("offset", location.Offset.ToString());
			}
			
			writer.WriteEndElement();
		}
		
		private static void DoViolation(XmlWriter writer, Violation violation)
		{
			writer.WriteStartElement("Violation");
									
			writer.WriteAttributeString("checkID", violation.CheckID);
			writer.WriteAttributeString("typeName", violation.TypeName);
			writer.WriteAttributeString("category", violation.Category);
			writer.WriteAttributeString("severity", violation.Severity.ToString());
			writer.WriteAttributeString("breaking", violation.Breaking ? "true" : "false");

			writer.WriteStartElement("Cause");
			writer.WriteString(BaseReport.Expand(Reformat.Text(violation.Cause)));
			writer.WriteEndElement();

			writer.WriteStartElement("Description");
			writer.WriteString(BaseReport.Expand(Reformat.Text(violation.Description)));
			writer.WriteEndElement();

			writer.WriteStartElement("Fix");
			writer.WriteString(BaseReport.Expand(Reformat.Text(violation.Fix)));
			writer.WriteEndElement();

			writer.WriteStartElement("CSharp");
			writer.WriteString(Reformat.Code(violation.Csharp));
			writer.WriteEndElement();

			writer.WriteEndElement();
		}
		#endregion
	}
}