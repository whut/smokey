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
	public class BaseDisposeableTest : TypeTest
	{	
		// test cases
		internal class Good1 : IDisposable
		{ 
			protected bool Disposed
			{
				get {return m_disposed;}
			}
					
			protected virtual void Dispose(bool disposing)	// has
			{
				if (!m_disposed)
				{
					m_disposed = true;
				}
			}
			
			public void Dispose()
			{
				Dispose(true);
				GC.SuppressFinalize(this);
			}
			
			private bool m_disposed; 
		}
		
		internal sealed class Good2 : IDisposable		// sealed
		{ 
			private void DoDispose(bool disposing)	
			{
				if (!m_disposed)
				{
					m_disposed = true;
				}
			}
			
			public void Dispose()
			{
				DoDispose(true);
				GC.SuppressFinalize(this);
			}
			
			private bool m_disposed; 
		}
		
		internal class Good3						// not disposable
		{ 
			private void DoDispose(bool disposing)	
			{
				if (!m_disposed)
				{
					m_disposed = true;
				}
			}
			
			public void Dispose()
			{
				DoDispose(true);
				GC.SuppressFinalize(this);
			}
			
			private bool m_disposed; 
		}
		
		internal class Good4 : Good1				// indirectly IDisposable
		{ 
			public void Work()
			{
				Console.WriteLine("hey");
			}
		}
		
		internal class Good5 : Good1, IDisposable	// this IDisposable doesn't count
		{ 
			public void Work()
			{
				Console.WriteLine("hey");
			}
		}
		
		public interface Good6a : IDisposable
		{
			string Site { get; set; }
		}

		public interface Good6b : Good6a, IDisposable
		{
			string BindingContext { get; set; }
			string DataBindings { get; }
		}

		internal class Bad1 : IDisposable
		{ 
			protected bool Disposed
			{
				get {return m_disposed;}
			}
					
			protected virtual void OnDispose(bool disposing)	// wrong name
			{
				if (!m_disposed)
				{
					m_disposed = true;
				}
			}
			
			public void Dispose()
			{
				OnDispose(true);
				GC.SuppressFinalize(this);
			}
			
			private bool m_disposed; 
		}

		// test code
		public BaseDisposeableTest() : base(
			new string[]{"Good1", "Good2", "Good3", "Good4", "Good5", "Good6a", "Good6b"},
			new string[]{"Bad1"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new BaseDisposeableRule(cache, reporter);
		}
	} 
}