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
using System.ComponentModel;
using System.IO;
using System.Reflection;
using Smokey.Framework.Support;
using Smokey.Internal.Rules;

#if OLD
namespace Smokey.Tests
{
	[TestFixture]
	public class DisposableFieldsTest : TypeTest
	{	
		#region Test classes
		private class GoodCase : IDisposable
		{
			~GoodCase()		
			{					
				DoDispose(false);
			}
				
			public void WriteLine(string line)
			{
				m_writer.WriteLine(line);
			}
		
			public void Dispose()
			{
				DoDispose(true);
				GC.SuppressFinalize(this);
			}
		
			private void DoDispose(bool disposing)
			{
				if (!m_disposed)
				{
					if (disposing)
						m_writer.Dispose();
								
					m_disposed = true;
				}
			}
				
			private StringWriter m_writer = new StringWriter();
			private bool m_disposed = false;
		}
				
		private class GoodCase2
		{
			public GoodCase2()
			{
			}
					
			public void WriteLine(string line)
			{
				ms_writer.WriteLine(line);
			}
		
			private static StringWriter ms_writer = new StringWriter();
		}

		private class GoodCase3
		{
			public GoodCase3(StringWriter writer)
			{
				m_writer = writer;
			}
					
			public void WriteLine(string line)
			{
				m_writer.WriteLine(line);
			}
		
			private StringWriter m_writer;
		}

		private class GoodCase4 : Component
		{
			public void WriteLine(string line)
			{
				m_writer.WriteLine(line);
			}
				
			protected override void Dispose(bool disposing)
			{
				if (disposing)
					m_writer.Dispose();
				
				base.Dispose(disposing);
			}
				
			private StringWriter m_writer = new StringWriter();
		}
				
		private class GoodCase5
		{
			public GoodCase5()
			{
				input_stream = Stream.Null;
				ints = new List<int>();
			}
					
			public Stream InputStream {
				get { return input_stream; }
			}

			public List<int> Ints {
				get { return ints; }
			}

			Stream input_stream;
			List<int> ints;
		}

		private class BadCase
		{
			public BadCase()
			{
			}
					
			public void WriteLine(string line)
			{
				m_writer.WriteLine(line);
			}
		
			private StringWriter m_writer = new StringWriter();
		}

		private class BadCase2
		{
			public BadCase2()
			{
				m_writer = new StringWriter();
			}
					
			public void WriteLine(string line)
			{
				m_writer.WriteLine(line);
			}
		
			private StringWriter m_writer;
		}

		private class BadCase3
		{
			public BadCase3()
			{
				DoInit();
			}
					
			public void WriteLine(string line)
			{
				m_writer.WriteLine(line);
			}
			
			private void DoInit()
			{
				m_writer = new StringWriter();
			}
		
			private StringWriter m_writer;
		}
		#endregion
		
		// test code
		public DisposableFieldsTest() : base(
			new string[]{"GoodCase", "GoodCase2", "GoodCase3", "GoodCase4", "GoodCase5"},
			new string[]{"BadCase", "BadCase2", "BadCase3"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new DisposableFieldsRule(cache, reporter);
		}
	} 
}
#endif
