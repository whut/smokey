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
using System.Collections.Generic;

namespace EvilDoer
{
	public class GoodDisposeRemove1 : IDisposable	// list isn't static
	{
		public GoodDisposeRemove1()
		{
			objects.Add(this);
		}
		
		public void Dispose()
		{
			if (!disposed)
			{
				Ignore.Value = objects.Remove(this);
				disposed = true;
			}
		}
							
		private bool disposed = false;
		private List<object> objects = new List<object>();
	}

	public class GoodDisposeRemove2		// not IDisposable
	{
		public GoodDisposeRemove2()
		{
			objects.Add(this);
		}
		
		public void Dispose()
		{
			if (!disposed)
			{
				Ignore.Value = objects.Remove(this);
				disposed = true;
			}
		}
							
		private bool disposed = false;
		private static List<object> objects = new List<object>();
	}

	public class GoodDisposeRemove3 : IDisposable	// stores WeakReference
	{
		public GoodDisposeRemove3()
		{
			objects.Add(new WeakReference(this));
		}
		
		public void Dispose()
		{
			if (!disposed)
			{
				Ignore.Value = objects.Remove(new WeakReference(this));
				disposed = true;
			}
		}
							
		private bool disposed = false;
		private static List<WeakReference> objects = new List<WeakReference>();
	}

	public class GoodDisposeRemove4 : IDisposable	// stores non ref type
	{
		public GoodDisposeRemove4()
		{
			objects.Add(10);
		}
		
		public void Dispose()
		{
			if (!disposed)
			{
				Ignore.Value = objects.Remove(20);
				disposed = true;
			}
		}
							
		private bool disposed = false;
		private static List<int> objects = new List<int>();
	}

	public class GoodDisposeRemove5 : IDisposable
	{
		public GoodDisposeRemove5()
		{
			objects.Add(this);
		}
		
		[DisableRule("C1027", "DisposeDoesStaticRemove")]
		public void Dispose()
		{
			if (!disposed)
			{
				Ignore.Value = objects.Remove(this);
				disposed = true;
			}
		}
		
		public static void Remove(BadDisposeRemove1 o)
		{
			Ignore.Value = objects.Remove(o);
		}
							
		private bool disposed = false;
		private static List<object> objects = new List<object>();
	}

	// C1027/DisposeDoesStaticRemove
	public class BadDisposeRemove1 : IDisposable
	{
		public BadDisposeRemove1()
		{
			objects.Add(this);
		}
		
		public void Dispose()
		{
			if (!disposed)
			{
				Ignore.Value = objects.Remove(this);
				disposed = true;
			}
		}
		
		public static void Remove(BadDisposeRemove1 o)
		{
			Ignore.Value = objects.Remove(o);
		}
							
		private bool disposed = false;
		private static List<object> objects = new List<object>();
	}

	[DisableRule("MS1018", "VisibleFields")]
	public class BaseDisposeRemove2 : IDisposable
	{
		public BaseDisposeRemove2()
		{
			objects.Add(this);
		}
		
		public virtual void Dispose()
		{
			if (!disposed)
			{				
				disposed = true;
			}
		}
							
		public static void Remove(object o)
		{
			Ignore.Value = objects.Remove(o);
		}
							
		protected bool disposed = false;
		protected static List<object> objects = new List<object>();
	}

	// C1027/DisposeDoesStaticRemove
	public class BadDisposeRemove2 : BaseDisposeRemove2
	{
		public override void Dispose()
		{
			BaseDisposeRemove2.Remove(this);
			base.Dispose();
		}
	}

	// C1027/DisposeDoesStaticRemove
	public class BadDisposeRemove3 : IDisposable
	{		
		public void Dispose()
		{
			if (!disposed)
			{
				BadDisposeRemove1.Remove(null);
				disposed = true;
			}
		}
									
		private bool disposed = false;
	}
}
