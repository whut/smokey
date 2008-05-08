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
using Smokey.Framework;
using Smokey.Framework.Support;
using Smokey.Tests;

namespace Custom
{
	// Tests should inherit from MethodTest, TypeTest, or AssemblyTest.
	[TestFixture]
	public class LogStartsWithStringTest : MethodTest
	{	
		#region Test classes
		// By convention the test cases are nested within the test type.
		// Either the class or method name should clearly indicate if
		// the test is expected to pass or fail.
		private class Cases
		{
			public void GoodLog1(int x)
			{
				Log.InfoLine(this, "x");
			}
			
			public void GoodLog2(int x)
			{
				Log.InfoLine(true, "x = {0}", x);
			}
			
			public void GoodLog3(int x)
			{
				Log.InfoLine((Rule) null, "x = {0}{1}", x, x*2);
			}
			
			public void GoodLog4(int x)
			{
				Log.InfoLine(true, "x = {0}{1}{2}{3}{4}", x, x*1, x*2, x*3, x*4);
			}
			
			public void BadLog1(int x)
			{
				Log.InfoLine("x");
			}
			
			public void BadLog2(int x)
			{
				Log.InfoLine("x = {0}", x);
			}
			
			public void BadLog3(int x)
			{
				Log.InfoLine("x = {0}{1}", "foo", x*2);
			}
			
			public void BadLog4(int x)
			{
				Log.InfoLine("x = {0}{1}{2}{3}", "foo", x*2, x*3, x*4, x*5);
			}
		}		
		#endregion
		
		// Here's our constructor. The arguments are:
		// 1) an array of the methods that are expected to pass
		// 2) an array of methods that are expected to fail 
		// 3) an array of methods which rules will need to visit but which 
		// aren't tested (these are for inter-procedural rules).
		// 4) The location of the assembly our test cases are in
		public LogStartsWithStringTest() : base(
			new string[]{"Cases.GoodLog1", "Cases.GoodLog2", "Cases.GoodLog3", "Cases.GoodLog4"}, 
			new string[]{"Cases.BadLog1", "Cases.BadLog2", "Cases.BadLog3", "Cases.BadLog4"}, 
			new string[0], 
			System.Reflection.Assembly.GetExecutingAssembly().Location)
		{
		}
		
		// This is where we create our rule. The base class will handle all
		// of the testing and validation.
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new LogStartsWithStringRule(cache, reporter);
		}
	} 
}
