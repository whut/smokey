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

#if OLD
namespace Smokey.Tests
{
	[TestFixture]
	public class ExceptionConstructorsTest : TypeTest
	{	
		// test classes
		public class Good1 : Exception
		{
			public Good1()
			{
			}
			
			public Good1(string message) : base(message) 
			{
			}
			
			public Good1(string message, Exception innerException) : 
				base (message, innerException)
			{
			}
			
			protected Good1(SerializationInfo info, StreamingContext context) : 
				base(info, context)
			{
			}
		}  			
			
		public class Good2 : Exception
		{
			public Good2()
			{
			}
						
			public Good2(string message, Exception innerException, string foo) : 
				base (message, innerException)
			{
			}
			
			protected Good2(SerializationInfo info, StreamingContext context) : 
				base(info, context)
			{
			}
		}  			
			
		public class Good3 : Exception
		{
			public Good3()
			{
			}
			
			public Good3(string message) : base(message) 
			{
			}
			
			public Good3(string message, Exception innerException, string foo) : 
				base (message + foo, innerException)
			{
			}
			
			protected Good3(SerializationInfo info, StreamingContext context) : 
				base(info, context)
			{
			}
		}  			
			
		public class Good4 : Exception
		{
			public Good4(string message) : base(message) 
			{
			}
			
			public Good4(string message, Exception innerException) : 
				base (message, innerException)
			{
			}
			
			protected Good4(SerializationInfo info, StreamingContext context) : 
				base(info, context)
			{
			}
		}  			
			
		public class Good5 : Exception
		{
			public Good5(int x)
			{
			}
			
			public Good5(string message) : base(message) 
			{
			}
			
			public Good5(string message, Exception innerException) : 
				base (message, innerException)
			{
			}
			
			protected Good5(SerializationInfo info, StreamingContext context) : 
				base(info, context)
			{
			}
		}  			
			
		public class Bad3 : Exception
		{
			public Bad3()
			{
			}
			
			public Bad3(string message) : base(message) 
			{
			}
						
			protected Bad3(SerializationInfo info, StreamingContext context) : 
				base(info, context)
			{
			}
		}  			
			
		public class Bad4 : Exception
		{
			public Bad4()
			{
			}
			
			public Bad4(string message) : base(message) 
			{
			}
			
			public Bad4(string message, Exception innerException) : 
				base (message, innerException)
			{
			}
		}  			
			
		public class Bad5 : Exception
		{
			public Bad5(string message) : base(message) 
			{
			}
		}  			
			
		// test code
		public ExceptionConstructorsTest() : base(
			new string[]{"Good1", "Good2", "Good3", "Good4", "Good5"},
			new string[]{"Bad3", "Bad4", "Bad5"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new ExceptionConstructorsRule(cache, reporter);
		}
	} 
}
#endif
