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

#if OLD
namespace Smokey.Tests
{
	[TestFixture]
	public class AttributesNeedUsageTest : TypeTest
	{	
		// test classes
		[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
		public class GoodAttribute : Attribute
		{		
			public GoodAttribute(string address) 
			{
				m_address = address;
			}
			
			public string Address
			{
				get {return m_address;}
			}
		
			private readonly string m_address;
		}
		
		public abstract class GoodAttribute2 : Attribute
		{		
			public GoodAttribute2(string address) 
			{
				m_address = address;
			}
			
			public string Address
			{
				get {return m_address;}
			}
		
			private readonly string m_address;
		}
		
		public class BadAttribute : Attribute
		{		
			public BadAttribute(string address) 
			{
				m_address = address;
			}
			
			public string Address
			{
				get {return m_address;}
			}
		
			private readonly string m_address;
		}
		
		// test code
		public AttributesNeedUsageTest() : base(
			new string[]{"GoodAttribute", "GoodAttribute2"},
			new string[]{"BadAttribute"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new AttributesNeedUsageRule(cache, reporter);
		}
	} 
}
#endif
