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
	public class ConsistentEqualityTest : TypeTest
	{	
		#region Test classes
		private class Good1
		{
			public string Address {get; set;}
			
			public override bool Equals(object rhsObj)
			{
				if (rhsObj == null)			
					return false;
				
				Good1 rhs = rhsObj as Good1;
				return this == rhs;
			}

			public override int GetHashCode()
			{
				return name.GetHashCode() + Address.GetHashCode();
			}

			// == matches GetHashCode
			public static bool operator==(Good1 lhs, Good1 rhs)
			{
				if (object.ReferenceEquals(lhs, rhs))
					return true;
				
				if ((object) lhs == null || (object) rhs == null)
					return false;
				
				return lhs.name == rhs.name && lhs.Address == rhs.Address;
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
		}

		private class Good2
		{
			public override bool Equals(object rhsObj)
			{
				if (rhsObj == null)			
					return false;
				
				Good2 rhs = rhsObj as Good2;
				return this == rhs;
			}

			public override int GetHashCode()
			{
				return name.GetHashCode() ^ address.GetHashCode();
			}

			public static bool operator==(Good2 lhs, Good2 rhs)
			{
				return lhs == null ? rhs == null : lhs.Equals(rhs);
			}
			
			public static bool operator!=(Good2 lhs, Good2 rhs)
			{
				return !(lhs == rhs);
			}
				
			// Equals matches GetHashCode
			public bool Equals(Good2 rhs)	
			{
				if (object.ReferenceEquals(this, rhs))
					return true;
				
				if ((object) rhs == null)
					return false;
				
				return name == rhs.name && address == rhs.address;
			}
			
			private string name = "ted";
			private string address = "main street";
		}

		private class Good3
		{
			// Equals matches GetHashCode
			public override bool Equals(object rhsObj)
			{
				if (object.ReferenceEquals(this, rhsObj))
					return true;
									
				Good3 rhs = rhsObj as Good3;
				if (rhs == null)
					return false;
				
				return name == rhs.name && address == rhs.address;
			}

			public override int GetHashCode()
			{
				return name.GetHashCode() ^ address.GetHashCode();
			}

			public static bool operator==(Good3 lhs, Good3 rhs)
			{
				return lhs == null ? rhs == null : lhs.Equals(rhs);
			}
			
			public static bool operator!=(Good3 lhs, Good3 rhs)
			{
				return !(lhs == rhs);
			}
							
			private string name = "ted";
			private string address = "main street";
		}

		private class Good4
		{
			public override bool Equals(object rhsObj)
			{
				if (rhsObj == null)			
					return false;
				
				Good4 rhs = rhsObj as Good4;
				return this == rhs;
			}

			public override int GetHashCode()
			{
				return name.GetHashCode() ^ address.GetHashCode();
			}

			public static bool operator==(Good4 lhs, Good4 rhs)
			{
				return lhs == null ? rhs == null : lhs.Equals(rhs);
			}
			
			public static bool operator!=(Good4 lhs, Good4 rhs)
			{
				return !(lhs == rhs);
			}
				
			// Equals calls a helper
			public bool Equals(Good4 rhs)	
			{
				return DoEquals(rhs);
			}
			
			public bool DoEquals(Good4 rhs)	
			{
				if (object.ReferenceEquals(this, rhs))
					return true;
				
				if ((object) rhs == null)
					return false;
				
				return address == rhs.address;	// does not match GetHashCode but we cant catch it
			}
			
			private string name = "ted";
			private string address = "main street";
		}

		private class Good5
		{
			public override bool Equals(object rhsObj)
			{
				if (rhsObj == null)			
					return false;
				
				Good5 rhs = rhsObj as Good5;
				return this == rhs;
			}

			// GetHashCode calls a helper
			public override int GetHashCode()
			{
				return DoGetHashCode();
			}

			public int DoGetHashCode()
			{
				return address.GetHashCode();	// does not match Equals but we cant catch it
			}

			public static bool operator==(Good5 lhs, Good5 rhs)
			{
				return lhs == null ? rhs == null : lhs.Equals(rhs);
			}
			
			public static bool operator!=(Good5 lhs, Good5 rhs)
			{
				return !(lhs == rhs);
			}
				
			public bool Equals(Good5 rhs)	
			{
				if (object.ReferenceEquals(this, rhs))
					return true;
				
				if ((object) rhs == null)
					return false;
				
				return name == rhs.name && address == rhs.address;
			}
			
			private string name = "ted";
			private string address = "main street";
		}
		
		private class Good6
		{
			public override bool Equals(object rhsObj)
			{
				if (rhsObj == null)			
					return false;
				
				Good6 rhs = rhsObj as Good6;
				return this == rhs;
			}

			// properties work
			public override int GetHashCode()
			{
				return Name.GetHashCode() ^ address.GetHashCode();
			}

			public static bool operator==(Good6 lhs, Good6 rhs)
			{
				if (object.ReferenceEquals(lhs, rhs))
					return true;
				
				if ((object) lhs == null || (object) rhs == null)
					return false;
				
				return lhs.Name == rhs.Name && lhs.address == rhs.address;
			}
			
			public static bool operator!=(Good6 lhs, Good6 rhs)
			{
				return !(lhs == rhs);
			}
				
			public bool Equals(Good6 rhs)	
			{
				return this == rhs;
			}
			
			public string Name {get {return name;}}
			
			private string name = "ted";
			private string address = "main street";
		}

		public struct Good7
		{				
			public override bool Equals(object rhsObj)
			{
				if (rhsObj == null)						
					return false;
				
				if (GetType() != rhsObj.GetType()) 
					return false;
			
				Good7 rhs = (Good7) rhsObj;					
				return x == rhs.x && y == rhs.y;
			}
				
			public bool Equals(Good7 rhs)	
			{					
				return x == rhs.x && y == rhs.y;
			}
	
			public static bool operator==(Good7 lhs, Good7 rhs)
			{
				return lhs.x == rhs.x && lhs.y == rhs.y;
			}
			
			public static bool operator!=(Good7 lhs, Good7 rhs)
			{
				return !(lhs == rhs);
			}
			
			public override int GetHashCode()
			{
				return x ^ y;
			}
			
			private int x, y;
		}

		public struct Good8
		{				
			public override bool Equals(object rhsObj)
			{
				if (rhsObj == null)						
					return false;
				
				if (GetType() != rhsObj.GetType()) 
					return false;
			
				Good8 rhs = (Good8) rhsObj;					
				return this == rhs;
			}
				
			public bool Equals(Good8 rhs)	
			{					
				return this == rhs;
			}
	
			public static bool operator==(Good8 lhs, Good8 rhs)
			{
				return DoIsEquals(lhs.x, rhs.x) && DoIsEquals(lhs.y, rhs.y);
			}
			
			private static bool DoIsEquals(int lhs, int rhs)
			{
				return lhs == rhs;
			}
			
			public static bool operator!=(Good8 lhs, Good8 rhs)
			{
				return !(lhs == rhs);
			}
			
			public override int GetHashCode()
			{
				return x ^ y;
			}
			
			private int x, y;
		}

		private class Good9
		{
			public override bool Equals(object rhsObj)
			{
				if (rhsObj == null) 
					return false;
	
				Good9 uri = rhsObj as Good9;
				return InternalEquals(uri);		// helper, so we say rule passes regardless
			}

			bool InternalEquals(Good9 uri)
			{
				return this.source == uri.source;
			}

			public override int GetHashCode() 
			{
				return source.GetHashCode();
			}
		
			public static bool operator ==(Good9 u1, Good9 u2)
			{
				return object.Equals(u1, u2);
			}
	
			public static bool operator !=(Good9 u1, Good9 u2)
			{
				return !(u1 == u2);
			}
				
			public bool Equals(Good9 rhs)	
			{
				return this == rhs;
			}
						
			private string source = "main street";
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

			// Doesn't use address
			public override int GetHashCode()
			{
				return name.GetHashCode();
			}

			// == matches GetHashCode
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

			// Doesn't use address
			public override int GetHashCode()
			{
				return name.GetHashCode();
			}

			public static bool operator==(Bad2 lhs, Bad2 rhs)
			{
				return lhs == null ? rhs == null : lhs.Equals(rhs);
			}
			
			public static bool operator!=(Bad2 lhs, Bad2 rhs)
			{
				return !(lhs == rhs);
			}
				
			// Equals matches GetHashCode
			public bool Equals(Bad2 rhs)	
			{
				if (object.ReferenceEquals(this, rhs))
					return true;
				
				if ((object) rhs == null)
					return false;
				
				return name == rhs.name && address == rhs.address;
			}
			
			private string name = "ted";
			private string address = "main street";
		}

		private class Bad3
		{
			public override bool Equals(object rhsObj)
			{
				if (object.ReferenceEquals(this, rhsObj))
					return true;
									
				Bad3 rhs = rhsObj as Bad3;
				if ((object) rhs == null)
					return false;
				
				return name == rhs.name && address == rhs.address;
			}

			// Doesn't use address.
			public override int GetHashCode()
			{
				return name.GetHashCode();
			}

			public static bool operator==(Bad3 lhs, Bad3 rhs)
			{
				return lhs == null ? rhs == null : lhs.Equals(rhs);
			}
			
			public static bool operator!=(Bad3 lhs, Bad3 rhs)
			{
				return !(lhs == rhs);
			}
							
			private string name = "ted";
			private string address = "main street";
		}

		private class Bad4
		{
			public override bool Equals(object rhsObj)
			{
				if (rhsObj == null)			
					return false;
				
				Bad4 rhs = rhsObj as Bad4;
				return this == rhs;
			}

			public override int GetHashCode()
			{
				return name.GetHashCode() ^ address.GetHashCode();
			}

			// Doesn't use name.
			public static bool operator==(Bad4 lhs, Bad4 rhs)
			{
				if (object.ReferenceEquals(lhs, rhs))
					return true;
				
				if ((object) lhs == null || (object) rhs == null)
					return false;
				
				return lhs.address == rhs.address;
			}
			
			public static bool operator!=(Bad4 lhs, Bad4 rhs)
			{
				return !(lhs == rhs);
			}
				
			public bool Equals(Bad4 rhs)	
			{
				return this == rhs;
			}
			
			private string name = "ted";
			private string address = "main street";
		}
		
		private class Bad5
		{
			public override bool Equals(object rhsObj)
			{
				if (rhsObj == null)			
					return false;
				
				Bad5 rhs = rhsObj as Bad5;
				return this == rhs;
			}

			// GetHashCode uses field, == uses property
			public override int GetHashCode()
			{
				return name.GetHashCode() ^ address.GetHashCode();
			}

			public static bool operator==(Bad5 lhs, Bad5 rhs)
			{
				if (object.ReferenceEquals(lhs, rhs))
					return true;
				
				if ((object) lhs == null || (object) rhs == null)
					return false;
				
				return lhs.Name == rhs.Name && lhs.address == rhs.address;
			}
			
			public static bool operator!=(Bad5 lhs, Bad5 rhs)
			{
				return !(lhs == rhs);
			}
				
			public bool Equals(Bad5 rhs)	
			{
				return this == rhs;
			}
			
			public string Name {get {return name;}}
			
			private string name = "ted";
			private string address = "main street";
		}
		
		private class Bad6
		{
			public override bool Equals(object rhsObj)
			{
				if (rhsObj == null)			
					return false;
				
				Bad6 rhs = rhsObj as Bad6;
				return this == rhs;
			}

			// GetHashCode uses name and foo, == uses name and address
			public override int GetHashCode()
			{
				return name.GetHashCode() ^ foo.GetHashCode();
			}

			public static bool operator==(Bad6 lhs, Bad6 rhs)
			{
				if (object.ReferenceEquals(lhs, rhs))
					return true;
				
				if ((object) lhs == null || (object) rhs == null)
					return false;
				
				return lhs.name == rhs.Name && lhs.address == rhs.address;
			}
			
			public static bool operator!=(Bad6 lhs, Bad6 rhs)
			{
				return !(lhs == rhs);
			}
				
			public bool Equals(Bad6 rhs)	
			{
				return this == rhs;
			}
			
			public string Name {get {return name;}}
			
			private string name = "ted";
			private string address = "main street";
			private string foo = "the foo";
		}
		
		private class Bad7
		{
			// Equals uses name and foo, == uses name and address.
			public override bool Equals(object rhsObj)
			{
				if (rhsObj == null)			
					return false;
				
				Bad7 rhs = rhsObj as Bad7;
				return name == rhs.Name && foo == rhs.foo;
			}

			public override int GetHashCode()
			{
				return base.GetHashCode();
			}

			public static bool operator==(Bad7 lhs, Bad7 rhs)
			{
				if (object.ReferenceEquals(lhs, rhs))
					return true;
				
				if ((object) lhs == null || (object) rhs == null)
					return false;
				
				return lhs.name == rhs.Name && lhs.address == rhs.address;
			}
			
			public static bool operator!=(Bad7 lhs, Bad7 rhs)
			{
				return !(lhs == rhs);
			}
				
			public bool Equals(Bad7 rhs)	
			{
				return this == rhs;
			}
			
			public string Name {get {return name;}}
			
			private string name = "ted";
			private string address = "main street";
			private string foo = "the foo";
		}
		#endregion
		
		// test code
		public ConsistentEqualityTest() : base(
			new string[]{"Good1", "Good2", "Good3", "Good4", "Good5", "Good6", 
			"Good7", "Good8", "Good9"},
			
			new string[]{"Bad1", "Bad2", "Bad3", "Bad4", "Bad5", "Bad6", "Bad7"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new ConsistentEqualityRule(cache, reporter);
		}
	} 
}
