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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;

#if OLD
namespace Smokey.Tests
{
	[TestFixture]
	public class InternalInterfaceTest : MethodTest
	{	
		#region Test classes
		internal interface III
		{
			void Greet();
		}
		
		public interface PI
		{
			void Greet();
		}
		
		public class Good1 : III
		{
			public void Greet()			// not virtual
			{
				Console.WriteLine("hi");
			}
		}
		
		public class Base2 : III	
		{
			public virtual void Greet()
			{
				Console.WriteLine("hi");
			}
		}
		
		public sealed class Good2 : Base2	// sealed class
		{
			public override void Greet()
			{
				Console.WriteLine("hi!!");
			}
		}
		
		internal class Good3 : III		// internal class
		{
			public virtual void Greet()
			{
				Console.WriteLine("hi");
			}
		}
		
		public class Good4 : III	
		{
			internal Good4()			// no public ctors
			{
			}
			
			public virtual void Greet()
			{
				Console.WriteLine("hi");
			}
		}
				
		public class Good5 : PI			// interface is public
		{
			public virtual void Greet()
			{
				Console.WriteLine("hi");
			}
		}
				
		public class Bad1 : III
		{
			public virtual void Greet()
			{
				Console.WriteLine("hi");
			}
		}
		#endregion
		
		// test code
		public InternalInterfaceTest() : base(
			new string[]{"Good1.Greet", "Good2.Greet", "Good3.Greet", "Good4.Greet", "Good5.Greet"},
			new string[]{"Bad1.Greet"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new InternalInterfaceRule(cache, reporter);
		}
	} 
}
#endif
