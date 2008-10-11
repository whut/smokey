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
	public class GoodDisposable : IDisposable
	{
		~GoodDisposable()		
		{					
			Dispose(false);
		}
	
		public GoodDisposable(IntPtr handle)
		{
			this.handle = handle;
		}
	
		public void Work()
		{
			if (disposed)		
				throw new ObjectDisposedException(GetType().Name);
				
			System.Diagnostics.Debug.WriteLine(handle);
			GC.KeepAlive(this);
		}
		
		public void Dispose()
		{
			Dispose(true);
	
			GC.SuppressFinalize(this);
		}
	
		protected virtual void Dispose(bool disposing)
		{
			if (!disposed)
			{
				Ignore.Value = NativeMethods.CloseHandle(handle);
				handle = IntPtr.Zero;
	
				disposed = true;
			}
		}
	
		private static class NativeMethods
		{
			[System.Runtime.InteropServices.DllImport("Kernel32")]
			[return: MarshalAs(UnmanagedType.U1)] 
			public extern static bool CloseHandle(IntPtr handle);
		}
		
		private IntPtr handle;
		private bool disposed = false;
	}
}