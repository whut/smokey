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
	public class TypedCollectionTest : TypeTest
	{	
		public class Good1 : ICollection<int>
		{	
			public int Count
			{
				get {return 0;}
			}
			
			public bool IsReadOnly
			{
				get {return true;}
			}
			
			public void Add(int n)
			{
			}
			
			public void Clear()
			{
			}
			
			public bool Contains(int n)
			{
				return false;
			}
			
			public void CopyTo(int[] d, int c)
			{
			}
			
			public bool Remove(int n)
			{
				return false;
			}
			
			public IEnumerator<int> GetEnumerator()
			{
				return null;
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return null;
			}
		}			

		public class Bad1 : ICollection
		{	
			public int Count
			{
				get {return 0;}
			}
		
			public bool IsSynchronized
			{
				get {return false;}
			}
			
			public object SyncRoot
			{
				get {return this;}
			}
			
			public void CopyTo(Array a, int c)
			{
			}
			
			public IEnumerator GetEnumerator()
			{
				return null;
			}
		}			

		// test code
		public TypedCollectionTest() : base(
			new string[]{"Good1"},
			new string[]{"Bad1"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new TypedCollectionRule(cache, reporter);
		}
	} 
}