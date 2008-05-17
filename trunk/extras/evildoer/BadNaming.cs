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

namespace EvilDoer
{
	// D1001/ClassCanBeMadeStatic
	// P1012/NotInstantiated
	internal class BadNaming
	{				
		// D1032/UnusedMethod
		public BadNaming()
		{
		}
		
		// D1032/UnusedMethod
		// MO1000/UseMonoNaming
		public static void BadParam(int not_cool)
		{
			System.Diagnostics.Debug.WriteLine("hello" + not_cool);
		}

		// MO1000/UseMonoNaming
		// D1032/UnusedMethod
		public static void camelCase(int cool)
		{
			System.Diagnostics.Debug.WriteLine("hello" + cool);
		}
	}

	// MO1000/UseMonoNaming
	internal class InconsistentName1
	{				
		protected int s_field;
	}

	// P1020/NotSealed
	internal class InconsistentName2 : InconsistentName1
	{				
		protected int ms_field;
	}

	internal sealed class InconsistentName3 : InconsistentName1
	{				
	}
}