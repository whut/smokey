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

#if OLD
namespace Smokey.Tests
{
	[TestFixture]
	public class ToStringOnStringTest : MethodTest
	{	
		#region Test cases
		private class Cases
		{
			public string Good1(int x)
			{
				return x.ToString();
			}

			public string Good2(int x)
			{
				return x.ToString("X2");
			}

			public string Bad1(string x)
			{
				return x.ToString();
			}

			public string Bad2(int x)
			{
				string xx = x.ToString();
				return xx.ToString();
			}
			
			public string Bad3()
			{
				return m_foo.ToString();
			}
			
			private string m_foo = "foo";
		}
		#endregion
		
		// test code
		public ToStringOnStringTest() : base(
			new string[]{"Cases.Good1", "Cases.Good2"},
			new string[]{"Cases.Bad1", "Cases.Bad2", "Cases.Bad3"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new ToStringOnStringRule(cache, reporter);
		}
	} 
}
#endif
