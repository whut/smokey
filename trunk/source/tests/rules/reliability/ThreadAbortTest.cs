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
	[TestFixture]
	public class ThreadAbortTest : MethodTest
	{	
		#region Test classes
		private class Cases
		{
			public Cases()
			{
				m_thread = new Thread(this.DoThread);
				m_thread.Start();
			}

			public void Stop()
			{
				m_running = false;
			}

			public void Kill1()
			{
				if (m_thread != null)
				{
					m_thread.Abort();
					m_thread = null;
				}
			}

			public void Kill2()
			{
				if (m_thread != null)
				{
					m_thread.Abort(this);
					m_thread = null;
				}
			}

			public void DoThread(object instance)
			{
				m_running = true;
				
				while (m_running)
				{
					Console.WriteLine("hey");
				}
			}

			private Thread m_thread;
			private volatile bool m_running;
		}		
		#endregion
		
		// test code
		public ThreadAbortTest() : base(
			new string[]{"Cases.Stop"},
			new string[]{"Cases.Kill1", "Cases.Kill2"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new ThreadAbortRule(cache, reporter);
		}
	} 
}

