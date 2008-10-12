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
using System.Runtime.CompilerServices; 
using System.Threading;

// R1014/StaticSetter
namespace EvilDoer
{
	public static class BadNames
	{
		// D1022/ObsoleteMessage
		[Obsolete]
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

	// MS1014/EnumsNeedZero
	enum BadEnum1 {Alpha = 1, Beta};

	// MS1021/PluralEnumFlags
	[Flags]
	enum BadFlag {None = 0x0000, Alpha = 0x0001, Beta = 0x0002};

	// C1034/InvalidEnumFlags
	[Flags]
	enum Bads {None = 0, Alpha = 1, Beta = 2, Delta = 3, Gamma = 4};

	// MS1022/FlagsUseNone
	// MS1025/SuffixName3
	[Flags]
	enum BadFlags {Blah = 0x0000, Alpha = 0x0001, Beta = 0x0002};

	// MS1001/AttributesNeedUsage
	// P1013/UnsealedAttribute
	public class BadAttribute : Attribute
	{		
		public BadAttribute(string address) 
		{
			this.address = address;
		}
		
		public string Address
		{
			get {return address;}
		}
	
		private readonly string address;
	}

	// D1009/AttributeProperties
	// MS1001/AttributesNeedUsage
	public sealed class Bad5Attribute : ContextStaticAttribute
	{		
		public Bad5Attribute(string name) 
		{
			this.name = name;
			Console.Error.WriteLine(name + vendor);
		}
							
		[Bad("hmm")]
		public string Name
		{
			get {return name;}
		}
	
		public string Vendor		// need a getter for optional	
		{
			set {vendor = value;}
		}
	
		private string name;
		private string vendor;
	}
		
	public abstract class BaseBad
	{
		public abstract string Badness();
	}
	
	internal static class Static
	{
		public static void BadUpdate(string text)
		{
			my_text = text;
		}
				
		public static string Value()
		{
			return my_text;
		}

		private static string my_text = string.Empty;
	}
	
	// MS1027/PreferredTerm
	// D1066/Finalizable (not IDisposable)
	public class HandleLogIn
	{
		~HandleLogIn()
		{
			Static.BadUpdate("hey");
		}
	}
	
	// R1001/DisposeNativeResources
	public class NativeArray
	{
		public NativeArray()
		{
			native = new IntPtr[0];
		}
		
		public object Native
		{
			get {return native;}
		}
		
		private IntPtr[] native;
	}
	
	// R1001/DisposeNativeResources
	public class NativeList
	{
		public NativeList()
		{
			native = new List<IntPtr>();
		}
		
		public object Native
		{
			get {return native;}
		}
		
		private List<IntPtr> native;
	}
	
	internal static class Unused
	{
		public static object Value 
		{
			[DisableRule("C1022", "UseSetterValue")]	
			set {}
		}
	}

	// R1001/DisposeNativeResources
	// MS1006/ICloneable
	// MS1018/VisibleFields
	// R1011/HashMatchesEquals
	public class BadClass2 : BaseBad, ICloneable
	{			
		// R1005/CtorCallsVirtual
		public BadClass2(string bad)
		{
			resource = CreateHandle();
			Console.Error.WriteLine("how bad: {0}", bad + Badness());
			Unused.Value = ThreadPool.QueueUserWorkItem(DoCallback1);
			Unused.Value = ThreadPool.QueueUserWorkItem(DoCallback2);
		}
				
		private static void DoCallback1(object state)
		{
			Static.BadUpdate("hey");
			Console.Error.WriteLine(Static.Value());
		}
		
		private static void DoCallback2(object state)
		{
			Inner.Set((int) state);
			Console.Error.WriteLine(my_int);
		}
		
		// D1001/ClassCanBeMadeStatic
		public class Inner
		{
			public static void Set(int x)
			{
				BadClass2.my_int = x;
			}
		}

		// C1022/UseSetterValue
		public object this[int index] 
		{ 
			get {return null;}
			set {Unused.Value = index;}
		}

		// PO1001/DllImportPath
		// PO1002/DllImportExtension
		// D1020/NativeMethods
		// S1020/VisiblePInvoke
		[DllImport("bin/libaspell.so")]
		public static extern void BadExtern();
		
		public object Clone()
		{
			return new BadClass2(string.Empty);
		}
		
		// MS1010/ReservedExceptions
		public static void Reserved(object x)
		{
			if (x == null)
				throw new NullReferenceException("x is null");
				
			Console.Error.WriteLine("{0}", x);			
		}

		// P1010/ToStringOnString
		// D1007/UseBaseTypes
		// R1034/ValidateArgs1
		[DisableRule("G1003", "FormatProvider")]
		public static string SillyString(string x)
		{
			return x.ToString();
		}

		// D1038/DontExit2
		public static void Exit()
		{
			System.Environment.Exit(1);
		}

		public override string Badness()
		{
			return "really bad";
		}
		
		// R1009/CompareFloats
		public static bool BadArith(double x, double y)
		{
			double x1 = x + y;
			double x2 = x * y;
			return x1 == x2;
		}

		// MS1008/LockThis
		public void Work()
		{
			lock (this)
			{
				Console.Error.WriteLine(x);
			}
		}

		// MS1015/SynchronizedAttribute   
		[MethodImplAttribute(MethodImplOptions.Synchronized)]
		public void Work2()
		{
			Console.Error.WriteLine(x);
		}

		// MS1020/EqualsRequiresNullCheck1
		// R1034/ValidateArgs1
		public override bool Equals(object rhsObj)
		{			
			if (GetType() != rhsObj.GetType())
				return false;
				
			BadClass2 rhs = (BadClass2) rhsObj;			
			return this == rhs;
		}
		
		// MS1020/EqualsRequiresNullCheck1
		public static bool operator==(BadClass2 lhs, BadClass2 rhs)
		{
			return lhs.x == rhs.x;
		}
		
		public static bool operator!=(BadClass2 lhs, BadClass2 rhs)
		{
			return !(lhs == rhs);
		}

		// R1012/ObjectHashCode1
		public override int GetHashCode()
		{
			return x ^ resource.GetHashCode() ^ base.GetHashCode();
		}
		
		// C1002/MalformedFormatString
		public string StringProp
		{
			[DisableRule("G1003", "FormatProvider")]
			get {return string.Format("{0} = {1}", x);}
		}
		
		public string GetStringProp()
		{
			return "hey";
		}
		
		// R1013/WeakIdentityLock
		public void StringLock(int a)
		{
			lock (text)
			{
				x = a;
			}
		}
						
		// D1020/NativeMethods
		[System.Runtime.InteropServices.DllImport("Kernel32")]
		private extern static IntPtr CreateHandle();

		public static void God()
		{
			List<object> o = new List<object>();
			
			o.Add(new BadClass());
			o.Add(new BadClass2("s"));
			o.Add(new BadDisposable(IntPtr.Zero));
			o.Add(new BadDisposable2(IntPtr.Zero));
//			o.Add(new BadNaming());
			o.Add(new BadStruct(0, 0));
			o.Add(new BadStruct2(0, 0));
			o.Add(new BrokenClass());
			o.Add(12);
			o.Add(new Cyclic1(0));
			o.Add(new Cyclic2(0));
			o.Add(new Cyclic3(0));
			o.Add(new Cyclic4());
			o.Add(new BadException("s", "t"));
			o.Add(new BadAttribute("s"));
			o.Add(new Bad5Attribute("s"));
			o.Add(new HandleLogIn());
			o.Add(new Big.Type00());
			o.Add(new Big.Type01());
			o.Add(new Big.Type02());
			o.Add(new Big.Type03());
			o.Add(new Big.Type04());
			o.Add(new Big.Type05());
			o.Add(new Big.Type06());
			o.Add(new Big.Type07());
			o.Add(new Big.Type08());
			o.Add(new Big.Type09());
			
			o.Add(new Big.Type10());
			o.Add(new Big.Type11());
			o.Add(new Big.Type12());
			o.Add(new Big.Type13());
			o.Add(new Big.Type14());
			o.Add(new Big.Type15());
			o.Add(new Big.Type16());
			o.Add(new Big.Type17());
			o.Add(new Big.Type18());
			o.Add(new Big.Type19());

			o.Add(new Big.Type20());
			o.Add(new Big.Type21());
			o.Add(new Big.Type22());
			o.Add(new Big.Type23());
			o.Add(new Big.Type24());
			o.Add(new Big.Type25());
			o.Add(new Big.Type26());
			o.Add(new Big.Type27());
			o.Add(new Big.Type28());
			o.Add(new Big.Type29());

			Console.Error.WriteLine(o.Count);
		}
		
		public void Foobar()
		{
		}
			
		public void FooBar()
		{
		}
		
		// D1059/IdentifierCasing
		public void Baz(int xx, int yy, int xX)
		{
		}
		
		// P1021/TooManyLocals
		public int TooManyLocals(int x0)
		{
			int x1 = x0 * 37;
			int x2 = x1 * 37;
			int x3 = x2 * 37;
			int x4 = x3 * 37;
			int x5 = x4 * 37;
			int x6 = x5 * 37;
			int x7 = x6 * 37;
			int x8 = x7 * 37;
			int x9 = x8 * 37;
			int x10 = x9 * 37;
			int x11 = x10 * 37;
			int x12 = x11 * 37;
			int x13 = x12 * 37;
			int x14 = x13 * 37;
			int x15 = x14 * 37;
			int x16 = x15 * 37;
			int x17 = x16 * 37;
			int x18 = x17 * 37;
			int x19 = x18 * 37;
			int x20 = x19 * 37;
			int x21 = x20 * 37;
			int x22 = x21 * 37;
			int x23 = x22 * 37;
			int x24 = x23 * 37;
			int x25 = x24 * 37;
			int x26 = x25 * 37;
			int x27 = x26 * 37;
			int x28 = x27 * 37;
			int x29 = x28 * 37;
			int x30 = x29 * 37;
			int x31 = x30 * 37;
			int x32 = x31 * 37;
			int x33 = x32 * 37;
			int x34 = x33 * 37;
			int x35 = x34 * 37;
			int x36 = x35 * 37;
			int x37 = x36 * 37;
			int x38 = x37 * 37;
			int x39 = x38 * 37;
			int x40 = x39 * 37;
			int x41 = x40 * 37;
			int x42 = x41 * 37;
			int x43 = x42 * 37;
			int x44 = x43 * 37;
			int x45 = x44 * 37;
			int x46 = x45 * 37;
			int x47 = x46 * 37;
			int x48 = x47 * 37;
			int x49 = x48 * 37;
			int x50 = x49 * 37;
			int x51 = x50 * 37;
			int x52 = x51 * 37;
			int x53 = x52 * 37;
			int x54 = x53 * 37;
			int x55 = x54 * 37;
			int x56 = x55 * 37;
			int x57 = x56 * 37;
			int x58 = x57 * 37;
			int x59 = x58 * 37;
			int x60 = x59 * 37;
			int x61 = x60 * 37;
			int x62 = x61 * 37;
			int x63 = x62 * 37;
			int x64 = x63 * 37;
			int x65 = x64 * 37;
			int x66 = x65 * 37;
			int x67 = x66 * 37;
			int x68 = x67 * 37;
			int x69 = x68 * 37;
			return x69;
		}
			
		private int x;
		private IntPtr resource;
		protected int foo;
		private string text = "your lock here";
		private static int my_int;
	}
}

