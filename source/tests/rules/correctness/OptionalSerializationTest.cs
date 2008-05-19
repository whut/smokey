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
	public class OptionalSerializationTest : TypeTest
	{	
		#region Test classes
		[Serializable]
		internal sealed class Good1
		{		
			public void Use()
			{
				Console.WriteLine(m_optional);
			}
			
			[OnDeserialized]
			protected void OnDeserialized(StreamingContext context)
			{
				m_optional = 5;
			}
		
			[OptionalField(VersionAdded = 2)]
			private int m_optional = 5;	
		}

		[Serializable]
		internal sealed class Good2
		{		
			public void Use()
			{
				Console.WriteLine(m_optional);
			}
			
			[OnDeserializing]
			protected void OnDeserialized(StreamingContext context)
			{
				m_optional = 5;
			}
		
			[OptionalField(VersionAdded = 2)]
			private int m_optional = 5;	
		}

		[Serializable]
		internal sealed class Bad1
		{		
			public void Use()
			{
				Console.WriteLine(m_optional);
			}
					
			[OptionalField(VersionAdded = 2)]
			private int m_optional = 5;	
		}
		#endregion
		
		// test code
		public OptionalSerializationTest() : base(
			new string[]{"Good1", "Good2"},
			new string[]{"Bad1"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new OptionalSerializationRule(cache, reporter);
		}
	} 
}
