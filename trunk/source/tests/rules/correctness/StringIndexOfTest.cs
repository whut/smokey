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
	public class StringIndexOfTest : MethodTest
	{	
		#region Test classes
		public class Cases
		{
			public static void Good1(string s)
			{
				int i = s.IndexOf('x');
				if (i >= 0)
					Console.WriteLine("yes");
				else
					Console.WriteLine("no");
			}
			
			public static void Good2(string s)
			{
				int i = s.IndexOf('x', 1);
				if (i > 0)
					Console.WriteLine("yes");
				else
					Console.WriteLine("no");
			}

			public static void Good3(string s)
			{
				int i = s.IndexOf("x", 1);
				if (i > 0)
					Console.WriteLine("yes");
				else
					Console.WriteLine("no");

				i = s.IndexOfAny(new char[]{'x'}, 1);
				if (i > 0)
					Console.WriteLine("yes2");
				else
					Console.WriteLine("no2");
			}

			public static void Bad1(string s)
			{
				int i = s.IndexOf('x');
				if (i > 0)
					Console.WriteLine("yes");
				else
					Console.WriteLine("no");
			}

			public static void Bad2(string s)
			{
				int i = s.IndexOf("x");
				if (i > 0)
					Console.WriteLine("yes");
				else
					Console.WriteLine("no");
			}

			public static void Bad3(string s)
			{
				int i = s.IndexOfAny(new char[]{'x'});
				if (i > 0)
					Console.WriteLine("yes");
				else
					Console.WriteLine("no");
			}

			public static bool Bad4(string s)
			{
				int i = s.IndexOf('x');
				return i > 0;
			}

			public static void Bad5(string s)
			{
				if (s.IndexOf('x') > 0)
					Console.WriteLine("yes");
				else
					Console.WriteLine("no");
			}

			public static void Bad6(string s)
			{
				int i = s.IndexOf('x');
				if (0 < i)
					Console.WriteLine("yes");
				else
					Console.WriteLine("no");
			}
		}
		#endregion
		
		// test code
		public StringIndexOfTest() : base(
			new string[]{"Cases.Good1", "Cases.Good2", "Cases.Good3"},
			new string[]{"Cases.Bad1", "Cases.Bad2", "Cases.Bad3", "Cases.Bad4", 
				"Cases.Bad5", "Cases.Bad6"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new StringIndexOfRule(cache, reporter);
		}
	} 
}
