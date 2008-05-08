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
	public class DisposableButNoFinalizerTest : TypeTest
	{	
		#region Test classes
		private class GoodCase : IDisposable
		{
			~GoodCase()		
			{					
				DoDispose(false);
			}
		
			public GoodCase(TextWriter writer)
			{
				m_writer = writer;
			}
		
			public void WriteLine(string line)
			{
				m_writer.WriteLine(line);
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
					if (disposing)
						m_writer.Dispose();
								
					m_disposed = true;
				}
			}
				
			private TextWriter m_writer;
			private bool m_disposed = false;
		}
				
		private class BadCase : IDisposable
		{		
			public BadCase(IntPtr handle)
			{
				m_handle1 = handle;
				m_handle2 = handle;
			}
		
			public void WriteLine(string line)
			{
				Console.WriteLine(m_handle2);
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
					if (disposing)
						CloseHandle(m_handle1);
								
					m_disposed = true;
				}
			}
				
			[System.Runtime.InteropServices.DllImport("Kernel32")]
			private extern static bool CloseHandle(IntPtr handle);
	
			private IntPtr m_handle1;
			private IntPtr m_handle2;
			private bool m_disposed = false;
		}
				
		private class BadCase2 : IDisposable
		{				
			public void WriteLine(string line)
			{
				Console.WriteLine(m_handle1);
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
					m_disposed = true;
				}
			}
					
			private HandleRef m_handle1;
			private bool m_disposed = false;
		}
		#endregion
		
		// test code
		public DisposableButNoFinalizerTest() : base(
			new string[]{"GoodCase"},
			new string[]{"BadCase", "BadCase2"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new DisposableButNoFinalizerRule(cache, reporter);
		}
	} 
}
