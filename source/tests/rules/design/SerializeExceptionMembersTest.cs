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
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Smokey.Framework.Support;
using Smokey.Internal.Rules;
using System.Runtime.Serialization;

namespace Smokey.Tests
{
	[TestFixture]
	public class SerializeExceptionMembersTest : TypeTest
	{	
		// test classes
		[Serializable]
		public class Good1 : Exception
		{
		}  			
									
		public class Good2 : Exception
		{
			public static string ms_data;
		}  			
									
		public class Good3 : Exception
		{
			public override void GetObjectData(SerializationInfo info, StreamingContext context)
			{
				info.AddValue("m_data", m_data, typeof(string));
				base.GetObjectData(info, context);
			}

			public string m_data;
		}  			
									
		public class Bad1 : Exception
		{
			public string m_data;
		}  			
					
		// test code
		public SerializeExceptionMembersTest() : base(
			new string[]{"Good1", "Good2", "Good3"},
			new string[]{"Bad1"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new SerializeExceptionMembersRule(cache, reporter);
		}
	} 
}

