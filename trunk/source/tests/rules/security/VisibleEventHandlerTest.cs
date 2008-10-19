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
using System.Diagnostics;
using System.IO;
using System.Reflection;

#if OLD
namespace Smokey.Tests
{
	[TestFixture]
	public class VisibleEventHandlerTest : MethodTest
	{	
		#region Test classes
		public class Good
		{
			public void Hmm()
			{
				A(this, EventArgs.Empty);
			}
			
			private void A(object o, EventArgs e)
			{
			}

			public void B(Good o, EventArgs e)
			{
			}
		}
		
		public class MyEvent : EventArgs
		{
		}

		public class Bad
		{
			public void A(object o, EventArgs e)
			{
			}

			protected void B(object o, MyEvent e)
			{
			}
		}
		#endregion
		
		// test code
		public VisibleEventHandlerTest() : base(
			new string[]{"Good.A", "Good.B"},
			new string[]{"Bad.A", "Bad.B"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new VisibleEventHandlerRule(cache, reporter);
		}
	} 
}
#endif
