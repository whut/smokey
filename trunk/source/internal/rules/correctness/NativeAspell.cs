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

namespace Smokey.Internal.Rules
{
	using AspellConfig = IntPtr;
	using AspellCanHaveError = IntPtr;
	using AspellSpeller = IntPtr;
	using AspellWordList = IntPtr;
	using AspellStringEnumeration = IntPtr;
	
	internal static class NativeMethods
	{
		#region Config --------------------------------------------------------
		[DllImport("aspell")]
		public static extern AspellConfig new_aspell_config();

//		[DllImport("aspell")]
//		public static extern void aspell_config_replace(AspellConfig config, string key, string value);
		#endregion

		#region Can have error
		[DllImport("aspell")]
		public static extern uint aspell_error_number(AspellCanHaveError ths);

		[DllImport("aspell")]
		public static extern IntPtr aspell_error_message(AspellCanHaveError ths);
		#endregion

		#region Speller
		[DllImport("aspell")]
		public static extern AspellCanHaveError new_aspell_speller(AspellConfig config);

		[DllImport("aspell")]
		public static extern AspellSpeller to_aspell_speller(AspellCanHaveError ths);

		[DllImport("aspell")]
		public static extern int aspell_speller_check(AspellSpeller ths, string word, int wordSize);

//		[DllImport("aspell")]
//		public static extern AspellWordList aspell_speller_suggest(AspellSpeller ths, string word, int wordSize);

		[DllImport("aspell")]
		public static extern void delete_aspell_speller(AspellSpeller ths);
		#endregion
		
		#region Word List -----------------------------------------------------
//		[DllImport("aspell")]
//		public static extern AspellStringEnumeration aspell_word_list_elements(AspellWordList words);
		#endregion

		#region String Enumeration --------------------------------------------
//		[DllImport("aspell")]
//		public static extern IntPtr aspell_string_enumeration_next(AspellStringEnumeration ase);

//		[DllImport("aspell")]
//		public static extern void delete_aspell_string_enumeration(AspellStringEnumeration ase);
		#endregion
	} 
}	