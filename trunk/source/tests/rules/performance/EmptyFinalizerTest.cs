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
	public class EmptyFinalizerTest : TypeTest
	{	
		#region Test cases
		private class GoodCase1 	
		{
		}
		
		private class GoodCase2 		
		{
			IntPtr ptr;
			
			public GoodCase2()
			{
				ptr = (IntPtr) 1;
			}
			
			public IntPtr Handle 
			{
				get {return ptr;}
			}
			
			~GoodCase2()
			{
				ptr = IntPtr.Zero;
			}
		}
		
		private class BadCase 	
		{
			~BadCase()
			{
			}
		}
		#endregion
		
		// test code
		public EmptyFinalizerTest() : base(
			new string[]{"GoodCase1", "GoodCase2"},
			new string[]{"BadCase"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new EmptyFinalizerRule(cache, reporter);
		}
	} 
}
#endif