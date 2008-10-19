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
	public class EventSignatureTest : EventTest
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
	
		public class Good
		{
			public event EventHandler<NameChangedEventArgs> Event1;
			
			public event EventHandler Event2;
			
			public delegate void Callback3(object sender, EventArgs e);
			public event Callback3 Event3;
			
			public void Fire()
			{
				Event1(this, null);
				Event2(this, null);
				Event3(this, null);
			}
		}			
		
		public class Bad	
		{
			public delegate int Callback1(object sender, EventArgs e);
			public event Callback1 Event1;

			public delegate void Callback2(object sender);
			public event Callback2 Event2;

			public delegate void Callback3(EventArgs e);
			public event Callback3 Event3;

			public delegate void Callback4(Bad sender, EventArgs e);
			public event Callback4 Event4;

			public delegate void Callback5(object sender, Bad e);
			public event Callback5 Event5;

			public delegate void Callback6(object sender, EventArgs e, int n);
			public event Callback6 Event6;

			public delegate void Callback7(object xxx, EventArgs e);
			public event Callback7 Event7;

			public delegate void Callback8(object sender, EventArgs xxx);
			public event Callback8 Event8;

			protected delegate void Callback9(Bad sender, EventArgs e);
			protected event Callback9 Event9;
			
			public void Fire()
			{
				Event1(this, null);
				Event2(this);
				Event3(null);
				Event4(this, null);
				Event5(this, null);
				Event6(this, null, 2);
				Event7(this, null);
				Event8(this, null);
				Event9(this, null);
			}
		}			
				
		// test code
		public EventSignatureTest() : base(
			new string[]{"Good.Event1", "Good.Event2", "Good.Event3"},
			
			new string[]{"Bad.Event1", "Bad.Event2", "Bad.Event3", "Bad.Event4",
			"Bad.Event5", "Bad.Event6", "Bad.Event7", "Bad.Event8", "Bad.Event9"},
			
			new string[]{"NameChangedEventArgs"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new EventSignatureRule(cache, reporter);
		}
	} 
}
#endif
