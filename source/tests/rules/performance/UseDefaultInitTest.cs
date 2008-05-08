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

namespace Smokey.Tests
{
	[TestFixture]
	public class UseDefaultInitTest : MethodTest
	{	
		#region Test cases
		private struct Struct
		{
			public Struct(int b)
			{
				blah = b;
			}
			
			public int blah;
		}
		
		private class GoodCase1
		{
			public GoodCase1(string s)
			{
				m_string = s;
				m_int = 1;
				Console.WriteLine("m_string = {0}, m_int = {1}", m_string, m_int);
			}

			private string m_string;
			private int m_int;
		}

		private class GoodCase2
		{
			public GoodCase2(string s)
			{
				m_string = s;
				Console.WriteLine("m_string = {0}, m_int = {1}", m_string, m_int);
			}

			private string m_string;
			private int m_int;
		}

		private class GoodCase3
		{
			public GoodCase3(int n)
			{
				m_int = n;
				m_struct = new Struct(n);
				Console.WriteLine("m_string = {0}, m_int = {1}", m_string, m_int + m_struct.blah);
			}

			private string m_string;
			private int m_int;
			private Struct m_struct;
		}

		private struct GoodCase4
		{
			public GoodCase4(int n)
			{
				m_a = n;
				m_b = 0;
				Console.WriteLine("sum = {0}", m_a + m_b);
			}

			private int m_a;
			private int m_b;
		}

		private class BadCase1
		{
			public BadCase1(string s)
			{
				m_string = s;
				m_int = 0;
				Console.WriteLine("m_string = {0}, m_int = {1}", m_string, m_int);
			}

			private string m_string;
			private int m_int;
		}

		private class BadCase2
		{
			public BadCase2(int n)
			{
				m_string = null;
				m_int = n;
				Console.WriteLine("m_string = {0}, m_int = {1}", m_string, m_int);
			}

			private string m_string;
			private int m_int;
		}

		private class BadCase3
		{
			public BadCase3(int n)
			{
				m_int = n;
				m_struct = new Struct();
				Console.WriteLine("m_string = {0}, m_int = {1}", m_string, m_int + m_struct.blah);
			}

			private string m_string;
			private int m_int;
			private Struct m_struct;
		}

		private class BadCase4
		{
			public BadCase4(string s)
			{
				m_string = s;
				m_float = 0.0f;
				Console.WriteLine("m_string = {0}, m_float = {1}", m_string, m_float);
			}

			private string m_string;
			private float m_float;
		}
		#endregion
		
		// test code
		public UseDefaultInitTest() : base(
			new string[]{"GoodCase1.GoodCase1", "GoodCase2.GoodCase2", "GoodCase3.GoodCase3", "GoodCase4.GoodCase4"},
			new string[]{"BadCase1.BadCase1", "BadCase2.BadCase2", "BadCase3.BadCase3", "BadCase4.BadCase4"},
			new string[]{"Struct.Struct"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new UseDefaultInitRule(cache, reporter);
		}
	} 
}
