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
	public class SuffixNameTest : TypeTest
	{	
		// test classes		
		public class Good1Attribute : Attribute
		{
		}			
		
		public class Good2Exception : ArgumentException
		{
		}			
		
		public class Bad1 : Attribute
		{
		}			
		
		public class Bad2 : EventArgs
		{
		}			
		
		public class Bad3 : Exception
		{
		}			
		
		public class Bad4 : ArgumentException
		{
		}			
		
//		public class Bad5 : System.Collections.ICollection
//		{
//		}			
		
//		public class Bad6 : System.Collections.IDictionary
//		{
//		}			
		
		public class Bad7 : System.Collections.IEnumerable
		{
			public System.Collections.IEnumerator GetEnumerator()
			{
				return m_ints.GetEnumerator();
			}
			
			private System.Collections.Generic.List<int> m_ints = new System.Collections.Generic.List<int>();
		}			
		
		public class Bad8 : System.Collections.Queue
		{
		}			
		
		public class Bad9 : System.Collections.Stack
		{
		}			
		
		public class Bad10 : System.Collections.Generic.ICollection<int>
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
		
//		public class Bad11 : System.Collections.Generic.IDictionary<int, int>
//		{
//		}			
		
		public class Bad12 : System.Data.DataSet
		{
		}			
		
		public class Bad13 : System.Data.DataTable
		{
		}			
		
//		public class Bad14 : System.IO.Stream
//		{
//		}			
		
//		public class Bad15 : System.Security.IPermission
//		{
//		}			
		
//		public class Bad16 : System.Security.Policy.IMembershipCondition
//		{
//		}			
				
		// test code
		public SuffixNameTest() : base(
			new string[]{"Good1Attribute", "Good2Exception"},
		
			new string[]{"Bad1", "Bad2", "Bad3", "Bad4", "Bad7", "Bad8", "Bad9", 
			"Bad10", "Bad12", "Bad13"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new SuffixNameRule(cache, reporter);
		}
	} 
}
#endif
