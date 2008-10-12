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
using System.Reflection;
using Smokey.Framework.Support;
using Smokey.Internal.Rules;

namespace Smokey.Tests
{
	[TestFixture]
	public class ConstantResultTest : MethodTest
	{	
		#region Test classes
		private class Good
		{
			public override bool Equals(object rhs)
			{	
				return GetType() == rhs.GetType();
			}

			public override int GetHashCode()
			{	
				return GetType().GetHashCode();
			}
			
			public int CompareTo(Good rhs)
			{
				return GetType().ToString().CompareTo(rhs.GetType().ToString());
			}
               
			public static bool operator==(Good lhs, Good rhs)
			{
				return lhs.ToString() == rhs.ToString();
			}
			
			public static bool operator!=(Good lhs, Good rhs)
			{
				return lhs.ToString() != rhs.ToString();
			}
			
			public static bool operator>(Good lhs, Good rhs)
			{
				return lhs.ToString().GetHashCode() > rhs.ToString().GetHashCode();
			}
			
			public static bool operator<(Good lhs, Good rhs)
			{
				return lhs.ToString().GetHashCode() < rhs.ToString().GetHashCode();
			}
			
			public static bool operator>=(Good lhs, Good rhs)
			{
				return lhs.ToString().GetHashCode() >= rhs.ToString().GetHashCode();
			}
			
			public static bool operator<=(Good lhs, Good rhs)
			{
				return lhs.ToString().GetHashCode() <= rhs.ToString().GetHashCode();
			}
		}

		private class Bad
		{
			public override bool Equals(object rhs)
			{	
				return false;
			}

			public override int GetHashCode()
			{	
				return 0;
			}
			
			public int CompareTo(Bad rhs)
			{
				return 100;
			}
               
			public static bool operator==(Bad lhs, Bad rhs)
			{
				return true;
			}
			
			public static bool operator!=(Bad lhs, Bad rhs)
			{
				return false;
			}
			
			public static bool operator>(Bad lhs, Bad rhs)
			{
				return false;
			}
			
			public static bool operator<(Bad lhs, Bad rhs)
			{
				return false;
			}
			
			public static bool operator>=(Bad lhs, Bad rhs)
			{
				return false;
			}
			
			public static bool operator<=(Bad lhs, Bad rhs)
			{
				return false;
			}
		}
		#endregion
		
		// test code
		public ConstantResultTest() : base(
			new string[]{"Good.Equals", "Good.GetHashCode", "Good.CompareTo", "Good.op_Equality", "Good.op_Inequality", "Good.op_GreaterThanOrEqual", "Good.op_LessThan", "Good.op_LessThanOrEqual"},
			new string[]{"Bad.Equals", "Bad.GetHashCode", "Bad.CompareTo", "Bad.op_Equality", "Bad.op_Inequality", "Bad.op_GreaterThanOrEqual", "Bad.op_LessThan", "Bad.op_LessThanOrEqual"})
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new ConstantResultRule(cache, reporter);
		}
	} 
}
