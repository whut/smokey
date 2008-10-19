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
	public class VisiblePInvokeTest : MethodTest
	{	
		#region Test classes
		public class GoodCases1
		{
			[System.Runtime.InteropServices.DllImport("Kernel32")]
			internal extern static bool M1(IntPtr handle);			

			[System.Runtime.InteropServices.DllImport("Kernel32")]
			private extern static bool M2(IntPtr handle);			

			[System.Runtime.InteropServices.DllImport("Kernel32")]
			private extern static bool M3(IntPtr handle);			

			public static bool M4(IntPtr handle)
			{
				return M2(handle) && M3(handle);
			}
		}

		internal sealed class GoodCases2
		{
			[System.Runtime.InteropServices.DllImport("Kernel32")]
			public extern static bool M1(IntPtr handle);			
		}

		private class GoodCases3
		{
			[System.Runtime.InteropServices.DllImport("Kernel32")]
			public extern static bool M1(IntPtr handle);			
		}

		public class BadCases1
		{
			[System.Runtime.InteropServices.DllImport("Kernel32")]
			public extern static bool M1(IntPtr handle);			
		}
		#endregion
		
		// test code
		public VisiblePInvokeTest() : base(
			new string[]{"GoodCases1.M1", "GoodCases1.M2", "GoodCases1.M3", 
				"GoodCases1.M4", "GoodCases2.M1", "GoodCases3.M1"},
			new string[]{"BadCases1.M1"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new VisiblePInvokeRule(cache, reporter);
		}
	} 
}
#endif
