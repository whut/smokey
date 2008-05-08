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
using Smokey.Framework.Support;
using Smokey.Tests;

namespace Custom
{
	// Tests should inherit from MethodTest, TypeTest, or AssemblyTest.
	[TestFixture]
	public class PublicStartsWithDoTest : MethodTest
	{	
		#region Test classes
		// By convention the test cases are nested within the test type.
		// Either the class or method name should clearly indicate if
		// the test is expected to pass or fail.
		private class GoodCases
		{
			public double Y
			{
				get {return x;}
			}
			
			public double GetX()
			{
				return x;
			}
			
			protected double GetPI()
			{
				return DoGetPi();
			}
			
			private double DoGetPi()
			{
				return X;
			}						

			private double X	
			{
				get {return x;}
				set {x = value;}
			}
			
			private double x;
		}		

		private class BadCases
		{
			public int DoSum(int x, int y)
			{
				return x + y;
			}
			
			protected double DoGetPI()
			{
				return 3.1415;
			}
		}		
		#endregion
		
		// Here's our constructor. The arguments are:
		// 1) an array of the methods that are expected to pass
		// 2) an array of methods that are expected to fail 
		// 3) an array of methods which rules will need to visit but which 
		// aren't tested (these are for inter-procedural rules).
		// 4) The location of the assembly our test cases are in
		public PublicStartsWithDoTest() : base(
			new string[]{"GoodCases.get_Y", "GoodCases.GetX", "GoodCases.GetPI", 
			"GoodCases.DoGetPi", "GoodCases.get_X", "GoodCases.set_X"}, 
			
			new string[]{"BadCases.DoSum", "BadCases.DoGetPI"}, 
			
			new string[0], 
			
			System.Reflection.Assembly.GetExecutingAssembly().Location)
		{
		}
		
		// This is where we create our rule. The base class will handle all
		// of the testing and validation.
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new PublicStartsWithDoRule(cache, reporter);
		}
	} 
}
