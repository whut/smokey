// Copyright (C) 2008 Jesse Jones
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

#if OLD
namespace Smokey.Tests
{
	[TestFixture]
	public class ObjectDisposedExceptionTest : MethodTest
	{	
		#region Test classes
		private class Cases
		{
			public object Good1()
			{
				return new ObjectDisposedException(GetType().Name);
			}
			
			public object Good2()
			{
				return new ObjectDisposedException(GetType().Name, new Exception("inner"));
			}
			
			public object Good3()
			{
				return new ObjectDisposedException(GetType().Name, "my message");
			}
			
			public object Good4(string s)
			{
				return new ObjectDisposedException(s);
			}
			
			public object Bad1()
			{
				return new ObjectDisposedException("Cases");
			}
			
			public object Bad2()
			{
				return new ObjectDisposedException("Cases", new Exception("inner"));
			}
			
			public object Bad3(string s)
			{
				return new ObjectDisposedException("Cases", s);
			}
		}		
		#endregion
		
		// test code
		public ObjectDisposedExceptionTest() : base(
			new string[]{"Cases.Good1", "Cases.Good2", "Cases.Good3"},
			new string[]{"Cases.Bad1", "Cases.Bad2", "Cases.Bad3"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new ObjectDisposedExceptionRule(cache, reporter);
		}
	} 
}
#endif
