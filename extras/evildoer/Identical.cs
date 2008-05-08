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
	public static class Identical1
	{
		public static void Print1(string s)
		{
			Console.Error.WriteLine(s);		// trivial methods are OK
		}
		
		public static void Bad(string s)
		{
			Console.Error.WriteLine("value: {0}", s);		
			Console.Error.WriteLine("sum: {0}, product: {1}", s + s, s.Substring(10));		
			Console.Error.WriteLine("length: {0}", s.Length);
			
			int sum = 0;
			foreach (char c in s)
				sum += (int) c;
			Console.Error.WriteLine("summation: {0}", sum);
		} 		
		
		public static void Normalize(string s)
		{
			Console.Error.WriteLine("value: {0}", s);		
			Console.Error.WriteLine("sum: {0}, product: {1}", s + s, s.Substring(10));		
			Console.Error.WriteLine("length: {0}", s.Length);
			
			int sum = 0;
			foreach (char c in s)
				sum += (int) c;
			Console.Error.WriteLine("summation: {0}", sum);

			Console.Error.WriteLine("value: {0}", value);
			Print1(s);
		} 		
		
		private static int value;
	}

	public static class Identical2
	{
		public static void Print1(string s)
		{
			Console.Error.WriteLine(s);		// trivial methods are OK
		}
		
		public static void MyBad(string s)
		{
			Console.Error.WriteLine("value: {0}", s);		
			Console.Error.WriteLine("sum: {0}, product: {1}", s + s, s.Substring(10));		
			Console.Error.WriteLine("length: {0}", s.Length);
			
			int sum = 0;
			foreach (char c in s)
				sum += (int) c;
			Console.Error.WriteLine("summation: {0}", sum);
		} 		

		public static void MyBad2(string ms)
		{
			Console.Error.WriteLine("value: {0}", ms);		
			Console.Error.WriteLine("sum: {0}, product: {1}", ms + ms, ms.Substring(10));		
			Console.Error.WriteLine("length: {0}", ms.Length);
			
			int sum = 0;
			foreach (char c in ms)
				sum += (int) c;
			Console.Error.WriteLine("summation: {0}", sum);
		} 		

		public static void MyBad3(string ms)
		{
			Console.Error.WriteLine("value: {0}", ms);		
			Console.Error.WriteLine("sum: {0}, product: {1}", ms + ms, ms.Substring(10));		
			Console.Error.WriteLine("length: {0}", ms.Length);
			
			int sumX = 0;
			foreach (char c in ms)
				sumX += (int) c;
			Console.Error.WriteLine("summation: {0}", sumX);
		} 		
		
		public static void Normalize(string s)
		{
			Console.Error.WriteLine("value: {0}", s);		
			Console.Error.WriteLine("sum: {0}, product: {1}", s + s, s.Substring(10));		
			Console.Error.WriteLine("length: {0}", s.Length);
			
			int sum = 0;
			foreach (char c in s)
				sum += (int) c;
			Console.Error.WriteLine("summation: {0}", sum);

			Console.Error.WriteLine("value: {0}", value);
			Print1(s);
		} 		
		
		private static int value;
	}
}
