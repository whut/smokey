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
using Smokey.Internal.Rules;

namespace Smokey.Tests
{
	[TestFixture]
	public class DestroyStackTraceTest : MethodTest
	{	
		#region Test classes
		private class GoodCase
		{
			public void Run()
			{
				try
				{
					Console.WriteLine("cool stuff goes here");
				}
				catch (ApplicationException e)
				{
					Console.Error.WriteLine(e.Message);
					throw;		// preserves stack crawl of the original error
				}
			}

			public void Run2()
			{
				try
				{
					Console.WriteLine("more cool stuff");
				}
				catch (ApplicationException e)
				{
					Console.Error.WriteLine(e.Message);
					throw new ApplicationException("cool is buggy", e);		// retain original error
				}
			}

			public void Run3()
			{
				try
				{
					Console.WriteLine("more cool stuff");
				}
				catch (ApplicationException e)
				{
					Console.Error.WriteLine(e.Message);
					
					e = new ApplicationException("bogus");
					throw new ApplicationException("cool is buggy", e);		// retain original error
				}
			}

			public void Run4()
			{
				try
				{
					Console.WriteLine("cool stuff goes here");
				}
				catch (ApplicationException e)
				{
					Console.Error.WriteLine(e.Message);
					throw;		// preserves stack crawl of the original error
				}
				finally
				{
					Console.WriteLine("cleanup");
				}
			}
		}
				
		private class BadCase
		{
			public void Run()
			{
				try
				{
					Console.WriteLine("Run code");
				}
				catch (ApplicationException e)
				{
					throw e;	
				}
			}

			public void Run2()
			{
				try
				{
					Console.WriteLine("Run2 code");
				}
				catch (ApplicationException e)
				{
					int j = 0;
					for (int k = 0; k < 10; k++) 
					{
						j += 10;
						Console.WriteLine(j);
					}

					throw e;	
				}
			}

			public void Run3()
			{
				try
				{
					Console.WriteLine("Run3 code");
				}
				catch (ApplicationException e)
				{
					int i = 0;
					for (int k = 0; k < 10; k++) 
					{
						i += 10;
						Console.WriteLine(i);
						if ((i % 1234) > 56)
							throw;
					}
					throw e;	
				}
			}
		}
		#endregion
		
		// test code
		public DestroyStackTraceTest() : base(
			new string[]{"GoodCase.Run", "GoodCase.Run2", "GoodCase.Run3", "GoodCase.Run4"},
			new string[]{"BadCase.Run", "BadCase.Run2", "BadCase.Run3"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new DestroyStackTraceRule(cache, reporter);
		}
	} 
}
