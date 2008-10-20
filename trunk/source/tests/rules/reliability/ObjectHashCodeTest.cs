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
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Smokey.Framework.Support;
using Smokey.Internal.Rules;

namespace Smokey.Tests
{
	[TestFixture]
	public class ObjectHashCodeTest : TypeTest
	{	
		#region Test classes
		private class Good1
		{
			public override bool Equals(object rhsObj)
			{
				if (rhsObj == null)			
					return false;
				
				Good1 rhs = rhsObj as Good1;
				return this == rhs;
			}

			// no base call
			public override int GetHashCode()
			{
				return name.GetHashCode() ^ address.GetHashCode();
			}

			public static bool operator==(Good1 lhs, Good1 rhs)
			{
				if (object.ReferenceEquals(lhs, rhs))
					return true;
				
				if ((object) lhs == null || (object) rhs == null)
					return false;
				
				return lhs.name == rhs.name && lhs.address == rhs.address;
			}
			
			public static bool operator!=(Good1 lhs, Good1 rhs)
			{
				return !(lhs == rhs);
			}
				
			public bool Equals(Good1 rhs)	
			{
				return this == rhs;
			}
			
			private string name = "ted";
			private string address = "main street";
		}

		private class Good2 : Good1
		{
			public override bool Equals(object rhsObj)
			{
				if (rhsObj == null)			
					return false;
				
				Good2 rhs = rhsObj as Good2;
				return this == rhs;
			}

			// base isn't Object
			public override int GetHashCode()
			{
				return base.GetHashCode() ^ state.GetHashCode();
			}

			public static bool operator==(Good2 lhs, Good2 rhs)
			{
				return lhs == null ? rhs == null : lhs.Equals(rhs);
			}
			
			public static bool operator!=(Good2 lhs, Good2 rhs)
			{
				return !(lhs == rhs);
			}
				
			public bool Equals(Good2 rhs)	
			{
				if (object.ReferenceEquals(this, rhs))
					return true;
				
				if ((object) rhs == null)
					return false;
					
				if (!base.Equals(rhs))
					return false;
				
				return state == rhs.state;
			}
			
			private string state = "happy";
		}

		private struct Good3
		{
			public override bool Equals(object rhsObj)
			{
				if (rhsObj == null)			
					return false;
				
				Good3 rhs = (Good3) rhsObj;
				return this == rhs;
			}

			// inefficient, but will work as long as equals checks all the state
			public override int GetHashCode()
			{
				return base.GetHashCode();
			}

			public static bool operator==(Good3 lhs, Good3 rhs)
			{								
				return lhs.name == rhs.name && lhs.address == rhs.address;
			}
			
			public static bool operator!=(Good3 lhs, Good3 rhs)
			{
				return !(lhs == rhs);
			}
				
			public bool Equals(Good3 rhs)	
			{
				return this == rhs;
			}
			
			private string name;
			private string address;
		}

		private class Good4
		{
			// Doesn't check any state
			public override bool Equals(object rhsObj)
			{
				return base.Equals(rhsObj);
			}

			public override int GetHashCode()
			{
				return base.GetHashCode();
			}
			
			public void Stuff()
			{
				Console.WriteLine(name + address);
			}
			
			private string name = "ted";
			private string address = "main street";
		}

		private class Bad1
		{
			public override bool Equals(object rhsObj)
			{
				if (rhsObj == null)			
					return false;
				
				Bad1 rhs = rhsObj as Bad1;
				return this == rhs;
			}

			// Equals check state, but we call base
			public override int GetHashCode()
			{
				return base.GetHashCode();
			}

			public static bool operator==(Bad1 lhs, Bad1 rhs)
			{
				if (object.ReferenceEquals(lhs, rhs))
					return true;
				
				if ((object) lhs == null || (object) rhs == null)
					return false;
				
				return lhs.name == rhs.name && lhs.address == rhs.address;
			}
			
			public static bool operator!=(Bad1 lhs, Bad1 rhs)
			{
				return !(lhs == rhs);
			}
				
			public bool Equals(Bad1 rhs)	
			{
				return this == rhs;
			}
			
			private string name = "ted";
			private string address = "main street";
		}

		private class Bad2
		{
			public override bool Equals(object rhsObj)
			{
				if (rhsObj == null)			
					return false;
				
				Bad2 rhs = rhsObj as Bad2;
				return this == rhs;
			}

			// Equals check state, but we call base
			public override int GetHashCode()
			{
				return name.GetHashCode() ^ base.GetHashCode() ^ address.GetHashCode();
			}

			public static bool operator==(Bad2 lhs, Bad2 rhs)
			{
				if (object.ReferenceEquals(lhs, rhs))
					return true;
				
				if ((object) lhs == null || (object) rhs == null)
					return false;
				
				return lhs.name == rhs.name && lhs.address == rhs.address;
			}
			
			public static bool operator!=(Bad2 lhs, Bad2 rhs)
			{
				return !(lhs == rhs);
			}
				
			public bool Equals(Bad2 rhs)	
			{
				return this == rhs;
			}
			
			private string name = "ted";
			private string address = "main street";
		}

		private class Bad3
		{
			public override bool Equals(object rhsObj)
			{
				if (rhsObj == null)			
					return false;
				
				Bad3 rhs = rhsObj as Bad3;
				return this == rhs;
			}

			// Equals check state, but we call base
			public override int GetHashCode()
			{
				return name.GetHashCode() ^ base.GetHashCode() ^ address.GetHashCode();
			}

			public static bool operator==(Bad3 lhs, Bad3 rhs)
			{
				if (object.ReferenceEquals(lhs, rhs))
					return true;
				
				if ((object) lhs == null || (object) rhs == null)
					return false;
				
				return lhs.Name == rhs.Name && lhs.Address == rhs.Address;
			}
			
			public static bool operator!=(Bad3 lhs, Bad3 rhs)
			{
				return !(lhs == rhs);
			}
				
			public bool Equals(Bad3 rhs)	
			{
				return this == rhs;
			}
			
			public string Name {get {return name;}}
			public string Address {get {return address;}}
			
			private string name = "ted";
			private string address = "main street";
		}
		#endregion
		
		// test code
		public ObjectHashCodeTest() : base(
			new string[]{"Good1", "Good2", "Good3", "Good4"},
			new string[]{"Bad1", "Bad2", "Bad3"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new ObjectHashCodeRule(cache, reporter);
		}
	} 
}

