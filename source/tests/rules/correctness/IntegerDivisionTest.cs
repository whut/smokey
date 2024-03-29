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
	public class IntegerDivisionTest : MethodTest
	{	
		#region Test classes
		public class Cases
		{
			public static double Good1(int n, int m)
			{
				return (double) n / m;
			}

			public static double Good2(int n, int m)
			{
				return n / (double) m;
			}

			public static double Good3(double n, double m)
			{
				return n / m;
			}

			public static double Good4(int n, int m)
			{
				int x = n / m;
				return x;
			}

			public static float Good5(int n, double m)
			{
				float x = (float) (n / m);
				return x;
			}

			public static double Bad1(int n, int m)
			{
				return n / m;
			}

			public static float Bad2(int n, int m)
			{
				return n / m;
			}
		}
		#endregion
		
		// test code
		public IntegerDivisionTest() : base(
			new string[]{"Cases.Good1", "Cases.Good2", "Cases.Good3", "Cases.Good4", "Cases.Good5"},
			new string[]{"Cases.Bad1", "Cases.Bad2"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new IntegerDivisionRule(cache, reporter);
		}
	} 
}

