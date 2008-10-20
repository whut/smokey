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
	public class InfiniteRecursionTest : MethodTest
	{	
		#region Test classes
		private class GoodCases
		{
			public int Compute(int x)
			{
				return x + 1;
			}

			public override int GetHashCode()
			{
				return base.GetHashCode();
			}
			
			public static int Fibonacci(int n)
			{
				if (n < 2)
					return n;
				
				return Fibonacci(n - 1) + Fibonacci(n - 2);
			}
			
			public void Complex(int x)
			{
				switch (x)
				{
					case 0:
						if (x > 0)
							Complex(x - 1);
						break;
						
					case 1:
						if (x < 0)
							Complex(x + 1);
						break;
				}
				
				if (x == 100)
					Complex(100);
			} 
		}

		private class BadCases
		{
			// direct recursion
			public virtual int DirectRecurse(int x)
			{
				return DirectRecurse(x);
			} 

			public override int GetHashCode()
			{
				return m_x ^ GetHashCode();
			}

			public int RecursiveProperty 
			{
				get {return RecursiveProperty;}
			}		
			
			public int m_x;
		}
		#endregion
		
		// test code
		public InfiniteRecursionTest() : base(
			new string[]{"GoodCases.Compute", "GoodCases.GetHashCode", "GoodCases.Fibonacci", "GoodCases.Complex"},
			new string[]{"BadCases.DirectRecurse", "BadCases.GetHashCode", "BadCases.get_RecursiveProperty"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new InfiniteRecursionRule(cache, reporter);
		}
	} 
}

