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
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Smokey.Framework.Support;
using Smokey.Internal.Rules;

#if OLD
namespace Smokey.Tests
{
	[TestFixture]
	public class HashUsesXorTest : MethodTest
	{			
		// test classes
		internal struct Good1		
		{		
			public Good1(int x, int y)
			{
				m_x = x;
				m_y = y;
			}
			
			public static Good1 operator+(Good1 lhs, Good1 rhs)
			{
				return new Good1(lhs.m_x + rhs.m_x, lhs.m_y + rhs.m_y);
			}

			public static Good1 operator-(Good1 lhs, Good1 rhs)
			{
				return new Good1(lhs.m_x - rhs.m_x, lhs.m_y - rhs.m_y);
			}
  
			public override bool Equals(object rhsObj)
			{
				return true;
			}
								
			public override int GetHashCode()
			{
				int hash;
				
				unchecked
				{
					hash = m_x.GetHashCode() + m_y.GetHashCode();
				}
				
				return hash;
			}
			
			public int Xor()
			{
				int hash = 23;
				
				unchecked
				{
					hash = hash*37 + m_x.GetHashCode();
					hash = hash*37 + m_y.GetHashCode();
				}
				
				return hash;
			}

			private int m_x, m_y;
		}
								
		internal struct Good2		
		{		
			public Good2(int x, int y)
			{
				m_x = x;
				m_y = y;
			}
			
			public static Good2 operator+(Good2 lhs, Good2 rhs)
			{
				return new Good2(lhs.m_x + rhs.m_x, lhs.m_y + rhs.m_y);
			}
			
			public override bool Equals(object rhsObj)
			{
				return true;
			}
								
			public override int GetHashCode()
			{
				int hash = 23;
				
				unchecked
				{
					hash = hash*37 + m_x.GetHashCode();
					hash = hash*37 + m_y.GetHashCode();
				}
				
				return hash;
			}
			
			private int m_x, m_y;
		}
																
		internal struct Bad1		
		{		
			public Bad1(int x, int y)
			{
				m_x = x;
				m_y = y;
			}
			
			public static Bad1 operator+(Bad1 lhs, Bad1 rhs)
			{
				return new Bad1(lhs.m_x + rhs.m_x, lhs.m_y + rhs.m_y);
			}

			public static Bad1 operator-(Bad1 lhs, Bad1 rhs)
			{
				return new Bad1(lhs.m_x - rhs.m_x, lhs.m_y - rhs.m_y);
			}
  			
			public override bool Equals(object rhsObj)
			{
				return true;
			}
								
			public override int GetHashCode()
			{
				int hash = 23;
				
				unchecked
				{
					hash = m_x.GetHashCode() ^ m_y.GetHashCode();
				}
				
				return hash;
			}

			private int m_x, m_y;
		}
								
		// test code
		public HashUsesXorTest() : base(
			new string[]{"Good1.GetHashCode", "Good2.GetHashCode", "Good1.Xor"},
			new string[]{"Bad1.GetHashCode"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new HashUsesXorRule(cache, reporter);
		}
	} 
}
#endif
