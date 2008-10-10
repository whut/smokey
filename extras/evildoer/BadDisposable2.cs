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
using System.IO;

namespace EvilDoer
{
	// MS1013/DisposableButNoFinalizer
	public class BadDisposable2 : IDisposable
	{	
		public BadDisposable2(IntPtr handle)
		{
			this.handle = handle;
		}
	
		// R1036/ObjectDisposedException
		public void Work()
		{				
			if (disposed)		
				throw new ObjectDisposedException("object is disposed!");
				
			System.Diagnostics.Debug.WriteLine(handle);
		}

		public void Dispose()
		{
			DoDispose(true);
			GC.SuppressFinalize(this);
		}
	
		private void DoDispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
					Ignore.Value = NativeMethods.CloseHandle(handle);
				
				disposed = true;
			}
		}
		
		private static class NativeMethods
		{
			[System.Runtime.InteropServices.DllImport("Kernel32")]
			public extern static bool CloseHandle(IntPtr handle);
		}
		
		private IntPtr handle;
		private bool disposed = false;
	}
}