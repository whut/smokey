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
using Smokey.Framework.Support;
using Smokey.Internal.Rules;

#if OLD
namespace Smokey.Tests
{
	[TestFixture]
	public class LargeStructTest : TypeTest
	{	
		// test classes
		public enum Greet {Hello, Gutentag, Bonjour};
		
		public struct Point
		{
			public int x, y;
		}			

		public struct GoodStruct0
		{
		}			
		
		public struct GoodStruct4
		{
			public int a;
		}			
		
		public struct GoodStruct8
		{
			public int a, b;
		}			
		
		public struct GoodStruct12
		{
			public int a, b, c;
		}			
		
		public struct GoodStruct16
		{
			public int a, b, c, d;
		}			
		
		public struct GoodEnum16
		{
			public Greet a, b, c, d;
		}			
		
		public struct GoodStaticStruct8
		{
			public int a, b;
			public static int q, w, e, r;
		}			
		
		public struct GoodNestedClass8
		{
			public string a, b, c, d;
		}			
		
		public struct GoodNestedStruct16
		{
			public Point a, b;
		}			
		
		public class GoodClass20
		{
			public int a, b, c, d, e;
		}			
				
		public struct BadStruct17
		{
			public int a, b, c, d;
			public Byte q;
		}			
		
		public struct BadStruct20
		{
			public int a, b, c, d, e;
		}			
				
		public struct BadEnum20
		{
			public Greet a, b, c, d, e;
		}			
		
		public struct BadNestedClass20
		{
			public string a, b, c, d, e;
		}			
		
		public struct BadNestedStruct24
		{
			public Point a, b, c;
		}			
		
		// test code
		
		// test code
		public LargeStructTest() : base(
			new string[]{"GoodStruct0", "GoodStruct4", "GoodStruct8",
				"GoodStruct12", "GoodStruct16", "GoodStaticStruct8", "GoodNestedClass8",
				"GoodClass20", "GoodEnum16", "GoodNestedStruct16"},
		
			new string[]{"BadStruct17", "BadStruct20", "BadNestedClass20",
				"BadEnum20", "BadNestedStruct24"},
		
			new string[]{"Greet", "Point"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new LargeStructRule(cache, reporter);
		}
	} 
}
#endif
