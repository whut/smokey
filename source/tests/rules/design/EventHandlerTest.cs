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
	public class EventHandlerTest : TypeTest
	{	
		// test classes
		public class NameChangedEventArgs : EventArgs
		{
			public readonly string OldName;
			public readonly string NewName;
			
			public NameChangedEventArgs(string oldName, string newName)
			{
				OldName = oldName;
				NewName = newName;
			}
		}
	
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
			public event EventHandler<NameChangedEventArgs> Event;
			
			public void Fire()
			{
				Event(this, null);
			}
		}			
		
		public class Bad1
		{
			public delegate void Callback1(object sender, EventArgs e);
			public event Callback1 Event1;
			
			public void Fire()
			{
				Event1(this, EventArgs.Empty);
			}
		}			
				
		public class Bad2
		{
			public delegate void Callback5(object sender, NameChangedEventArgs e);
			public event Callback5 Event5;
			
			public void Fire()
			{
				Event5(this, null);
			}
		}			
				
		public class Bad3
		{
			public delegate void Callback5(object sender, ResolveEventArgs e);
			public event Callback5 Event5;
			
			public void Fire()
			{
				Event5(this, null);
			}
		}			
				
		public class Bad4
		{
			public delegate void CallbackXXX(object sender, NameChangedEventArgs e);
		}			
							
		// test code
		public EventHandlerTest() : base(
			new string[]{"Good1", "Good2"},
			
			new string[]{"Bad1", "Bad2", "Bad3", "Bad4"},
			
			new string[]{"Bad1/Callback1", "Bad2/Callback5", "Bad3/Callback5", "Bad4/CallbackXXX"}
			)	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new EventHandlerRule(cache, reporter);
		}
	} 
}
#endif
