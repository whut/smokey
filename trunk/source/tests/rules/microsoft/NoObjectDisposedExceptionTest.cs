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
using System.Reflection;
using Smokey.Framework.Support;
using Smokey.Internal.Rules;

#if OLD
namespace Smokey.Tests
{
	[TestFixture]
	public class NoObjectDisposedExceptionTest : TypeTest
	{	
		// test classes
		public class GoodResource : IDisposable
		{
			~GoodResource()		
			{					
				DoDispose(false);
			}
		
			public GoodResource(IntPtr handle)
			{
				m_handle = handle;
			}
		
			public void Work()
			{
				if (m_disposed)		
					throw new ObjectDisposedException(GetType().Name);
					
				Console.WriteLine(m_handle);
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
					m_handle = IntPtr.Zero;
		
					m_disposed = true;
				}
			}
				
			private IntPtr m_handle;
			private bool m_disposed = false;
		}			
		
		public class GoodResource2 : IDisposable
		{
			~GoodResource2()		
			{					
				DoDispose(false);
			}
		
			public GoodResource2(IntPtr handle)
			{
				m_handle = handle;
			}
		
			public void Work()
			{
				Console.WriteLine("...");
				DoWork();					
			}
		
			public void DoWork()
			{
				if (m_disposed)		
					throw new ObjectDisposedException("");
					
				Console.WriteLine(m_handle);
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
					m_handle = IntPtr.Zero;
		
					m_disposed = true;
				}
			}
				
			private IntPtr m_handle;
			private bool m_disposed = false;
		}			
		
		public class BadResource : IDisposable
		{
			~BadResource()		
			{					
				DoDispose(false);
			}
		
			public BadResource(IntPtr handle)
			{
				m_handle = handle;
			}
		
			public void Work()
			{					
				Console.WriteLine(m_handle);
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
					m_handle = IntPtr.Zero;
		
					m_disposed = true;
				}
			}
				
			private IntPtr m_handle;
			private bool m_disposed = false;
		}			
		
		// test code
		public NoObjectDisposedExceptionTest() : base(
			new string[]{"GoodResource", "GoodResource2"},
			new string[]{"BadResource"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new NoObjectDisposedExceptionRule(cache, reporter);
		}
	} 
}
#endif