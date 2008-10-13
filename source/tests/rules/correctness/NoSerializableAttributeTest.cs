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
using System.Reflection;
using System.Runtime.Serialization;

namespace Smokey.Tests
{
	[TestFixture]
	public class NoSerializableAttributeTest : TypeTest
	{	
		#region Test classes
		[Serializable]
		public class Good1 : ISerializable
		{
			public Good1(string name)
			{
				m_name = name;
			}
			
			protected Good1(SerializationInfo info, StreamingContext context)
			{
				m_name = info.GetString("name");
			}
			
			public void GetObjectData(SerializationInfo info, StreamingContext context)
			{
				info.AddValue("name", m_name);
			}
			
			private string m_name;
		}

		public class Bad1 : ISerializable
		{
			public Bad1(string name)
			{
				m_name = name;
			}
			
			protected Bad1(SerializationInfo info, StreamingContext context)
			{
				m_name = info.GetString("name");
			}
			
			public void GetObjectData(SerializationInfo info, StreamingContext context)
			{
				info.AddValue("name", m_name);
			}
			
			private string m_name;
		}

		private class Bad2 : ISerializable
		{
			public Bad2(string name)
			{
				m_name = name;
			}
			
			protected Bad2(SerializationInfo info, StreamingContext context)
			{
				m_name = info.GetString("name");
			}
			
			public void GetObjectData(SerializationInfo info, StreamingContext context)
			{
				info.AddValue("name", m_name);
			}
			
			private string m_name;
		}
		#endregion
		
		// test code
		public NoSerializableAttributeTest() : base(
			new string[]{"Good1"},
			new string[]{"Bad1", "Bad2"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new NoSerializableAttributeRule(cache, reporter);
		}
	} 
}
