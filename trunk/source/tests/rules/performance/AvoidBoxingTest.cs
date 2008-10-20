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

using NUnit.Framework;
using System;
using System.Collections.Generic;
using Smokey.Framework.Support;
using Smokey.Internal.Rules;

namespace Smokey.Tests
{
	[TestFixture]
	public class AvoidBoxingTest : MethodTest
	{	
		#region Test cases
		private class GoodCase
		{
			public static void Good(int n)
			{
				ms_ints.Add(n);									
				Console.WriteLine("len: {0}", n.ToString());	
				Console.WriteLine("len: {0}", n);				// up to 3 boxes are ok
				Console.WriteLine("len: {0}", n);			
				Console.WriteLine("len: {0}", n);			
			}
			
			private static List<int> ms_ints = new List<int>();
		}

		private class BadCase
		{
			public static void Ungood(int n)
			{
				Console.WriteLine("len: {0}", n);				// 4 boxes are bad
				Console.WriteLine("len: {0}", n);			
				Console.WriteLine("len: {0}", n);			
				Console.WriteLine("len: {0}", n);			
			}
		}
		#endregion
		
		// test code
		public AvoidBoxingTest() : base(
			new string[]{"GoodCase.Good"},
			new string[]{"BadCase.Ungood"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new AvoidBoxingRule(cache, reporter);
		}
	} 
}

