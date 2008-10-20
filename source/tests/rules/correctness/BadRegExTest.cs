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
using System.Text.RegularExpressions;

namespace Smokey.Tests
{
	[TestFixture]
	public class BadRegExTest : MethodTest
	{	
		#region Test classes
		public class Cases
		{
			public static bool Good1(string s)
			{
				Regex r = new Regex("foo");
				return r.IsMatch(s);
			}

			public static bool Good2(string s)
			{
				return Regex.IsMatch(s, "foo|bar");
			}

			public static bool Good3(string s)
			{
				return Regex.IsMatch(s, 
					@"# Matches invalid empty brackets # 
					(?\)| # Matches a valid parameter reference 
					\[^\]+)\>| 
					# Matches opened brackes that are not properly closed # 
					(?\]*(?!\>))", 
					RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace);
			}

			public static bool Bad1(string s)
			{
				Regex r = new Regex("foo++");
				return r.IsMatch(s);
			}

			public static bool Bad2(string s)
			{
				return Regex.IsMatch(s, "foo++");
			}

			public static Match Bad3(string s)
			{
				return Regex.Match(s, "foo++");
			}

			public static MatchCollection Bad4(string s)
			{
				return Regex.Matches(s, "foo++");
			}

			public static string Bad5(string s)
			{
				return Regex.Replace(s, "foo++", "bar");
			}

			public static string[] Bad6(string s)
			{
				return Regex.Split(s, "foo++");
			}
		}
		#endregion
		
		// test code
		public BadRegExTest() : base(
			new string[]{"Cases.Good1", "Cases.Good2", "Cases.Good3"},
			new string[]{"Cases.Bad1", "Cases.Bad2", "Cases.Bad3", "Cases.Bad4"
				, "Cases.Bad5", "Cases.Bad6"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new BadRegExRule(cache, reporter);
		}
	} 
}

