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
	public class EqualsCantCastTest : MethodTest
	{	
		#region Test classes
		private class GoodCases
		{
			public override bool Equals(object rhsObj)
			{				
				if (GetType() != rhsObj.GetType()) 
					return false;
				
				GoodCases rhs = (GoodCases) rhsObj;				
				return x == rhs.x && y == rhs.y;
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
				try
				{
					GoodCases2 rhs = (GoodCases2) rhsObj;				
					return x == rhs.x && y == rhs.y;
				}
				catch (Exception)
				{
					return false;
				}
			}
								
			public override int GetHashCode()
			{
				return x ^ y;
			}
			
			private int x, y;
		}
				
		private class GoodCases3
		{
			public override bool Equals(object rhsObj)
			{				
				if (rhsObj == null)	
					return false;
				
				GoodCases3 rhs = rhsObj as GoodCases3;
				if ((object) rhs == null)
					return false;
				
				return x == rhs.x && y == rhs.y;
			}
								
			public override int GetHashCode()
			{
				return x ^ y;
			}
			
			private int x, y;
		}
				
		private class GoodCases4
		{
			public override bool Equals (object obj)
			{
				if (obj is GoodCases4)
					return (GoodCases4) obj == this;
				else
					return false;
			}
								
			public override int GetHashCode()
			{
				return x ^ y;
			}
			
			private int x, y;
		}
				
		private class BadCases
		{
			public override bool Equals(object rhsObj)
			{				
				BadCases rhs = (BadCases) rhsObj;
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
		public EqualsCantCastTest() : base(
			new string[]{"GoodCases.Equals", "GoodCases2.Equals", "GoodCases3.Equals", "GoodCases4.Equals"},
			
			new string[]{"BadCases.Equals"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new EqualsCantCastRule1(cache, reporter);
		}
	} 
}
