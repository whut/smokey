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

using Mono.Cecil;
using NUnit.Framework;
using System;
using System.Reflection;
using System.Text;
using Smokey.Framework.Support;
using Smokey.Internal.Rules;

namespace Smokey.Tests
{
	[TestFixture]
	public class StringCompareTest : MethodTest
	{	
		#region Test cases
		private class Cases
		{
			public bool Good1(string s)
			{
				return s.Length == 0;
			}

			public bool Good2(string s)
			{
				return string.IsNullOrEmpty(s);
			}

			public bool Bad1(string s)
			{
				return s.Equals("");
			}

			public bool Bad2(string s)
			{
				return !s.Equals("");
			}

			public bool Bad3(string s)
			{
				return s.Equals(string.Empty);
			}

			public bool Bad4(string s)
			{
				return "".Equals(s);
			}

			public bool Bad5(string s)
			{
				return string.Empty.Equals(s);
			}

			public bool Bad6(string s)
			{
				return s == "";
			}

			public bool Bad7(string s)
			{
				return "" == s;
			}

			public bool Bad8(string s)
			{
				return s != "";
			}
		}		
		#endregion
		
		// test code
		public StringCompareTest() : base(
			new string[]{"Cases.Good1", "Cases.Good2"},
			new string[]{"Cases.Bad1", "Cases.Bad2", "Cases.Bad3", "Cases.Bad4", "Cases.Bad5", "Cases.Bad6", "Cases.Bad7", "Cases.Bad8"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new StringCompareRule(cache, reporter);
		}
	} 
}

