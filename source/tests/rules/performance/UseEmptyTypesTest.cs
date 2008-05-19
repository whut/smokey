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
	public class UseEmptyTypesTest : MethodTest
	{	
		#region Test cases
		private class GoodCases
		{
			public Type[] E()
			{
				return Type.EmptyTypes;
			}

			public Type[] NE1()
			{
				return new Type[2]{typeof(int), typeof(long)};
			}

			public Type[] NE2()
			{
				Type[] r = {typeof(int), typeof(long)};
				return r;
			}

			public object S()
			{
				return new string[0];
			}

			public void AddDictFile(string text)
			{
				string[] words = text.Split();
				foreach (string word in words)
				{
					if (word.Length > 0)
					{
						Console.WriteLine(word);
					}
				}
			}		
		}

		private class BadCases
		{
			public Type[] E1()
			{
				return new Type[0];
			}

			public Type[] E2()
			{
				return new Type[]{};
			}

			public Type[] E3()
			{
				Type[] r = {};
				return r;
			}
		}
		#endregion

		// test code
		public UseEmptyTypesTest() : base(
			new string[]{"GoodCases.E", "GoodCases.NE1", "GoodCases.NE2", 
				"GoodCases.S", "GoodCases.AddDictFile"},
				
			new string[]{"BadCases.E1", "BadCases.E2", "BadCases.E3"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new UseEmptyTypesRule(cache, reporter);
		}
	} 
}
