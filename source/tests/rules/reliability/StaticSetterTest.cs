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

using NUnit.Framework;
using System;
using System.IO;
using System.Threading;
using Smokey.Framework.Support;
using Smokey.Internal.Rules;

#if OLD
namespace Smokey.Tests
{
	[TestFixture]
	public class StaticSetterTest : AssemblyTest
	{	
		#region Test classes
		public static class Static
		{
			public static void GoodUpdate(string text)
			{
				lock (ms_lock)
				{
					ms_text = text;
				}
			}
			
			public static void BadUpdate(string text)
			{
				ms_text = text;
			}
			
			public static string Value()
			{
				return ms_text;
			}

			public static void VAtomicSet(bool value)
			{
				ms_vatomic = value;
			}
						
			public static void AtomicSet(bool value)
			{
				ms_atomic = value;
			}
			
			public static int Foo()
			{
				int result = 0;
				
				object temp = ms_vatomic;
				result += temp.GetHashCode();
				result += ms_atomic.GetHashCode();

				return result;
			}
			
			private static object ms_lock = new object();
			private static string ms_text = string.Empty;
			private static volatile bool ms_vatomic;
			private static bool ms_atomic;
		}		

		public class GoodCase1
		{
			public GoodCase1()
			{
				m_thread = new Thread(this.DoThread);
				m_thread.Start();
			}

			public void DoThread(object instance)
			{
				while (true)
				{
					Static.GoodUpdate("hey");
					Console.WriteLine(Static.Value());
				}
			}

			private Thread m_thread;
		}		

		public class GoodCase2
		{
			public GoodCase2(string path)
			{
				m_stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 1024, FileOptions.Asynchronous);
				m_stream.BeginRead(m_data, 0, m_data.Length, DoCallback, m_stream);
			}

			public static void DoCallback(IAsyncResult result)
			{
				Static.GoodUpdate("hey");
				Console.WriteLine(Static.Value());
			}
			
			private FileStream m_stream;
			private Byte[] m_data = new Byte[100];
		}		

		public class GoodCase3
		{
			public GoodCase3()
			{
				ThreadPool.QueueUserWorkItem(DoCallback, 5);
			}

			public static void DoCallback(object state)
			{
				Static.GoodUpdate("hey");
				Console.WriteLine(Static.Value());
			}
		}		

		public class GoodCase4
		{
			public GoodCase4(string path)
			{
				m_stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 1024, FileOptions.Asynchronous);
				m_stream.BeginRead(m_data, 0, m_data.Length, DoCallback, m_stream);
			}

			public void DoCallback(IAsyncResult result)	
			{
				DoSetState("hey");
				Console.WriteLine(Static.Value());
			}
			
			private static void DoSetState(string state)	// private so OK
			{
				ms_state = state;
				Console.WriteLine(ms_state);
			}
			
			private FileStream m_stream;
			private Byte[] m_data = new Byte[100];
			private static string ms_state;
		}		

		public class GoodCase5
		{
			public GoodCase5()
			{
				m_thread = new Thread(this.DoThread);
				m_thread.Start();
			}

			public void DoThread(object instance)
			{
				while (true)
				{
					Static.VAtomicSet(true);
					Console.WriteLine(Static.Value());
				}
			}

			private Thread m_thread;
		}		

		public class BadCase1
		{
			public BadCase1()
			{
				m_thread = new Thread(this.DoThread);
				m_thread.Start();
			}

			public void DoThread(object instance)
			{
				while (true)
				{
					Static.BadUpdate("hey");
					Console.WriteLine(Static.Value());
				}
			}

			private Thread m_thread;
		}		

		public class BadCase2
		{
			public BadCase2()
			{
				m_thread = new Thread(this.DoThread);
				m_thread.Start();
			}

			public void DoThread(object instance)
			{
				while (true)
				{
					DoWork();
				}
			}

			public void DoWork()
			{
				Static.BadUpdate("hey");
				Console.WriteLine(Static.Value());
			}

			private Thread m_thread;
		}		

		public class BadCase3
		{
			public BadCase3()
			{
				m_thread = new Thread(new ThreadStart(DoThread));
				m_thread.Start();
			}

			public void DoThread()
			{
				while (true)
				{
					Static.BadUpdate("hey");
					Console.WriteLine(Static.Value());
				}
			}

			private Thread m_thread;
		}		

		public class BadCase4
		{
			public BadCase4()
			{
				m_thread = new Thread(BadCase4.DoThread);
				m_thread.Start();
			}

			public static void DoThread(object instance)
			{
				while (true)
				{
					Static.BadUpdate("hey");
					Console.WriteLine(Static.Value());
				}
			}

			private Thread m_thread;
		}		

		public class BadCase5
		{
			public BadCase5()
			{
				ThreadPool.QueueUserWorkItem(DoCallback, 5);
			}

			public static void DoCallback(object state)
			{
				Static.BadUpdate("hey");
				Console.WriteLine(Static.Value());
			}
		}		

		public class BadCase6
		{
			public BadCase6()
			{
				ThreadPool.QueueUserWorkItem(DoCallback);
			}

			public static void DoCallback(object state)
			{
				Static.BadUpdate("hey");
				Console.WriteLine(Static.Value());
			}
		}		

		public class BadCase7
		{
			public BadCase7()
			{
				m_timer = new Timer(DoCallback, 5, 0, 2000);
				m_timer.Dispose();
			}

			public static void DoCallback(object state)
			{
				Static.BadUpdate("hey");
				Console.WriteLine(Static.Value());
			}
			
			private Timer m_timer;
		}		

		public class BadCase8
		{
			public BadCase8(string path)
			{
				m_stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 1024, FileOptions.Asynchronous);
				m_stream.BeginRead(m_data, 0, m_data.Length, DoCallback, m_stream);
			}

			public void DoCallback(IAsyncResult result)	
			{
				DoSetState("hey");
				Console.WriteLine(Static.Value());
			}
			
			protected static void DoSetState(string state)	// protected so bad
			{
				ms_state = state;
				Console.WriteLine(ms_state);
			}
			
			private FileStream m_stream;
			private Byte[] m_data = new Byte[100];
			private static string ms_state;
		}		

		public class BadCase9
		{
			public BadCase9()
			{
				m_thread = new Thread(this.DoThread);
				m_thread.Start();
			}

			public void DoThread(object instance)
			{
				while (true)
				{
					Static.AtomicSet(true);					// atomic, but not volatile
					Console.WriteLine(Static.Value());
				}
			}

			private Thread m_thread;
		}		
		#endregion
		
		// test code
		public StaticSetterTest() : base(
			new string[]{"GoodCase1", "GoodCase2", "GoodCase3", "GoodCase4", "GoodCase5"},
			new string[]{"BadCase1", "BadCase2", "BadCase3", "BadCase4", "BadCase5",
				"BadCase6", "BadCase7", "BadCase8", "BadCase9"},
			new string[]{"Static"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new StaticSetterRule(cache, reporter);
		}
	} 
}
#endif
