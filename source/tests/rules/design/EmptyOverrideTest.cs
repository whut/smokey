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
	public class EmptyOverrideTest : MethodTest
	{	
		// test cases
		public class Baser
		{
			public virtual void Virtual4()
			{
				Console.WriteLine("hmm");
			}
		}
				
		public class Base : Baser
		{
			public virtual void Virtual1()
			{
				Console.WriteLine("hmm");
			}
			
			public virtual void Virtual2()
			{
				Console.WriteLine("hmm");
			}
			
			public virtual void Virtual3()
			{
			}
		}
				
		public class Cases : Base
		{
			public override void Virtual1()
			{
				base.Virtual1();
			}
			
			public override void Virtual3()	// base is empty so this is OK
			{
			}
			
			public void NoVirtual()	
			{
			}
			
			public virtual void NewVirtual()
			{
			}
			
			public override void Virtual2()	// base is not empty so this is bad
			{
			}
			
			public override void Virtual4()	
			{
			}
		}
				
		// test code
		public EmptyOverrideTest() : base(
			new string[]{"Cases.Virtual1", "Cases.Virtual3", "Cases.NoVirtual", "Cases.NewVirtual"},
			new string[]{"Cases.Virtual2", "Cases.Virtual4"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new EmptyOverrideRule(cache, reporter);
		}
	} 
}
#endif
