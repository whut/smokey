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

using NUnit.Framework;
using System;
using System.Collections.Specialized;
using System.Configuration;
using Smokey.Framework;
using Smokey.Internal;

namespace Smokey.Tests
{
	[TestFixture]
	public class ReformatCodeTest : BaseTest
	{		 
		// Tabs are replaced by four spaces.
		[Test]
		public void ReplaceTabs()
		{
			string text     = "Four score and\tseven years\tago";
			string expected = "Four score and    seven years    ago\n";
			string actual   = Reformat.Code(text);

			Assert.AreEqual(expected, actual);
		}

		// Whitespace at the start of the first line is removed from each line.
		[Test]
		public void RemovePadding()
		{
			string text     = "  Four score and\n   seven years\n    ago";
			string expected = "Four score and\n seven years\n  ago\n";
			string actual   = Reformat.Code(text);

			Assert.AreEqual(expected, actual);
		}
		
		// Whitespace is only removed if it matches the whitespace at the 
		// start of the first line.
		[Test]
		public void DontRemovePadding()
		{
			string text     = "  Four score and\n   seven years\n ago";
			string expected = "Four score and\n seven years\n ago\n";
			string actual   = Reformat.Code(text);
			Assert.AreEqual(expected, actual);

			text     = "Four score and\n   seven years\n\tago our";
			expected = "Four score and\n   seven years\n    ago our\n";
			actual   = Reformat.Code(text);
			Assert.AreEqual(expected, actual);
		}

		// Blank lines are preserved.
		[Test]
		public void BlankLine()
		{
			string text     = "  Four score and\n   \nseven years\n\n    ago";
			string expected = "Four score and\n \nseven years\n \n  ago\n";
			string actual   = Reformat.Code(text);

			Assert.AreEqual(expected, actual);
		}
	} 
}
