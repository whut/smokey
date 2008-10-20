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
using Smokey.Framework;
using Smokey.Framework.Support;
using Smokey.Internal.Rules;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;

namespace Smokey.Tests
{
	[TestFixture]
	public class NonSerializableFieldTest : TypeTest
	{	
		#region Test classes
		public class Non
		{
			public int m_value;
		}

		[SerializableAttribute]
		public class Good1
		{
			public Good1(string name)
			{
				m_name = name;
				Console.WriteLine(m_name + m_non);
			}
						
			private string m_name;
			[NonSerializedAttribute]
			private Non m_non;
		}

		[SerializableAttribute]
		public class Good2
		{
			public Good2(string name)
			{
				m_name = name;
				Console.WriteLine(m_name);
			}
						
			private string m_name;
		}

		[SerializableAttribute]
		public class Good3 : ISerializable
		{
			public Good3(string name)
			{
				m_name = name + m_non;
			}
			
			protected Good3(SerializationInfo info, StreamingContext context)
			{
				m_name = info.GetString("name");
			}
			
			public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
			{
				info.AddValue("name", m_name);
			}
						
			private string m_name;
			private Non m_non;
		}

		[SerializableAttribute]
		public class Good4
		{
			public Good4(Log.Level level)
			{
				m_level = level;
				Console.WriteLine(m_level);
			}
						
			private Log.Level m_level;
		}

		[SerializableAttribute]
		public class Bad1
		{
			public Bad1(string name)
			{
				m_name = name;
				Console.WriteLine(m_name + m_non);
			}
						
			private string m_name;
			private Non m_non;
		}
		#endregion
		
		// test code
		public NonSerializableFieldTest() : base(
			new string[]{"Good1", "Good2", "Good3", "Good4"},
			new string[]{"Bad1"},
			new string[]{"Non"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new NonSerializableFieldRule(cache, reporter);
		}
	} 
}

