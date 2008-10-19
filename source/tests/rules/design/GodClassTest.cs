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
	public class GodClassTest : TypeTest
	{			
		// test classes
		internal class Good1
		{		
			public object Compute()
			{
				List<object> result = new List<object>();
				
				result.Add(DateTime.Now);
				result.Add(TimeSpan.Zero);
				
				return result;
			}
		}
								
		internal class Good2
		{		
			public object Compute(int x)
			{
				List<Func<int>> result = new List<Func<int>>();
				
				result.Add(() => x + 1);		// don't count lambdas
				result.Add(() => x + 2);
				result.Add(() => x + 3);
				result.Add(() => x + 4);
				result.Add(() => x + 5);
				
				return result;
			}
		}
								
		internal class Good3
		{		
			public object Compute(int x)
			{
				List<object> result = new List<object>();
				
				result.Add(new {F0 = 1, F1 = "orange"});		// don't count anonymous types
				result.Add(new {F1 = 1, F2 = "orange"});
				result.Add(new {F1 = 1, F3 = "orange"});
				result.Add(new {F1 = 1, F4 = "orange"});
				result.Add(new {F1 = 1, F5 = "orange"});
				
				return result;
			}
		}
								
		internal class Bad1
		{		
			public object Compute()
			{
				List<object> result = new List<object>();
				
				result.Add(new Dictionary<int, long>());		// this is five types, not three
				result.Add(new Dictionary<int, string>());
				result.Add(new Dictionary<int, float>());
				
				return result;
			}
		}
								
		// test code
		public GodClassTest() : base(
			new string[]{"Good1", "Good2", "Good3"},
			new string[]{"Bad1"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new GodClassRule(cache, reporter, 5);
		}
	} 
}
#endif
