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
	public class StringUri1Test : TypeTest
	{	
		#region Test classes
		private class Good1			// has overload
		{
			public void Foo(int a, string inUri)
			{
			}

			public void Foo(int a, Uri inUri)
			{
			}
		}

		private class Good2			// weird arg name
		{
			public void Foo(int a, string uRi)
			{
			}
		}

		private class Good3			// has overload
		{
			public void Foo(int a, string TheUriName)
			{
			}

			public void Foo(int a, Uri inUri)
			{
			}
		}

		private class Good4			// arg isn't a string
		{
			public void Foo(int a, float inUri)
			{
			}
		}

		private class Bad1
		{
			public void Foo(int a, string inUri)
			{
			}
		}

		private class Bad2
		{
			public void Foo(int a, string theUrlArg)
			{
			}
		}

		private class Bad3
		{
			public void Foo(int a, string theUrlArg)
			{
			}

			public void Foo(Uri theUrlArg)
			{
			}
		}
		#endregion
		
		// test code
		public StringUri1Test() : base(
			new string[]{"Good1", "Good2", "Good3", "Good4"},
			new string[]{"Bad1", "Bad2", "Bad3"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new StringUri1Rule(cache, reporter);
		}
	} 
}
