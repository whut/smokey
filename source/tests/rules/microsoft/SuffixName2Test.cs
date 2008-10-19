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
using Smokey.Framework.Support;
using Smokey.Internal.Rules;

#if OLD
namespace Smokey.Tests
{
	[TestFixture]
	public class SuffixName2Test : TypeTest
	{	
		// test classes		
		public class Good1Attribute : Attribute
		{
		}			
		
		public class Good2Collection : System.Collections.Queue
		{
		}			
		
		public class Good3Collection : System.Collections.Generic.ICollection<int>
		{
			System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
			{
				return m_ints.GetEnumerator();
			}
			
			public System.Collections.Generic.IEnumerator<int> GetEnumerator()
			{
				return null;
			}
			
			public int Count 
			{
				get {return m_ints.Count;}
			}
			
			public bool IsReadOnly
			{
				get {return false;}
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
			
			public void CopyTo(int[] x, int i)
			{
			}
			
			public bool Remove(int n)
			{
				return false;
			}
			
			private System.Collections.Generic.List<int> m_ints = new System.Collections.Generic.List<int>();
		}	
		
		public delegate void Good4EventHandler(object o, EventArgs e);
								
		public class Bad1Attribute 
		{
		}			
		
		public class Bad2Collection : Bad1Attribute
		{
		}			
		
		public class Bad3Dictionary
		{
		}			
		
		public class Bad4EventArgs
		{
		}			
						
		public class Bad5EventHandler
		{
		}			
						
		public class Bad6Exception : Bad5EventHandler
		{
		}			
						
		public class Bad7Permission
		{
		}			
						
		public class Bad8Queue
		{
		}			
						
		public class Bad9Stack
		{
		}			
						
		public class Bad10Stream
		{
		}			
						
		// test code
		public SuffixName2Test() : base(
			new string[]{"Good1Attribute", "Good2Collection", "Good3Collection",
			"Good4EventHandler"},
		
			new string[]{"Bad1Attribute", "Bad2Collection", "Bad3Dictionary", "Bad4EventArgs", 
			"Bad5EventHandler", "Bad6Exception", 
			"Bad7Permission", "Bad8Queue", "Bad9Stack", "Bad10Stream"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new SuffixName2Rule(cache, reporter);
		}
	} 
}
#endif
