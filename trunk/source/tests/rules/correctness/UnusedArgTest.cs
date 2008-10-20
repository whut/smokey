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
using Smokey.Internal;
using Smokey.Internal.Rules;

namespace Smokey.Tests
{
	[TestFixture]
	public class UnusedArgTest : MethodTest
	{	
		#region Test classes
		public class Cases
		{
			public static void Good1(int s)
			{
				Console.WriteLine(s);
			}
			
			public static int Good2(int s, int t)
			{
				return s + t;
			}

			public static void Good3(int s, int t)
			{
			}

			public static int Good4(int s, int t)
			{
				Unused.Value = t;
				
				return s + s;
			}

			public int Good5(int s, int t)
			{				
				return s + t;
			}

			public static void Bad1(int s)
			{
				Console.WriteLine(10);
			}
			
			public static int Bad2(int s, int t)
			{
				return s + s;
			}
			
			public int Bad3(int s, int t)
			{
				return s + s;
			}
		}
		#endregion
		
		// test code
		public UnusedArgTest() : base(
			new string[]{"Cases.Good1", "Cases.Good2", "Cases.Good3", "Cases.Good4", "Cases.Good5"},
			new string[]{"Cases.Bad1", "Cases.Bad2", "Cases.Bad3"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new UnusedArgRule(cache, reporter);
		}
	} 
}

