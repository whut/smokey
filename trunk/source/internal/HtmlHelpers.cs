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
//using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Smokey.Framework;

namespace Smokey.Internal
{
	internal static class HtmlHelpers
	{			
		public static void WriteHeader(TextWriter stream)
		{
			stream.WriteLine("<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.01//EN\"");
			stream.WriteLine("   \"http://www.w3.org/TR/1999/REC-html401-19991224/strict.dtd\">");
			stream.WriteLine("<html lang=\"EN\">");
			stream.WriteLine("<!-- machine generated on {0} -->", DateTime.Now);
		}
		
		public static void WriteCss(TextWriter stream)	
		{
			string text = @"
/* grouping */
body {background-color: #C1E4E9;}
body.nav {background-color: #9CB9E8;}
h1.heading {text-align: center;}
h2.category {}
table.location {margin-bottom: 1%;}
table.inline {margin-bottom: -5%;}
ul.nav {margin-left: -10%;}
li.nav {list-style-position: outside;}

/* text */
span.name {font-weight: bolder;}
span.Error {color: #DB2226;}
span.Warning {color: #9C7B4C;}
span.Nitpick {color: #47B346;}

/* dividers */
hr.separator {}

/* code */
pre.code {
	background-color: #E8CDB7;
	border: 2px outset; 
	padding-left: 3px;
	padding-right: 3px;
	padding-top: 3px;
	padding-bottom: 3px;
	margin-top: -5px;
}

span.keyword {color: #0000CC;}
span.comment {color: #800000;}
span.string {color: #008040;}
";
			
			stream.WriteLine(text);
		}
		
		public static void WriteSeverity(TextWriter stream, string name, string text)
		{
			stream.Write("<p>");
				stream.Write("<span class = \"name\">");
					stream.Write(name);
					stream.Write(": ");
				stream.Write("</span>");
				
				stream.Write("<span class = \"{0}\">", text);
					stream.Write(text);
				stream.Write("</span>");
			stream.WriteLine("</p>");
		}
		
		public static void WriteText(TextWriter stream, string name, string text)
		{
			stream.Write("<p>");
				stream.Write("<span class = \"name\">");
					stream.Write(name);
					stream.Write(": ");
				stream.Write("</span>");
				
				stream.Write(DoMungeText(text));
			stream.WriteLine("</p>");
		}
		
		public static string ColorizeCode(string code)
		{						
			string result = ms_codeRE.Replace(code, match =>
			{
				if (match.Groups[1].Length > 0)
					return "<span class = \"comment\">" + match.Groups[1].Value + "</span>";
				
				else if (match.Groups[2].Length > 0)
					return "<span class = \"string\">" + match.Groups[2].Value + "</span>";

				DBC.Assert(match.Groups[3].Length > 0, "expected a keyword match");
				if (Array.IndexOf(ms_keywords, match.Groups[3].Value) >= 0)
					return "<span class = \"keyword\">" + match.Groups[3].Value + "</span>";
				else
					return match.Groups[3].Value;
			});

			return result;
		}
		
		public static string ProcessLinks(string text)
		{
			string result = ms_urlRE.Replace(text, match =>
			{
				if (match.Groups[1].Length > 0)
					return "<a href = \"" + match.Groups[1].Value + "\">here</a>";
				
				else
					return match.Groups[0].Value;
			});

			return result;
		}
						
		public static string Escape(string text)
		{
			string result = text;
			
			result = result.Replace("&", "&amp;");
			result = result.Replace("<", "&lt;");
			result = result.Replace(">", "&gt;");
			result = result.Replace("\"", "&quot;");
			
			return result;
		}
		
		#region Private methods		
		private static string DoMungeText(string text)
		{
			string result;
			
			result = Reformat.Text(text);			// don't have to do this for html, but it makes the html source look better
			result = Escape(result);
			result = DoParagraph(result);
			result = DoBulletedList(result);
			result = DoTable(result);
			result = ProcessLinks(result);
			
			return result;
		}
		
		private static string DoParagraph(string text)
		{
			StringBuilder result = new StringBuilder(text.Length);

			text = text.Replace(NewLine.Windows, NewLine.Unix);	// need to do this so split doesnt freak out
			string[] lines = text.Split(NewLine.Mac[0], NewLine.Unix[0]);

			for (int i = 0; i < lines.Length; ++i)
			{
				string line = lines[i];
				
				if (line.Trim().Length == 0)
				{
					result.Append("<br>");
					result.Append("<br>");
				}
				else
				{
					result.Append(line);
				}
				result.Append(Environment.NewLine);
			}
			
			return result.ToString();
		}
						
		private static string DoBulletedList(string text)
		{
			StringBuilder result = new StringBuilder(text.Length);

			text = text.Replace(NewLine.Windows, NewLine.Unix);	// need to do this so split doesnt freak out
			string[] lines = text.Split(NewLine.Mac[0], NewLine.Unix[0]);

			bool inList = false;
			for (int i = 0; i < lines.Length; ++i)
			{
				string line = lines[i];
				
				if (line.StartsWith("* "))
				{
					if (!inList)
					{
						result.Append("</p><ul>");
						inList = true;
					}

					result.Append("<li>");
					line = line.Remove(0, 1);
					line = line.TrimStart();
					result.Append(line);
					result.Append("</li>");
				}
				else
				{
					if (inList)
					{
						result.Append("</ul><p>");
						inList = false;
					}
					result.Append(line);
				}	
				result.Append(Environment.NewLine);
			}

			if (inList)
				result.Append("</ul>");
			
			return result.ToString();
		}
						
		private static string DoTable(string text)
		{
			StringBuilder result = new StringBuilder(text.Length);

			text = text.Replace(NewLine.Windows, NewLine.Unix);	// need to do this so split doesnt freak out
			string[] lines = text.Split(NewLine.Mac[0], NewLine.Unix[0]);

			bool inTable = false;
			for (int i = 0; i < lines.Length; ++i)
			{
				string line = lines[i];
								
				if (line.StartsWith("||"))
				{
					if (!inTable)
					{
						result.Append("<br></p><table class = \"inline\">");
						result.Append(Environment.NewLine);
						inTable = true;
					}
					
					result.Append("<tr>");
					string[] cols = line.Split(new char[]{'|'}, StringSplitOptions.RemoveEmptyEntries);
					foreach (string col in cols)
					{
						result.Append("<td>");
						result.Append(col);
						result.Append("</td> ");
					}
					result.Append("</tr>");
				}
				else
				{
					if (inTable)
					{
						result.Append(Environment.NewLine);
						result.Append("</table><p>");
						inTable = false;
					}
					result.Append(line);
				}	
				result.Append(Environment.NewLine);
			}

			if (inTable)
				result.Append("</table>");
			
			return result.ToString();
		}
		#endregion
				
		#region Fields
		private static string[] ms_keywords = new string[]{"#define",  "#if",  "#elif",  "#else",  "#endif",  "#endregion",  "#error",  "#line",  "#pragma",  "#region",  
"#undef",  "#warning",  "abstract",  "as",  "base",  "bool",  "break",  "byte",  "case",  "catch",  "char",  "checked",  
"class",  "const",  "continue",  "decimal",  "default",  "delegate",  "do",  "double",  "else",  "enum",  "event",  
"explicit",  "extern",  "false",  "finally",  "fixed",  "float",  "for",  "foreach",  "get",  "goto",  "if",  "implicit",  
"in",  "int",  "interface",  "internal",  "is",  "lock",  "long",  "namespace",  "new",  "null",  "object",  "operator", 
"out",  "override",  "params",  "private",  "protected",  "public",  "readonly",  "ref",  "return",  "sbyte",  "sealed",  
"set",  "short",  "sizeof",  "stackalloc",  "static",  "string",  "struct",  "switch",  "this",  "throw",  "true",  "try",  
"typeof",  "uint",  "ulong",  "unchecked",  "unsafe",  "ushort",  "using",  "var",  "virtual", "void",  "volatile",  
"while",  "where",  "yield"};

		private static Regex ms_codeRE = new Regex(
			@"(//.+$) | (&quot;.*?&quot;) | (\w+)", 
			RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace);

		private static Regex ms_urlRE = new Regex(
			@"&lt; ((http | https | ftp | file) :// .+?) &gt;",
			RegexOptions.Singleline | RegexOptions.IgnorePatternWhitespace);
		#endregion
	}
}