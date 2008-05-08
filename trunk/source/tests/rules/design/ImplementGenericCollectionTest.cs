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
using System.Reflection;
using Smokey.Framework.Support;
using Smokey.Internal.Rules;

using Old = System.Collections;

namespace Smokey.Tests
{
	[TestFixture]
	public class ImplementGenericCollectionTest : TypeTest
	{	
		// test classes
		public class Good1 : Old.IEnumerable, IEnumerable<int>
		{
			Old.IEnumerator Old.IEnumerable.GetEnumerator()
			{
				return m_ints.GetEnumerator();
			}
			
			public IEnumerator<int> GetEnumerator()
			{
				foreach (int n in m_ints)
					yield return n;
			}
			
			private List<int> m_ints = new List<int>();
		}			
		
		public class Good2 : Old.SortedList
		{
		}			
		
		public class Good3 : List<int>
		{
		}			
		
		public class Bad1 : Old.IEnumerable
		{
			public Old.IEnumerator GetEnumerator()
			{
				return m_ints.GetEnumerator();
			}
						
			private List<int> m_ints = new List<int>();
		}			
		
		// test code
		public ImplementGenericCollectionTest() : base(
			new string[]{"Good1", "Good2", "Good3"},
			new string[]{"Bad1"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new ImplementGenericCollectionRule(cache, reporter);
		}
	} 
}