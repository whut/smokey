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
	public class BaseGoodDisposable2 : IDisposable
	{
		~BaseGoodDisposable2()		
		{					
			Dispose(false);
		}
			
		public void Dispose()
		{
			if (!disposed)
				Dispose(true);
	
			GC.SuppressFinalize(this);
		}
	
		protected virtual void Dispose(bool disposing)
		{
			disposed = true;
		}
		
		protected bool Disposed {get {return disposed;}}
	
		private bool disposed = false;
	}

	public class GoodDisposable2 : BaseGoodDisposable2
	{
		public GoodDisposable2()
		{
			this.handle = NativeMethods.CreateHandle();
		}
	
		public void Work()
		{
			if (Disposed)		
				throw new ObjectDisposedException(GetType().Name);
				
			System.Diagnostics.Debug.WriteLine(handle);
		}
			
		public static GoodDisposable2 Instantiate()
		{
			return new GoodDisposable2();
		}
			
		protected override void Dispose(bool disposing)
		{
			Ignore.Value = NativeMethods.CloseHandle(handle);
			handle = IntPtr.Zero;
			
			base.Dispose(disposing);
		}
	
		private static class NativeMethods
		{
			[System.Runtime.InteropServices.DllImport("Kernel32")]
			public extern static IntPtr CreateHandle();
	
			[System.Runtime.InteropServices.DllImport("Kernel32")]
			[return: MarshalAs(UnmanagedType.U1)] 
			public extern static bool CloseHandle(IntPtr handle);
		}
		
		private IntPtr handle;
	}
}