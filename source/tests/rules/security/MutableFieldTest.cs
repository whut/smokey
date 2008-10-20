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
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security;
using System.Security.Permissions;

namespace Smokey.Tests
{
	[TestFixture]
	public class MutableFieldTest : TypeTest
	{	
		#region Test classes
		public class MyCollection : KeyedCollection<string, MutableFieldTest>
		{
			protected override string GetKeyForItem(MutableFieldTest item)
			{
				return item.ToString();
			}
		}
		
		public class Good
		{
			public UriBuilder GetBuilder()
			{
				return m_builder;
			}
			
			public readonly string Name;			// not mutable
			public UriBuilder m_weak;				// not readonly
			public readonly DateTime Time;			// value type
			private readonly UriBuilder m_builder;	// not externally visible
		}

		public class Bad1
		{
			public readonly UriBuilder Builder;
		}

		public class Bad2
		{
			public readonly WeakReference Weak;
		}

		public class Bad3
		{
			public readonly MyCollection Collection;
		}

		public class Bad4
		{
			public readonly Stack<int> Collection;
		}
		#endregion
		
		// test code
		public MutableFieldTest() : base(
			new string[]{"Good"},
			new string[]{"Bad1", "Bad2", "Bad3", "Bad4"},
			new string[]{"MyCollection"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new MutableFieldRule(cache, reporter);
		}
	} 
}

