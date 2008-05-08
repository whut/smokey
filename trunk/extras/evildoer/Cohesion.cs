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
	public class Cohesive
	{
		~Cohesive()
		{
			System.Diagnostics.Debug.WriteLine("hello");
		}
		
		Cohesive()
		{
		}
		
		public string Name
		{
			get {return name;}
		}
		
		public string Address
		{
			get {return address;}
		}
		
		public string Phone
		{
			get {return phone;}
		}
		
		public string Xxx
		{
			get {return xxx;}
		}
		
		public string Yyy
		{
			get {return yyy;}
		}
		
		public string ToString1()
		{
			return name + address + phone;
		}
		
		public int GetHashCode1()
		{
			int v;
			unchecked
			{
				v = name.GetHashCode() + phone.GetHashCode() + order.GetHashCode();
			}
			
			return v;
		}
		
		public bool Validate()
		{
			return name != null && order != null;
		}
		
		public void Reset()
		{
			name = "bob";
			address = "maple street";
			phone = "666-7777";
			order = "television";
			xxx = "television";
			yyy = "television";
		}
		
		private string name, address, phone, order, xxx, yyy;
	}

	// D1046/MaximizeCohesion
	public class HalfCohesive
	{
		public string ToString1()
		{
			return name + kind;
		}
		
		public int GetHashCode1()
		{
			int v;
			unchecked
			{
				v = type.GetHashCode() + kind.GetHashCode();
			}
			
			return v;
		}
		
		public int GetHashCode2()
		{
			int v;
			unchecked
			{
				v = name.GetHashCode() + type.GetHashCode();
			}
			
			return v;
		}
		
		public bool Validate()
		{
			return x != 0 && y != 0;
		}
		
		public void Reset()
		{
			x = 1;
			z = 3;
			System.Diagnostics.Debug.WriteLine(z);
		}
		
		public void Reset2()
		{
			y = 2;
			z = 4;
		}
		
		private int x, y, z;
		private string name = "hello", type = "integer", kind = "mass";
	}

	public class PoorlyCohesive
	{
		public string ToString1()
		{
			return name + kind;
		}
		
		public int GetHashCode1()
		{
			int v;
			unchecked
			{
				v = type.GetHashCode() + kind.GetHashCode();
			}
			
			return v;
		}
		
		public int GetHashCode2()
		{
			int v;
			unchecked
			{
				v = name.GetHashCode() + type.GetHashCode() + 1;
			}
			
			return v;
		}
		
		public bool Validate()
		{
			return x != 0 && y != 0;
		}
		
		public void Reset()
		{
			x = 1;
			z = 3;
		}
		
		public void Reset2()
		{
			y = 2;
			z = 4;
		}
		
		public string Stringify()
		{
			return x + " " + y + z + name + type + kind;
		}
		
		private int x, y, z;
		private string name = "hello", type = "integer", kind = "mass";
	}
}