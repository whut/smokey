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
using System.Collections.Generic;
using System.IO;
using System.Security;

// C1000/StringSpelling
[assembly: EvilDoer.Bad("here is a missspelled word")]

namespace EvilDoer
{
	// MS1002/CustomException
	// D1015/ExceptionConstructors
	// D1024/SerializeException
	// D1025/SerializeExceptionMembers
	public class BadException : ApplicationException
	{
		public BadException(string mesg, string key) : base(mesg)
		{
			this.key = key;
		}
		
		public string Key {get {return key;}}
		
		private string key;
	}			
	
	// P1007/NonGenericCollections
	// R1000/DisposableFields
	// S1007/UnmanagedCodeSecurity
	[SuppressUnmanagedCodeSecurity]
	public class BadClass
	{			
		// D1001/ClassCanBeMadeStatic
		public class Nested
		{
			// PO1000/NewLineLiteral
			public static void BadLiteral()
			{
				Console.Error.WriteLine("hello\nworld");
			}
		}
		
		// D1050/UnusedField
		internal int unusedfield1;
		internal string unusedfield2 = "hello";
		
		// P1000/EmptyFinalizer
		~BadClass()
		{
		}
		
		// MS1003/DontDestroyStackTrace
		public static void TrashStackTrace()
		{
			try
			{
				Console.Error.WriteLine("Run code");
			}
			catch (ApplicationException e)
			{
				Console.Error.WriteLine(e.Message);
				throw e;	
			}
		}
		
		public static int GoodMeet2(int x)
		{
			string s = null;
			
			if (x > 0)
				s = "foo";
				
			return s.GetHashCode();
		}
		
		public static int GoodMeet(int x)
		{
			string s = null;
			int r = 0;
			
			if (x > 0)
			{
				s = "foo";
				r = s.GetHashCode();
			}
			
			return r;
		}
				
		// PO1000/NewLineLiteral
		public static void UnixNewLine()
		{
			Console.Error.WriteLine("hello\nworld");
		}

		// PO1000/NewLineLiteral
		public static void WindowsNewLine()
		{
			Console.Error.WriteLine("hello\r\nworld");
		}

		// PO1000/NewLineLiteral
		public void MacNewLine()
		{
			writer.WriteLine("hello\rworld");
		}

		// P1002/UseStringEmpty
		public static void WriteBlankLine()
		{
			Console.Error.WriteLine("");
		}

		public int Square(int x)
		{
			return x * x;
		}
		
		// R1004/DisposeScopedObjects
		public static int ReadAll1(string path, int x)
		{				
			StreamReader stream = new StreamReader(path);
			char[] buffer = new char[10];
			int count = stream.Read(buffer, x, 10 - x);
			
			return count;
		}

		// P1003/AvoidBoxing
		public void Boxing(int n)
		{
			object o = n;
			Console.Error.WriteLine("length: {0}", o);			// 4 boxes are bad
			Console.Error.WriteLine("length: {0}", n);			
			Console.Error.WriteLine("length: {0}", n);			
			Console.Error.WriteLine("length: {0}", ints.Add(n));		
			
			text2 = "hey";
		}

		// P1004/AvoidUnboxing
		// C1000/StringSpelling
		public static void Unboxing(object o)
		{
			int x = (int) o;							// 2 unboxes are bad
			float y = (float) o;
			Console.Error.WriteLine("eep: {0}", x + y);	
		}

		// C1001/InfiniteRecursion
		public override int GetHashCode()
		{
			DoOut(out text3);
			text4 = null;
			return GetHashCode();
		}
		
		// MS1020/EqualsRequiresNullCheck1
		// MS1004/EqualsCantCast
		public override bool Equals(object rhsObj)
		{			
			BadClass rhs = (BadClass) rhsObj;				
			return x == rhs.x;
		}
			
		// MS1020/EqualsRequiresNullCheck1
		public bool Equals(BadClass rhs)
		{			
			return x == rhs.x;
		}

		// MS1011/DontSwallowException
		public static void Swallows()
		{
			try
			{
				Console.Error.WriteLine("Run code");
			}
			catch (Exception)
			{
			}
		}

		// MS1020/EqualsRequiresNullCheck1
		public static bool operator==(BadClass lhs, BadClass rhs)
		{
			return lhs.x == rhs.x;
		}
		
		public static bool operator!=(BadClass lhs, BadClass rhs)
		{
			return !(lhs == rhs);
		}
		
		// C1002/MalformedFormatString
		public static string SkippedFormat(int x, int y)
		{
			return string.Format("x = {1}, y = {2}", x, y);
		}

		// P1005/StringConcat
		// MS1012/ThrowDerived
		public static void Loop(string x, string y, int count)
		{
			if (count < 0)
				throw new Exception("count is negative");
				
			string r = x;
			
			for (int i = 0; i < count; ++i)
				r += y;
				
			Console.Error.WriteLine(r);
		}
		
		// D1002/MethodTooComplex
		// D1049/IdenticalCodeBlocks
		public void Complex(int x, int y)
		{
			if (x + y > 100)
				Console.Error.WriteLine("100");
			else if (x + y > 200)
				Console.Error.WriteLine("200");
			else if (x + y > 300)
				Console.Error.WriteLine("300");
			else if (x + y > 400)
				Console.Error.WriteLine("400");
			else if (x + y > 500)
				Console.Error.WriteLine("500");
			else if (x + y > 600)
				Console.Error.WriteLine("600");
			else if (x + y > 700)
				Console.Error.WriteLine("700");
			else if (x + y > 800)
				Console.Error.WriteLine("800");
			else if (x + y > 900)
				Console.Error.WriteLine("900");
			else if (x + y > 1000)
				Console.Error.WriteLine("1000");

			if (x - y > 100)
				Console.Error.WriteLine("100");
			else if (x - y > 200)
				Console.Error.WriteLine("200");
			else if (x - y > 300)
				Console.Error.WriteLine("300");
			else if (x - y > 400)
				Console.Error.WriteLine("400");
			else if (x - y > 500)
				Console.Error.WriteLine("500");
			else if (x - y > 600)
				Console.Error.WriteLine("600");
			else if (x - y > 700)
				Console.Error.WriteLine("700");
			else if (x - y > 800)
				Console.Error.WriteLine("800");
			else if (x - y > 900)
				Console.Error.WriteLine("900");
			else if (x - y > 1000)
				Console.Error.WriteLine("1000");

			if (x - y > 100)
				Console.Error.WriteLine("100");
			else if (x - y > 200)
				Console.Error.WriteLine("200");
			else if (x - y > 300)
				Console.Error.WriteLine("300");
			else if (x - y > 400)
				Console.Error.WriteLine("400");
			else if (x - y > 500)
				Console.Error.WriteLine("500");
			else if (x - y > 600)
				Console.Error.WriteLine("600");
			else if (x - y > 700)
				Console.Error.WriteLine("700");
			else if (x - y > 800)
				Console.Error.WriteLine("800");
			else if (x - y > 900)
				Console.Error.WriteLine("900");
			else if (x - y > 1000)
				Console.Error.WriteLine("1000");

			if (x * y > 100)
				Console.Error.WriteLine("100");
				
			Console.Error.WriteLine(text1);
			Console.Error.WriteLine(text2);
			Console.Error.WriteLine(text3);
			Console.Error.WriteLine(text4);
		}
		
		private static void DoOut(out string s)
		{
			s = "hello";
		}

		private int x;
		private ArrayList ints = new ArrayList();
		private StringWriter writer = new StringWriter();
		private string text1;		// this one is bad
		private string text2;
		private string text3;
		private string text4;		// this is bad too
	}
}