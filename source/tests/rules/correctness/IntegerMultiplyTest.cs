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

#if OLD
namespace Smokey.Tests
{
	[TestFixture]
	public class IntegerMultiplyTest : MethodTest
	{	
		#region Test classes
		public class Cases
		{
			public static long Good1(int n, int m)
			{
				return (long) n * m;
			}

			public static long Good2(int n, int m)
			{
				return n * (long) m;
			}

			public static long Good3(long n, long m)
			{
				return n * m;
			}

			public static long Good4(long n, int m)
			{
				return n * m;
			}

			public static long Good5(int n, long m)
			{
				return n * m;
			}

			public long Good6(long n, int m)
			{
				return n * m;
			}

			public long Good7(int n, long m)
			{
				return n * m;
			}

			public long Good8(int n, long m)
			{
				int x = 2 * n;
				return x * m;
			}

			public long Good9(int n, long m)
			{
				int x = 2 * n;
				return m * x;
			}

			public static long Good10(long n, long m)
			{
				long x = 2 * n;
				long y = 5 * n;
				return y * x;
			}

			public static long Good11(long n)
			{
				return 10L * n;
			}

			public static long Bad1(int n, int m)
			{
				return n * m;
			}

			public static long Bad2(int n, int m)
			{
				int x = 2 * n;
				return x * m;
			}

			public static long Bad3(int n, int m)
			{
				int x = 2 * n;
				return n * x;
			}

			public static long Bad4(int n, int m)
			{
				int x = 2 * n;
				int y = 5 * n;
				return y * x;
			}

			public static long Bad5(int n, int m)
			{
				return 10 * (n + m);
			}

			public static long Bad6(int n, int m)
			{
				return (n + m) * 33;
			}
			
			public long Bad7(int n, int m)
			{
				return (n + m) * m_value;
			}
			
			public long Bad8(int n, int m)
			{
				return (n + m) * ms_value;
			}
			
			private int m_value = 100;
			private static int ms_value = 10;
		}
		#endregion
		
		// test code
		public IntegerMultiplyTest() : base(
			new string[]{"Cases.Good1", "Cases.Good2", "Cases.Good3"
						, "Cases.Good4", "Cases.Good5", "Cases.Good6", "Cases.Good7"
						, "Cases.Good8", "Cases.Good9", "Cases.Good10", 
						"Cases.Good11"},
						
			new string[]{"Cases.Bad1", "Cases.Bad2", "Cases.Bad3", "Cases.Bad4",
						"Cases.Bad5", "Cases.Bad6", "Cases.Bad7", "Cases.Bad8"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new IntegerMultiplyRule(cache, reporter);
		}
	} 
}
#endif
