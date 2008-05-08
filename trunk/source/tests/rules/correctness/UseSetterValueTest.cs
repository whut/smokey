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
using Smokey.Internal;
using Smokey.Internal.Rules;

namespace Smokey.Tests
{
	[TestFixture]
	public class UseSetterValueTest : MethodTest
	{	
		#region Test classes
		public class Good1
		{
			public string Name 
			{
				get {return m_name;}
				set {m_name = value;}
			}
			
			public static string Text
			{
				get {return m_text;}
				set {m_text = value;}
			}
			
			public string Captain 
			{
				get {return string.Empty;}
				set {Ignore.Value = value;}
			}

			private string m_name;
			private static string m_text;
		}
				
		public class Bad1
		{
			public string Captain 
			{
				get {return string.Empty;}
				set {}
			}

			public static string Text
			{
				get {return string.Empty;}
				set {}
			}
		}
				
		public class Bad2
		{
			public string Captain 
			{
				set {}
			}
		}
		#endregion
		
		// test code
		public UseSetterValueTest() : base(
			new string[]{"Good1.set_Name", "Good1.set_Captain", "Good1.set_Text"},
			new string[]{"Bad1.set_Captain", "Bad1.set_Text", "Bad2.set_Captain"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new UseSetterValueRule(cache, reporter);
		}
	} 
}
