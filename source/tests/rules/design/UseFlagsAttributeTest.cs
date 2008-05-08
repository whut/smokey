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
	public class UseFlagsAttributeTest : TypeTest
	{	
		// test classes
		[Flags]
		internal enum Good1
		{
			None   = 0x0000,	
			First  = 0x0001,
			Second = 0x0002,
			Third  = 0x0004,
			All    = First | Second | Third,
		}
				
		internal enum Good2
		{
			None   = 0x0000,	
			First  = 0x0001,
		}
				
		internal enum Good3
		{
			None   = 0x0000,	
			First  = 0x0001,
			Second = 0x0002,
		}
				
		internal enum Good4
		{
			None   = 0x0000,	
			First  = 0x0001,
			Second = 0x0002,
			Third  = 0x0003,
		}
				
		internal enum Good5
		{
			None   = 0x0000,	
			First  = 0x0001,
			Second = 0x0002,
			Third  = 0x0003,
			Four   = 0x0004,
		}
				
		internal enum Good6
		{
			None   = 0x0000,	
			First  = 0x0001,
			Second = 0x0002,
			Third  = 0x0004,
			Four   = 0x0007,
			Five   = 0x0008,
			Six    = 0x0009,
		}
				
		internal enum Good7
		{
			None   = 0x0000,	
			First  = 0x0001,
			Second = 0x0002,
			Third  = 0x0004,
			LB     = 0x00FF,
			HB     = 0xFFF0,
		}
				
		internal enum Good8
		{
			None   = 0x0000,	
			First  = 0x0001,
			Second = 0x0002,
			Six    = 0x0009,
			Sixx   = 0x0010,
			Sixxx  = 0x0011,
		}
				
		internal enum Good9 : sbyte
		{
			kTXNRightTab   = -1,
			kTXNLeftTab    = 0,
			kTXNCenterTab  = 1
		}

		internal enum Bad1
		{
			None   = 0x0000,	
			First  = 0x0001,
			Second = 0x0002,
			Third  = 0x0004,
		}
				
		internal enum Bad2
		{
			None   = 0x0000,	
			First  = 0x0001,
			Second = 0x0002,
			Third  = 0x0004,
			All    = First | Second | Third,
		}
				
		internal enum Bad3
		{
			None   = 0x0000,	
			First  = 0x0001,
			Second = 0x0002,
			Third  = 0x0004,
			LB     = 0x00FF,
			HB     = 0xFF00,
		}
				
		// test code
		public UseFlagsAttributeTest() : base(
			new string[]{"Good1", "Good2", "Good3", 
				"Good4", "Good5", "Good6", "Good7", "Good8", "Good9"},
			
			new string[]{"Bad1", "Bad2", "Bad3"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new UseFlagsAttributeRule(cache, reporter);
		}
	} 
}