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
using System.Text;
using Smokey.Framework;

namespace Smokey.Internal
{
	// Text in the xml is formatted for the xml so we need to reformat it
	// before displaying it to users.
	internal static class Reformat
	{		
		// For Text we need to remove whitespace from the start of lines
		// and remove new lines at the end of non-blank lines. 
		public static string Text(string text)
		{
			text = text.Replace(NewLine.Windows, NewLine.Unix);	// need to do this so split doesnt freak out
			string[] lines = text.Split(NewLine.Mac[0], NewLine.Unix[0]);
			
			StringBuilder result = new StringBuilder(text.Length);
			for (int i = 0; i < lines.Length; ++i)
			{
				string line = lines[i].Trim();
				
				if (line.Length > 0)
				{
					if (line.StartsWith("*"))			// bulleted list
						result.Append(Environment.NewLine);
					else if (line.StartsWith("||"))		// table
						result.Append(Environment.NewLine);
					result.Append(line);
					if (i + 1 < lines.Length && lines[i + 1].Length > 0)
						result.Append(' ');
				}
				else
				{
					result.Append(Environment.NewLine);
					result.Append(Environment.NewLine);
				}
			}

			return result.ToString();
		}
		
		// Replace tabs with four spaces. Remove whitespace from the first
		// line and the same amount of whitespace from the remaining lines.
		public static string Code(string text)
		{
			text = text.Replace("\t", "    ");
			text = text.Replace(NewLine.Windows, NewLine.Unix);	// need to do this so split doesnt freak out

			string[] lines = text.Split(NewLine.Mac[0], NewLine.Unix[0]);
			
			StringBuilder result = new StringBuilder(text.Length);
			if (lines.Length > 0)
			{				
				int count = 0;
				string line = lines[0];
				while (count < line.Length && char.IsWhiteSpace(line[count]))
					++count;
				string padding = line.Substring(0, count);
					
				for (int i = 0; i < lines.Length; ++i)
				{
					if (lines[i].StartsWith((padding)))
						line = lines[i].Substring(count);
					else
						line = lines[i];
						
					if (line.Length == 0)
						result.Append(' ');	// without this blank lines dont show up when nant evil is used...
					
					result.Append(line);	
					result.Append(Environment.NewLine);
				}
			}
			
			return result.ToString();
		}
	}
}