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
	public class DerivedDisposeableTest : TypeTest
	{	
		// test cases
		internal class GoodBase1 : IDisposable
		{ 
			~GoodBase1()
			{
				Dispose(false);
			}
					
			protected bool Disposed
			{
				get {return m_disposed;}
			}
					
			protected virtual void Dispose(bool disposing)
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
		
		internal abstract class GoodBase2 : IDisposable
		{ 
			~GoodBase2()
			{
				Dispose(false);
			}
					
			protected abstract void Dispose(bool disposing);
			
			public void Dispose()
			{
				Dispose(true);
				GC.SuppressFinalize(this);
			}			
		}
				
		internal class GoodDerived1 : GoodBase1
		{ 
			protected override void Dispose(bool disposing)
			{
				if (!Disposed)
				{
					base.Dispose(disposing);
				}
			}	
		}

		internal class GoodDerived2 : GoodBase2
		{ 
			protected override void Dispose(bool disposing)
			{
				// base is abstract
			}	
		}

		internal class NoBaseCall : GoodBase1
		{ 
			protected override void Dispose(bool disposing)
			{
				if (!Disposed)		// doesn't call base Dispose
				{
				}
			}	
		}

		internal class BadBaseCall1 : GoodBase1
		{ 
			protected override void Dispose(bool disposing)
			{
				if (!Disposed)		
				{
					base.Dispose();		// wrong dispose method
				}
			}	
		}

		internal class BadBaseCall2 : GoodBase1
		{ 
			protected override void Dispose(bool disposing)
			{
				if (!Disposed)		
				{
					Dispose();		// wrong dispose method
				}
			}	
		}

		internal class BadBaseCall3 : GoodBase1
		{ 
			public void Work()
			{
				m_writer.WriteLine("hey");
			}

			protected override void Dispose(bool disposing)
			{
				if (!Disposed)		
				{
					m_writer.Dispose();		// wrong dispose method
				}
			}	

			private StringWriter m_writer = new StringWriter();
		}

		internal class HasFinalizer : GoodBase1
		{ 
			~HasFinalizer()
			{
				Dispose(false);
			}
			
			protected override void Dispose(bool disposing)
			{
				if (!Disposed)		
				{
					base.Dispose(disposing);
				}
			}	
		}

		// test code
		public DerivedDisposeableTest() : base(
			new string[]{"GoodBase1", "GoodBase2", "GoodDerived1", "GoodDerived2"},
			new string[]{"NoBaseCall", "BadBaseCall1", "BadBaseCall2", "BadBaseCall3",
				"HasFinalizer"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new DerivedDisposeableRule(cache, reporter);
		}
	} 
}