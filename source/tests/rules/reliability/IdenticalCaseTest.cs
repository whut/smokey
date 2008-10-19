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
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Smokey.Framework.Support;
using Smokey.Internal.Rules;

#if OLD
namespace Smokey.Tests
{
	[TestFixture]
	public class IdenticalCaseTest : MethodTest
	{	
		#region Test Classes
		private class Cases
		{
			public void Good1(string s)
			{
				switch (s.Length)
				{
					case 0:
						Console.WriteLine("zero");
						break;
						
					case 1:
						Console.WriteLine("one");
						break;
						
					case 2:
					case 3:
					case 4:
						Console.WriteLine("many");
						break;
				}
			}
			
			public void Bad1(string s)
			{
				switch (s.Length)
				{
					case 0:
						Console.WriteLine("zero");
						break;
						
					case 1:
						Console.WriteLine("zero");
						break;
						
					case 3:
					case 4:
					case 5:
						Console.WriteLine("one");
						break;
				}
			}
			
			public void Bad2(string s)
			{
				switch (s.Length)
				{
					case 0:
						Console.WriteLine("zero");
						break;
						
					case 1:
						Console.WriteLine("one");
						break;
						
					case 3:
					case 4:
					case 5:
						Console.WriteLine("zero");
						break;
				}
			}
		}		
		#endregion
		
		// test code
		public IdenticalCaseTest() : base(
			new string[]{"Cases.Good1"},
			new string[]{"Cases.Bad1", "Cases.Bad2"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new IdenticalCaseRule(cache, reporter);
		}
	} 
}
#endif
