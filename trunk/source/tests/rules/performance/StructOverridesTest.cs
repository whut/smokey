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
	public class StructOverridesTest : TypeTest
	{	
		// test classes		
		internal struct Good1
		{		
			public override bool Equals(object rhsObj)
			{
				if (rhsObj == null)						// objects may be null
					return false;
				
				if (GetType() != rhsObj.GetType()) 
					return false;
			
				Good1 rhs = (Good1) rhsObj;					
				return x == rhs.x && y == rhs.y;
			}
						
			public override int GetHashCode()
			{
				return x.GetHashCode() ^ y.GetHashCode();
			}
			
			private int x, y;
		}
			
		internal struct Bad2
		{		
			public override int GetHashCode()
			{
				return x.GetHashCode() ^ y.GetHashCode();
			}
			
			private int x, y;
		}
		
		internal struct Bad3
		{		
			public void Stuff()
			{
				Console.WriteLine("sum = {0}", x + y);
			}
			
			private int x, y;
		}
		
		// test code
		public StructOverridesTest() : base(
			new string[]{"Good1"},
			new string[]{"Bad2", "Bad3"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new StructOverridesRule(cache, reporter);
		}
	} 
}