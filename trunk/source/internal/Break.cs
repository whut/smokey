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
using System.Globalization;
using System.Text;
using Smokey.Framework;

namespace Smokey.Internal
{
	// More or less Unicode aware line breaking code.
	internal static class Break
	{		
		// Break lines so that they have maxWidth characters or less per line.
		// Lines are broken on spaces and dashes. See
		// <http://unicode.org/reports/tr14/tr14-20.html> and <http://unicode.org/reports/tr14/>
		// for more info than you'll want to know for breaking Unicode lines correctly.
		public static string Lines(string text, int maxWidth)
		{
			DBC.Pre(maxWidth > 0, "maxWidth should be positive");
			
			StringBuilder result = new StringBuilder(text.Length + text.Length/maxWidth);
			
			int width = 0, index = 0;
			while (index < text.Length)
			{
				bool addBreak = false;
				char ch = text[index];
								
				// If the next char is a non-breaking character then we need 
				// to figure out how large the word is and insert a line break
				// if it's to large.
				if (!DoIsBreakableChar(ch))	
				{
					int wordWidth = DoGetWordWidth(text, index);
					if (width + wordWidth <= maxWidth)	// word fits
					{
						result.Append(text, index, wordWidth);
						index += wordWidth;
						width += wordWidth;
					}
					else if (width == 0)						// word too big to break cleanly, so we'll just add it
					{
						result.Append(text, index, wordWidth);
						index += wordWidth;
						addBreak = true;
					}
					else										// word is to big, so we'll break the line
					{
						addBreak = true;
					}
				}
				
				// If it's a new line then break the line. 
				else if (index + 1 < text.Length && ch == '\r' && text[index + 1] == '\n')
				{
					index += 2;
					addBreak = true;
				}
				else if (ch == '\n' || ch == '\r')
				{
					index += 1;
					addBreak = true;
				}

				// If it's breakable text just add the char.
				else
				{
					result.Append(ch);
					index += 1;
					width += 1;
					addBreak = width == maxWidth;
				}
				
				if (addBreak && index < text.Length)
				{
					while (index < text.Length && DoIsBreakableChar(text[index]))
					{
						result.Append(text[index]);
						++index;
					}

					if (index > 0 && text[index - 1] != ' ')
					{
						result.Append(' ');			// without this blank lines dont show up when nant evil is used...
					}
					
					result.Append(Environment.NewLine);
					width = 0;
				}
			}
			
			return result.ToString();
		}
		
		#region Private methods
		private static bool DoIsBreakableChar(char ch)
		{
			bool breakable = false;

			UnicodeCategory category = CharUnicodeInfo.GetUnicodeCategory(ch);			
			switch (category)
			{
				case UnicodeCategory.Control:
				case UnicodeCategory.LineSeparator:
					breakable = true;
					break;
					
				case UnicodeCategory.SpaceSeparator:
					breakable = ch != '\x00A0' &&		// non-breaking space
								ch != '\x202F';			// narrow no-break space
					break;
					
				case UnicodeCategory.DashPunctuation:
					breakable = ch != '\x2011';			// non-breaking hypen
					break;
			}
			
			return breakable;
		}
		
		private static int DoGetWordWidth(string text, int start)
		{
			int width = 0;
			
			while (start + width < text.Length && !DoIsBreakableChar(text[start + width]))
			{
				++width;
			}
			
			return width;
		}	
		#endregion
	}
}