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
	public class UseStringEmptyTest : MethodTest
	{	
		#region Test cases
		private class GoodCase
		{
			public void Arg()
			{
				Console.WriteLine(string.Empty);
			}

			public void Operand1(string str)
			{
				if (str == string.Empty)
					Console.WriteLine("empty");
			}

			public void Operand2(string str)
			{
				if (string.Empty == str)
					Console.WriteLine("empty");
			}

			public void Target(string str)
			{
				Console.WriteLine(string.Empty.Length);
			}
		}

		private class BadCase
		{
			public void Arg()
			{
				Console.WriteLine("");
			}

			public void Operand1(string str)
			{
				if (str == "")
					Console.WriteLine("empty");
			}

			public void Operand2(string str)
			{
				if ("" == str)
					Console.WriteLine("empty");
			}

			public void Target(string str)
			{
				Console.WriteLine("".Length);
			}
		}
		#endregion

		// test code
		public UseStringEmptyTest() : base(
			new string[]{"GoodCase.Arg", "GoodCase.Operand1", "GoodCase.Operand2", "GoodCase.Target"},
			new string[]{"BadCase.Arg", "BadCase.Operand1", "BadCase.Operand2", "BadCase.Target"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new UseStringEmptyRule(cache, reporter);
		}
	} 
}

