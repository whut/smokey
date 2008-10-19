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
using System.Runtime.InteropServices;
using Smokey.Framework;
using Smokey.Framework.Support;

#if OLD
namespace Smokey.Internal.Rules
{
	internal static partial class NativeMethods
	{
		#region Config --------------------------------------------------------
		[DllImport("aspell")]
		public static extern IntPtr new_aspell_config();

		[DllImport("aspell")]
		public static extern void delete_aspell_config(AspellConfigHandle handle);

//		[DllImport("aspell")]
//		public static extern void aspell_config_replace(AspellConfigHandle handle, string key, string value);
		#endregion

		#region Can have error ------------------------------------------------
		[DllImport("aspell")]
		public static extern void delete_aspell_can_have_error(AspellCanHaveErrorHandle handle);

		[DllImport("aspell")]
		public static extern uint aspell_error_number(AspellCanHaveErrorHandle ths);

		[DllImport("aspell")]
		public static extern IntPtr aspell_error_message(AspellCanHaveErrorHandle ths);
		#endregion

		#region Speller -------------------------------------------------------
		[DllImport("aspell")]
		public static extern IntPtr new_aspell_speller(AspellConfigHandle handle);

		[DllImport("aspell")]
		public static extern IntPtr aspell_speller_error(AspellSpellerHandle ths);

		[DllImport("aspell")]
		public static extern IntPtr aspell_speller_error_message(AspellSpellerHandle ths);

		[DllImport("aspell")]
		public static extern IntPtr to_aspell_speller(AspellCanHaveErrorHandle ths);

		[DllImport("aspell")]
		public static extern int aspell_speller_check(AspellSpellerHandle ths, string word, int wordSize);
		
		[DllImport("aspell")]
		public static extern int aspell_speller_add_to_session(AspellSpellerHandle ths, string word, int wordSize);

		[DllImport("aspell")]
		public static extern void delete_aspell_speller(AspellSpellerHandle ths);
		#endregion
	} 
}	