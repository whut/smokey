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

using NUnit.Framework;
using System;
using System.Collections.Generic;
using Smokey.Framework.Support;
using Smokey.Internal.Rules;

#if OLD
namespace Smokey.Tests
{
	[TestFixture]
	public class UseIEquatableTest : TypeTest
	{	
		#region Test cases
		private struct Good1 : IEquatable<Good1>
		{		
			public override bool Equals(object rhsObj)
			{
				if (rhsObj == null)						
					return false;
				
				if (GetType() != rhsObj.GetType()) 
					return false;
			
				Good1 rhs = (Good1) rhsObj;					
				return this == rhs;
			}
				
			public bool Equals(Good1 rhs)
			{					
				return this == rhs;
			}

			public static bool operator==(Good1 lhs, Good1 rhs)
			{
				return lhs.x == rhs.x && lhs.y == rhs.y;
			}
			
			public static bool operator!=(Good1 lhs, Good1 rhs)
			{
				return !(lhs == rhs);
			}
			
			public override int GetHashCode()
			{
				int hash;
				
				unchecked
				{
					hash = x.GetHashCode() + y.GetHashCode();
				}
				
				return hash;
			}
			
			private int x, y;
		}

		private class Good2
		{		
			public bool Equals(int rhs)
			{					
				return false;
			}
		}

		private struct Good3
		{		
			public override bool Equals(object rhsObj)
			{
				if (rhsObj == null)						
					return false;
				
				if (GetType() != rhsObj.GetType()) 
					return false;
			
				Good3 rhs = (Good3) rhsObj;					
				return x == rhs.x && y == rhs.y;
			}
							
			public override int GetHashCode()
			{
				int hash;
				
				unchecked
				{
					hash = x.GetHashCode() + y.GetHashCode();
				}
				
				return hash;
			}
			
			private int x, y;
		}

		private struct Bad1 
		{		
			public override bool Equals(object rhsObj)
			{
				if (rhsObj == null)						
					return false;
				
				if (GetType() != rhsObj.GetType()) 
					return false;
			
				Bad1 rhs = (Bad1) rhsObj;					
				return this == rhs;
			}
				
			public bool Equals(Bad1 rhs)
			{					
				return this == rhs;
			}

			public static bool operator==(Bad1 lhs, Bad1 rhs)
			{
				return lhs.x == rhs.x && lhs.y == rhs.y;
			}
			
			public static bool operator!=(Bad1 lhs, Bad1 rhs)
			{
				return !(lhs == rhs);
			}
			
			public override int GetHashCode()
			{
				int hash;
				
				unchecked
				{
					hash = x.GetHashCode() + y.GetHashCode();
				}
				
				return hash;
			}
			
			private int x, y;
		}
		#endregion
		
		// test code
		public UseIEquatableTest() : base(
			new string[]{"Good1", "Good2", "Good3"},
			new string[]{"Bad1"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new UseIEquatableRule(cache, reporter);
		}
	} 
}
#endif
