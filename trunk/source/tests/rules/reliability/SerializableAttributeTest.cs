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
using System.Runtime.Serialization;

namespace Smokey.Tests
{
	[TestFixture]
	public class SerializableAttributeTest : TypeTest
	{	
		#region Test classes
		[Serializable]
		public class Good1
		{
			public Good1(string name)
			{
				m_name = name;
			}
						
			public void Work()
			{
				Console.WriteLine(m_name);
			}
						
			private string m_name;
		}

		[Serializable]
		public class Good2 : Good1
		{
			public Good2(string name) : base(name)
			{
			}
		}

		public class Good3
		{
			public Good3(string name)
			{
			}
		}

		public class Good4
		{
			public delegate void Hook(int x);
			
			public Good4(Hook h)
			{
				hook = h;
			}
			
			public void Work()
			{
				lock (lock1)
				{
					Console.WriteLine("hello");
				}
				
				hook(2);
			}
			
			private Hook hook;
			private object lock1 = new object();
		}

		public enum Good5 {One, Two, Three};

		public struct Good6
		{
			public int data;
		}

		public delegate void Good7(int x);

		public class Bad1 : Good1
		{
			public Bad1(string name) : base(name)
			{
			}
		}
		#endregion
		
		// test code
		public SerializableAttributeTest() : base(
			new string[]{"Good1", "Good2", "Good3", "Good4", "Good5", "Good6", "Good7"},
			new string[]{"Bad1"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new SerializableAttributeRule(cache, reporter);
		}
	} 
}
