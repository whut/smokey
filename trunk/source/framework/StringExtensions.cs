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
