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
using System.Text.RegularExpressions;

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

#if OLD
namespace Smokey.Tests
{
	[TestFixture]
	public class DoubleCheckedLockingTest : MethodTest
	{	
		#region Test classes
		public class Cases
		{
			public static Cases Good1()
			{
				if (ms_instance == null) 
				{
					lock (ms_lock) 
					{
						if (ms_instance == null) 
						{
							ms_instance = new Cases();
						}
					}
				}
				
				return ms_instance;
			}
				
			public static Cases Bad1()		// incorrect
			{
				if (!ms_inited) 
				{
					lock (ms_lock) 
					{
						if (ms_instance == null) 
						{
							ms_instance = new Cases();
							ms_inited = true;
						}
					}
				}
				
				return ms_instance;
			}
				
			public static Cases Bad2()		// incorrect
			{
				if (!ms_inited) 
				{
					lock (ms_lock) 
					{
						if (ms_instance == null) 
						{
							ms_inited = true;
							ms_instance = new Cases();
						}
					}
				}
				
				return ms_instance;
			}
				
			public static Cases Bad3()		// silly
			{
				if (Thread.VolatileRead(ref ms_initialized) == 0) 
				{
					lock (ms_lock) 
					{
						if (ms_instance == null) 
						{
							ms_instance = new Cases();
							ms_initialized = 1;
						}
					}
				}
				
				return ms_instance;
			}
				
			public static Cases Bad4()		// inefficient
			{
				lock (ms_lock) 
				{
					if (ms_instance == null) 
					{
						ms_instance = new Cases();
					}
				}
				
				return ms_instance;
			}
				
			private static Cases ms_instance;
			private static bool ms_inited;
			private static int ms_initialized;
			private static object ms_lock = new object();
		}
		#endregion
		
		// test code
		public DoubleCheckedLockingTest() : base(
			new string[]{"Cases.Good1"},
			new string[]{"Cases.Bad1", "Cases.Bad2", "Cases.Bad3", "Cases.Bad4"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new DoubleCheckedLockingRule(cache, reporter);
		}
	} 
}
#endif
