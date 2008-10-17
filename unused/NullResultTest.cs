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
using System.Reflection;
using Smokey.Framework.Support;
using Smokey.Internal.Rules;

namespace Smokey.Tests
{
	[TestFixture]
	public class NullResultTest : AssemblyTest
	{	
		#region Test classes
		public class Good1
		{
			public string GetName(int x)
			{
				if (x >= 0)
					return x.ToString();
				else
					return "hmm";
			}
			
			public void User(int x)
			{
				string name = GetName(x);
				Console.WriteLine(name.Length);
			}
		}

		public class Good2
		{
			public string GetName(int x)
			{
				if (x >= 0)
					return x.ToString();
				else
					return null;
			}
			
			public void User(int x)
			{
				string name = GetName(x);
				if (x < 0)
					name = "foo";
					
				Console.WriteLine(name.Length);
			}
		}

		public class Good3
		{
			public string GetName(int x)
			{
				if (x >= 0)
					return x.ToString();
				else
					return null;
			}
			
			public void User(int x)
			{
				string name = GetName(x);
				if (x < 0)
					return;
					
				Console.WriteLine(name.Length);
			}
		}

		public class Bad1
		{
			public string GetName(int x)
			{
				if (x >= 0)
					return x.ToString();
				else
					return null;
			}
			
			public void User(int x)
			{
				string name = GetName(x);
				Console.WriteLine(name.Length);
			}
		}
		#endregion
		
		// test code
		public NullResultTest() : base(
			new string[]{"Good1", "Good2", "Good3"},
			new string[]{"Bad1"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new NullResultRule(cache, reporter);
		}
	} 
}
