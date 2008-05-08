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

using Mono.Cecil;
using NUnit.Framework;
using Smokey.Framework.Support;
using Smokey.Internal.Rules;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security;
using System.Security.Permissions;

namespace Smokey.Tests
{
	[TestFixture]
	public class SecureAssertsTest : MethodTest
	{	
		#region Test Classes
		public class Good1
		{
			[PermissionSet(System.Security.Permissions.SecurityAction.Demand, Name = "FullTrust")]
			public static void A(string text)
			{
				PermissionSet p = new PermissionSet(PermissionState.Unrestricted);
				p.Assert();
			}
		}

		[PermissionSet(System.Security.Permissions.SecurityAction.Demand, Name = "FullTrust")]
		public class Good2
		{
			public static void A(string text)
			{
				PermissionSet p = new PermissionSet(PermissionState.Unrestricted);
				p.Assert();
			}
		}

		[PermissionSet(System.Security.Permissions.SecurityAction.Demand, Name = "FullTrust")]
		public class Good3
		{
			public static void A(string text)
			{
				EnvironmentPermission p = new EnvironmentPermission(EnvironmentPermissionAccess.Read, "foo");
				p.Assert();
			}
		}

		public class Good4
		{
			[SecurityPermission(System.Security.Permissions.SecurityAction.Demand, SerializationFormatter = true)]
			public static void A(string text)
			{
				PermissionSet p = new PermissionSet(PermissionState.Unrestricted);
				p.Assert();
			}
		}

		public class Good5
		{
			[PermissionSet(System.Security.Permissions.SecurityAction.LinkDemand, Name = "FullTrust")]
			[PermissionSet(System.Security.Permissions.SecurityAction.Assert, Name = "FullTrust")]
			public static void A(string text)
			{
				Console.WriteLine(100);
			}
		}

		public class Good6
		{
			[PermissionSet(System.Security.Permissions.SecurityAction.Assert, Name = "FullTrust")]
			[PermissionSet(System.Security.Permissions.SecurityAction.LinkDemand, Name = "FullTrust")]
			public static void A(string text)
			{
				Console.WriteLine(100);
			}
		}

		public class Bad1
		{
			public static void A(string text)
			{
				PermissionSet p = new PermissionSet(PermissionState.Unrestricted);
				p.Assert();
			}
		}

		public class Bad2
		{
			public static void A(string text)
			{
				EnvironmentPermission p = new EnvironmentPermission(EnvironmentPermissionAccess.Read, "foo");
				p.Assert();
			}
		}

		public class Bad3
		{
			[PermissionSet(System.Security.Permissions.SecurityAction.Assert, Name = "FullTrust")]
			public static void A(string text)
			{
				Console.WriteLine(100);
			}
		}
		#endregion
		
		// test code
		public SecureAssertsTest() : base(
			new string[]{"Good1.A", "Good2.A", "Good3.A", "Good4.A", "Good5.A", "Good6.A"},
			new string[]{"Bad1.A", "Bad2.A", "Bad3.A"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new SecureAssertsRule(cache, reporter);
		}
	} 
}
