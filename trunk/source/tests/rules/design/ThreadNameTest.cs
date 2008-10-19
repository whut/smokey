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

using Smokey.Framework.Support;
using Smokey.Internal.Rules;

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;

#if OLD
namespace Smokey.Tests
{
	[TestFixture]
	public class ThreadNameTest : MethodTest
	{	
		// test classes
		public class Cases
		{
			public object Good1()
			{
				Thread t = new Thread(this.DoThread);
				t.Name = "foo";
				return t;
			}

			public object Good2()
			{
				Thread t = new Thread(this.DoThread);
				Thread u = new Thread(this.DoThread);
				t.Name = "foo";
				u.Name = "foo2";
				return new[] {t, u};
			}

			public object Bad1()
			{
				Thread t = new Thread(this.DoThread);
				return t;
			}

			public object Bad2()
			{
				Thread t = new Thread(this.DoThread, 2048);
				return t;
			}

			public object Bad3()
			{
				Thread t = new Thread(this.DoThread);
				Thread u = new Thread(this.DoThread);
				t.Name = "foo";
				return new[] {t, u};
			}

			private void DoThread()
			{
			}
		}			
						
		// test code
		public ThreadNameTest() : base(
			new string[]{"Cases.Good1", "Cases.Good2"},
			new string[]{"Cases.Bad1", "Cases.Bad2", "Cases.Bad3"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new ThreadNameRule(cache, reporter);
		}
	} 
}
#endif
