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
using System.Reflection;
using Smokey.Framework.Support;
using Smokey.Internal.Rules;

namespace Smokey.Tests
{
	[TestFixture]
	public class EqualsMissesStateTest : TypeTest
	{	
		#region Test classes
		private class Good1
		{
			public string Name
			{
				get {return m_name;}		// Equals tests this
				set {m_name = value;}
			}
			
			public override bool Equals(object rhsObj)
			{
				if (rhsObj == null)			
					return false;
				
				Good1 rhs = rhsObj as Good1;
				return m_name == rhs.m_name && m_address == rhs.m_address;
			}

			public override int GetHashCode()
			{
				return m_name.GetHashCode() + m_address.GetHashCode();
			}
						
			private string m_name = "ted";
			private string m_address = "main street";
		}

		private class Good2
		{
			public string Name
			{
				get {return m_name;}		
				set {m_name = value; m_firstName = m_name.Split()[0];}
			}
			
			public string FirstName
			{
				get {return m_firstName;}		// no setter
			}
			
			public override bool Equals(object rhsObj)
			{
				if (rhsObj == null)			
					return false;
				
				Good2 rhs = rhsObj as Good2;
				return m_name == rhs.m_name && m_address == rhs.m_address;
			}

			public override int GetHashCode()
			{
				return m_name.GetHashCode() + m_address.GetHashCode();
			}
						
			private string m_name = "ted";
			private string m_address = "main street";
			private string m_firstName = "ted";
		}

		private class Good3
		{
			public string FirstName
			{
				get {return m_firstName;}		
				set {m_firstName = value; m_firstName = m_firstName + " " + m_lastName;}
			}
			
			public string LastName
			{
				get {return m_lastName;}		
				set {m_lastName = value; m_firstName = m_firstName + " " + m_lastName;}
			}
			
			public string Name
			{
				get {return m_name;}		// not trivial
				set {m_firstName = value.Split()[0]; m_lastName = value.Split()[1]; m_firstName = m_firstName + " " + m_lastName;}
			}
			
			public override bool Equals(object rhsObj)
			{
				if (rhsObj == null)			
					return false;
				
				Good3 rhs = rhsObj as Good3;
				return m_firstName == rhs.m_firstName && m_lastName == rhs.m_lastName;
			}

			public override int GetHashCode()
			{
				return m_firstName.GetHashCode() + m_lastName.GetHashCode();
			}
						
			private string m_firstName = "ted";
			private string m_lastName = "main street";
			private string m_name = "ted";
		}

		private class Good4
		{
			public string FirstName {get; set;} 
			public string LastName {get; set;} 

			public override bool Equals(object rhsObj)
			{
				if (rhsObj == null)			
					return false;
				
				Good4 rhs = rhsObj as Good4;
				return FirstName == rhs.FirstName && LastName == rhs.LastName;
			}

			public override int GetHashCode()
			{
				return FirstName.GetHashCode() + LastName.GetHashCode();
			}
		}
		
		private class Good5
		{
			public string Name
			{
				get {return m_name;}		
				set {m_name = value;}
			}
			
			public string FirstName
			{
				get {return m_firstName;}		// Equals doesnt check this, but setter is protected
				protected set {m_firstName = value;}
			}
			
			public override bool Equals(object rhsObj)
			{
				if (rhsObj == null)			
					return false;
				
				Good5 rhs = rhsObj as Good5;
				return m_name == rhs.m_name && m_address == rhs.m_address;
			}

			public override int GetHashCode()
			{
				return m_name.GetHashCode() + m_address.GetHashCode();
			}
						
			private string m_name = "ted";
			private string m_address = "main street";
			private string m_firstName = "ted";
		}

		private class Good6
		{
			public string Name
			{
				get {return m_name;}		// operator== tests this
				set {m_name = value;}
			}
			
			public override bool Equals(object rhsObj)
			{
				if (rhsObj == null)			
					return false;
				
				Good6 rhs = rhsObj as Good6;
				return this == rhs;			// calls a static method
			}
			
			public static bool operator==(Good6 lhs, Good6 rhs)
			{
				return lhs.m_name == rhs.m_name && lhs.m_address == rhs.m_address;
			}

			public static bool operator!=(Good6 lhs, Good6 rhs)
			{
				return !(lhs == rhs);
			}

			public override int GetHashCode()
			{
				return m_name.GetHashCode() + m_address.GetHashCode();
			}
						
			private string m_name = "ted";
			private string m_address = "main street";
		}

		private class Good7
		{
			public string Name
			{
				get {return m_name;}	
				set {m_name = value;}
			}
			
			public override bool Equals(object rhsObj)
			{
				if (rhsObj == null)			
					return false;
				
				Good7 rhs = rhsObj as Good7;
				return DoIsEquals(rhs);			// calls an instance method
			}
			
			private bool DoIsEquals(Good7 rhs)
			{
				return m_name == rhs.m_name && m_address == rhs.m_address;
			}

			public override int GetHashCode()
			{
				return m_name.GetHashCode() + m_address.GetHashCode();
			}
						
			private string m_name = "ted";
			private string m_address = "main street";
		}

		private class Bad1
		{
			public string Name
			{
				get {return m_name;}		
				set {m_name = value;}
			}
			
			public string FirstName
			{
				get {return m_firstName;}		// Equals doesnt check this
				set {m_firstName = value;}
			}
			
			public override bool Equals(object rhsObj)
			{
				if (rhsObj == null)			
					return false;
				
				Bad1 rhs = rhsObj as Bad1;
				return m_name == rhs.m_name && m_address == rhs.m_address;
			}

			public override int GetHashCode()
			{
				return m_name.GetHashCode() + m_address.GetHashCode();
			}
						
			private string m_name = "ted";
			private string m_address = "main street";
			private string m_firstName = "ted";
		}

		private class Bad2
		{
			public string Name
			{
				get {return m_name;}		
				set {m_name = value;}
			}
			
			public string FirstName
			{
				get {return m_firstName;}		// operator== doesnt check this
				set {m_firstName = value;}
			}
			
			public override bool Equals(object rhsObj)
			{
				if (rhsObj == null)			
					return false;
				
				Bad2 rhs = rhsObj as Bad2;
				return this == rhs;
			}

			public static bool operator==(Bad2 lhs, Bad2 rhs)
			{
				if (object.ReferenceEquals(lhs, rhs))
					return true;
				
				if ((object) lhs == null || (object) rhs == null)
					return false;
				
				return lhs.m_name == rhs.m_name && lhs.m_address == rhs.m_address;
			}
			
			public static bool operator!=(Bad2 lhs, Bad2 rhs)
			{
				return !(lhs == rhs);
			}
				
			public override int GetHashCode()
			{
				return m_name.GetHashCode() + m_address.GetHashCode();
			}
						
			private string m_name = "ted";
			private string m_address = "main street";
			private string m_firstName = "ted";
		}

		private class Bad3
		{
			public string FirstName {get; set;} 
			public string LastName {get; set;} 
			public string Name {get; set;} 			// Equals doesn't check this

			public override bool Equals(object rhsObj)
			{
				if (rhsObj == null)			
					return false;
				
				Bad3 rhs = rhsObj as Bad3;
				return FirstName == rhs.FirstName && LastName == rhs.LastName;
			}

			public override int GetHashCode()
			{
				return FirstName.GetHashCode() + LastName.GetHashCode();
			}
		}		
		#endregion
		
		// test code
		public EqualsMissesStateTest() : base(
			new string[]{"Good1", "Good2", "Good3", "Good4", "Good5", "Good6", "Good7"},
			new string[]{"Bad1", "Bad2", "Bad3"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new EqualsMissesStateRule(cache, reporter);
		}
	} 
}
