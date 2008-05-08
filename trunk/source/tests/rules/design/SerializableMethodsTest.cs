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
using System.Runtime.Serialization;
using System.Reflection;

namespace Smokey.Tests
{
	[TestFixture]
	public class SerializableMethodsTest : MethodTest
	{	
		#region Test classes
		internal class GoodCases
		{
			public void Use(StreamingContext context)
			{
				A(context);
				B(context);
				C(1);
			}
			
			[OnSerializingAttribute]
			private void A(StreamingContext context)
			{
			}

			[OnDeserializingAttribute]
			private void B(StreamingContext context)
			{
			}

			private void C(int foo)
			{
			}
		}

		internal class BadCases
		{
			public void Use(StreamingContext context)
			{
				A(context);
				B(context);
				C(context, 1);
			}
			
			[OnSerializingAttribute]
			protected void A(StreamingContext context)
			{
			}

			[OnDeserializingAttribute]
			private int B(StreamingContext context)
			{
				return 0;
			}

			[OnDeserializingAttribute]
			private void C(StreamingContext context, int dummy)
			{
			}
		}
		#endregion
		
		// test code
		public SerializableMethodsTest() : base(
			new string[]{"GoodCases.A", "GoodCases.B", "GoodCases.C"},
			new string[]{"BadCases.A", "BadCases.B", "BadCases.C"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new SerializableMethodsRule(cache, reporter);
		}
	} 
}
