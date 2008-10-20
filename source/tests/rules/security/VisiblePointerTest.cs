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
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security;
using System.Security.Permissions;

namespace Smokey.Tests
{
	[TestFixture]
	public class VisiblePointerTest : TypeTest
	{	
		#region Test classes
		public class Good
		{
			public IntPtr GetPtr()
			{
				return m_ptr2;
			}
			
			public readonly IntPtr m_ptr1;		// readonly
			public string m_name;				// not IntPtr
			private readonly IntPtr m_ptr2;		// not externally visible
		}

		public class Bad1
		{
			public IntPtr m_ptr;
		}

		public class Bad2
		{
			public UIntPtr m_ptr;
		}

		public class Bad3
		{
			protected UIntPtr m_ptr;
		}
		#endregion
		
		// test code
		public VisiblePointerTest() : base(
			new string[]{"Good"},
			new string[]{"Bad1", "Bad2", "Bad3"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new VisiblePointerRule(cache, reporter);
		}
	} 
}

