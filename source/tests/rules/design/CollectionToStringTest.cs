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
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Smokey.Framework.Support;
using Smokey.Internal.Rules;

#if OLD
namespace Smokey.Tests
{
	[TestFixture]
	public class CollectionToStringTest : MethodTest
	{	
		// test cases
		public class Cases
		{
			public void Good1(int[] a)
			{
				Console.WriteLine("{0}", a.Length);
			}
			
			public void Good2(List<int> a)
			{
				Console.WriteLine("{0}", a.Count);
			}
			
			public void Bad1(int[] a)
			{
				Console.WriteLine("{0}", a.ToString());
			}
			
			public void Bad2(int[] a)
			{
				Console.WriteLine("{0}", a);
			}
			
			public void Bad3(int[] a)
			{
				int[] b = new int[]{a[0]};
				Console.WriteLine("{0}", b);
			}
			
			public void Bad4(int[] a)
			{
				m_array = new int[]{a[0]};
				Console.WriteLine("{0}", m_array);
			}
			
			public void Bad5(char[] a)
			{
				Console.WriteLine("{0}", a);
			}
			
			public void Bad6(char[] a)
			{
				Console.WriteLine("{0}{1}", a, "one");
			}
			
			public void Bad7(List<int> a)
			{
				Console.WriteLine("{0}{1}", a, "one");
			}
			
			private int[] m_array;
		}
				
		// test code
		public CollectionToStringTest() : base(
			new string[]{"Cases.Good1", "Cases.Good2"},
			new string[]{"Cases.Bad1", "Cases.Bad2", "Cases.Bad3", "Cases.Bad4", 
				"Cases.Bad5", "Cases.Bad6", "Cases.Bad7"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new CollectionToStringRule(cache, reporter);
		}
	} 
}
#endif
