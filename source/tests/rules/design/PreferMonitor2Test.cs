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

namespace Smokey.Tests
{
	[TestFixture]
	public class PreferMonitor2Test : MethodTest
	{	
		// test classes
		public class Cases
		{						
			public object Good()
			{
				List<object> r = new List<object>();
				
				r.Add(new Mutex());
				r.Add(new Mutex(true));
				r.Add(new Semaphore(2, 3));
				
				return r;
			}

			public object Bad1()
			{
				return new Mutex(false, "bad");
			}

			public object Bad2()
			{
				bool state;
				return new Mutex(false, "bad", out state);
			}

			public object Bad3()
			{
				return new Semaphore(2, 3, "bad");
			}

			public object Bad4()
			{
				bool state;
				return new Semaphore(2, 3, "bad", out state);
			}
		}			
												
		// test code
		public PreferMonitor2Test() : base(
			new string[]{"Cases.Good"},		
			new string[]{"Cases.Bad1", "Cases.Bad2", "Cases.Bad3", "Cases.Bad4"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new PreferMonitor2Rule(cache, reporter);
		}
	} 
}