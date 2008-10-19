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

namespace Smokey.Tests
{
	[AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = false)]
	internal sealed class ThreadRootAttribute : Attribute
	{		
		public ThreadRootAttribute(string name) 
		{
			Name = name;
		}
		
		public string Name {get; private set;}
	}

	[AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Property, AllowMultiple = false)]
	internal sealed class ThreadSafeAttribute : Attribute
	{		
	}

	[TestFixture]
	public class ThreadSafeAttrTest : AssemblyTest
	{	
		#region Test classes
		private class Good1
		{
			public Good1()
			{
				m_thread = new Thread(this.DoThread);
				m_thread.Start();
			}

			[ThreadRoot("Good1")]
			public void DoThread(object instance)
			{
				while (true)
				{
					Console.WriteLine("hey");
				}
			}

			private Thread m_thread;
		}		

		private class Good2
		{
			public Good2(string path)
			{
				m_stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 1024, FileOptions.Asynchronous);
				m_stream.BeginRead(m_data, 0, m_data.Length, DoCallback, m_stream);
			}

			[ThreadSafe]
			public static void DoCallback(IAsyncResult result)
			{
				Console.WriteLine("hey");
			}
			
			private FileStream m_stream;
			private Byte[] m_data = new Byte[100];
		}		

		private class Good3
		{
			public Good3()
			{
				m_thread1 = new Thread(this.DoThread1);
				m_thread1.Start();

				m_thread2 = new Thread(this.DoThread2);
				m_thread2.Start();
			}

			[ThreadRoot("1")]
			public void DoThread1()		
			{
				while (true)
				{
					DoWork();
				}
			}

			[ThreadRoot("2")]
			public void DoThread2()		
			{
				while (true)
				{
					DoWork();
					DoSomeWork();
				}
			}
			
			[ThreadSafe]
			private void DoWork()			
			{
				Console.WriteLine("hey");
			}

			private void DoSomeWork()			
			{
				Console.WriteLine("hey");
			}

			private Thread m_thread1;
			private Thread m_thread2;
		}		

		public class Good4
		{
			public Good4()
			{
				m_thread = new Thread(this.DoThread1);
				m_thread.Start();
			}

			[ThreadSafe]
			public void DoThread1()		
			{
				while (true)
				{
					DoWork();
				}
			}
			
			[ThreadSafe]
			private void DoWork()	
			{
				Console.WriteLine("hey");
			}

			private Thread m_thread;
		}		

		[ThreadSafe]
		public class Good5
		{
			public Good5()
			{
				m_thread = new Thread(this.DoThread1);
				m_thread.Start();
				DoWork();
			}

			[ThreadRoot("1")]
			public void DoThread1()		
			{
				while (true)
				{
					DoWork();
				}
			}
			
			private void DoWork()	
			{
				Console.WriteLine("hey");
			}

			private Thread m_thread;
		}		

		[ThreadSafe]
		private class Good6
		{
			public Good6()
			{
				m_thread1 = new Thread(this.DoThread1);
				m_thread1.Start();

				m_thread2 = new Thread(this.DoThread2);
				m_thread2.Start();
			}

			public void Process()			
			{
				Console.WriteLine("hey");
			}

			[ThreadRoot("1")]
			public void DoThread1()		
			{
				while (true)
				{
					DoWork();
				}
			}

			[ThreadRoot("2")]
			public void DoThread2()		
			{
				while (true)
				{
					DoWork();
					DoSomeWork();
				}
			}
			
			private void DoWork()			
			{
				Console.WriteLine("hey");
			}

			private void DoSomeWork()			
			{
				Console.WriteLine("hey");
			}

			private Thread m_thread1;
			private Thread m_thread2;
		}		

		private class Bad1
		{
			public Bad1()
			{
				m_thread = new Thread(this.DoThread);
				m_thread.Start();
			}

			public void DoThread(object instance)	// not marked with thread root
			{
				while (true)
				{
					Console.WriteLine("hey");
				}
			}

			private Thread m_thread;
		}		

		private class Bad2
		{
			public Bad2()
			{
				ThreadPool.QueueUserWorkItem(DoCallback, 5);
			}

			public static void DoCallback(object state)	// not marked with thread root
			{
				Console.WriteLine("hey");
			}
		}		

		private class Bad3
		{
			public Bad3()
			{
				m_thread1 = new Thread(this.DoThread1);
				m_thread1.Start();

				m_thread2 = new Thread(this.DoThread2);
				m_thread2.Start();
			}

			[ThreadRoot("1")]
			public void DoThread1()		
			{
				while (true)
				{
					DoWork();
				}
			}

			[ThreadRoot("2")]
			public void DoThread2()		
			{
				while (true)
				{
					DoWork();
				}
			}
			
			private void DoWork()			// not marked as thread safe
			{
				Console.WriteLine("hey");
			}

			private Thread m_thread1;
			private Thread m_thread2;
		}		

		public class Bad4
		{
			public Bad4()
			{
				m_thread = new Thread(this.DoThread1);
				m_thread.Start();

				DoWork();
			}

			[ThreadRoot("1")]
			public void DoThread1()		
			{
				while (true)
				{
					DoWork();
				}
			}
			
			private void DoWork()			// not marked as thread safe
			{
				Console.WriteLine("hey");
			}

			private Thread m_thread;
		}		

		public class Bad5
		{
			public Bad5()
			{
				m_thread = new Thread(this.DoThread1);
				m_thread.Start();
			}

			[ThreadRoot("1")]
			public void DoThread1()		
			{
				while (true)
				{
					DoWork();
				}
			}
			
			[ThreadSafe]
			private void DoWork()			// thread safe, but called from one thread
			{
				Console.WriteLine("hey");
			}

			private Thread m_thread;
		}		

		public class Bad6
		{
			public Bad6()
			{
				m_thread = new Thread(this.DoThread1);
				m_thread.Start();
				DoWork();
			}

			[ThreadRoot("Main")]			// we're lieing here...
			public void DoThread1()		
			{
				while (true)
				{
					DoWork();
				}
			}
			
			[ThreadSafe]
			private void DoWork()			// thread safe, but called from one thread
			{
				Console.WriteLine("hey");
			}

			private Thread m_thread;
		}		

		[ThreadSafe]
		private class Bad7
		{
			public void Process()			
			{
				Console.WriteLine("hey");
			}
		}		
		#endregion
		
		// test code
		public ThreadSafeAttrTest() : base(
			new string[]{"Good1", "Good2", "Good3", "Good4", "Good5", "Good6"},
			new string[]{"Bad1", "Bad2", "Bad3", "Bad4", "Bad5", "Bad6", "Bad7"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new ThreadSafeAttrRule(cache, reporter);
		}
	} 
}
#endif
