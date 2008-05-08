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
using System.IO;
using System.Reflection;

namespace Smokey.Tests
{
	[TestFixture]
	public class BaseDisposableTest : MethodTest
	{	
		#region Test classes
		internal class MyResource : IDisposable
		{
			~MyResource()		
			{					
				Dispose(false);
			}
		
			public MyResource(IntPtr handle)
			{
				m_handle = handle;
			}
		
			public void DoWork()
			{
				NativeMethods.WorkIt(m_handle);
			}
		
			public void Dispose()
			{
				Dispose(true);		
				GC.SuppressFinalize(this);
			}
			
			protected bool Disposed
			{
				get {return m_disposed;}
			}
		
			protected virtual void Dispose(bool disposing)
			{
				if (!m_disposed)
				{
					NativeMethods.CloseHandle(m_handle);
					m_handle = IntPtr.Zero;
		
					m_disposed = true;
				}
			}
					
			private static class NativeMethods
			{
				[System.Runtime.InteropServices.DllImport("Kernel32")]
				public extern static bool CloseHandle(IntPtr handle);
			
				[System.Runtime.InteropServices.DllImport("Kernel32")]
				public extern static void WorkIt(IntPtr handle);
			}
			
			private IntPtr m_handle;
			private bool m_disposed;
		}			

		internal class Good1 : MyResource
		{
			public Good1(IntPtr handle) : base(handle)
			{
			}
			
			protected override void Dispose(bool disposing)
			{
				if (!Disposed)
				{
					Console.WriteLine("filler");
					base.Dispose(disposing);
				}					
			}
		}			

		internal class Bad1 : MyResource
		{
			public Bad1(IntPtr handle) : base(handle)
			{
			}
			
			protected override void Dispose(bool disposing)
			{
				if (!Disposed)
				{
					Console.WriteLine("filler");
				}					
			}
		}			
		#endregion
		
		// test code
		public BaseDisposableTest() : base(
			new string[]{"Good1.Dispose"},
			new string[]{"Bad1.Dispose"},
			new string[]{"MyResource.MyResource"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new BaseDisposableRule(cache, reporter);
		}
	} 
}
