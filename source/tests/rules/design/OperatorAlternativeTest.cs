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
	public class OperatorAlternativeTest : TypeTest
	{	
		// test classes
		public class Good1
		{
			public Good1(int value)
			{
				m_value = value;
			}
			
			public static Good1 operator +(Good1 lhs, Good1 rhs)
			{
				return new Good1(lhs.m_value + rhs.m_value);
			}
			
			public Good1 Add(Good1 rhs)
			{
				return new Good1(m_value + rhs.m_value);
			}
			
			private int m_value;
		}
				
		internal sealed class Good2
		{
			public Good2(int value)
			{
				m_value = value;
			}
			
			public static Good2 operator +(Good2 lhs, Good2 rhs)
			{
				return new Good2(lhs.m_value + rhs.m_value);
			}
			
			private int m_value;
		}
								
		public class Bad1
		{
			public Bad1(int value)
			{
				m_value = value;
			}
			
			public static Bad1 operator +(Bad1 lhs, Bad1 rhs)
			{
				return new Bad1(lhs.m_value + rhs.m_value);
			}
			
			private int m_value;
		}
					
		// test code
		public OperatorAlternativeTest() : base(
			new string[]{"Good1", "Good2"},
			new string[]{"Bad1"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new OperatorAlternativeRule(cache, reporter);
		}
	} 
}
#endif
