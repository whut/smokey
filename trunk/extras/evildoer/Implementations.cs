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

namespace EvilDoer
{
	public interface IWorker
	{
		int Work(int w);
	}
	
	internal interface IInternal
	{
		int Work(int w);
	}
	
	// ok: class is public
	public class PublicWorker : IWorker
	{
		public int Work(int w)
		{
			return 0;
		}
	
		public int More(int w)
		{
			return w+w;
		}
	}			
	
	// ok: interface is internal
	internal class Worker : IInternal
	{
		public int Work(int w)
		{
			return 0;
		}
	
		public int More(int w)
		{
			return w+w;
		}
	}			
	
	// ok: all public methods are declared in interfaces
	internal class Worker2 : IWorker
	{
		public Worker2(int v)
		{
			value = v;
		}
		
		public int Work(int w)
		{
			return value+w;
		}
		
		public override string ToString()	// Public, Virtual, HideBySig
		{
			return "worker";
		}
		
		private int value;
	}			
	
	// ok: More is called externally
	internal class Worker3 : IWorker
	{
		public Worker3(int v)
		{
			value = v;
		}
		
		public int Work(int w)	// Public, Final, Virtual, HideBySig, VtableLayoutMask
		{
			return More(w);
		}

		public int More(int w)	// Public, HideBySig
		{
			return value+w;
		}
		
		private int value;
	}			
	
	// ok: interface is indirect
	internal class Worker4 : Worker3
	{
		public Worker4() : base(4)
		{
			worker = new InternalWorker(3);
		}
		
		public int More2(int w)
		{
			return worker.Work(w);
		}
		
		private IWorker worker;
	}			
	
	// bad: More is called only by this class
	// D1051/PublicImplementation
	internal class InternalWorker : IWorker
	{
		public InternalWorker(int v)
		{
			worker = new Worker3(v);
		}
		
		public int Work(int w)
		{
			return More(w);
		}

		public int More(int w)
		{
			return worker.More(w);
		}
		
		private Worker3 worker;
	}			
	
	internal class InternalWorker2 : IWorker
	{
		public InternalWorker2(int v)
		{
			worker = new Worker3(v);
		}
		
		public int Work(int w)
		{
			return worker.More(w) + 1;
		}

		public int More(int w)
		{
			return worker.More(w);
		}
		
		private Worker3 worker;
	}			
	
	// bad: More is only called by a subclass
	// D1051/PublicImplementation
	internal class InternalWorker3 : IWorker
	{
		public InternalWorker3(int v)
		{
			worker = new Worker3(v);
		}
		
		public int Work(int w)
		{
			return worker.More(w) + 1;
		}

		public int More(int w)
		{
			return worker.More(w);
		}
		
		private Worker3 worker;
	}			
	
	internal class InternalWorker4 : InternalWorker3
	{
		public InternalWorker4(int v) : base(v)
		{
		}
		
		public int Work2(int w)
		{
			return base.More(w) + 1;
		}
	}			
}