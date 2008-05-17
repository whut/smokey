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

using System;
using System.Threading;
using System.Windows.Forms;

namespace EvilDoer
{
	public class GoodLocker1
	{
		public void Work()
		{
			lock (lock1)
			{
				DoWork();
			}
		}
		
		private void DoWork()
		{
			lock (lock2)
			{
			}
		}
		
		private object lock1 = new object();
		private object lock2 = new object();
	}

	public class GoodLocker2
	{
		public void Work(GoodLocker2 g)
		{
			lock (lock1)
			{
				DoWork(g);
			}
		}
		
		private void DoWork(GoodLocker2 g)
		{
			lock (g.lock1)
			{
			}
		}
		
		private object lock1 = new object();
	}

	public class GoodLocker3
	{
		[DisableRule("D1048", "GuiUsesConsole")]
		public void Work()
		{
			DoWork();
			
			lock (lock1)
			{
				Console.WriteLine("hello");
			}

			DoWork();
		}
		
		private void DoWork()
		{
			lock (lock1)
			{
			}
		}
		
		private object lock1 = new object();
	}

	// R1037/RecursiveLock1
	public class BadLocker1		
	{
		public void Work1()
		{
			DoWork();
		}
		
		public void Work2()
		{
			lock (lock1)
			{
				DoWork();
			}
		}
		
		private void DoWork()
		{
			lock (lock1)
			{
			}
		}
		
		private object lock1 = new object();
	}

	// R1037/RecursiveLock1
	public static class BadLocker2
	{
		public static void Work1()
		{
			DoWork();
		}
		
		public static void Work2()
		{
			lock (lock1)
			{
				DoWork();
			}
		}
		
		private static void DoWork()
		{
			lock (lock1)
			{
			}
		}
		
		private static object lock1 = new object();
	}

	// R1037/RecursiveLock1
	public class BadLocker3
	{
		public int Work(int x)
		{
			int result = x;
			
			lock (lock1)
			{
				if (x > 0)
					result = Work(x - 1);
			}
			
			return result;
		}

		private object lock1 = new object();
	}

	// R1037/RecursiveLock1
	public class BadLocker4
	{
		public void Work1()
		{
			DoWork(4);
		}
		
		public void Work2()
		{
			lock (lock1)
			{
				DoWork(4);
			}
		}
		
		private void DoWork(int x)
		{
			while (x > 0)		// loop so we don't get inlined
			{
				DoMoreWork();
				--x;
			}
		}
		
		private void DoMoreWork()
		{
			lock (lock1)
			{
			}
		}
		
		private object lock1 = new object();
	}
	
	public class GoodHook1
	{
		public delegate void Hook(int x);
		
		public GoodHook1(Hook h)
		{
			hook = h;
		}
		
		[DisableRule("D1048", "GuiUsesConsole")]
		public void Work()
		{
			lock (lock1)
			{
				Console.WriteLine("hello");
			}
			
			hook(2);
		}
		
		private Hook hook;
		private object lock1 = new object();
	}

	public class GoodHook2
	{
		public event EventHandler Hook;
				
		[DisableRule("D1048", "GuiUsesConsole")]
		public void Work()
		{
			lock (lock1)
			{
				Console.WriteLine("hello");
			}
			
			if (Hook != null)
				Hook(this, EventArgs.Empty);
		}
		
		private object lock1 = new object();
	}

	public class GoodHook3
	{		
		public GoodHook3(Control c)
		{
			control = c;
		}
		
		[DisableRule("D1048", "GuiUsesConsole")]
		public void Work()
		{
			lock (lock1)
			{
				Console.WriteLine(control.Invoke(null));
			}		
		}
		
		private Control control;
		private object lock1 = new object();
	}

	// R1038/RecursiveLock2
	public class BadHook1
	{
		public delegate void Hook(int x);
		
		public BadHook1(Hook h)
		{
			hook = h;
		}
		
		[DisableRule("D1048", "GuiUsesConsole")]
		public void Work()
		{
			lock (lock1)
			{
				Console.WriteLine("hello");
				hook(2);
			}		
		}
		
		private Hook hook;
		private object lock1 = new object();
	}

	// R1038/RecursiveLock2
	public class BadHook2
	{
		public event EventHandler Hook;
				
		[DisableRule("D1048", "GuiUsesConsole")]
		public void Work()
		{
			lock (lock1)
			{			
				if (Hook != null)
					Hook(this, EventArgs.Empty);

				Console.WriteLine("hello");
			}
		}
		
		private object lock1 = new object();
	}

	// R1038/RecursiveLock2
	public class BadHook3
	{
		public event EventHandler Hook;
				
		[DisableRule("D1048", "GuiUsesConsole")]
		public void Work()
		{
			lock (lock1)
			{			
				Console.WriteLine("hello");
				DoWork();
			}
		}
		
		[DisableRule("D1048", "GuiUsesConsole")]
		private void DoWork()
		{
			if (Hook != null)
				Hook(this, EventArgs.Empty);

			Console.WriteLine("hello");
		}
		
		private object lock1 = new object();
	}
}
