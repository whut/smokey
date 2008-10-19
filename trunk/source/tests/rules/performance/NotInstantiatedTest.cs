// Copyright (C) 2008 Jesse Jones
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
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Smokey.Framework.Support;
using Smokey.Internal.Rules;

#if OLD
namespace Smokey.Tests
{
	[TestFixture]
	public class NotInstantiatedTest : AssemblyTest
	{	
		#region Test classes
		internal class Base
		{		
		}

		// -------------------------
		internal class Good1a : Base
		{		
		}

		internal class Good1b
		{		
			public object Create()
			{
				return new object[]{new Good1a(), new Good1b()};
			}
		}

		// -------------------------
		internal struct Good2a
		{		
		}

		internal class Good2b
		{		
			public object Create()
			{
				return new object[]{new Good2a(), new Good2b()};
			}
		}

		// -------------------------
		internal class Bad1a : Base		// never instantiated
		{		
		}

		internal class Bad1b
		{		
			public object Create()
			{
				return new object[]{new Base(), new Bad1b()};
			}
		}

		// -------------------------
		internal struct Bad2a		// never instantiated
		{		
		}

		internal class Bad2b
		{		
			public object Create()
			{
				return new Bad2b();
			}
		}

		// -------------------------
		internal static class Tuple
		{
			public static Tuple2<T0, T1> Make<T0, T1>(T0 value0, T1 value1)
			{
				return new Tuple2<T0, T1>(value0, value1);
			}
		}
		
		internal struct Tuple2<T0, T1> 
		{	
			public Tuple2(T0 value0, T1 value1)
			{
				m_value0 = value0;
				m_value1 = value1;
			}
			
			public T0 First		{get {return m_value0;}}
			public T1 Second	{get {return m_value1;}}
									
			public override string ToString()
			{
				return string.Format("[{0}, {1}]", m_value0, m_value1);
			}
											
			private T0 m_value0;
			private T1 m_value1;
		}
		#endregion
		
		// test code
		public NotInstantiatedTest() : base(
			new string[]{"Base+Good1a+Good1b", "Good2a+Good2b", "Tuple+Tuple2`2"},
			new string[]{"Base+Bad1a+Bad1b", "Bad2a+Bad2b"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new NotInstantiatedRule(cache, reporter);
		}
	} 
}
#endif
