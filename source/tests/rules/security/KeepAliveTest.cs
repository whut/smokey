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

namespace Smokey.Tests
{
	[TestFixture]
	public class KeepAliveTest : TypeTest
	{	
		#region Test classes
		internal sealed class Good1 
		{
			~Good1()		
			{					
				NativeMethods.CloseHandle(m_handle);
			}
		
			public Good1(IntPtr handle)
			{
				m_handle = handle;
			}
		
			public void Work()
			{
				NativeMethods.WorkIt(m_handle);
				GC.KeepAlive(this);
			}
			
			private static class NativeMethods
			{
				[System.Runtime.InteropServices.DllImport("Kernel32")]
				public extern static bool CloseHandle(IntPtr handle);
			
				[System.Runtime.InteropServices.DllImport("Kernel32")]
				public extern static void WorkIt(IntPtr handle);
			}

			private IntPtr m_handle;
		}

		internal sealed class Good2
		{
			public Good2(IntPtr handle)
			{
				m_handle = handle;
			}
		
			public void Work()
			{
				NativeMethods.WorkIt(m_handle);
				GC.KeepAlive(this);
			}
			
			private static class NativeMethods
			{
				[System.Runtime.InteropServices.DllImport("Kernel32")]
				public extern static bool CloseHandle(IntPtr handle);
			
				[System.Runtime.InteropServices.DllImport("Kernel32")]
				public extern static void WorkIt(IntPtr handle);
			}

			private IntPtr m_handle;
		}
	
		internal sealed class Bad1 
		{
			~Bad1()		
			{					
				NativeMethods.CloseHandle(m_handle);
			}
		
			public Bad1(IntPtr handle)
			{
				m_handle = handle;
			}
		
			public void Work()
			{
				NativeMethods.WorkIt(m_handle);
//				GC.KeepAlive(this);
			}
			
			private static class NativeMethods
			{
				[System.Runtime.InteropServices.DllImport("Kernel32")]
				public extern static bool CloseHandle(IntPtr handle);
			
				[System.Runtime.InteropServices.DllImport("Kernel32")]
				public extern static void WorkIt(IntPtr handle);
			}

			private IntPtr m_handle;
		}
	
		internal sealed class Bad2 
		{
			~Bad2()		
			{					
				NativeMethods.CloseHandle(m_handle);
			}
		
			public Bad2(IntPtr handle)
			{
				m_handle = handle;
			}
		
			public void Work()
			{
				NativeMethods.WorkIt(m_handle);
				KeepAlive(this);	// techincally this would work, but it's not recommended
			}
			
			private void KeepAlive(object o)
			{
				Console.WriteLine(o);
			}
			
			private static class NativeMethods
			{
				[System.Runtime.InteropServices.DllImport("Kernel32")]
				public extern static bool CloseHandle(IntPtr handle);
			
				[System.Runtime.InteropServices.DllImport("Kernel32")]
				public extern static void WorkIt(IntPtr handle);
			}

			private IntPtr m_handle;
		}
		#endregion
		
		// test code
		public KeepAliveTest() : base(
			new string[]{"Good1", "Good2"},
			new string[]{"Bad1", "Bad2"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new KeepAliveRule(cache, reporter);
		}
	} 
}
