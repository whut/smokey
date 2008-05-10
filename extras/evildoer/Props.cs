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

using System.Collections.Generic;

namespace EvilDoer
{
	public class GoodProp1
	{
		public int X
		{
			get {return x;}
			set {x = value;}
		}
		
		private int x;
	}

	public class GoodProp2
	{
		public int X
		{
			get {return x;}
			set {x = value; y = value;}
		}
		
		public int Y
		{
			get {return y;}
			set {x = value; y = value;}
		}
		
		private int x;
		private int y;
	}

	public class GoodProp3
	{
		public int X
		{
			get {return x;}
			[DisableRule("C1022", "UseSetterValue")]
			set {}
		}
		
		private int x;
	}

	public class GoodProp4
	{
		public int X
		{
			get {return x;}
		}
		
		private int x;
	}

	public class GoodProp5
	{
		public int X
		{
			get {return x;}
			set {x = value;}
		}
		
		public int Y
		{
			[DisableRule("C1028", "InconsistentProperty")]
			get {return x;}
			set {y = value;}
		}
		
		public int Z
		{
			get {return y;}
		}
		
		private int x;
		private int y;
	}

	public class GoodProp6
	{
		public int X
		{
			get {if (z > 0) return x; else return y;}
			set {x = value; y = value;}
		}
		
		public int Y
		{
			get {return y;}
			set {y = value;}
		}
		
		public int Z
		{
			get {return z;}
			set {z = value;}
		}
		
		private int x;
		private int y;
		private int z;
	}

	public class BadProp1
	{
		public int X
		{
			get {return x;}
			set {x = value;}
		}
		
		// C1028/InconsistentProperty
		public int Y
		{
			get {return x;}
			set {y = value;}
		}
		
		public int Z
		{
			get {return y;}
		}
		
		private int x;
		private int y;
	}

	public class BadProp2
	{
		// C1028/InconsistentProperty
		public int X
		{
			get {if (z > 0) return x; else return z;}
			set {x = value; y = value;}
		}
		
		// C1028/InconsistentProperty
		public int Y
		{
			get {if (z > 0) return z; else return y;}
			set {x = value; y = value;}
		}
		
		public int Z
		{
			get {return z;}
			set {z = value;}
		}
		
		private int x;
		private int y;
		private int z;
	}
}