// Copyright (C) 2007-2008 Jesse Jones
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

using System;
using System.Threading;
using Smokey.Framework;

namespace Smokey.Internal
{
	// Prints progress information to stderr. Note that we only print if the
	// main thread is actually making progress.
	internal sealed class Progress : IDisposable
	{		
		// Starts our thread up.
		public Progress(bool verbose)
		{
			m_verbose = verbose;
			
			m_thread = new Thread(this.DoThread);
			m_thread.Start();
		}
		
		// Called when the framework starts processing a new rule.
		public void Add(string name)
		{
	        if (m_disposed)        
    	        throw new ObjectDisposedException(GetType().Name);
            
			lock (m_lock)
			{
				m_name = name;	// we don't want to pulse this, we want to reply on the timeout instead
			}
		}
				
		// Joins the thread.
		[DisableRule("D1042", "IdenticalMethods")]	// TODO: should have a base class for progress and watchdog
		public void Shutdown()
		{
			if (m_disposed)        
				throw new ObjectDisposedException(GetType().Name);
			
			lock (m_lock)
			{
				if (m_running)
				{
					m_running = false;
		            Monitor.PulseAll(m_lock);
				}
			}

			if (m_thread != null)
			{
				bool terminated = m_thread.Join(1000);
				DBC.Assert(terminated, "thread didn't terminate");
				
				m_thread = null;
			}
		}
		
		public void Dispose()
		{
			if (!m_disposed)
			{
				Shutdown();		
				m_disposed = true;
			}
		}

		#region Private Methods -----------------------------------------------
		private void DoThread(object instance)
		{
			Unused.Value = instance;
			
			bool wrote = false;
			
			lock (m_lock)
			{
				while (m_running)
				{
					while (m_running && m_name == null)
					{
						Unused.Value = Monitor.Wait(m_lock, m_interval);
					}
	
					if (m_running && m_name != null)	// only write if we got new info
					{
						if (m_verbose)
							if (wrote)
								Console.Error.Write("...{0}", m_name);
							else
								Console.Error.Write("{0}", m_name);
						else
							Console.Error.Write('.');
						wrote = true;
						m_name = null;
					}
				}
			}
			
			if (wrote)
				Console.Error.WriteLine();
		}
		#endregion
		
		#region Fields --------------------------------------------------------
		private Thread m_thread;
		private readonly TimeSpan m_interval = TimeSpan.FromSeconds(5);
		private readonly bool m_verbose;
		private bool m_disposed = false;

		private object m_lock = new object();
			private bool m_running = true;
			private string m_name;
		#endregion
	}
}