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
	public class ReservedExceptionsTest : MethodTest
	{	
		#region Test classes
		private class GoodCase
		{
			public void Arg(int x)
			{
				if (x < 0)
					throw new ArgumentException("x is negative");
					
				Console.WriteLine("cool stuff goes here");
			}
		}
				
		private class BadCase
		{
			public void Base1(int x)
			{
				if (x < 0)
					throw new IndexOutOfRangeException("x is negative");
					
				Console.WriteLine("cool stuff goes here");
			}

			public void Base2(int x)
			{
				if (x < 0)
					throw new NullReferenceException("x is negative");
					
				Console.WriteLine("cool stuff goes here");
			}
		}
		#endregion
		
		// test code
		public ReservedExceptionsTest() : base(
			new string[]{"GoodCase.Arg"},
			new string[]{"BadCase.Base1", "BadCase.Base2"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new ReservedExceptionsRule(cache, reporter);
		}
	} 
}

