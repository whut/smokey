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
	public class AttributePropertiesTest : TypeTest
	{	
		public delegate void Handler(object o, EventArgs e);

		// test classes
		[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
		internal sealed class Good1 : Attribute
		{		
			public Good1(string name) 
			{
				m_name = name;
			}
			
			public string Name
			{
				get {return m_name;}
			}
		
			public string Vendor
			{
				get {return m_vendor;}
				set {m_vendor = value;}
			}
		
			private string m_name;
			private string m_vendor;
		}
		
		[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
		internal sealed class Good2 : Attribute
		{		
			public Good2(string name) 
			{
				m_name = name;
			}
			
			public Good2(string name, string vendor) 
			{
				m_name = name;
				m_vendor = vendor;
			}
			
			public string Name
			{
				get {return m_name;}
			}
		
			public string Vendor
			{
				get {return m_vendor;}
			}
		
			private string m_name;
			private string m_vendor;
		}
		
		[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
		internal sealed class Good3 : Attribute
		{					
			public string Name
			{
				get {return m_name;}
				set {m_name = value;}
			}
		
			public string Vendor
			{
				get {return m_vendor;}
				set {m_vendor = value;}
			}
		
			private string m_name;
			private string m_vendor;
		}
		
		[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
		internal sealed class Good4 : Attribute
		{					
			public static readonly string Foo = "hey";
			
			public string Name
			{
				get {return m_name;}
				set {m_name = value;}
			}
				
			private string m_name;
		}
		
		[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
		internal sealed class Bad1 : Attribute
		{		
			public Bad1(string name) 
			{
				m_name = name;
			}
						
			public string Name
			{
				get {return m_name;}
			}
		
			public string Vendor		// need a setter here for optional
			{
				get {return m_vendor;}	
			}
		
			private string m_name;
			private string m_vendor;
		}
		
		[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
		internal sealed class Bad2 : Attribute
		{		
			public Bad2(string name) 
			{
				m_name = name;
			}
						
			public string Name			// need a getter for required
			{ 
				set {m_name = value;}
			}
		
			public string Vendor		
			{
				get {return m_vendor;}	
				set {m_vendor = value;}
			}
		
			protected string m_name;
			private string m_vendor;
		}
		
		[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
		internal sealed class Bad3 : Attribute
		{		
			public Bad3(string name) 
			{
				m_name = name;			// no getter for required
			}
								
			public string Vendor		
			{
				get {return m_vendor;}	
				set {m_vendor = value;}
			}
		
			protected string m_name;
			private string m_vendor;
		}
		
		[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
		internal sealed class Bad4 : Attribute
		{		
			public Bad4(string name) 
			{
				m_name = name;
			}
								
			public string Name
			{
				get {return m_name;}
			}
		
			public string Vendor		// need a getter for optional	
			{
				set {m_vendor = value;}
			}
		
			protected string m_name;
			protected string m_vendor;
		}
		
		// test code
		public AttributePropertiesTest() : base(
			new string[]{"Good1", "Good2", "Good3", "Good4"},
			new string[]{"Bad1", "Bad2", "Bad3", "Bad4"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new AttributePropertiesRule(cache, reporter);
		}
	} 
}

