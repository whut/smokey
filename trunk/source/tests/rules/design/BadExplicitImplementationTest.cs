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
	public class BadExplicitImplementationTest : TypeTest
	{	
		public interface ITest
		{
			void SomeMethod();
		}
		
		// test classes
		public class Good1 : ITest		// has alternative
		{
			void ITest.SomeMethod() 
			{
			}
		
			protected void SomeMethod()
			{
			}	
		}
								
		public class Good2 : ITest		// has alternative
		{
			void ITest.SomeMethod() 
			{
			}
		
			public void SomeMethod()
			{
			}	
		}
								
		public sealed class Good3 : ITest	// sealed
		{
			void ITest.SomeMethod() 
			{
			}
		}
								
		public class Good4 : ITest			// not explicit
		{
			public void SomeMethod() 
			{
			}
		}
								
		private class Good5 : ITest			// not externally visible
		{
			public void SomeMethod() 
			{
			}
		}
								
		public class Bad1 : ITest
		{
			void ITest.SomeMethod() 
			{
			}
		}
								
		// test code
		public BadExplicitImplementationTest() : base(
			new string[]{"Good1", "Good2", "Good3", "Good4", "Good5"},
			new string[]{"Bad1"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new BadExplicitImplementationRule(cache, reporter);
		}
	} 
}

