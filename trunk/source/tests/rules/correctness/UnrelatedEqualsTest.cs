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
using Smokey.Framework.Support;
using Smokey.Internal.Rules;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;

namespace Smokey.Tests
{
	[TestFixture]
	public class UnrelatedEqualsTest : MethodTest
	{	
		#region Test classes
		public class Cases
		{
			public static bool Good1(string x, string y)
			{
				return x.Equals(y);
			}
			
			public static bool Good2(object x, string y)
			{
				return x.Equals(y);
			}
			
			public static bool Good3(string x, object y)
			{
				return x.Equals(y);
			}
			
			public static bool Good4(object x, object y)
			{
				return x.Equals(y);
			}
			
			public static bool Good5(Exception x, DivideByZeroException y)
			{
				return x.Equals(y);
			}
			
			public static bool Good6(Uri x, ISerializable y)
			{
				return x.Equals(y);
			}
			
			public static bool Bad1(string x, int y)
			{
				return x.Equals(y);	
			}
			
			public static bool Bad2(string x, int[] y)
			{
				return x.Equals(y);
			}
			
			public static bool Bad3(Uri x, IAsyncResult y)
			{
				return x.Equals(y);
			}
		}
		#endregion
		
		// test code
		public UnrelatedEqualsTest() : base(
			new string[]{"Cases.Good1", "Cases.Good2", "Cases.Good3", 
						"Cases.Good4", "Cases.Good5", "Cases.Good6"},
			new string[]{"Cases.Bad1", "Cases.Bad2", "Cases.Bad3"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new UnrelatedEqualsRule(cache, reporter);
		}
	} 
}
