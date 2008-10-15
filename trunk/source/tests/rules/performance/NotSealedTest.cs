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
	public class NotSealedTest : AssemblyTest
	{	
		#region Test cases
		private abstract class Good1			// not concrete
		{		
		}

		public class Good2						// externally visible
		{		
		}

		private sealed class Good3				// sealed
		{		
		}

		private class Good4						// has derived						
		{		
		}

		private sealed class Good5 : Good4		// sealed
		{		
		}

		private class Good6<T>						// has derived						
		{		
			public void Print(T foo)
			{
				Console.WriteLine(foo);
			}
		}

		private sealed class Good7 : Good6<string>	// sealed
		{		
		}

		private class Bad1						
		{		
		}

		private class Bad2<T>						
		{		
			public void Print(T foo)
			{
				Console.WriteLine(foo);
			}
		}
		#endregion
		
		// test code
		public NotSealedTest() : base(
			new string[]{"Good1+Good2+Good3+Good4+Good5", "Good6`1+Good7"},
			new string[]{"Good1+Bad1", "Good1+Bad2`1"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new NotSealedRule(cache, reporter);
		}
	} 
}