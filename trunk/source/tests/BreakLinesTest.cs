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
	public class BreakLinesTest : BaseTest
	{		
		// Short lines aren't broken.
		[Test]
		public void ShortLines()
		{
			string text     = "Four score and\nseven years\rago our\r\nfathers";
			string expected = "Four score and *seven years *ago our *fathers";
			string actual   = Break.Lines(text, 80);

			Assert.AreEqual(expected.Replace("*", Environment.NewLine), actual);
		}
		
		// Long lines are broken.
		[Test]
		public void LongLines()
		{
			string text     = "Four score and seven years ago our fathers";
			string expected = "Four score *and seven *years ago *our fathers";
			string actual   = Break.Lines(text, 12);

			Assert.AreEqual(expected.Replace("*", Environment.NewLine), actual);
		}
		
		// Very long words are not broken.
		[Test]
		public void VeryLongWord()
		{
			string text     = "Four score AndSevenYearsAgoOur fathers";
			string expected = "Four score *AndSevenYearsAgoOur *fathers";
			string actual   = Break.Lines(text, 14);

			Assert.AreEqual(expected.Replace("*", Environment.NewLine), actual);
		}
		
		// Broken lines don't start with break characters.
		[Test]
		public void LineStarts()
		{
			string text     = "Four score    and seven     - years ago our fathers";
			string expected = "Four score    *and seven     - *years ago *our fathers";
			string actual   = Break.Lines(text, 12);

			Assert.AreEqual(expected.Replace("*", Environment.NewLine), actual);
		}
		
		// Breaks happen at spaces and dashes.
		[Test]
		public void BreakOnSpacesAndDashes()
		{
			string text     = "Four score, and seven-years";
			string expected = "Four score, *and seven- *years";
			string actual   = Break.Lines(text, 12);

			Assert.AreEqual(expected.Replace("*", Environment.NewLine), actual);
		}
	} 
}
