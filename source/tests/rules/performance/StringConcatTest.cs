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
	public class StringConcatTest : MethodTest
	{	
		#region Test cases
		private class GoodCase
		{
			public void NoLoop(string x, string y)
			{
				string r;
				
				if (x.Contains("hello"))
					r = x + y;
				else
					r = x;
					
				Console.WriteLine(r);
			}

			public void StringBuilder(string x, string y, int count)
			{				
				StringBuilder builder = new StringBuilder();
				
				builder.Append(x);
				for (int i = 0; i < count; ++i)
					builder.Append(y);
					
				string r = builder.ToString();
				Console.WriteLine(r);
			}
		}

		private class BadCase
		{
			public void Loop(string x, string y, int count)
			{
				string r = x;
				
				for (int i = 0; i < count; ++i)
					r += y;
					
				Console.WriteLine(r);
			}
		}
		#endregion
		
		// test code
		public StringConcatTest() : base(
			new string[]{"GoodCase.NoLoop", "GoodCase.StringBuilder"},
			new string[]{"BadCase.Loop"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new StringConcatRule(cache, reporter);
		}
	} 
}
