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
	public struct GoodStruct : IEquatable<GoodStruct>
	{				
		public GoodStruct(int x, int y)
		{ 
			this.x = x;
			this.y = y;
		}
							
		public override bool Equals(object rhsObj)
		{
			if (rhsObj == null)						
				return false;
			
			if (GetType() != rhsObj.GetType()) 
				return false;
		
			GoodStruct rhs = (GoodStruct) rhsObj;					
			return x == rhs.x && y == rhs.y;
		}
			
		public bool Equals(GoodStruct rhs)	
		{					
			return x == rhs.x && y == rhs.y;
		}

		public static bool operator==(GoodStruct lhs, GoodStruct rhs)
		{
			return lhs.x == rhs.x && lhs.y == rhs.y;
		}
		
		public static bool operator!=(GoodStruct lhs, GoodStruct rhs)
		{
			return !(lhs == rhs);
		}
		
		public override int GetHashCode()
		{
			return x + y;
		}
		
		private int x, y;
	}
}