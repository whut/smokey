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

namespace Smokey.Tests
{
	[TestFixture]
	public class SealedProtectedTest : TypeTest
	{	
		// test cases
		public class Good1		// not sealed
		{
			public int PublicMethod()
			{
				return m_value;
			}
			
			protected virtual void ProtectedMethod()
			{
			}

			protected int m_value = 10;
		}
				
		public sealed class Good2	// no protected
		{
			public void PublicMethod()
			{
			}
		}

		public sealed class Good3 : Good1	// override
		{			
			protected override void ProtectedMethod()
			{
			}
		}
				
		public abstract class Good4		// not sealed
		{
			public int PublicMethod()
			{
				AbstractMethod();
				return 10;
			}
			
			protected abstract void AbstractMethod();
		}
				
		public sealed class Good5 : Good4		
		{
			protected override void AbstractMethod()	// override, so OK
			{
			}
		}
				
		public sealed class Bad1
		{
			public void PublicMethod()
			{
			}
			
			protected void ProtectedMethod()
			{
			}
		}

		public sealed class Bad3
		{
			public int PublicMethod()
			{
				return m_value;
			}
						
			protected int m_value = 10;
		}

		// test code
		public SealedProtectedTest() : base(
			new string[]{"Good1", "Good2", "Good3", "Good4", "Good5"},
			new string[]{"Bad1", "Bad3"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new SealedProtectedRule(cache, reporter);
		}
	} 
}