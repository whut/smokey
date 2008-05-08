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
//using System.Collections.Specialized;
//using System.Configuration;
using Smokey.Framework;
using Smokey.Framework.Support;	
using Smokey.Internal;
using SR = System.Reflection;

namespace Smokey.Tests
{
	[TestFixture]
	public class MethodMatcherTest : BaseTest
	{		
		public MethodMatcherTest()
		{
			var loc = SR.Assembly.GetExecutingAssembly().Location;
			m_assembly = AssemblyFactory.GetAssembly(loc);
			m_module = m_assembly.MainModule;
		}
		
		// Trivial comparisons work.
		[Test]
		public void Trivial()
		{
			SR.MethodInfo mi = typeof(String).GetMethod("ToString", Type.EmptyTypes);
			MethodReference mr1 = m_module.Import(mi);

			Assert.IsTrue(MethodMatcher.Match(mr1, mr1));
		}

		// Base class matching works
		[Test]
		public void Base()
		{
			SR.MethodInfo mi = typeof(String).GetMethod("ToString", Type.EmptyTypes);
			MethodReference mr1a = m_module.Import(mi);

			mi = typeof(String).GetMethod("ToString", new Type[]{typeof(IFormatProvider)});
			MethodReference mr1b = m_module.Import(mi);

			mi = typeof(Object).GetMethod("ToString");
			MethodReference mr2 = m_module.Import(mi);

			Assert.IsTrue(MethodMatcher.Match(mr1a, mr2));
			Assert.IsFalse(MethodMatcher.Match(mr1b, mr2));
		}

		// Generic bases work.
//		[Test]
		public void GenericBase()
		{
			SR.MethodInfo mi = typeof(Int16).GetMethod("CompareTo", new Type[]{typeof(Int16)});
			MethodReference mr1 = m_module.Import(mi);
			
			Type[] types = typeof(Int16).GetInterfaces();
			foreach (Type t in types)
			{
				if (t.IsGenericType)
				{
					mi = t.GetMethod("CompareTo");
					if (mi != null)
					{
						MethodReference mr2 = m_module.Import(mi);	// this throws
						Console.WriteLine(mr1);
						Console.WriteLine(mr2);
		
						Assert.IsTrue(MethodMatcher.Match(mr1, mr2));
					}
				}
			}
		}

		private AssemblyDefinition m_assembly;
		private ModuleDefinition m_module;
	} 
}
