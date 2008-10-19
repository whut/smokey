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

using Smokey.Framework;

using System;
using System.Runtime.InteropServices;

#if OLD
namespace Smokey.Internal.Rules
{
	internal sealed class AspellConfigHandle : SafeHandle
	{
//		public AspellConfigHandle() : base(IntPtr.Zero, true)
//		{
//		}
		
		public static AspellConfigHandle Default = new AspellConfigHandle(1);
		
		public override bool IsInvalid 
		{ 
			get {return handle == IntPtr.Zero;}
		}
	
		protected override bool ReleaseHandle()
		{
			NativeMethods.delete_aspell_config(this);
			
			return true;
		}
		
		private AspellConfigHandle(int dummy) : base(IntPtr.Zero, true)
		{
			Unused.Value = dummy;

			handle = NativeMethods.new_aspell_config();
		}
	}

	internal sealed class AspellCanHaveErrorHandle : SafeHandle
	{
//		public AspellCanHaveErrorHandle() : base(IntPtr.Zero, true)
//		{
//		}
				
		public AspellCanHaveErrorHandle(AspellConfigHandle config) : base(IntPtr.Zero, true)
		{
			handle = NativeMethods.new_aspell_speller(config);
		}
						
		public override bool IsInvalid 
		{ 
			get {return handle == IntPtr.Zero;}
		}
		
		protected override bool ReleaseHandle()
		{
			NativeMethods.delete_aspell_can_have_error(this);
			
			return true;
		}
	}

	internal sealed class AspellSpellerHandle : SafeHandle
	{
//		public AspellSpellerHandle() : base(IntPtr.Zero, true)
//		{
//		}
				
		public AspellSpellerHandle(AspellCanHaveErrorHandle err) : base(IntPtr.Zero, true)
		{
			handle = NativeMethods.to_aspell_speller(err);
			err.SetHandleAsInvalid();			// to_aspell_speller is a cast so we have to reset err to avoid a double delete...
		}
						
		public override bool IsInvalid 
		{ 
			get {return handle == IntPtr.Zero;}
		}
		
		protected override bool ReleaseHandle()
		{
			NativeMethods.delete_aspell_speller(this);
			
			return true;
		}
	}
}
