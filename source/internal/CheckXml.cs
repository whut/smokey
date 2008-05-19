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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Xml;
using Smokey.Framework;
using Smokey.Framework.Support;
using Smokey.Internal.Rules;

namespace Smokey.Internal
{
#if DEBUG
	internal sealed class CheckXml
	{			
		public void Check()
		{
			Assembly assembly = Assembly.GetExecutingAssembly();
			foreach (string file in ViolationDatabase.XmlFiles())
			{
				Stream stream = assembly.GetManifestResourceStream(file);
				LoadXML(stream);
			}
		}
		
		#region Private methods		
		public void LoadXML(Stream stream)
		{						
			XmlDocument xml = new XmlDocument();
			XmlReader reader = XmlReader.Create(stream);
			xml.Load(reader);
			
			foreach (XmlNode child in xml.ChildNodes)	
			{
				if (child.Name == "Violations")
					DoViolations(child);			
			}
		}

		private void DoViolations(XmlNode violations)
		{
			foreach (XmlNode child in violations.ChildNodes)
			{
				if (child.Name == "Violation")
					DoViolation(child);			
			}
		}

		private void DoViolation(XmlNode violation)
		{
			foreach (XmlNode child in violation.ChildNodes)
			{
				if (child.Name == "Translation")
				{
					DoTranslation(child);
				}
			}			
		}
				
		private void DoTranslation(XmlNode translation)
		{
			string lang = translation.Attributes["lang"].Value;
			string typeName = translation.Attributes["typeName"].Value;
			string category = translation.Attributes["category"].Value;
						
			string results = string.Empty;
			foreach (XmlNode child in translation.ChildNodes)
			{
				if (child.Name == "Cause" || child.Name == "Description" || child.Name == "Fix")
				{
					if (lang == "en")		// TODO: could support non-english as well
					{
						string result = DoSpellCheck(child.InnerText);		
						if (result.Length > 0)
							results = string.Format("{0}{1}{2}", results, result, Environment.NewLine);			
					}
				}
				else if (child.Name == "CSharp")
				{
					string sep = new string('\x82', 1);
					string text = child.InnerText.Replace("// --------------------------------------------------------", sep);
					string[] parts = text.Split(new char[]{sep[0]}, StringSplitOptions.RemoveEmptyEntries);
					
					foreach (string part in parts)
					{
						string result = DoCompile(part);		
						if (result.Length > 0)
							results = string.Format("{0}{1}{2}", results, result, Environment.NewLine);			
					}
				}
			}
			
			if (results.Length > 0)
			{
				string padding = string.Empty; 
				if (40 - typeName.Length - category.Length > 0)
					padding = "-".PadRight(40 - typeName.Length - category.Length, '-');
					
				Console.WriteLine("----- checking {0}/{1} {2}", category, typeName, padding);
				Console.WriteLine(results);
			}
		}
		
		private string DoSpellCheck(string text)
		{
			string result = string.Empty;
			
			List<string> misspelled = Aspell.Instance.Check(text);
			misspelled = misspelled.RemoveIf(w => Array.IndexOf(m_ignore, w) >= 0);
			
			if (misspelled.Count > 0)
				result = string.Format("misspelled: {0}", string.Join(" ", misspelled.ToArray()));
				
			return result;
		}
		
		private static string DoCompile(string text)
		{
			// Write the C# code out to a temp file.
			string srcPath = Path.GetTempFileName();
			using (StreamWriter stream = new StreamWriter(srcPath)) 
			{
				stream.WriteLine("using System;");
				stream.WriteLine(text);
			}	
			
			// Add referrences to assemblies as need be.
			string refs = string.Empty;
			if (text.Contains("using System.Data;"))
			{
				refs += "-r:System.Data.dll ";
			}
			if (text.Contains("using System.Windows.Forms;"))
			{
				refs += "-r:System.Windows.Forms.dll ";
			}
			if (text.Contains("using System.Drawing;"))
			{
				refs += "-r:System.Drawing.dll ";
			}
			if (text.Contains("using Mono.GetOptions;"))
			{
				refs += "-r:Mono.GetOptions.dll ";
			}
			
			// Compile the code.
			string libPath = Path.GetTempFileName();
			string args = string.Format("-target:library -out:{0} -warnaserror+ -warn:4 {1} {2}", libPath, srcPath, refs);
			ProcessStartInfo info = new ProcessStartInfo("gmcs", args);
			info.RedirectStandardError = true;
			info.RedirectStandardOutput = true;
			info.UseShellExecute = false;
			
			Process process = Process.Start(info);
			process.WaitForExit();
			
			// Returns the results.
			string result = string.Empty;
			
			if (process.ExitCode != 0)
			{
				result = process.StandardError.ReadToEnd();
			}
			
			File.Delete(srcPath);
			File.Delete(libPath);
			
			return result;
        }
		#endregion
		
		#region Fields
		private string[] m_ignore = new string[]{
			"bitwise", "destructor", "destructors", "en-us", "enum", "finalizer", "Hashtable", 
			"hungarian", "metadata", "namespace", "Namespace", "namespaces", "non-null", "non-private", 
			"non-static", "num", "Refactor", "refactored", "runtime", "struct", "Structs", "structs", 
			"subclassed", "unboxed", "unboxing", "unmanaged"
		};
		#endregion
	}
#endif
}