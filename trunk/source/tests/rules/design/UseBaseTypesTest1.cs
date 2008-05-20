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
using System.IO;
using System.Reflection;

namespace Smokey.Tests
{
	[TestFixture]
	public class UseBaseTypesTest1 : MethodTest
	{	
		// test classes
		public class Good
		{
			public string Type(TypeInitializationException e)
			{
				return e.TypeName;
			}

			public void AddRange(List<int> l)
			{
				List<int> m = new List<int>();
				l.AddRange(m);
			}

			public void TwoInterfaces(List<int> l)
			{
				l.RemoveAt(0);
				l.Remove(100);
			}

			public string TwoCalls(TypeInitializationException e)
			{
				return e.TypeName + e.TypeName;
			}

			public string CrossCalls(TypeInitializationException e)
			{
				return e.GetType().ToString() + e.TypeName;
			}

			public object AddImplementation(Type implementation)
			{
				return implementation.GetConstructor(null);
			}
		}			
		
		public class Bad	
		{
			public string Help(TypeInitializationException e)
			{
				return e.Message;
			}

//			public void Clear(List<int> l)
//			{
//				l.Clear();
//			}

//			public void TwoCalls(List<int> l)
//			{
//				l.RemoveAt(0);
//				l.RemoveAt(1);
//			}

			public string CrossCalls(TypeInitializationException e)
			{
				return e.GetType().ToString() + e.HelpLink;
			}
		}			
				
		// test code
		public UseBaseTypesTest1() : base(
			new string[]{"Good.Type", "Good.AddRange", "Good.TwoInterfaces",
				"Good.TwoCalls", "Good.CrossCalls", "Good.AddImplementation"},
			
			new string[]{"Bad.Help", "Bad.CrossCalls"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new UseBaseTypesRule(cache, reporter);
		}
	} 
}