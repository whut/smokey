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
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Smokey.Framework.Support;
using Smokey.Internal.Rules;

namespace Smokey.Tests
{
	[TestFixture]
	public class Const1Test : TypeTest
	{	
		// test classes		
		public sealed class Good1
		{
			internal const int LinesPerPage = 40;
			internal static readonly Exception Error = new Exception("foo");
			public static readonly int FontSize = 12;
		}			
						
		public class Good2
		{
			internal static readonly int Value = "hey".GetHashCode();
		}			
				
		public class Bad1
		{
			internal static readonly int Value = 40;
		}			
				
		public class Bad2
		{
			internal static readonly short Value = 40;
		}			
				
		public class Bad3
		{
			internal static readonly bool Value = true;
		}			
				
		public class Bad4
		{
			internal static readonly AttributeTargets Value = AttributeTargets.Enum;
		}			
				
		public class Bad5
		{
			internal static readonly string Value = "hey";
		}			
								
		// test code
		public Const1Test() : base(
			new string[]{"Good1"},
			new string[]{"Bad1", "Bad2", "Bad3", "Bad4", "Bad5"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new Const1Rule(cache, reporter);
		}
	} 
}