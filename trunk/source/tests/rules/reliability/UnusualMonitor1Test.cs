// Copyright (C) 2008 Jesse Jones
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
using Smokey.Framework.Support;
using Smokey.Internal;
using Smokey.Internal.Rules;
using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;

namespace Smokey.Tests
{
	[TestFixture]
	public class UnusualMonitor1Test : MethodTest
	{	
		#region Test classes
		private class Good1		// uses the pattern
		{
			public object Consume()
			{
				object result = null;
				
				lock (m_lock)
				{
					while (m_queue.Count == 0)
					{
						Ignore.Value = Monitor.Wait(m_lock);
					}
					
					result = m_queue.Dequeue();
				}
				
				return result;
			}
			
			private readonly object m_lock = new object();
			private Queue m_queue = new Queue();
		}		

		private class Good2		// uses timeout
		{
			public object Consume()
			{
				object result = null;
				
				lock (m_lock)
				{
					Ignore.Value = Monitor.Wait(m_lock, 100);
					
					result = m_queue.Dequeue();
				}
				
				return result;
			}
			
			private readonly object m_lock = new object();
			private Queue m_queue = new Queue();
		}		

		private class Good3		// uses complex predicate
		{
			public object Consume()
			{
				object result = null;
				
				lock (m_lock)
				{
					while (m_queue.Count == 0 && m_count == 0)
					{
						Ignore.Value = Monitor.Wait(m_lock);
					}
					
					result = m_queue.Dequeue();
					m_count = 0;
				}
				
				return result;
			}
			
			private readonly object m_lock = new object();
			private Queue m_queue = new Queue();
			private int m_count = 1;
		}		

		private class Good4		// uses the pattern
		{
			public object Consume()
			{
				object result = null;
				
				lock (m_lock)
				{
					while (m_queue.Count == 0)
					{
						Monitor.Wait(m_lock);
					}
					
					result = m_queue.Dequeue();
				}
				
				return result;
			}
			
			private readonly object m_lock = new object();
			private Queue m_queue = new Queue();
		}		

		private class Good5		// uses the pattern
		{
			public object Consume()
			{
				object result = null;
				
				lock (m_lock)
				{
					while (m_queue.Count == 0)
					{
						Ignore.Value = Monitor.Wait(m_lock);
					}
					
					result = m_queue.Dequeue();
				}
				
				return result;
			}
			
			private static object m_lock = new object();
			private static Queue m_queue = new Queue();
		}		

		private class Good6		// uses the pattern
		{
			public object Consume()
			{
				object result = null;
				
				lock (m_lock)
				{
					while (m_queue.Count == 0)
					{
						Monitor.Wait(m_lock);
					}
					
					result = m_queue.Dequeue();
				}
				
				return result;
			}
			
			private static object m_lock = new object();
			private static Queue m_queue = new Queue();
		}		

		private class Bad1		// uses if instead of loop
		{
			public object Consume()
			{
				object result = null;
				
				lock (m_lock)
				{
					if (m_queue.Count == 0)
					{
						Ignore.Value = Monitor.Wait(m_lock);
					}
					
					result = m_queue.Dequeue();
				}
				
				return result;
			}
			
			private readonly object m_lock = new object();
			private Queue m_queue = new Queue();
		}		

		private class Bad2		// no loop
		{
			public object Consume()
			{
				object result = null;
				
				lock (m_lock)
				{
					Ignore.Value = Monitor.Wait(m_lock);
					
					result = m_queue.Dequeue();
				}
				
				return result;
			}
			
			private readonly object m_lock = new object();
			private Queue m_queue = new Queue();
		}		

		private class Bad3		// loop does stuff
		{
			public object Consume()
			{
				object result = null;
				
				lock (m_lock)
				{
					while (m_queue.Count == 0)
					{
						Ignore.Value = Monitor.Wait(m_lock);
						Console.WriteLine("waiting");
					}
					
					result = m_queue.Dequeue();
				}
				
				return result;
			}
			
			private readonly object m_lock = new object();
			private Queue m_queue = new Queue();
		}		

		private class Bad4		// no loop
		{
			public object Consume()
			{
				object result = null;
				
				lock (m_lock)
				{
					if (m_queue.Count == 0)
					{
						Ignore.Value = Monitor.Wait(m_lock);
					}
					
					result = m_queue.Dequeue();
				}
				
				return result;
			}
			
			private static object m_lock = new object();
			private static Queue m_queue = new Queue();
		}		

		private class Bad5		// loop does stuff
		{
			public object Consume()
			{
				object result = null;
				
				lock (m_lock)
				{
					while (m_queue.Count == 0)
					{
						Monitor.Wait(m_lock);
						Console.WriteLine("waiting");
					}
					
					result = m_queue.Dequeue();
				}
				
				return result;
			}
			
			private static object m_lock = new object();
			private static Queue m_queue = new Queue();
		}		

		private class Bad6		// completely hosed
		{
			public void Consume()
			{
				lock (items) 
				{
					/* find the next item */
					while ((current = queue.Peek()) == null) 
					{
						if (queue.Count != 0)
						{
							Console.WriteLine("PopBlock()");
							Monitor.Wait(items);
							Console.WriteLine("PushBlock()");
						}
					}
				}
				Console.WriteLine(current);
			}
			
			private object items = new object();
			private Queue queue = new Queue();
			private object current;
		}		
		#endregion
		
		// test code
		public UnusualMonitor1Test() : base(
			new string[]{"Good1.Consume", "Good2.Consume", "Good3.Consume", "Good4.Consume", "Good5.Consume", "Good6.Consume"},
			new string[]{"Bad1.Consume", "Bad2.Consume", "Bad3.Consume", "Bad4.Consume", "Bad5.Consume", "Bad6.Consume"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new UnusualMonitor1Rule(cache, reporter);
		}
	} 
}
