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
	public class DisposeNativeResourcesTest : TypeTest
	{	
		#region Test classes
		private class GoodCase
		{
			public GoodCase(IntPtr resource)	// doesn't own the resource
			{
				m_resource = resource;
			}
						
			public void WriteLine()
			{
				Console.WriteLine("using {0}", m_resource);
			}
		
			private IntPtr m_resource;
		}
		
		private class GoodCase2 : IDisposable
		{
			~GoodCase2()		
			{					
				DoDispose(false);
			}
		
			public GoodCase2()
			{
				m_resource = CreateHandle();
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
					Console.WriteLine("disposing of {0}", m_resource);
					m_disposed = true;
				}
			}
				
			[System.Runtime.InteropServices.DllImport("Kernel32")]
			private extern static IntPtr CreateHandle();

			private IntPtr m_resource;
			private bool m_disposed = false;
		}
		
		private class Wrapper
		{
			public IntPtr Ptr;
			
			public IntPtr Pointer
			{
				get {return Ptr;}
			}
		}
				
		private class GoodCase3
		{
			public GoodCase3(Wrapper resource)	// doesn't own the resource
			{
				m_resource = resource.Ptr;
			}
						
			public void WriteLine()
			{
				Console.WriteLine("using {0}", m_resource);
			}
		
			private IntPtr m_resource;
		}

		private class GoodCase4					// doesn't own the resource
		{
			public GoodCase4(Wrapper resource)
			{
				m_resource = resource.Pointer;
			}
						
			public void WriteLine()
			{
				Console.WriteLine("using {0}", m_resource);
			}
		
			private IntPtr m_resource;
		}

		public class GoodCase5				// doesn't new
		{
			public void Work()
			{				
				Console.WriteLine(handle);
			}
					
			private IntPtr handle = (IntPtr) 0;
		}

		private class BadCase : IDisposable				// no finalizer
		{
			public BadCase()
			{
				m_resource = CreateHandle();
			}
					
			public void Dispose()
			{
			}
				
			public void WriteLine()
			{
				Console.WriteLine("using {0}", m_resource);
			}
		
			[System.Runtime.InteropServices.DllImport("Kernel32")]
			private extern static IntPtr CreateHandle();

			private IntPtr m_resource;
		}

		private class BadCase2				// not IDisposable
		{
			~BadCase2()
			{
			}
			
			public BadCase2()
			{
				m_resource = new IntPtr(100);
			}
					
			public void WriteLine()
			{
				Console.WriteLine("using {0}", m_resource);
			}
		
			private System.IntPtr m_resource;
		}

		private class BadCase3				// no finalizer and not IDisposable
		{
			public BadCase3()
			{
				m_resource = new System.Runtime.InteropServices.HandleRef();
			}
					
			public void WriteLine()
			{
				Console.WriteLine("using {0}", m_resource);
			}
		
			private System.Runtime.InteropServices.HandleRef m_resource;
		}
		#endregion
		
		// test code
		public DisposeNativeResourcesTest() : base(
			new string[]{"GoodCase", "GoodCase2", "GoodCase3", "GoodCase4", "GoodCase5"},
			new string[]{"BadCase", "BadCase2", "BadCase3"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new DisposeNativeResourcesRule(cache, reporter);
		}
	} 
}
