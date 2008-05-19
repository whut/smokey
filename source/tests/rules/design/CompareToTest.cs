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

namespace Smokey.Tests
{
	[TestFixture]
	public class CompareToTest : TypeTest
	{	
		// test classes
		internal sealed class Good1 : IComparable<Good1>
		{		
			public int CompareTo(Good1 rhs)	
			{					
				int result = m_name.CompareTo(rhs.m_name);
				if (result == 0)
					result = m_address.CompareTo(rhs.m_address);
					
				return result;
			}

			public override bool Equals(object rhsObj)
			{
				if (rhsObj == null)		
					return false;
				
				Good1 rhs = rhsObj as Good1;
				return this == rhs;
			}
										
			public override int GetHashCode()
			{
				return m_name.GetHashCode() ^ m_address.GetHashCode();
			}
			
			private string m_name = "ted";
			private string m_address = "main street";
		}
								
		internal sealed class Good2
		{		
			public int CompareTo(Good2 rhs)	
			{					
				int result = m_name.CompareTo(rhs.m_name);
				if (result == 0)
					result = m_address.CompareTo(rhs.m_address);
					
				return result;
			}
		
			private string m_name = "ted";
			private string m_address = "main street";
		}
								
		internal sealed class Bad1 : IComparable<Bad1>
		{		
			public int CompareTo(Bad1 rhs)	
			{					
				int result = m_name.CompareTo(rhs.m_name);
				if (result == 0)
					result = m_address.CompareTo(rhs.m_address);
					
				return result;
			}
			
			private string m_name = "ted";
			private string m_address = "main street";
		}
								
		// test code
		public CompareToTest() : base(
			new string[]{"Good1", "Good2"},
			new string[]{"Bad1"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new CompareToRule(cache, reporter);
		}
	} 
}