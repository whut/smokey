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
using System.Runtime.InteropServices;
using System.Threading;

namespace EvilDoer
{
	public static class GoodNames
	{
		[Obsolete("Use the FirstName and LastName properties instead.")]
		public static string Name
		{
			get {return "Name";}
		}
		
		public static string FirstName
		{
			get {return "FirstName";}
		}
		
		public static string LastName
		{
			get {return "LastName";}
		}
	}

	public class NameChangedEventArgs : EventArgs
	{
		public readonly string OldName;
		public readonly string NewName;
		
		public NameChangedEventArgs(string oldName, string newName)
		{
			OldName = oldName;
			NewName = newName;
		}
	}
	
	[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | 
		AttributeTargets.Constructor | AttributeTargets.Enum | AttributeTargets.Interface | 
		AttributeTargets.Method | AttributeTargets.Struct, AllowMultiple = true)]
	public sealed class DisableRuleAttribute : Attribute
	{		
		public DisableRuleAttribute(string id, string name) 
		{
			this.id = id;
			this.name = name;
		}
		
		public string ID
		{
			get {return id;}
		}
	
		public string Name
		{
			get {return name;}
		}
	
		private string id;
		private string name;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct XWindowAttributes 
	{ 
		internal IntPtr		all_event_masks;
		internal IntPtr		your_event_mask;
	}

	[DisableRule("D1050", "UnusedField")]
	internal sealed class ZWindowAttributes 
	{ 
		internal IntPtr		all_event_masks;
		internal IntPtr		your_event_mask;

		[DllImport("libaspell")]
		[DisableRule("D1020", "NativeMethods")]	
		public static extern void GoodExtern();
	}

	[DisableRule("R1000", "DisposableFields")]	
	[DisableRule("D1052", "PreferMonitor1")]	
	public class GoodResetEvent
	{						
		public object Use()
		{
			return sync;
		}
		
		private AutoResetEvent sync = new AutoResetEvent(false);
	}			

	public class GoodClass2
	{				
		public event EventHandler<NameChangedEventArgs> NameChanged;
		
		public GoodClass2()
		{
			Ignore.Value = ThreadPool.QueueUserWorkItem(DoCallback2);
		}
		
		private static void DoCallback2(object state)
		{
			Inner.Set((int) state);
			Console.Error.WriteLine(my_int);
		}
		
		private static class Inner		// this is private so threaded static setter is ok
		{
			public static void Set(int x)
			{
				GoodClass2.my_int = x;
			}
		}

		[DisableRule("MS1010", "ReservedExceptions")]
		public static void Reserved(object x)
		{
			if (x == null)
				throw new NullReferenceException("x is null");
				
			Console.Error.WriteLine("{0}", x);
		}

		public void ChangeName(string name)
		{
			if (name != this.name)
			{
				EventHandler<NameChangedEventArgs> temp = NameChanged;
				if (temp != null)
					temp(this, new NameChangedEventArgs(this.name, name));
					
				this.name = name;
			}
		}
		
		public override int GetHashCode()
		{
			return name.GetHashCode() ^ address.GetHashCode();
		}
		
		public override bool Equals(object rhsObj)
		{
			GoodClass2 rhs = rhsObj as GoodClass2;
			if ((object) rhs == null)
				return false;
			
			return name == rhs.name && address == rhs.address;
		}
			
		public bool Equals(GoodClass2 rhs)
		{
			if ((object) rhs == null)
				return false;
			
			return name == rhs.name && address == rhs.address;
		}

		public static bool operator==(GoodClass2 lhs, GoodClass2 rhs)
		{
			if (object.ReferenceEquals(lhs, rhs))
				return true;
			
			if ((object) lhs == null || (object) rhs == null)
				return false;
			
			return lhs.name == rhs.name && lhs.address == rhs.address;
		}
		
		public static bool operator!=(GoodClass2 lhs, GoodClass2 rhs)
		{
			return !(lhs == rhs);
		}
		
		private string name = "hello", address = "goodbye";
		private static int my_int;
	}
}