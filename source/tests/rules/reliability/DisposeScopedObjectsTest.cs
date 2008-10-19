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

#if OLD
namespace Smokey.Tests
{
	[TestFixture]
	public class DisposeScopedObjectsTest : MethodTest
	{	
		#region Test classes
		private class GoodCases
		{
			public string ReadAll1(string path)
			{
				string text;
				
				using (StreamReader stream = new StreamReader(path)) 
				{
					string temp = stream.ReadToEnd();
					text = temp;
				}
				
				return text;
			}

			public int ReadAll2(string path, int x)
			{				
				StreamReader stream = new StreamReader(path);
				char[] buffer = new char[10];
				int count = stream.Read(buffer, x, 10 - x);
				stream.Dispose();
				
				return count;
			}

			public string ReadAll3(string path)
			{				
				string text;
				
				StreamReader stream = new StreamReader(path);
				
				try
				{
					text = stream.ReadToEnd();
				}
				finally
				{
					stream.Dispose();
				}
				
				return text;
			}

			public string ReadAll4(string path)
			{				
				if (m_stream == null)
					m_stream = new StreamReader(path);
									
				return m_stream.ReadToEnd();
			}

			public string ReadAll5(string path)
			{				
				if (m_stream == null)
				{
					StreamReader stream = new StreamReader(path);
					m_stream = stream;
				}
				
				return m_stream.ReadToEnd();
			}
			
			private StreamReader m_stream;
		}

		private class BadCases
		{
			public int ReadAll1(string path, int x)
			{				
				StreamReader stream = new StreamReader(path);
				char[] buffer = new char[10];
				int count = stream.Read(buffer, x, 10 - x);
				
				return count;
			}

			public string ReadAll2(string path)
			{				
				StreamReader stream = new StreamReader(path);
				string text = stream.ReadToEnd();
				
				return text;
			}

			public string ReadAll3(string path)
			{				
				string text;
				
				StreamReader stream = new StreamReader(path);
				
				try
				{
					text = stream.ReadToEnd();
				}
				finally
				{
					Console.WriteLine("where's the dispose?");
				}
				
				return text;
			}

			public string ReadAll4(string path)
			{				
				string text;
				
				StreamReader stream = new StreamReader(path);
				
				try
				{
					text = stream.ReadToEnd();
				}
				finally
				{
					if (stream != null)
						Console.WriteLine("where's the dispose?");
				}
				
				return text;
			}
		}
		#endregion
		
		// test code
		public DisposeScopedObjectsTest() : base(
			new string[]{"GoodCases.ReadAll1", "GoodCases.ReadAll2", "GoodCases.ReadAll3", 
			"GoodCases.ReadAll4", "GoodCases.ReadAll5"},
			
			new string[]{"BadCases.ReadAll1", "BadCases.ReadAll2", "BadCases.ReadAll3", "BadCases.ReadAll4"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new DisposeScopedObjectsRule(cache, reporter);
		}
	} 
}
#endif
