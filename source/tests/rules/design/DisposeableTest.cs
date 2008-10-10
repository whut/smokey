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

namespace Smokey.Tests
{
	[TestFixture]
	public class DisposeableTest : TypeTest
	{	
		// test cases
		public class Good1					// not IDisposable
		{
			public void Work()
			{
				m_writer.WriteLine("hey");
			}

			private StringWriter m_writer = new StringWriter();
		}
				
		public class Good2 : IDisposable
		{
			public Good2()
			{
			}
			
			public void Work()
			{
				if (m_disposed)		
					throw new ObjectDisposedException(GetType().Name);
					
				m_writer.WriteLine("hey");
			}

			public void Dispose()
			{
				m_writer.Dispose();
				m_disposed = true;
			}
			
			private bool m_disposed;
			private StringWriter m_writer = new StringWriter();
		}			

		public class Good3 : IDisposable
		{
			~Good3()
			{
				Dispose(false);
			}
						
			public void Work()
			{
				if (m_disposed)		
					throw new ObjectDisposedException(GetType().Name);
			}
		
			public void Dispose()
			{
				Dispose(true);
				GC.SuppressFinalize(this);
			}
			
			private void Dispose(bool disposing)
			{
				if (!m_disposed)
					m_disposed = true;
			}
			
			private bool m_disposed = false; 
		}			

		public class NoThrow1 : IDisposable
		{
			public void Work()			// doesn't throw ObjectDisposedException
			{
				Console.WriteLine("hey");
			}

			public void Dispose()
			{
			}
		}			

		public class NoThrow2 : Good2
		{
			public void MoreWork()			// doesn't throw ObjectDisposedException
			{
				Console.WriteLine("hey");
			}
		}			

		public class FieldNotDisposed : IDisposable
		{			
			public void Work()
			{
				if (m_disposed)		
					throw new ObjectDisposedException(GetType().Name);
					
				m_writer1.WriteLine("hey");
				m_writer2.WriteLine("hey");
			}

			public void Dispose()
			{
				m_writer1.Dispose();		// only one writer is disposed
				m_disposed = true;
			}
			
			private bool m_disposed;
			private StringWriter m_writer1 = new StringWriter();
			private StringWriter m_writer2 = new StringWriter();
		}			

		public class NoSuppress : IDisposable
		{
			~NoSuppress()
			{
				Dispose(false);
			}
						
			public void Work()
			{
				if (m_disposed)		
					throw new ObjectDisposedException(GetType().Name);
			}
		
			public void Dispose()		// does not call SuppressFinalize
			{
				Dispose(true);
			}
			
			private void Dispose(bool disposing)
			{
				if (!m_disposed)
					m_disposed = true;
			}
			
			private bool m_disposed = false; 
		}			

		// test code
		public DisposeableTest() : base(
			new string[]{"Good1", "Good2", "Good3"},
			new string[]{"NoThrow1", "NoThrow2", "FieldNotDisposed", "NoSuppress"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new DisposeableRule(cache, reporter);
		}
	} 
}