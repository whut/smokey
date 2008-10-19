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
	public class ClassCanBeMadeStaticTest : TypeTest
	{	
		// test classes
		public static class Good
		{
			public static void Helper1(int x)
			{
				Console.WriteLine("x = {0}", x);
			}

			public static void Helper2(int x)
			{
				Console.WriteLine("x = {0}", x);
			}
		}			
		
		public class SemiGood
		{
			public static void Helper1(int x)
			{
				Console.WriteLine("x = {0}", x);
			}

			public static void Helper2(int x)
			{
				Console.WriteLine("x = {0}", x);
			}

			public void Helper3(int x)
			{
				Console.WriteLine("x = {0}", x + m_y);
			}
			
			public int m_y;
		}			
		
		public class GoodBase
		{
			public virtual void Helper1(int x)
			{
				Console.WriteLine("x = {0}", x);
			}

			public virtual void Helper2(int x)
			{
				Console.WriteLine("x = {0}", x);
			}
		}			
		
		public class GoodDerived : GoodBase
		{
			public static void Helper3(int x)
			{
				Console.WriteLine("x = {0}", x + x);
			}
		}			
		
		public class Bad1
		{
			public static void Helper1(int x)
			{
				Console.WriteLine("x = {0}", x);
			}

			public static void Helper2(int x)
			{
				Console.WriteLine("x = {0}", x);
			}
		}			
		
		public sealed class Bad2
		{
			private Bad2()
			{
			}
			
			public static void Helper1(int x)
			{
				Console.WriteLine("x = {0}", x);
			}

			public static void Helper2(int x)
			{
				Console.WriteLine("x = {0}", x);
			}
		}			
		
		// test code
		public ClassCanBeMadeStaticTest() : base(
			new string[]{"Good", "SemiGood", "GoodBase", "GoodDerived"},
			new string[]{"Bad1", "Bad2"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new ClassCanBeMadeStaticRule(cache, reporter);
		}
	} 
}
#endif
