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
using System.Runtime.Serialization;

namespace Smokey.Tests
{
	[TestFixture]
	public class BaseSerializable2Test : MethodTest
	{	
		#region Test classes
		public abstract class Base : ISerializable
		{
			public Base()
			{
				m_baseValue = 3;
			}
			
			public Base(int value)
			{
				m_baseValue = value;
			}
			
			protected Base(SerializationInfo info, StreamingContext context)
			{
				m_baseValue = info.GetInt32("baseValue");
			}
						
			public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
			{
				info.AddValue("baseValue", m_baseValue);
			}
				
			private int m_baseValue;
		}

		public class Good1 : Base
		{
			public Good1()
			{
				m_derivedValue = 4;
			}
			
			protected Good1(SerializationInfo info, StreamingContext context) 
				: base(info, context)
			{
				m_derivedValue = info.GetInt32("derivedValue");
			}
						
			public override void GetObjectData(SerializationInfo info, StreamingContext context)
			{
				info.AddValue("derivedValue", m_derivedValue);
				base.GetObjectData(info, context);
			}
				
			private int m_derivedValue;
		}

		private class Good2 : Base
		{
			protected Good2(SerializationInfo info, StreamingContext context) 
				: base(info, context)
			{
				m_derivedValue = info.GetInt32("derivedValue");
			}
						
			public override void GetObjectData(SerializationInfo info, StreamingContext context)
			{
				info.AddValue("derivedValue", m_derivedValue);
			}
				
			private int m_derivedValue;
		}

		public class Bad1 : Base
		{
			protected Bad1(SerializationInfo info, StreamingContext context)
				: base(info, context)
			{
				m_derivedValue = info.GetInt32("derivedValue");
			}
						
			public override void GetObjectData(SerializationInfo info, StreamingContext context)
			{
				info.AddValue("derivedValue", m_derivedValue);
			}
				
			private int m_derivedValue;
		}
		#endregion
		
		// test code
		public BaseSerializable2Test() : base(
			new string[]{"Good1.GetObjectData", "Good2.GetObjectData"},
			new string[]{"Bad1.GetObjectData"},
			new string[]{"Base.Base"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new BaseSerializable2Rule(cache, reporter);
		}
	} 
}
