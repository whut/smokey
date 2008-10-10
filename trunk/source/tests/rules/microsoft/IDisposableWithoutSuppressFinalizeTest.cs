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
using System.Reflection;
using Smokey.Framework.Support;
using Smokey.Internal.Rules;

#if OLD
namespace Smokey.Tests
{
	[TestFixture]
	public class IDisposableWithoutSuppressFinalizeTest : MethodTest
	{	
		#region Test classes
		private class GoodCase : IDisposable
		{
			~GoodCase()		
			{					
				DoDispose(false);
			}
		
			public GoodCase(IntPtr handle)
			{
				m_handle = handle;
			}
		
			public void Dispose()
			{
				DoDispose(true);
		
				GC.SuppressFinalize(this);
			}
		
			private void DoDispose(bool disposing)
			{
				if (!m_disposed)
				{
					CloseHandle(m_handle);
					m_handle = IntPtr.Zero;
		
					m_disposed = true;
				}
			}
		
			[System.Runtime.InteropServices.DllImport("Kernel32")]
			private extern static bool CloseHandle(IntPtr handle);
		
			private IntPtr m_handle;
			private bool m_disposed = false;
		}
				
		private class GoodCase2 : IDisposable
		{
			~GoodCase2()		
			{					
				DoDispose(false);
			}
		
			public GoodCase2(IntPtr handle)
			{
				m_handle = handle;
			}
		
			public void Dispose()
			{
				try
				{
					DoDispose(true);
				}
				finally
				{
					GC.SuppressFinalize(this);
				}
			}
		
			private void DoDispose(bool disposing)
			{
				if (!m_disposed)
				{
					CloseHandle(m_handle);
					m_handle = IntPtr.Zero;
		
					m_disposed = true;
				}
			}
		
			[System.Runtime.InteropServices.DllImport("Kernel32")]
			private extern static bool CloseHandle(IntPtr handle);
		
			private IntPtr m_handle;
			private bool m_disposed = false;
		}
				
		private class BadCase : IDisposable
		{
			~BadCase()		
			{					
				DoDispose(false);
			}
		
			public BadCase(IntPtr handle)
			{
				m_handle = handle;
			}
		
			public void Dispose()
			{
				DoDispose(true);
			}
		
			private void DoDispose(bool disposing)
			{
				if (!m_disposed)
				{
					CloseHandle(m_handle);
					m_handle = IntPtr.Zero;
		
					m_disposed = true;
				}
			}
		
			[System.Runtime.InteropServices.DllImport("Kernel32")]
			private extern static bool CloseHandle(IntPtr handle);
		
			private IntPtr m_handle;
			private bool m_disposed = false;
		}
		#endregion
		
		// test code
		public IDisposableWithoutSuppressFinalizeTest() : base(
			new string[]{"GoodCase.Dispose", "GoodCase2.Dispose"},
			new string[]{"BadCase.Dispose"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new IDisposableWithoutSuppressFinalizeRule(cache, reporter);
		}
	} 
}
#endif