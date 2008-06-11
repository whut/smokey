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
	public class ArgumentException2Test : MethodTest
	{	
		#region Test classes
		public class Cases
		{
			public static void Good1(int s)
			{
				throw new ArgumentNullException("s");
			}
			
			public static void Good2(int t)
			{
				throw new ArgumentOutOfRangeException("t");
			}

			public static void Good3(int A_1)
			{
				throw new ArgumentOutOfRangeException("t");
			}

			public static void Bad1(int s)
			{
				throw new ArgumentOutOfRangeException("s is negative");
			}

			public static void Bad2(int s, int t)
			{
				throw new ArgumentNullException("x");
			}
		}
		#endregion
		
		// test code
		public ArgumentException2Test() : base(
			new string[]{"Cases.Good1", "Cases.Good2", "Cases.Good3"},
			new string[]{"Cases.Bad1", "Cases.Bad2"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new ArgumentException2Rule(cache, reporter);
		}
	} 
}
