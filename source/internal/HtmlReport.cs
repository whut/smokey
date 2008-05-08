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

using Smokey.App;
using Smokey.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Smokey.Internal
{
	using Violations = Dictionary<Violation, List<Location>>;	
	
	internal static class ReportHelpers
	{
		public static Violations GetViolations(Error[] errors)
		{
			Violations violations = new Violations(errors.Length);
			
			foreach (Error error in errors)
			{
				List<Location> locations = null;
				
				if (!violations.TryGetValue(error.Violation, out locations))
				{
					locations = new List<Location>();
					violations.Add(error.Violation, locations);
				}
				
				locations.Add(error.Location);
			}
			
			return violations;
		}
	}	
	
	// Reports rule violations using html to a file.
	internal static class HtmlReport
	{		
		public static void Report(string assemblyPath, string outPath, Error[] errors, int numRules, TimeSpan totalTime)
		{
			TextWriter stream = outPath == "stdout" ? Console.Out : new StreamWriter(outPath);
			
			string assemblyFile = HtmlHelpers.Escape(Path.GetFileName(assemblyPath));
			DoWriteHeader(stream, assemblyFile);
				
			stream.WriteLine("<body>");
				stream.WriteLine("<h1 class = \"heading\">{0} Violations</h1>", assemblyFile);

			if (errors.Length == 0)
			{
				stream.WriteLine("<p>No errors.</p>");
			}
			else
			{
				int counter = 1;
				Violations violations = ReportHelpers.GetViolations(errors);
				DoWriteViolations(stream, Severity.Error, violations, ref counter);
				DoWriteViolations(stream, Severity.Warning, violations, ref counter);
				DoWriteViolations(stream, Severity.Nitpick, violations, ref counter);
				DoWriteSummary(stream, errors, numRules, totalTime);
			}
			stream.WriteLine("</body>");

			DoWriteTrailer(stream);

			stream.Flush();
		}
		
		#region Private methods		
		private static void DoWriteHeader(TextWriter stream, string assemblyFile)
		{
			HtmlHelpers.WriteHeader(stream);
			
			stream.WriteLine("<head>");
				stream.WriteLine("<title>{0} Violations</title>", assemblyFile);
		
				stream.WriteLine("<style type = \"text/css\">");
					HtmlHelpers.WriteCss(stream);
				stream.WriteLine("</style>");
			stream.WriteLine("</head>");
		}
		
		private static void DoWriteTrailer(TextWriter stream)
		{
			stream.WriteLine("</html>");
		}
		
		private static void DoWriteViolations(TextWriter stream, Severity severity, Violations violations, ref int counter)
		{
			foreach (KeyValuePair<Violation, List<Location>> pair in violations)
			{
				if (pair.Key.Severity == severity)
				{
					DoWriteViolation(stream, counter, pair.Key, pair.Value);
					counter += pair.Value.Count;
				}
			}
		}
		
		private static void DoWriteViolation(TextWriter stream, int counter, Violation violation, List<Location> locations)
		{
			string countStr = DoGetCounter(counter, locations.Count);
			
			stream.WriteLine("<h2 class = \"category\">{0} {1}</h2>", countStr, violation.TypeName);
			HtmlHelpers.WriteSeverity(stream, "Severity", violation.Severity.ToString());
			HtmlHelpers.WriteText(stream, "Breaking", violation.Breaking ? "Yes" : "No");
			HtmlHelpers.WriteText(stream, "CheckId", violation.CheckID);

			foreach (Location loc in locations)
			{
				stream.WriteLine("<table border = \"2\" rules = \"none\" class = \"location\">");
	
					stream.WriteLine("<tr>");
					stream.WriteLine("<td>File:</td>");
					stream.WriteLine("<td>");
						if (loc.Line >= 0)
							stream.WriteLine("{0}:{1}", HtmlHelpers.Escape(loc.File), loc.Line);
						else if (loc.File != null && loc.File.Length > 0)
							stream.WriteLine(HtmlHelpers.Escape(loc.File));
						else
							stream.WriteLine("&lt;unknown&gt;");
					stream.WriteLine("</td>");
				stream.WriteLine("</tr>");

				stream.WriteLine("<tr>");
					stream.WriteLine("<td>Name:</td>");
					stream.WriteLine("<td>");
						stream.WriteLine(HtmlHelpers.Escape(loc.Name));
					stream.WriteLine("</td>");
				stream.WriteLine("</tr>");

				if (loc.Details != null && loc.Details.Length > 0)
				{
					stream.WriteLine("<tr>");
						stream.WriteLine("<td>Details:</td>");
						stream.WriteLine("<td>");
							stream.WriteLine(HtmlHelpers.Escape(loc.Details));
						stream.WriteLine("</td>");
					stream.WriteLine("</tr>");
				}				
	
				stream.WriteLine("</table>");
			}

			HtmlHelpers.WriteText(stream, "Cause", BaseReport.Expand(violation.Cause));
			HtmlHelpers.WriteText(stream, "Description", BaseReport.Expand(violation.Description));
			HtmlHelpers.WriteText(stream, "Fix", violation.Fix);
		
			if (violation.Csharp.Length > 0)
			{
				string code = Reformat.Code(violation.Csharp);
				code = HtmlHelpers.Escape(code);
				code = HtmlHelpers.ProcessLinks(code);
				code = HtmlHelpers.ColorizeCode(code);
				stream.WriteLine("<pre class = \"code\">{0}</pre>", code);
			}

			stream.WriteLine("<hr class = \"separator\">");
		}
		
		private static void DoWriteSummary(TextWriter stream, Error[] errors, int numRules, TimeSpan totalTime)
		{
			int numErrors = 0, numWarnings = 0, numNitpicks = 0;
			foreach (Error error in errors)
			{
				if (error.Violation.Severity == Severity.Error)
					++numErrors;
				else if (error.Violation.Severity == Severity.Warning)
					++numWarnings;
				else
					++numNitpicks;
			}
			
			stream.Write("<p>");
			if (numErrors == 0)
				stream.Write("There were no errors ");
			else if (numErrors == 1)			
				stream.Write("There was one error ");
			else 			
				stream.Write("There were {0} errors ", numErrors.ToString());	// use ToString to avoid the AvoidBoxing warning

			if (numWarnings == 0)
				stream.WriteLine("and no warnings.");
			else if (numWarnings == 1)			
				stream.WriteLine("and one warning.");
			else 			
				stream.WriteLine("and {0} warnings.", numWarnings.ToString());
			stream.WriteLine("There were {0} nitpicks.", numNitpicks.ToString());
			stream.Write("</p>");

			stream.Write("<p>");
			stream.WriteLine("Checked {0} rules in {1:#.###} seconds.", numRules, totalTime.TotalSeconds);
			stream.WriteLine("</p>");
		}
		
		private static string DoGetCounter(int counter, int count)
		{
			string str;
			if (count == 1)			
				str = string.Format("{0}", counter);
			else
				str = string.Format("{0}-{1}", counter, counter + count - 1);
				
			return str;
		}
		#endregion
	}
}