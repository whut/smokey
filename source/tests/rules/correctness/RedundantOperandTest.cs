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
	public class RedundantOperandTest : MethodTest
	{	
		#region Test classes
		public class Cases
		{
			public static int Good1(int a, int b)
			{
				return a/b;
			}

			public static int Good2(int a, int b)
			{
				return (a + 1)/(a - 1);
			}

			public static int Good3(int a, int b, int c, int d)
			{
				return (a + Math.Min(c, d))/(b + Math.Min(c, d));
			}

			public static int Bad1(int a, int b)
			{
				return a/a;
			}

			public static int Bad2(int a, int b)
			{
				return (a + 1)/(a + 1);
			}

			public static int Bad3(int a, int b, int c, int d)
			{
				return (a + Math.Min(c, d))/(a + Math.Min(c, d));
			}

			public static bool Bad4(int a, int b)
			{
				return (-a) < (-a);
			}

			public static int Bad5(int a, int b)
			{
				return Math.Min(a - b, a - b);
			}

			public static bool Bad6(int a, int b)
			{
				return Equals(a - b, a - b);
			}

			public static bool Bad7(string a, string b)
			{
				return ReferenceEquals(a + b, a + b);
			}
		}
		
		public struct TablePosition 
		{	
			public int Column 
			{
				get {return column;}
				set {column = value;}
			}
	
			public int Row 
			{
				get {return row;}
				set {row = value;}
			}
		
			public bool Compare(TablePosition p1, TablePosition p2)
			{
				return !(p1.column == p2.column && p1.row == p2.row);
			}

			private int column, row;
		}
		#endregion
		
		// test code
		public RedundantOperandTest() : base(
			new string[]{"Cases.Good1", "Cases.Good2", "Cases.Good3", "TablePosition.Compare"},
			new string[]{"Cases.Bad1", "Cases.Bad2", "Cases.Bad3", "Cases.Bad4", 
				"Cases.Bad5", "Cases.Bad6", "Cases.Bad7"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new RedundantOperandRule(cache, reporter);
		}
	} 
}
