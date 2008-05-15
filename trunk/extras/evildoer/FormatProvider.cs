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
using System.Diagnostics;

namespace EvilDoer
{
	public class BaseStringer
	{
		public string ToString(string format, IFormatProvider provider)
		{
			f = format;
			return f + f;
		}
		
		private string f;
	}
	
	public class DerivedStringer : BaseStringer
	{
	}
	
	public static class FormatProviderCases
	{
		public static object Good1(object x, IFormatProvider p)
		{
			Debug.Assert(x != null);
			Debug.Assert(p != null);
			
			return Convert.ChangeType(x, p.GetType(), p);
		}

		public static int Good2(string x, IFormatProvider p)
		{
			Debug.Assert(x != null);
			Debug.Assert(p != null);

			return Convert.ToInt32(x, p);
		}

		public static string Good3(object x, IFormatProvider p)
		{
			Debug.Assert(x != null);
			Debug.Assert(p != null);

			return string.Format(p, "{0}", x);
		}

		public static int Good4(string s, IFormatProvider p)
		{
			Debug.Assert(s != null);
			Debug.Assert(p != null);

			return int.Parse(s, p);
		}

		[DisableRule("D1007", "UseBaseTypes")]
		public static string Good5(DerivedStringer x, IFormatProvider p)
		{
			Debug.Assert(x != null);
			Debug.Assert(p != null);

			return x.ToString("{0}", p);
		}

		[DisableRule("D1007", "UseBaseTypes")]
		public static string Good6(Exception x)
		{
			Debug.Assert(x != null);

			return x.ToString();
		}

		// G1003/FormatProvider
		public static object Bad1(object x)
		{
			Debug.Assert(x != null);

			return Convert.ChangeType(x, x.GetType());
		}

		// G1003/FormatProvider
		public static int Bad2(string x)
		{
			Debug.Assert(x != null);

			return Convert.ToInt32(x);
		}

		// G1003/FormatProvider
		public static string Bad3(object x)
		{
			Debug.Assert(x != null);

			return string.Format("{0}", x);
		}

		// G1003/FormatProvider
		public static int Bad4(string s)
		{
			Debug.Assert(s != null);

			return int.Parse(s);
		}

		// D1007/UseBaseTypes
		// G1003/FormatProvider
		public static string Bad5(DerivedStringer x)
		{
			Debug.Assert(x != null);

			return x.ToString();
		}
	}
}
