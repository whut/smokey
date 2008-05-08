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
using System.Reflection;
using System.Runtime.Serialization;
using Smokey.Framework.Support;
using Smokey.Internal.Rules;

namespace Smokey.Tests
{
	[TestFixture]
	public class VisibleFieldsTest : TypeTest
	{	
		#region Test cases
		internal class Good1		// class is internal
		{
			public int line1;
		}

		internal class Good2		// class is internal
		{
			protected int line1;
		}

		public class Good3	
		{
			public void Stuff()
			{
				Console.WriteLine(line1.ToString());
			}
			
			private int line1;		// field is private
		}

		public class Good4	
		{
			internal int line1;		// field is internal
		}

		public class Good5
		{
			public const int line1 = 1;		// field is const
		}

		public class Good6
		{
			public readonly int line1 = 1;		// field is readonly
		}

		public struct Bad1	
		{
			public int line1;
		}

		public class Bad2
		{
			protected int line1;
		}

		public class Bad3
		{
			protected internal int line1 = 1;	
		}
		#endregion
		
		// test code
		public VisibleFieldsTest() : base(
			new string[]{"Good1", "Good2", "Good3", "Good4", 
			"Good5", "Good6"},
			new string[]{"Bad1", "Bad2", "Bad3"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new VisibleFieldsRule(cache, reporter);
		}
	} 
}
