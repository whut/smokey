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
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Smokey.Framework.Support;
using Smokey.Internal.Rules;

namespace Smokey.Tests
{
	[TestFixture]
	public class RandomUsedOnceTest : MethodTest
	{	
		#region Test classes
		private class Cases
		{
			public int Good1()
			{
				return m_random.Next();
			}
			
			public int Good2()
			{
				Random r = new Random();
				return r.Next() + r.Next();
			}
			
			public int Good3()
			{
				Random r = new Random();
				int sum = 0;
				for (int i = 0; i < 10; ++i)
					sum += r.Next();
				return sum;
			}
			
			public int Bad1()
			{
				Random r = new Random();
				return r.Next();
			}
			
			public int Bad2()
			{
				Random r = new Random();
				return r.Next(3);
			}

			public int Bad3()
			{
				Random r = new Random();
				return r.Next(2, 1);
			}
			
			public int Bad4()
			{
				Random r = new Random();
				byte[] v = new byte[4];
				return r.Next(v[0]);
			}

			public int Bad5()
			{
				Random r = new Random();
				return (int) r.NextDouble();
			}
			
			private Random m_random = new Random();
		}		
		#endregion
		
		// test code
		public RandomUsedOnceTest() : base(
			new string[]{"Cases.Good1", "Cases.Good2",  "Cases.Good3"},
			new string[]{"Cases.Bad1", "Cases.Bad2", "Cases.Bad3", "Cases.Bad4", "Cases.Bad5"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new RandomUsedOnceRule(cache, reporter);
		}
	} 
}

