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
	public class ArrayIndexOfTest : MethodTest
	{	
		#region Test classes
		public class Cases
		{
			public static void Good1(int[] s)
			{
				int i = Array.IndexOf(s, 3);
				if (i >= 0)
					Console.WriteLine("yes");
				else
					Console.WriteLine("no");
			}
			
			public static void Good2(int[] s)
			{
				int i = Array.IndexOf(s, 3, 1);
				if (i > 0)
					Console.WriteLine("yes");
				else
					Console.WriteLine("no");
			}

			public static void Bad1(int[] s)
			{
				int i = Array.IndexOf(s, 3);
				if (i > 0)
					Console.WriteLine("yes");
				else
					Console.WriteLine("no");
			}

			public static bool Bad2(int[] s)
			{
				int i = Array.IndexOf(s, 3);
				return i > 0;
			}

			public static void Bad3(int[] s)
			{
				if (Array.IndexOf(s, 3) > 0)
					Console.WriteLine("yes");
				else
					Console.WriteLine("no");
			}

			public static void Bad4(int[] s)
			{
				int i = Array.IndexOf(s, 3);
				if (0 < i)
					Console.WriteLine("yes");
				else
					Console.WriteLine("no");
			}
		}
		#endregion
		
		// test code
		public ArrayIndexOfTest() : base(
			new string[]{"Cases.Good1", "Cases.Good2"},
			new string[]{"Cases.Bad1", "Cases.Bad2", "Cases.Bad3", "Cases.Bad4"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new ArrayIndexOfRule(cache, reporter);
		}
	} 
}

