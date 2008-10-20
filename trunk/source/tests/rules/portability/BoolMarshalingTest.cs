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
using System.Runtime.InteropServices;
using System.Reflection;
using Smokey.Framework.Support;
using Smokey.Internal.Rules;

namespace Smokey.Tests
{
	[TestFixture]
	public class BoolMarshalingTest : MethodTest
	{	
		#region Test cases
		private class Cases
		{
			[DllImport("aspell")]
			public static extern void GoodCase1();

			[DllImport("aspell")]
			public static extern void GoodCase2(int foo);

			[DllImport("aspell")]
			public static extern int GoodCase3();

			[DllImport("aspell")]
			public static extern void GoodCase4([MarshalAs(UnmanagedType.U1)] bool enable);

			[DllImport("aspell")]
			[return: MarshalAs(UnmanagedType.U1)] 
			public static extern bool GoodCase5();

			[DllImport("aspell")]
			public static extern void BadCase1(bool enable);

			[DllImport("aspell")]
			public static extern bool BadCase2();
		}
		#endregion
		
		// test code
		public BoolMarshalingTest() : base(
			new string[]{"Cases.GoodCase1", "Cases.GoodCase2", "Cases.GoodCase3", "Cases.GoodCase4", "Cases.GoodCase5"},
			new string[]{"Cases.BadCase1", "Cases.BadCase2"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new BoolMarshalingRule(cache, reporter);
		}
	} 
}

