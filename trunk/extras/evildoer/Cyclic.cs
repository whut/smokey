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
	public class Cyclic1
	{
		public Cyclic1(int a)
		{
			A = a;
		}
		
		public static void Foo(int a, Cyclic2 c)
		{
		}
		
		public static void Bar(int a, Cyclic3 c)
		{
		}
		
		public static void Bar(int a, Cyclic4 c)
		{
		}
		
		public readonly int A;
	}

	public class Cyclic2
	{
		public Cyclic2(int a)
		{
			foo = a;
		}
		
		public Cyclic1 Foo(int a)
		{
			return new Cyclic1(foo);
		}
		
		private int foo;
	}

	public class Cyclic3
	{
		public Cyclic3(int a)
		{
			foo = a;
		}
		
		public int Foo(int a)
		{
			Cyclic1 c = new Cyclic1(foo);
			return c.A;
		}
		
		private int foo;
	}

	public class Cyclic4
	{
		public int Foo()
		{
			return c.A;
		}
		
		private Cyclic1 c = new Cyclic1(10);
	}

	public abstract class Base1
	{
		public static void Foo(Derived c)
		{
		}
	}

	public class Base2 : Base1
	{
	}

	public class Derived : Base2
	{
	}

	public static class ACyclic1
	{
		public static void Foo(int a, Cyclic2 c)
		{
		}
	}
}
