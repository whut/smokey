// Copyright (C) 2008 Jesse Jones
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

namespace Smokey.Framework.Support
{
	/// <summary>Some helpful string related methods.</summary>
	public static class StringExtensions 
	{ 
		/// <summary>Split the string based on capitalization changes.</summary>
		/// <remarks>So, "inName" becomes ["in", "Name"].</remarks>
		public static string[] CapsSplit(this string str)
		{
			List<string> parts = new List<string>();
			
			int index = 0;
			while (index < str.Length)
			{
				int count = FindCapsRun(str, index);
				DBC.Assert(count > 0, "count is {0}", count);
				
				parts.Add(str.Substring(index, count));
				index += count;
			}
			
			return parts.ToArray();
		}
		
		/// <summary>Pattern may have a '*' at the start, at the end, at both the
		/// start and the end, or in the middle.</summary>
		public static bool Match(this string text, string pattern)
		{
			DBC.Pre(text != null, "text is null");
			DBC.Pre(pattern != null, "pattern is null");
			DBC.Pre(pattern.Length > 0, "pattern is empty");
			
			string p;
			if (pattern[0] == '*')
			{
				if (pattern[pattern.Length - 1] == '*')
				{
					p = pattern.Substring(1, pattern.Length - 2);
					DBC.Pre(p.IndexOf('*') < 0, "pattern has extra *'s: {0}", pattern);
					return text.Contains(p);
				}
				else
				{
					p = pattern.Substring(1);
					DBC.Pre(p.IndexOf('*') < 0, "pattern has extra *'s: {0}", pattern);
					return text.EndsWith(p);
				}
			}
			else if (pattern[pattern.Length - 1] == '*')
			{
				p = pattern.Substring(0, pattern.Length - 1);
				DBC.Pre(p.IndexOf('*') < 0, "pattern has extra *'s: {0}", pattern);
				return text.StartsWith(p);
			}
			else if (pattern.IndexOf('*') >= 0)
			{
				string[] patterns = pattern.Split('*');
				return text.StartsWith(patterns[0]) && text.EndsWith(patterns[1]);
			}
			else
			{
				return text == pattern;
			}
		}
		
		#region Private Methods -----------------------------------------------
		private static int FindCapsRun(string str, int index)
		{
			int count = 0;
			bool isLower = char.IsLower(str[index]);
			
			if (isLower)
			{
				// name
				while (index + count < str.Length && char.IsLower(str[index + count]))
					++count;
			}
			else if (index + 1 < str.Length && !char.IsLower(str[index + 1]))
			{
				// XML
				while (index + count < str.Length && !char.IsLower(str[index + count]))
					++count;
			}
			else
			{
				// Name
				++count;
				while (index + count < str.Length && char.IsLower(str[index + count]))
					++count;
			}
				
			return count;
		}
		#endregion
	}
} 
