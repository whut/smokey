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
	public class BeforeEventTest : TypeTest
	{	
		// test classes
		public class Good1
		{			
			public event EventHandler Event;	
			
			public void Fire()
			{
				Event(this, EventArgs.Empty);
			}
		}			
		
		public class Good2
		{
			public event EventHandler Closing;
			
			public void Fire()
			{
				Closing(this, EventArgs.Empty);
			}
		}			
		
		public class Good3
		{
			public event EventHandler Closed;
			
			public void Fire()
			{
				Closed(this, EventArgs.Empty);
			}
		}			
		
		public class Good4
		{
			public event EventHandler Before;
			
			public void Fire()
			{
				Before(this, EventArgs.Empty);
			}
		}			
		
		public class Good5
		{
			public event EventHandler After;
			
			public void Fire()
			{
				After(this, EventArgs.Empty);
			}
		}			
		
		public class Bad1
		{
			public event EventHandler BeforeClose;
			
			public void Fire()
			{
				BeforeClose(this, EventArgs.Empty);
			}
		}			
				
		public class Bad2
		{
			public event EventHandler AfterClose;
			
			public void Fire()
			{
				AfterClose(this, EventArgs.Empty);
			}
		}			
								
		// test code
		public BeforeEventTest() : base(
			new string[]{"Good1", "Good2", "Good3", "Good4", "Good5"},
			
			new string[]{"Bad1", "Bad2"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new BeforeEventRule(cache, reporter);
		}
	} 
}
#endif
