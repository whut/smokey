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
	public class DeepInheritanceTest : TypeTest
	{	
		// test classes
		public class Base1
		{
		}			
		
		public class Base2 : Base1
		{
		}			
		
		public class Base3 : Base2
		{
		}			
		
		public class Base4 : Base3
		{
		}			
		
		public class Base5 : Base4
		{
		}			
		
		public class Good1
		{
		}			
		
		public class Good2 : Base1
		{
		}			
		
		public class Good3 : Base2
		{
		}			
		
		public class Good4 : Base3
		{
		}			
		
		public class Good5 : Base4
		{
		}			
				
		public class Bad1 : Base5
		{
		}			
		
		// test code
		public DeepInheritanceTest() : base(
			new string[]{"Good1", "Good2", "Good3", "Good4", "Good5"},
			new string[]{"Bad1"},
			new string[]{"Base1", "Base2", "Base3", "Base4", "Base5"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new DeepInheritanceRule(cache, reporter);
		}
	} 
}