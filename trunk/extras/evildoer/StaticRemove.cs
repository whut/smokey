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
	// C1026/NoStaticRemove
	internal static class BadStatic1
	{				
		public static void Add(string s)
		{
			strings.Add(s);
		}
		
		private static List<string> strings = new List<string>();
	}

	// C1026/NoStaticRemove
	// P1007/NonGenericCollections
	internal static class BadStatic2
	{				
		public static void Add(string s)
		{
			strings.Add(s);
		}
		
		private static System.Collections.ArrayList strings = new System.Collections.ArrayList();
	}

	// C1026/NoStaticRemove
	internal static class BadStatic3
	{				
		public static void Add(string s)
		{
			strings.Add(s, 10);
		}
		
		private static Dictionary<string, int> strings = new Dictionary<string, int>();
	}

	internal class GoodStatic1
	{				
		public GoodStatic1()
		{
			prefix = "greeting";	// all adds are in a ctor
			strings.Add("hello");
			strings.Add("goodbye");
		}
		
		public string Get(int index)
		{
			return prefix + strings[index];
		}
		
		private string prefix;
		private static List<string> strings = new List<string>();
	}

	internal static class GoodStatic2
	{				
		public static void Add(string s)
		{
			strings.Add(s);
		}
		
		public static void Remove(int i)	// we do a remove
		{
			strings.RemoveAt(i);
		}
		
		public static string Get(int index)
		{
			return strings[index];
		}
		
		private static List<string> strings = new List<string>();
	}

	internal static class GoodStatic3		// we use weak refs
	{				
		public static void Add(string s)
		{
			strings.Add(new WeakReference(s));
		}
		
		private static List<WeakReference> strings = new List<WeakReference>();
	}

	internal static class GoodStatic4		// elem type is a value type
	{				
		public static void Add(int s)
		{
			strings.Add(s);
		}
		
		private static List<int> strings = new List<int>();
	}

	internal static class GoodStatic5		// table[key] = null special case
	{				
		public static void Add(string s)
		{
			strings.Add(s, "hello");
		}
		
		public static void Remove(string s)
		{
			strings[s] = null;
		}
		
		public static string Get(string s)
		{
			return strings[s];
		}
		
		private static Dictionary<string, string> strings = new Dictionary<string, string>();
	}
}