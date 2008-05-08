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
[assembly: SecurityCritical(SecurityCriticalScope.Everything)]

// R1024/UncheckedAssembly
namespace Aptca3
{
	public static class GoodClass1
	{				
		[PermissionSet(SecurityAction.Demand, Unrestricted = true)]
		public static void Work()
		{
			SecureClass.RevertDocument();
		}
	}

	public static class GoodClass2
	{				
		// S1006/ImperativeSecurity
		public static void Work()
		{
			NamedPermissionSet permissions = new NamedPermissionSet("Custom");
			permissions.Demand();		
					
			SecureClass.RevertDocument();
		}
	}

	[PermissionSet(SecurityAction.Demand, Unrestricted = true)]
	public class DerivedClass : BaseClass
	{				
		public override void Print(string text)
		{    
			base.Print("label: " + text);
		}
		
		public static int Sum(int x, int y)
		{
			return x + y;
		}

		[EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Read = "LOG_PATH")]
		[EnvironmentPermissionAttribute(SecurityAction.Assert, Read = "LOG_PATH")]
		[SecurityCritical]
		public static void Assert1()
		{
		}
	}
}