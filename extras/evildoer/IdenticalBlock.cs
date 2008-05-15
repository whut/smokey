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
using System.Text;
using System.Collections.Generic;

namespace EvilDoer
{
	public static class Branch
	{
		// D1049/IdenticalCodeBlocks
		[DisableRule("G1003", "FormatProvider")]
		public static void Alpha(int x, int y)
		{
			Console.Error.WriteLine(x);
			
			if (x > 0)
			{
				Console.Error.WriteLine("first: ", x.ToString());
				Console.Error.WriteLine("second: ", y.ToString());
				Console.Error.WriteLine("sum: ", (x + y).ToString());
				Console.Error.WriteLine("difference: ", (x - y).ToString());
				Console.Error.WriteLine("product: ", (x * y).ToString());
				Console.Error.WriteLine("quotient: ", (x / y).ToString());
				Console.Error.WriteLine("modulus: ", (x % y).ToString());
			}
			
			Console.Error.WriteLine("exit");
		}

		[DisableRule("G1003", "FormatProvider")]
		public static void Beta(int x, int y)
		{
			Console.Error.WriteLine("enter");
			
			if (x < 0)
			{
				Console.Error.WriteLine("first: ", x.ToString());
				Console.Error.WriteLine("second: ", y.ToString());
				Console.Error.WriteLine("sum: ", (x + y).ToString());
				Console.Error.WriteLine("difference: ", (x - y).ToString());
				Console.Error.WriteLine("product: ", (x * y).ToString());
				Console.Error.WriteLine("quotient: ", (x / y).ToString());
				Console.Error.WriteLine("modulus: ", (x % y).ToString());
			}
			
			System.Diagnostics.Debug.WriteLine("exit");
		}
	}

	public static class SameMethod
	{
		// D1049/IdenticalCodeBlocks
		[DisableRule("G1003", "FormatProvider")]
		public static void Gamma(int x, int y)
		{
			List<int> l = new List<int>();
			int j = 0;
			StringBuilder b = new StringBuilder();
			
			if (x > 0)
			{
				for (j = 0; j < l.Count; ++j)
				{					
					b.Append("list[");
					b.Append(j.ToString());
					b.Append("]  = ");
					b.Append(l[j].ToString());
					b.Append(l[j].ToString());
					b.Append(l[j].ToString());
					b.Append(l[j].ToString());
					b.Append(l[j].ToString());
					b.Append(l[j].ToString());
					
					Console.Error.WriteLine(b.ToString());
				}
			}
			
			if (x < 0)
			{
				for (j = 0; j < l.Count; ++j)
				{					
					b.Append("list[");
					b.Append(j.ToString());
					b.Append("]  = ");
					b.Append(l[j].ToString());
					b.Append(l[j].ToString());
					b.Append(l[j].ToString());
					b.Append(l[j].ToString());
					b.Append(l[j].ToString());
					b.Append(l[j].ToString());
					
					Console.Error.WriteLine(b.ToString());
				}
			}
			
			Console.Error.WriteLine("exit");
		}
	}

	public static class ReverseBranch
	{
		// D1049/IdenticalCodeBlocks
		[DisableRule("G1003", "FormatProvider")]
		public static void Alpha(int x, int y)
		{
			Console.Error.WriteLine(x);
			
			do
			{
				Console.Error.WriteLine("first: ", x.ToString());
				Console.Error.WriteLine("second: ", y.ToString());
				Console.Error.WriteLine("sum: ", (x + y).ToString());
				Console.Error.WriteLine("difference: ", (x - y).ToString());
				Console.Error.WriteLine("product: ", (x * y).ToString());
				Console.Error.WriteLine("quotient: ", (x / y).ToString());
				Console.Error.WriteLine("modulus: ", (x % y).ToString());
			}
			while (x > 0);
			
			Console.Error.WriteLine("exit");
		}

		[DisableRule("G1003", "FormatProvider")]
		public static void Beta(int x, int y)
		{
			Console.Error.WriteLine("enter");
			
			do
			{
				Console.Error.WriteLine("first: ", x.ToString());
				Console.Error.WriteLine("second: ", y.ToString());
				Console.Error.WriteLine("sum: ", (x + y).ToString());
				Console.Error.WriteLine("difference: ", (x - y).ToString());
				Console.Error.WriteLine("product: ", (x * y).ToString());
				Console.Error.WriteLine("quotient: ", (x / y).ToString());
				Console.Error.WriteLine("modulus: ", (x % y).ToString());
			}
			while (x < 0);
			
			System.Diagnostics.Debug.WriteLine("exit");
		}
	}

	public static class ExceptionHandlers
	{
		// D1049/IdenticalCodeBlocks
		[DisableRule("G1003", "FormatProvider")]
		public static void Alpha(int x, int y)
		{
			Console.Error.WriteLine(x);
			
			try
			{
				Console.Error.WriteLine("first case");
			}
			catch (ArgumentException)
			{
				Console.Error.WriteLine("first handler");
			}
			catch (Exception)
			{
				Console.Error.WriteLine("first: ", x.ToString());
				Console.Error.WriteLine("second: ", y.ToString());
				Console.Error.WriteLine("sum: ", (x + y).ToString());
				Console.Error.WriteLine("difference: ", (x - y).ToString());
				Console.Error.WriteLine("product: ", (x * y).ToString());
				Console.Error.WriteLine("quotient: ", (x / y).ToString());
				Console.Error.WriteLine("modulus: ", (x % y).ToString());
			}
			
			Console.Error.WriteLine("exit");
		}

		[DisableRule("G1003", "FormatProvider")]
		public static void Beta(int x, int y)
		{
			Console.Error.WriteLine("enter");
			
			try
			{
				Console.Error.WriteLine(y);
			}
			catch (ApplicationException)
			{
				Console.Error.WriteLine("first: ", x.ToString());
				Console.Error.WriteLine("second: ", y.ToString());
				Console.Error.WriteLine("sum: ", (x + y).ToString());
				Console.Error.WriteLine("difference: ", (x - y).ToString());
				Console.Error.WriteLine("product: ", (x * y).ToString());
				Console.Error.WriteLine("quotient: ", (x / y).ToString());
				Console.Error.WriteLine("modulus: ", (x % y).ToString());
			}
			catch (SystemException)
			{
				Console.Error.WriteLine(x);
			}
			
			System.Diagnostics.Debug.WriteLine("exit");
		}
	}
}
