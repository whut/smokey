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

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace EvilDoer
{
	public sealed class FooHandle : SafeHandle
	{
		public FooHandle() : base(IntPtr.Zero, true)
		{
		}
		
		public FooHandle(int flags) : base(NativeMethods.CreateFoo(flags), true)
		{
		}
		
		public override bool IsInvalid 
		{ 
			get 
			{
				return handle == IntPtr.Zero;
			}
		}
	
		protected override bool ReleaseHandle()
		{
			NativeMethods.CloseFoo(this);
			
			return true;
		}
	}

	internal static partial class NativeMethods
	{
		[System.Runtime.InteropServices.DllImport("someLib")]
		public extern static IntPtr CreateFoo(int flags);

		[System.Runtime.InteropServices.DllImport("someLib")]
		public extern static void CloseFoo(FooHandle handle);

		[System.Runtime.InteropServices.DllImport("someLib")]
		public extern static void WorkFoo(FooHandle handle);
	}
		
	public sealed class GoodDisposable : IDisposable
	{
		public GoodDisposable()
		{
			handle = new FooHandle(100);
		}
	
		public void Work()
		{
			if (disposed)		
				throw new ObjectDisposedException(GetType().Name);
				
			NativeMethods.WorkFoo(handle);
			GC.KeepAlive(this);
		}
		
		public void Dispose()
		{
			if (!disposed)
			{
				handle.Dispose();	
				disposed = true;
			}
		}
	
		private FooHandle handle;
		private bool disposed = false;
	}
}