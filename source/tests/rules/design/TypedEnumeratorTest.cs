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
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Smokey.Framework.Support;
using Smokey.Internal.Rules;

namespace Smokey.Tests
{
	[TestFixture]
	public class TypedEnumeratorTest : TypeTest
	{	
		public class Good1 : IEnumerator<int>
		{	
			public Good1(int first, int last)
			{				
				m_current = first;
				m_first = first;
				m_last = last;
			}
			
			public void Dispose()
			{
			}
			
			public int Current
			{
				get {return m_current;}
			}
			
			object IEnumerator.Current
			{
				get {return m_current;}
			}
			
			public bool MoveNext()
			{
				++m_current;
				return m_current <= m_last;
			}
			
			public void Reset()
			{
				m_current = m_first;
			}
			
			private int m_current;
			private int m_first;
			private int m_last;
		}			

		public class Good2 : IEnumerator
		{	
			public Good2(int first, int last)
			{				
				m_current = first;
				m_first = first;
				m_last = last;
			}
			
			public void Dispose()
			{
			}
			
			public int Current
			{
				get {return m_current;}
			}
			
			object IEnumerator.Current
			{
				get {return m_current;}
			}
			
			public bool MoveNext()
			{
				++m_current;
				return m_current <= m_last;
			}
			
			public void Reset()
			{
				m_current = m_first;
			}
			
			private int m_current;
			private int m_first;
			private int m_last;
		}			

		public class Bad1 : IEnumerator
		{	
			public Bad1(int first, int last)
			{				
				m_current = first;
				m_first = first;
				m_last = last;
			}
						
			public object Current
			{
				get {return m_current;}
			}
			
			public bool MoveNext()
			{
				++m_current;
				return m_current <= m_last;
			}
			
			public void Reset()
			{
				m_current = m_first;
			}
			
			private int m_current;
			private int m_first;
			private int m_last;
		}			

		// test code
		public TypedEnumeratorTest() : base(
			new string[]{"Good1", "Good2"},
			new string[]{"Bad1"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new TypedEnumeratorRule(cache, reporter);
		}
	} 
}