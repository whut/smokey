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
using System.Reflection;
using System.Runtime.Serialization;
using Smokey.Framework.Support;
using Smokey.Internal.Rules;

namespace Smokey.Tests
{
	[TestFixture]
	public class MonoNamingTest : TypeTest
	{	
		#region Test cases
		public interface IGoodInterface
		{
			void Work(); 
		}

		public class GoodCase
		{
			public void MethodName(int paramName)
			{
				Console.WriteLine("len: {0}", paramName + int_field);			
			}
			
			public void GetID(int paramName)
			{
				Console.WriteLine("len: {0}", paramName + int_field + line1);			
			}
			
			public int AutoProp {get; set;}
			
			private int int_field;
			private int line1;
		}

		[Serializable]
		public class GoodSerializable 
		{
			public void MethodName(int paramName)
			{
				Console.WriteLine("len: {0}", intField);			
			}
    
			private int intField;	// special case for Serializable classes
		}

		public interface BadInterface
		{
			void Work();
		}

		public class badClassName
		{
			public void MethodName(int paramName)
			{
				Console.WriteLine("len: {0}", paramName + int_field);			
			}
			
			private int int_field;
		}

		public class BadFieldName
		{
			public void MethodName(int paramName)
			{
				Console.WriteLine("len: {0}", paramName + mIntField);			
			}
			
			private int mIntField;
		}

		public class BadFieldName2
		{
			public void MethodName(int paramName)
			{
				Console.WriteLine("len: {0}", paramName + lpszName.Length);			
			}
			
			private string lpszName;
		}

		public class BadFieldName3
		{
			public void MethodName(int paramName)
			{
				Console.WriteLine("len: {0}", paramName + mName);			
			}
			
			private int mName;
		}

		public class BadFieldName4
		{
			public void MethodName(int paramName)
			{
				Console.WriteLine("len: {0}", paramName + Name);			
			}
			
			private int Name;
		}

		public class BadFieldName5
		{
			public void MethodName(int paramName)
			{
				Console.WriteLine("len: {0}", paramName + m_name);			
			}
			
			private int m_name;
		}

		public class BadFieldName6
		{
			public void MethodName(int paramName)
			{
				Console.WriteLine("len: {0}", paramName + _first_name);			
			}
			
			private int _first_name;
		}

		public class BadMethods1
		{
			public void badName(int paramName)
			{
				Console.WriteLine("len: {0}", paramName + int_field);			
			}
			
			public void BadParam(int _params)
			{
				Console.WriteLine("len: {0}", _params + int_field);			
			}
			
			public void HungarianParam(int dwLength)
			{
				Console.WriteLine("len: {0}", dwLength + int_field);			
			}
			
			public void AbrevIDDMethod(int length)
			{
				Console.WriteLine("len: {0}", length + int_field);			
			}
			
			public void Underscores(int num_items)
			{
				Console.WriteLine("len: {0}", num_items + int_field);			
			}
			
			private int int_field;
		}
		#endregion
		
		// test code
		public MonoNamingTest() : base(
			new string[]{"GoodCase", "IGoodInterface", "GoodSerializable"},
			new string[]{"badClassName", "BadFieldName", 
				"BadFieldName2", "BadFieldName3", "BadFieldName4", "BadFieldName5", "BadFieldName6",
				"BadMethods1", "BadInterface"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new MonoNamingRule(cache, reporter);
		}
	} 
}

