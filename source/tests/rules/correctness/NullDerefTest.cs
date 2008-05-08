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

namespace Smokey.Tests
{
	[TestFixture]
	public class NullDerefTest : MethodTest
	{	
		#region Test classes
		private class GoodCases
		{
			public List<int> CreateListAll(int c)
			{
				List<int> result = null;         
				
				if (c == 0)
					result = new List<int>();
				else 
					result = new List<int>(c);
			
				result.Add(1);
				
				return result;
			}

			public List<int> CreateListIf(int c)
			{
				List<int> result = null;         
				
				if (c == 0)
					result = new List<int>();
				else if (c > 0)
					result = new List<int>(c);
			
				if (result != null)	
					result.Add(1);
				
				return result;
			}

			public List<int> CreateListIf2(int c)
			{
				List<int> result = null;         
				
				if (c == 0)
					result = new List<int>();
				else if (c > 0)
					result = new List<int>(c);
			
				if (null != result)	
					result.Add(1);
				
				return result;
			}

			public List<int> CreateListThrow(int c)
			{
				List<int> result = null;         
				
				if (c == 0)
					result = new List<int>();
				else if (c > 0)
					result = new List<int>(c);
			
				if (result == null)	
					throw new ArgumentException("c can't be negative!");
				
				result.Add(1);
				
				return result;
			}

			public List<int> CreateListThrow2(int c)
			{
				List<int> result = null;         
				
				if (c == 0)
					result = new List<int>();
				else if (c > 0)
					result = new List<int>(c);
			
				if (null == result)	
					throw new ArgumentException("c can't be negative!");
				
				result.Add(1);
				
				return result;
			}

			public void Add()
			{
				List<int> result = null;         
			
				try
				{	
					int c = int.Parse("100");
					result = new List<int>(c);
						
					result.Add(1);
				}
				catch (ArgumentException)
				{
					if (result != null)	 
						result.Add(2);
				}
			}

			public bool Static(int c)
			{
				List<int> result = null;         
				
				if (c == 0)
					result = new List<int>();
				else if (c > 0)
					result = new List<int>(c);
			
				return List<int>.Equals(result, result);
			}
			
			private object DoGetExtension(int n)
			{
				return "hey";
			}
			
			public object ResolveFunction(int n)
			{
				object extension = null;
				
				if (n > 0)
					extension = DoGetExtension(n);
				
				if (extension == null) 
				{
					extension = DoGetExtension(n);
					if (extension == null)
						return null;
				}
				
				Console.WriteLine(extension.GetType());
				return extension;
			}
			
			private string DoPrint(string s, int n)
			{
				return s + n;
			}
			
			private int DoGetInt()
			{
				return 42 + m_head.GetHashCode();
			}

			public void GetURI(int n) 
			{
				string baseUri = null;
			
				if (n == 1)
					baseUri = "hmm";
		
				DoPrint(baseUri, DoGetInt());
			}

			public void OutParam(int n) 
			{				
				string name = null;
				
				if (!m_table.TryGetValue(n, out name))
				{
					name = "hi";
					m_table.Add(n, name);
				}
								
				Console.WriteLine(name.Length);
			}
			
			private class DictionaryNode
			{
				public DictionaryNode next;
				public object value;
				
				public bool HasNext {get {return next != null;}}
			}
			private DictionaryNode m_head;
			
			private DictionaryNode FindEntry(object key, out DictionaryNode entry)
			{
				entry = new DictionaryNode();
				return entry;
			}

			public void Remove (object key)
			{
				DictionaryNode prev;
				DictionaryNode entry = FindEntry (key, out prev);
				if (entry == null)
					return;
				if (prev == null)
					m_head = entry.next;
				else
					prev.next = entry.next;
				entry.value = null;
			}

			public int BytesToRead()
			{
				string value;
				if (!m_table.TryGetValue(5, out value))
					Console.WriteLine("oops");	

				return value.Length;;
			}

			private Dictionary<int, string> m_table;
		}
				
		private class BadCases
		{
			public void Add()
			{
				List<int> result = null; 
				
				string alpha = "alpha";
				string beta = "beta";
				string gamma = "gamma";
				Console.WriteLine(alpha + beta + gamma);
			
				try
				{	
					int c = int.Parse("100");
					result = new List<int>(c);
						
					result.Add(1);
				}
				catch (ArgumentException)
				{
					result.Add(2);
				}
			}

			private class DictionaryNode
			{
				public DictionaryNode next;
				public object value;
				
				public bool HasNext {get {return next != null;}}
			}

			public bool IsReadOnly(object x)
			{
				DictionaryNode attrib = (DictionaryNode) x;
				if (attrib != null)
					Console.WriteLine("blech");

				return attrib.HasNext;
			}

			public List<int> CreateList(int c)
			{
				List<int> result = null;         
				
				if (c == 0)
					result = new List<int>();
				else if (c > 0)
					Console.WriteLine("len = {0}", result.Count);
			
				result.Add(1);		
				
				return result;
			}
		}
		#endregion
		
		// test code
		public NullDerefTest() : base(
			new string[]{"GoodCases.CreateListAll", "GoodCases.CreateListIf", "GoodCases.CreateListIf2", "GoodCases.CreateListThrow", 
			"GoodCases.CreateListThrow2", "GoodCases.Add", "GoodCases.Static", "GoodCases.ResolveFunction", 
			"GoodCases.GetURI", "GoodCases.OutParam", "GoodCases.Remove", "GoodCases.BytesToRead"},
			
			new string[]{"BadCases.Add", "BadCases.CreateList", "BadCases.IsReadOnly"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new NullDerefRule(cache, reporter);
		}
	} 
}
