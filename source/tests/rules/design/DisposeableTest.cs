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
using System.Runtime.InteropServices;
using System.Reflection;
using Smokey.Framework.Support;
using Smokey.Internal.Rules;

namespace Smokey.Tests
{
	[TestFixture]
	public class DisposeableTest : TypeTest
	{	
		// test cases
		public sealed class Good1					// not IDisposable
		{
			public void Work()
			{
				m_writer.WriteLine("hey");
			}

			private StringWriter m_writer = new StringWriter();
		}
				
		public sealed class Good2 : IDisposable
		{
			public Good2()
			{
			}
			
			public void Work()
			{
				if (m_disposed)		
					throw new ObjectDisposedException(GetType().Name);
					
				if (DidWork != null)
					DidWork(this, EventArgs.Empty);
					
				m_writer.WriteLine("hey");
			}

			public void Dispose()
			{
				if (m_writer != null)
					m_writer.Dispose();
				m_disposed = true;
			}
			 
			public event EventHandler DidWork;

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

			public event EventHandler Event6
			{
				add {m_eventTable.Add(ms_nameKey, value);}
				remove {m_eventTable.Remove(ms_nameKey);}
			}
		
			public void Dispose()
			{
				Dispose(true);
				GC.SuppressFinalize(this);
			}
			
			protected virtual void Dispose(bool disposing)
			{
				if (!m_disposed)
					m_disposed = true;
			}
			
			protected bool m_disposed; 
			private Dictionary<object, EventHandler> m_eventTable = new Dictionary<object, EventHandler>();
			private static object ms_nameKey = new object();
		}			

		public sealed class Good4 : Good3
		{
			public void Foo()
			{
				if (m_disposed)		
					throw new ObjectDisposedException(GetType().Name);
					
				Console.WriteLine(DoDispose());
			}
			
			private string DoDispose()		// private so OK
			{
				return "hmm";
			}
		}			

		public class Good5 : IDisposable
		{
			~Good5()
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
				try
				{
					if (GetType().Name == "gah")
						throw new Exception("ack");
						
					Dispose(true);
					GC.SuppressFinalize(this);
				}
				catch
				{
				}
			}
			
			protected virtual void Dispose(bool disposing)
			{
				if (!m_disposed)
					m_disposed = true;
			}
			
			protected bool m_disposed; 
		}			

		public sealed class Good6 : IDisposable		// classes can have native resources
		{
			~Good6()
			{
				Dispose(false);
			}
					
			public void Dispose()
			{
				Dispose(true);
				GC.SuppressFinalize(this);
			}
			
			private void Dispose(bool disposing)
			{
				if (!m_disposed)
				{
					CloseHandle(m_data);
					m_disposed = true;
				}
			}
			
			[DllImport("Kernel32")]
			public extern static void CloseHandle(IntPtr handle);

			private bool m_disposed; 
			private IntPtr m_data;
		}			

		public class Good7 : IDisposable
		{
			public Good7()
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
				Dispose(true);
			}
			
			protected virtual void Dispose(bool disposing)
			{
				if (!m_disposed)
				{
					if (m_writer != null)
						m_writer.Dispose();
						
					m_disposed = true;
				}
			}
			
			private bool m_disposed;
			private StringWriter m_writer = new StringWriter();
		}			

		public class Good8 : IDisposable
		{
			public Good8(StringWriter writer)
			{
				m_writer = writer;
			}
			
			public void Work()
			{
				if (m_disposed)		
					throw new ObjectDisposedException(GetType().Name);
					
				m_writer.WriteLine("hey");
			}

			public void Dispose()
			{
				Dispose(true);
			}
			
			protected virtual void Dispose(bool disposing)
			{
				m_writer.Dispose();			// OK because we didn't new the field				
				m_disposed = true;
			}
			
			private bool m_disposed;
			private StringWriter m_writer;
		}			

		public class Good9 : IDisposable
		{
			public Good9(int value)
			{
				m_value = value;
			}
			
			public void Work()
			{
				if (m_disposed)		
					throw new ObjectDisposedException(GetType().Name);
					
				Console.WriteLine(m_value.ToString());
			}

			public void Dispose()
			{
				Dispose(true);
			}
			
			protected virtual void Dispose(bool disposing)
			{
				if (!m_disposed)
				{
					Console.WriteLine(m_value.ToString());	// not nullable so don't need an if check
					m_disposed = true;
				}
			}
			
			private bool m_disposed;
			private int m_value;
		}			

		public class Good10 : IDisposable
		{
			~Good10()
			{
				Dispose(false);
			}
						
			public void Work()
			{
				if (m_disposed)		
					throw new ObjectDisposedException(GetType().Name);
				++m_id;
			}
			
			public int Id
			{
				get {return m_id;}
			}
		
			public string Name {get; set;}

			public void Dispose()
			{
				Dispose(true);
				GC.SuppressFinalize(this);
			}
			
			protected virtual void Dispose(bool disposing)
			{
				if (!m_disposed)
					m_disposed = true;
			}
			
			protected bool m_disposed;
			private int m_id;
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

		public class NoThrow2 : Good3
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
				if (m_writer1 != null)
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
			
			private bool m_disposed; 
		}			

		public class BogusDisposeName1 : Good3
		{
			public new string Dispose()
			{
				return "hmm";
			}
		}			

		public class BogusDisposeName2 : IDisposable
		{
			public void Work()
			{
				if (m_disposed)		
					throw new ObjectDisposedException(GetType().Name);
					
				m_writer.WriteLine("hey");
			}

			public void Dispose()
			{
				if (m_writer != null)
					m_writer.Dispose();
				m_disposed = true;
			}
			
			public void Dispose(int hmm)
			{
			}
			
			private bool m_disposed;
			private StringWriter m_writer = new StringWriter();
		}			

		public class BogusDisposeName3 : IDisposable
		{
			public void Work()
			{
				if (m_disposed)		
					throw new ObjectDisposedException(GetType().Name);
					
				m_writer.WriteLine("hey");
			}

			public void Dispose()
			{
				if (m_writer != null)
					m_writer.Dispose();
				m_disposed = true;
			}
			
			public void Dispose(bool disposing, int hmm)
			{
			}
			
			private bool m_disposed;
			private StringWriter m_writer = new StringWriter();
		}			

		public class BogusDisposeName4 : Good3
		{
			public string OnDispose()
			{
				if (m_disposed)		
					throw new ObjectDisposedException(GetType().Name);
					
				return "hmm";
			}
		}			

		public class BogusDisposeName5 : Good3
		{
			public string DoDispose()
			{
				if (m_disposed)		
					throw new ObjectDisposedException(GetType().Name);
					
				return "hmm";
			}
		}			

		public class VirtualDispose : IDisposable
		{
			~VirtualDispose()
			{
				Dispose(false);
			}
						
			public void Work()
			{
				if (m_disposed)		
					throw new ObjectDisposedException(GetType().Name);
			}
		
			public virtual void Dispose()
			{
				Dispose(true);
				GC.SuppressFinalize(this);
			}
			
			protected virtual void Dispose(bool disposing)
			{
				if (!m_disposed)
					m_disposed = true;
			}
			
			protected bool m_disposed; 
		}			

		public sealed class PublicDispose1 : IDisposable
		{
			~PublicDispose1()
			{
				Dispose(false);
			}
								
			public void Dispose()
			{
				Dispose(true);
				GC.SuppressFinalize(this);
			}
			
			public void Dispose(bool disposing)	// sealed so this should be private
			{
				if (!m_disposed)
					m_disposed = true;
			}
			
			private bool m_disposed; 
		}			

		public class PublicDispose2 : IDisposable
		{
			~PublicDispose2()
			{
				Dispose(false);
			}
								
			public void Dispose()
			{
				Dispose(true);
				GC.SuppressFinalize(this);
			}
			
			public virtual void Dispose(bool disposing)	// not sealed so this should be protected
			{
				if (!m_disposed)
					m_disposed = true;
			}
			
			private bool m_disposed; 
		}			

		public class PrivateDispose : IDisposable
		{
			~PrivateDispose()
			{
				Dispose(false);
			}
								
			public void Dispose()
			{
				Dispose(true);
				GC.SuppressFinalize(this);
			}
			
			private void Dispose(bool disposing)	// not sealed so this should be virtual
			{
				if (!m_disposed)
					m_disposed = true;
			}
			
			private bool m_disposed; 
		}			

		public class DisposeThrows1 : IDisposable
		{
			~DisposeThrows1()
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
				if (GetType().Name == "gah")
					throw new Exception("ack");
					
				Dispose(true);
				GC.SuppressFinalize(this);
			}
			
			protected virtual void Dispose(bool disposing)
			{
				if (!m_disposed)
					m_disposed = true;
			}
			
			protected bool m_disposed; 
		}			

		public struct StructHasNative : IDisposable		// structs cannot have native resources
		{
			public void Dispose()
			{
				if (!m_disposed)
				{
					CloseHandle(m_data);
					m_disposed = true;
				}
			}
			
			[DllImport("Kernel32")]
			public extern static void CloseHandle(IntPtr handle);

			private bool m_disposed; 
			private IntPtr m_data;
		}			

		public class NoNullCheck1 : IDisposable
		{
			public NoNullCheck1()
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
				Dispose(true);
			}
			
			protected virtual void Dispose(bool disposing)
			{
				m_writer.Dispose();						
				m_disposed = true;
			}
			
			private bool m_disposed;
			private StringWriter m_writer = new StringWriter();
		}			

		public class NoNullCheck2 : IDisposable
		{
			public NoNullCheck2()
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
				Dispose(true);
			}
			
			protected virtual void Dispose(bool disposing)
			{
				if (!m_disposed)
				{
					m_writer.Dispose();						
					m_disposed = true;
				}
			}
			
			private bool m_disposed;
			private StringWriter m_writer = new StringWriter();
		}			

		// test code
		public DisposeableTest() : base(
			new string[]{"Good1", "Good2", "Good3", "Good4", "Good5", "Good6", 
				"Good7", "Good8", "Good9", "Good10"},
			new string[]{"NoThrow1", "NoThrow2", "FieldNotDisposed", "NoSuppress",
				"BogusDisposeName1", "BogusDisposeName2", "BogusDisposeName3", "BogusDisposeName4", "BogusDisposeName5",
				"VirtualDispose", "PublicDispose1", "PublicDispose2", "PrivateDispose",
				"DisposeThrows1", "StructHasNative", "NoNullCheck1", "NoNullCheck2"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new DisposeableRule(cache, reporter);
		}
	} 
}