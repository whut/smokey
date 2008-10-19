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

#if OLD
namespace Smokey.Tests
{
	[TestFixture]
	public class ExitCodeTest : MethodTest
	{	
		#region Test Cases
		private class Cases
		{
			public void Good1()
			{
				Environment.Exit(0);
				Environment.Exit(1);
				Environment.Exit(255);
			}

			public void Good2(int x)
			{
				int err = 0;
				
				if (x > 0)
				{
					Console.WriteLine("hmm");
					err = 100;
				}
				else
				{
					Console.WriteLine("haw");
					err = 200;
				}
			
				Environment.Exit(err);
			}

			public void Good3()
			{
				Environment.ExitCode = 0;
				Environment.ExitCode = 1;
				Environment.ExitCode = 255;
			}

			public void Bad1(string path)
			{
				Environment.Exit(-1);
			}

			public void Bad2(string path)
			{
				Environment.Exit(256);
			}

			public void Bad3(int x)
			{
				int err = 0;
				
				if (x > 0)
				{
					Console.WriteLine("hmm");
					err = 100;
				}
				else
				{
					Console.WriteLine("haw");
					err = 300;
				}
			
				Environment.Exit(err);
			}

			public void Bad4()
			{
				Environment.ExitCode = -1;
			}
		}
		#endregion
		
		// test code
		public ExitCodeTest() : base( 
			new string[]{"Cases.Good1", "Cases.Good2", "Cases.Good3"},
			new string[]{"Cases.Bad1", "Cases.Bad2", "Cases.Bad3", "Cases.Bad4"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new ExitCodeRule(cache, reporter);
		}
	} 
}
#endif
