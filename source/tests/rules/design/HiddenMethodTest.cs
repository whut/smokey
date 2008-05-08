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
	public class HiddenMethodTest : MethodTest
	{	
		// test classes
		internal class BaseType
		{
			public void Good1(string x, object y)
			{
				Console.WriteLine("Base: {0}, {1}", x, y);
			}
			
			public virtual void Good3(string x, object y)
			{
				Console.WriteLine("Base: {0}, {1}", x, y);
			}
			
			public void Bad1(string x, string y)
			{
				Console.WriteLine("Base: {0}, {1}", x, y);
			}
			
			public void Bad2(string x, DerivedType y)
			{
				Console.WriteLine("Base: {0}, {1}", x, y);
			}
		}
		
		internal class DerivedType : BaseType
		{
			public void Good1(string x, string y)
			{
				Console.WriteLine("Derived: {0}, {1}", x, y);
			}
			
			public void Good2(string x, string y)
			{
				Console.WriteLine("Derived: {0}, {1}", x, y);
			}
			
			public override void Good3(string x, object y)
			{
				Console.WriteLine("Derived: {0}, {1}", x, y);
			}
			
			public void Bad1(string x, object y)
			{
				Console.WriteLine("Derived: {0}, {1}", x, y);
			}
			
			public void Bad2(string x, BaseType y)
			{
				Console.WriteLine("Derived: {0}, {1}", x, y);
			}
		}
				
		// test code
		public HiddenMethodTest() : base(
			new string[]{"DerivedType.Good1", "DerivedType.Good2", "DerivedType.Good3"},
			
			new string[]{"DerivedType.Bad1", "DerivedType.Bad2"},

			new string[]{"BaseType."})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new HiddenMethodRule(cache, reporter);
		}
	} 
}