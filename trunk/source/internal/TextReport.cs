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
	
	// Reports rule violations using unformatted text to a file.
	internal static class TextReport
	{
		public static void Report(string assemblyPath, string outPath, Error[] errors, int numRules, TimeSpan totalTime)
		{
			if (ms_consoleWidth == 0)
				ms_consoleWidth = Settings.Get("consoleWidth", 80);
			
			Profile.Start("Report");
			Log.InfoLine(true, "generating a text report for {0} violations", errors.Length);

			TextWriter writer = outPath == "stdout" ? Console.Out : new StreamWriter(outPath);
				
			writer.WriteLine("Smokey '{0}' report:", assemblyPath);
			writer.WriteLine(" ");

			if (errors.Length == 0)
			{
				writer.WriteLine("no errors");
			}
			else
			{
				int counter = 1;
				Violations violations = ReportHelpers.GetViolations(errors);
				DoWriteViolations(writer, Severity.Error, violations, ref counter);
				DoWriteViolations(writer, Severity.Warning, violations, ref counter);
				DoWriteViolations(writer, Severity.Nitpick, violations, ref counter);
				DoSummary(writer, errors, numRules, totalTime);
			}
			
			writer.Flush();
			Profile.Stop("Report");
		}
		
		#region Private methods
		private static void DoWriteViolations(TextWriter writer, Severity severity, Violations violations, ref int counter)
		{
			foreach (KeyValuePair<Violation, List<Location>> pair in violations)
			{
				if (pair.Key.Severity == severity)
				{
					DoError(writer, counter, pair.Key, pair.Value);
					counter += pair.Value.Count;
				}
			}
		}
				
		private static string DoGetCounter(int counter, int count)
		{
			string str;
			if (count == 1)			
				str = string.Format("{0}. ", counter);
			else
				str = string.Format("{0}-{1}. ", counter, counter + count - 1);
				
			return str + "---------------------------------------------------------------------";
		}
		
		private static void DoError(TextWriter writer, int counter, Violation violation, List<Location> locations)
		{
			writer.WriteLine(DoGetCounter(counter, locations.Count));
			writer.WriteLine("TypeName: {0}", violation.TypeName);
			writer.WriteLine("CheckID:  {0}", violation.CheckID);
			writer.WriteLine("Category: {0}", violation.Category);
			writer.WriteLine("Severity: {0}", violation.Severity);
			writer.WriteLine("Breaking: {0}", violation.Breaking ? "Yes" : "No");
			writer.WriteLine(" ");

			foreach (Location loc in locations)
			{
				if (loc.Line >= 0)
					writer.WriteLine("File: {0}:{1}", loc.File, loc.Line);
				else if (loc.File != null && loc.File.Length > 0 && loc.File != "<unknown>")
					writer.WriteLine("File: {0}", loc.File);
					
				writer.WriteLine(loc.Name);
				if (loc.Offset >= 0 && loc.Line < 0)
					writer.WriteLine("Offset: {0:X4}", loc.Offset);

				if (loc.Details != null && loc.Details.Length > 0)
					writer.WriteLine(loc.Details);
				writer.WriteLine(" ");
			}

			writer.WriteLine("Cause:");
			writer.WriteLine(DoMungeText(violation.Cause));
			writer.WriteLine(" ");

			writer.WriteLine("Rule Description:");
			writer.WriteLine(DoMungeText(violation.Description));
			writer.WriteLine(" ");

			if (violation.Fix.Length > 0)
			{
				writer.WriteLine("How to Fix Violations:");
				writer.WriteLine(DoMungeText(violation.Fix));
				writer.WriteLine(" ");
			}
			
			if (violation.Csharp.Length > 0)
			{
				writer.WriteLine("C# Example:");
				writer.WriteLine(DoMungeCode(violation.Csharp));	
				writer.WriteLine(" ");
			}
		}
		
		private static string DoMungeText(string text)
		{
			string result;
			
			result = Reformat.Text(text);
			result = Break.Lines(result, ms_consoleWidth);
			result = DoBulletedList(result);
			result = DoTable(result);
			result = BaseReport.Expand(result);
			
			return result;
		}
		
		private static string DoBulletedList(string text)
		{
			StringBuilder result = new StringBuilder(text.Length);

			text = text.Replace(NewLine.Windows, NewLine.Unix);	// need to do this so split doesnt freak out
			string[] lines = text.Split(NewLine.Mac[0], NewLine.Unix[0]);

			for (int i = 0; i < lines.Length; ++i)
			{
				string line = lines[i];
				
				if (line.StartsWith("* "))
					result.Append("   ");
					
				result.Append(line);
					
				if (i + 1 < lines.Length)
					result.Append(Environment.NewLine);
			}
			
			return result.ToString();
		}
						
		private static string DoTable(string text)
		{
			StringBuilder result = new StringBuilder(text.Length);

			text = text.Replace(NewLine.Windows, NewLine.Unix);	// need to do this so split doesnt freak out
			string[] lines = text.Split(NewLine.Mac[0], NewLine.Unix[0]);

			for (int i = 0; i < lines.Length; ++i)
			{
				string line = lines[i];
				
				if (line.StartsWith("||"))
					result.Append("   ");
				
				line = line.Replace("||", string.Empty);
				result.Append(line);
					
				if (i + 1 < lines.Length)
					result.Append(Environment.NewLine);
			}
			
			return result.ToString();
		}
						
		private static string DoMungeCode(string text)
		{
			string result;
			
			result = Reformat.Code(text);
//			result = Break.Lines(result, ms_consoleWidth);
			result = result.TrimEnd(' ', '\r', '\n');
			
			return result;
		}
								
		private static void DoSummary(TextWriter writer, Error[] errors, int numRules, TimeSpan totalTime)
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
			
			if (numErrors == 0)
				writer.Write("There were no errors ");
			else if (numErrors == 1)			
				writer.Write("There was one error ");
			else 			
				writer.Write("There were {0} errors ", numErrors.ToString());	// use ToString to avoid the AvoidBoxing warning

			if (numWarnings == 0)
				writer.WriteLine("and no warnings.");
			else if (numWarnings == 1)			
				writer.WriteLine("and one warning.");
			else 			
				writer.WriteLine("and {0} warnings.", numWarnings.ToString());
			writer.WriteLine("There were {0} nitpicks.", numNitpicks.ToString());

			writer.WriteLine("Checked {0} rules in {1:#.###} seconds.", numRules, totalTime.TotalSeconds);
		}
		#endregion
		
		#region Fields
		private static int ms_consoleWidth;
		#endregion
	}
}