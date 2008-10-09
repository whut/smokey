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

using FullTrust;
using System;
using System.Reflection;
using System.Security;
using System.Security.Permissions;

[assembly: CLSCompliant(true)]	
[assembly: PermissionSet(SecurityAction.RequestMinimum, Unrestricted = true)]
[assembly: AssemblyDescription("functional test assembly")]

[assembly: AllowPartiallyTrustedCallers]
[assembly: SecurityCritical]

// S1001/APTCA2
// S1015/PartitionAssembly
namespace Aptca2
{
	public class DerivedClass : BaseClass
	{				
		public override void Print(string text)
		{    
			base.Print("label: " + text);
		}

		// this is critical code so we can assert
		[EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Read = "LOG_PATH")]
		[EnvironmentPermissionAttribute(SecurityAction.Assert, Read = "LOG_PATH")]
		[SecurityCritical]
		public static void Assert1()
		{
		}

		// this is transparent code
		// S1014/TransparentAssert2
		[EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Read = "LOG_PATH")]
		[EnvironmentPermissionAttribute(SecurityAction.Assert, Read = "LOG_PATH")]
		public static void Assert2()
		{
		}
	}
	
	public static class Partition
	{
		public static void Good1()
		{
			DoTransparent();
		}
		
		[SecurityCritical]
		public static void Good2()
		{
			DoCritical1();
		}
		
		public static void Good3()
		{
			DoSemiCritical();
		}
				
		public static void Good4()
		{
			DoLoop1(1);
		}
				
		public static void Bad1()
		{
			DoCritical1();
		}
		
		public static void Bad2()
		{
			Console.Error.WriteLine("1");
			DoCallCritical();
			Console.Error.WriteLine("2");
		}
		
		public static void Bad3()
		{
			DoCritical2();
		}
		
		private static void DoTransparent()
		{
			Console.Error.WriteLine("5");
		}
		
		private static void DoCallCritical()
		{
			DoCritical1();
		}
		
		[SecurityCritical]
		private static void DoCritical1()
		{
			Console.Error.WriteLine("3");
		}		
		
		[SecurityCritical]
		[SecurityTreatAsSafeAttribute]
		private static void DoSemiCritical()
		{
			Console.Error.WriteLine("4");
		}		
		
		[SecurityCritical]
		[SecurityTreatAsSafeAttribute]
		private static void DoCritical2()
		{
			DoCritical1();
		}		

		private static void DoLoop1(int x)
		{
			if (x < 10)
				DoLoop2(x/2);
		}		

		private static void DoLoop2(int x)
		{
			if (x > 10)
				DoLoop1(x*2);
		}		
	}
}