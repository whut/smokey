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
using System.Runtime.InteropServices;

namespace EvilDoer
{
	// R1002/DisposeFields
	// S1003/KeepAlive
	// D1063/BaseDisposable (no Dispose(bool))
	// D1067/PreferSafeHandle
	public class BadDisposable : IDisposable
	{
		~BadDisposable()		
		{					
			DoDispose(false);
		}
	
		public BadDisposable(IntPtr handle)
		{
			this.handle = handle;
		}
	
		public override bool Equals(object rhsObj)
		{			
			if (rhsObj == null)		
				return false;
					
			if (GetType() != rhsObj.GetType())
				return false;
				
			BadDisposable rhs = (BadDisposable) rhsObj;			
			return disposed == rhs.disposed;
		}
		
		// C1021/RecursiveEquality
		public static bool operator==(BadDisposable lhs, BadDisposable rhs)
		{			
			if (lhs == null || rhs == null)
				return false;
			
			return lhs.disposed == rhs.disposed;
		}
				
		public static bool operator!=(BadDisposable lhs, BadDisposable rhs)
		{			
			return !(lhs == rhs);
		}
				
		public override int GetHashCode()
		{
			return disposed.GetHashCode();
		}
		
		public void Work()
		{				
			writer.WriteLine(handle);
		}

		public void MoreWork()
		{				
			writer.WriteLine(handle);
		}

		public void Dispose()
		{
			DoDispose(true);
		}
	
		// C1018/IgnoredReturn
		private void DoDispose(bool disposing)
		{
			if (!disposed)
			{
				NativeMethods.CloseHandle(handle);
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
		private StringWriter writer = new StringWriter();
	}
}