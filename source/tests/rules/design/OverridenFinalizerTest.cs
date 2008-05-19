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
using Smokey.Framework.Support;
using Smokey.Internal.Rules;

namespace Smokey.Tests
{
	[TestFixture]
	public class OverridenFinalizerTest : TypeTest
	{	
		// test classes
		internal class MyResource : IDisposable
		{
			~MyResource()		
			{					
				Dispose(false);
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
					CloseHandle(m_handle);
					m_handle = IntPtr.Zero;
		
					m_disposed = true;
				}
			}
		
			[System.Runtime.InteropServices.DllImport("Kernel32")]
			private extern static bool CloseHandle(IntPtr handle);
				
			private IntPtr m_handle;
			private bool m_disposed;
		}			

		internal sealed class Good1 : MyResource
		{
			protected override void Dispose(bool disposing)
			{
				base.Dispose(disposing);
			}
		}			

		internal sealed class Bad1 : MyResource
		{
			~Bad1()
			{
			}
			
			protected override void Dispose(bool disposing)
			{
				base.Dispose(disposing);
			}
		}			
				
		// test code
		public OverridenFinalizerTest() : base(
			new string[]{"Good1"},
			new string[]{"Bad1"},
			new string[]{"MyResource"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new OverridenFinalizerRule(cache, reporter);
		}
	} 
}