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
using System.Collections;

namespace EvilDoer
{
	public static class GoodStringUri1
	{
		[DisableRule("D1048", "GuiUsesConsole")]
		public static void Foo(string s)
		{
			Console.WriteLine("hello");
			Foo(new Uri(s));
			Console.WriteLine("goodbye");
		}

		[DisableRule("D1048", "GuiUsesConsole")]
		public static void Foo(Uri u)
		{
			Console.WriteLine("work");
		}
	}

	public static class GoodStringUri2
	{
		[DisableRule("D1048", "GuiUsesConsole")]
		public static void Foo(int arg, string s)
		{
			Console.WriteLine("hello");
			Foo(arg, new Uri(s));
			Console.WriteLine("goodbye");
		}

		[DisableRule("D1048", "GuiUsesConsole")]
		public static void Foo(int arg, Uri u)
		{
			Console.WriteLine("work");
		}
	}

	public static class GoodStringUri3
	{
		[DisableRule("D1048", "GuiUsesConsole")]
		public static void Foo(int arg, string s)
		{
			Console.WriteLine("hello");
			Console.WriteLine("goodbye");
		}

		[DisableRule("D1048", "GuiUsesConsole")]
		public static void Bar(int arg, Uri u)
		{
			Console.WriteLine("work");
		}
	}
	
	public static class GoodStringUri4
	{
		[DisableRule("D1048", "GuiUsesConsole")]
		public static void Foo(int arg, float s)
		{
			Console.WriteLine("hello");
			Console.WriteLine("goodbye");
		}

		[DisableRule("D1048", "GuiUsesConsole")]
		public static void Foo(int arg, Uri u)
		{
			Console.WriteLine("work");
		}
	}
	
	public class GoodStringUri5
	{
		[DisableRule("D1048", "GuiUsesConsole")]
		public void Foo(int arg, string s)
		{
			value = arg;
			
			Console.WriteLine("hello");
			Foo(arg, new Uri(s));
			Console.WriteLine("goodbye");
		}

		[DisableRule("D1048", "GuiUsesConsole")]
		public void Foo(int arg, Uri u)
		{
			Console.WriteLine(value + arg);
		}
		
		private int value = 10;
	}

	// R1030/StringUriOverload
	public static class BadStringUri1
	{
		[DisableRule("D1048", "GuiUsesConsole")]
		public static void Foo(string s)
		{
			Console.WriteLine("hello");
			Console.WriteLine("goodbye");
		}

		[DisableRule("D1048", "GuiUsesConsole")]
		public static void Foo(Uri u)
		{
			Console.WriteLine("work");
		}
	}

	// R1030/StringUriOverload
	public static class BadStringUri2
	{
		[DisableRule("D1048", "GuiUsesConsole")]
		public static void Foo(int arg, string s)
		{
			Console.WriteLine("hello");
			Console.WriteLine("goodbye");
		}

		[DisableRule("D1048", "GuiUsesConsole")]
		public static void Foo(int arg, Uri u)
		{
			Console.WriteLine("work");
		}
	}

	// R1030/StringUriOverload
	public static class BadStringUri3
	{
		[DisableRule("D1048", "GuiUsesConsole")]
		public static void Foo(string s, int arg)
		{
			Console.WriteLine("hello");
			Console.WriteLine("goodbye");
		}

		[DisableRule("D1048", "GuiUsesConsole")]
		public static void Foo(Uri u, int arg)
		{
			Console.WriteLine("work");
		}
	}
}
