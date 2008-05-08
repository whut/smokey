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

using Mono.Cecil;
using NUnit.Framework;
using System;
using System.Reflection;
using Smokey.Framework.Support;
using Smokey.Internal.Rules;

namespace Smokey.Tests
{
	[TestFixture]
	public class EqualsRequiresNullCheckTest : MethodTest
	{	
		#region Test classes
		private class GoodCases
		{
			public override bool Equals(object rhsObj)
			{
				if (rhsObj == null)			
					return false;
				
				GoodCases rhs = rhsObj as GoodCases;
				if ((object) rhs == null)	
					return false;
				
				return x == rhs.x && y == rhs.y;
			}
				
			public static bool operator==(GoodCases lhs, GoodCases rhs)
			{
				if (object.ReferenceEquals(lhs, rhs))
					return true;
				
				if ((object) lhs == null || (object) rhs == null)
					return false;
				
				return lhs.x == rhs.x && lhs.y == rhs.y;
			}
			
			public static bool operator!=(GoodCases lhs, GoodCases rhs)
			{
				return !(lhs == rhs);
			}
				
			public override int GetHashCode()
			{
				return x ^ y;
			}
			
			private int x, y;
		}
				
		private class GoodCases2
		{
			public override bool Equals(object rhsObj)
			{
				if (rhsObj == null)			
					return false;
				
				GoodCases2 rhs = rhsObj as GoodCases2;
				if ((object) rhs == null)	
					return false;
				
				return x == rhs.x && y == rhs.y;
			}
							
			public static bool operator==(GoodCases2 lhs, GoodCases2 rhs)
			{
				if (object.ReferenceEquals(lhs, rhs))
					return true;
				
				if ((object) lhs == null || (object) rhs == null)
					return false;
				
				return lhs.x == rhs.x && lhs.y == rhs.y;
			}
			
			public static bool operator!=(GoodCases2 lhs, GoodCases2 rhs)
			{
				return !(lhs == rhs);
			}
			
			public override int GetHashCode()
			{
				return x ^ y;
			}
			
			private int x, y;
		}
				
		private class GoodCases3
		{
			public override bool Equals(object obj)
			{
				if (!(obj is GoodCases3))
					return false;
	
				if (obj == this)
					return true;
	
				return ((GoodCases3) obj).Value == x;
			}
								
			
			public override int GetHashCode()
			{
				return x ^ y;
			}
			
			public int Value
			{
				get {return x;}
			}
			
			private int x, y;
		}
				
		private class GoodCases4
		{
			public override bool Equals(object obj)
			{
				if (obj is GoodCases4)
				{
					GoodCases4 rhs = (GoodCases4) obj;
					return rhs.Value == x;
				}
				
				return false;	
			}
								
			public override int GetHashCode()
			{
				return x ^ y;
			}
			
			public int Value
			{
				get {return x;}
			}
			
			private int x, y;
		}
								
		private class GoodCases5
		{
			public override bool Equals(object rparam) 
			{
				return (rparam == null) ? false : this.IsEqual(rparam.ToString());
			}
								
			public override int GetHashCode()
			{
				return x ^ y;
			}
			
			public bool IsEqual(string rhs) 
			{
				return x.ToString() == rhs;
			}
			
			private int x, y;
		}
								
		private class BadCases
		{
			public override bool Equals(object rhsObj)
			{				
				BadCases rhs = rhsObj as BadCases;
				return x == rhs.x && y == rhs.y;
			}
				
			public static bool operator==(BadCases lhs, BadCases rhs)
			{
				return lhs.x == rhs.x && lhs.y == rhs.y;
			}
			
			public static bool operator!=(BadCases lhs, BadCases rhs)
			{
				return !(lhs == rhs);
			}
				
			public override int GetHashCode()
			{
				return x ^ y;
			}
						
			private int x, y;
		}
				
		private class BadCases2
		{
			public bool Equals(BadCases2 rhs)
			{				
				return x == rhs.x && y == rhs.y;
			}
				
			public override int GetHashCode()
			{
				return x ^ y;
			}
						
			private int x, y;
		}
		#endregion
		
		// test code
		public EqualsRequiresNullCheckTest() : base(
			new string[]{"GoodCases.Equals", "GoodCases.op_Equality", "GoodCases2.Equals", 
			"GoodCases3.Equals", "GoodCases4.Equals", "GoodCases5.Equals"},
			
			new string[]{"BadCases.Equals", "BadCases.op_Equality", "BadCases2.Equals"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new EqualsRequiresNullCheckRule(cache, reporter);
		}
	} 
}
