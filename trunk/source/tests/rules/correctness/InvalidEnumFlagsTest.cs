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
	public class InvalidEnumFlagsTest : TypeTest
	{	
		#region Test classes
		enum GoodNonFlag {A, B, C, D};
		
		[Flags]
		enum GoodFlags1 {A = 0x0000, B = 0x0001, C = 0x0002, D = 0x0004};

		[Flags]
		enum GoodFlags2 {B = 0x0001, C = 0x0002, D = 0x0004, E = 0x0008};

		[Flags]
		enum GoodFlags3 {A, B, C, D, E = 16};

		[Flags]
		enum BadFlags1 {A, B, C, D};

		[Flags]
		enum BadFlags2 {A = 11, B = 12, C = 13, D = 14};

		[Flags]
		enum BadFlags3 {A = 14, B = 12, C = 11, D = 13};
		#endregion
		
		// test code
		public InvalidEnumFlagsTest() : base(
			new string[]{"GoodNonFlag", "GoodFlags1", "GoodFlags2", "GoodFlags3"},
			new string[]{"BadFlags1", "BadFlags2", "BadFlags3"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new InvalidEnumFlagsRule(cache, reporter);
		}
	} 
}
#endif
