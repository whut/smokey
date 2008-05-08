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
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Smokey.Framework.Support;
using Smokey.Internal.Rules;

namespace Smokey.Tests
{
	[TestFixture]
	public class NativeMethodsTest : MethodTest
	{	
		// test classes
		internal static class NativeMethods
		{
			[DllImport("Kernel32")]
			public extern static bool CloseHandle(IntPtr handle);
		}
				
		internal static class SafeNativeMethods
		{
			[DllImport("Kernel32")]
			public extern static bool CloseHandle(IntPtr handle);
		}
				
		internal static class UnsafeNativeMethods
		{
			[DllImport("Kernel32")]
			public extern static bool CloseHandle(IntPtr handle);
		}
				
		internal static class Good
		{
			public static void CloseHandle(IntPtr handle)
			{
			}
		}
				
		internal static class Bad1
		{
			[DllImport("Kernel32")]
			public extern static bool CloseHandle(IntPtr handle);
		}
				
		// test code
		public NativeMethodsTest() : base(
			new string[]{"NativeMethods.CloseHandle", "SafeNativeMethods.CloseHandle", 
				"UnsafeNativeMethods.CloseHandle", "Good.CloseHandle"},
			
			new string[]{"Bad1.CloseHandle"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new NativeMethodsRule(cache, reporter);
		}
	} 
}