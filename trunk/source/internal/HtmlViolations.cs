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

namespace Smokey.Internal
{
#if DEBUG
	internal class HtmlViolations
	{			
		public void Write(string path)
		{
			DoLoadViolations();
			DoWriteHtml(path);
		}
		
		#region Private Methods -----------------------------------------------
		private void DoWriteHtml(string root)
		{
			if (!Directory.Exists(root))
				Ignore.Value = Directory.CreateDirectory(root);
				
			string path = Path.Combine(root, "Violations.css");
			DoWriteCss(path);
			
			foreach (KeyValuePair<string, List<Violation>> entry in m_entries)
			{
				path = Path.Combine(root, string.Format("{0}.html", entry.Key));
				path = path.Replace(" ", "_");
				using (TextWriter stream = new StreamWriter(path))
				{
					DoWriteHeader(stream, entry.Key);
					DoWriteBody(stream, entry.Key, entry.Value);
					DoWriteTrailer(stream);
				}
			}
			
			DoWriteFrames(root);
			DoWriteDefaultFrame(root);
			DoWriteNavBar(root);
		}
		
		private static void DoWriteFrames(string root)
		{
			string path = Path.Combine(root, "frames.html");
			using (TextWriter stream = new StreamWriter(path))
			{
				stream.WriteLine("<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.01 Frameset//EN\">");
				stream.WriteLine("<html lang=\"EN\">");
				stream.WriteLine("<!-- machine generated on {0} -->", DateTime.Now);
				stream.WriteLine();
	
				stream.WriteLine("<head>");
				stream.WriteLine("    <title>Violations</title>");
				stream.WriteLine("</head>");
				stream.WriteLine();

				stream.WriteLine("<frameset cols = \"20%,80%\">");
				stream.WriteLine("	<frame src = \"navbar.html\">");
				stream.WriteLine("	<frame src = \"default.html\" name = \"main\">");
				stream.WriteLine("</frameset>");
				stream.WriteLine();

				stream.WriteLine("</html>");
			}
		}
		
		private static void DoWriteDefaultFrame(string root)
		{
			string path = Path.Combine(root, "default.html");
			using (TextWriter stream = new StreamWriter(path))
			{
				stream.WriteLine("<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.01//EN\"");
				stream.WriteLine("   \"http://www.w3.org/TR/1999/REC-html401-19991224/strict.dtd\">");
				stream.WriteLine("<html lang=\"EN\">");
				stream.WriteLine("<!-- machine generated on {0} -->", DateTime.Now);
				stream.WriteLine();
	
				stream.WriteLine("<head>");
				stream.WriteLine("    <link rel=\"stylesheet\" type=\"text/css\" href=\"Violations.css\">");
				stream.WriteLine("    <title>Violations</title>");
				stream.WriteLine("</head>");

				stream.WriteLine("<body>");
				stream.WriteLine();
				stream.WriteLine("<p>These are all of the rules included with Smokey. However it's also possible to write custom rules.</p>");
				stream.WriteLine();
				stream.WriteLine("</body>");

				stream.WriteLine("</html>");
			}
		}
		
		private void DoWriteNavBar(string root)
		{
			string path = Path.Combine(root, "navbar.html");
			using (TextWriter stream = new StreamWriter(path))
			{
				stream.WriteLine("<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.01 Transitional//EN\"");
				stream.WriteLine("   \"http://www.w3.org/TR/1999/REC-html401-19991224/strict.dtd\">");
				stream.WriteLine("<html lang=\"EN\">");
				stream.WriteLine("<!-- machine generated on {0} -->", DateTime.Now);
	
				stream.WriteLine("<head>");
				stream.WriteLine("    <title>Violations</title>");
				stream.WriteLine("    <link rel=\"stylesheet\" type=\"text/css\" href=\"Violations.css\">");
				stream.WriteLine("</head>");

				stream.WriteLine("<body class = \"nav\">");
				stream.WriteLine("<ul class = \"nav\">");
				foreach (string category in m_entries.Keys)
				{
					string file = string.Format("{0}.html", category).Replace(" ", "_");
					stream.WriteLine("<li class = \"nav\"><a href=\"./{0}\" target = \"main\">{1}</a></li>", file, category);
				}
				stream.WriteLine("</ul>");
				stream.WriteLine("</body>");

				stream.WriteLine("</html>");
			}
		}
		
		private static void DoWriteBody(TextWriter stream, string category, List<Violation> violations)
		{
			stream.WriteLine("<body>");
			stream.WriteLine("<h1 class = \"heading\">{0} Rules</h1>", category);

			violations.Sort((lhs, rhs) => lhs.TypeName.CompareTo(rhs.TypeName));
			
			for (int i = 0; i < violations.Count; ++i)
			{
				Violation violation = violations[i];
			
				DoWriteViolation(stream, violation);
				if (i + 1 < violations.Count)
					stream.WriteLine("<hr class = \"separator\">");
					
				stream.WriteLine();
			}

			stream.WriteLine("</body>");
		}
		
		private static void DoWriteViolation(TextWriter stream, Violation violation)
		{
			stream.WriteLine("<a name = \"{0}\">", violation.TypeName);
				stream.WriteLine("<h2 class = \"category\">{0}</h2>", violation.TypeName);
			stream.WriteLine("</a>");
			HtmlHelpers.WriteText(stream, "CheckId", violation.CheckID);
			HtmlHelpers.WriteSeverity(stream, "Severity", violation.Severity.ToString());
			HtmlHelpers.WriteText(stream, "Breaking", violation.Breaking ? "Yes" : "No");
			HtmlHelpers.WriteText(stream, "Cause", violation.Cause);
			HtmlHelpers.WriteText(stream, "Description", violation.Description);
			HtmlHelpers.WriteText(stream, "Fix", violation.Fix);
		
			if (violation.Csharp.Length > 0)
			{
				string code = Reformat.Code(violation.Csharp);
				code = HtmlHelpers.Escape(code);
				code = HtmlHelpers.ProcessLinks(code);
				code = HtmlHelpers.ColorizeCode(code);
				stream.WriteLine("<pre class = \"code\">{0}</pre>", code);
			}
		}
		
		private static void DoWriteHeader(TextWriter stream, string category)
		{
			HtmlHelpers.WriteHeader(stream);
			
			stream.WriteLine("<head>");
			stream.WriteLine("    <link rel=\"stylesheet\" type=\"text/css\" href=\"Violations.css\">");
			stream.WriteLine("    <title>{0} Violations</title>", HtmlHelpers.Escape(category));
			stream.WriteLine("</head>");
		}
		
		private static void DoWriteTrailer(TextWriter stream)
		{
			stream.WriteLine("</html>");
		}
		
		private static void DoWriteCss(string path)	
		{
			using (TextWriter stream = new StreamWriter(path))
			{
				HtmlHelpers.WriteCss(stream);
			}
		}

		private void DoLoadViolations()
		{
			foreach (Violation violation in ViolationDatabase.Violations)
			{
				List<Violation> violations;
				if (!m_entries.TryGetValue(violation.Category, out violations))
				{
					violations = new List<Violation>();
					m_entries.Add(violation.Category, violations);
				}
				
				violations.Add(violation);
			}
		}
		#endregion
				
		#region Fields --------------------------------------------------------
		private SortedList<string, List<Violation>> m_entries = new SortedList<string, List<Violation>>();
		#endregion
	}
#endif
}