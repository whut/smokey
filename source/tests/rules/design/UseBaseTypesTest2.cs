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
	public class UseBaseTypesTest2 : MethodTest
	{	
		// test classes
		public interface IInterface
		{
			void ImplementMe(int x);
		}			
				
		public class Base
		{
			public void BaseMethod(int x)
			{
				Console.WriteLine(x);
			}

			public virtual void OverridenMethod(int x)
			{
				Console.WriteLine(x);
			}
		}			
		
		public class Derived : Base, IInterface
		{
			public void DerivedMethod(int x)
			{
				Console.WriteLine(x);
			}

			public override void OverridenMethod(int x)
			{
				Console.WriteLine(x);
			}

			public void ImplementMe(int x)
			{
				Console.WriteLine(x);
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
	
		public class Good
		{
			public event EventHandler<NameChangedEventArgs> NameChanged;

			public void DCall(Derived d)
			{
				d.DerivedMethod(10);
			}

			public void TwoCalls(Derived d)
			{
				d.DerivedMethod(10);
				d.DerivedMethod(20);
			}

			public void CrossCalls(Derived d)
			{
				d.BaseMethod(10);
				d.DerivedMethod(20);
			}

			public void StoreField(Derived d)
			{
				m_d = d;
				d.BaseMethod(10);
			}

			public void StoreStaticField(Derived d)
			{
				ms_d = d;
				d.BaseMethod(10);
			}

			public void StoreLocal(Derived d)
			{
				Base b = d;				// we don't try to analyze how locals are used
				b.BaseMethod(10);
				d.BaseMethod(10);
			}

			public void Call(Derived d)
			{
				d.BaseMethod(10);
				StoreField(d);
			}

			public static void Static(Derived d)
			{
				d.DerivedMethod(10);
			}

			public static void ValueType(int n)
			{
				Console.WriteLine(n);
			}

			public static void Reserved(object x)
			{					
				Console.WriteLine("{0}", x);
			}

			public static bool operator==(Good lhs, Good rhs)
			{
				if (object.ReferenceEquals(lhs, rhs))
					return true;
				
				if ((object) lhs == null || (object) rhs == null)
					return false;
				
				return lhs.m_name == rhs.m_name;
			}

			public static bool operator!=(Good lhs, Good rhs)
			{
				return false;
			}

			public override bool Equals(object rhs)
			{
				return false;
			}

			public override int GetHashCode()
			{
				return 0;
			}

			public void Add(string key, string value)
			{				
				m_table[key] = value;
			}
		
			public Derived m_d;
			public static Derived ms_d;
			public string m_name;
			public Dictionary<string, string> m_table = new Dictionary<string, string>();
		}			
		
		public class Bad	
		{
			public void BCall(Derived d)
			{
				d.BaseMethod(10);
			}

			public void TwoCalls(Derived d)
			{
				d.BaseMethod(10);
				d.BaseMethod(20);
			}

			public void ICall(Derived d)
			{
				d.ImplementMe(20);
			}

			public void CrossCalls(Derived d)
			{
				d.BaseMethod(10 + d.GetHashCode());
			}

			public void StoreField(Derived d)
			{
				m_b = d;
				d.BaseMethod(10);
			}

			public void StoreStaticField(Derived d)
			{
				ms_b = d;
				d.BaseMethod(10);
			}

			public void Call(Derived d)
			{
				DoBaseCall(d);
			}

			public static void Static(Derived d)
			{
				d.BaseMethod(10);
			}

			public static void WriteLine(Derived d)
			{
				Console.WriteLine(d);
			}

			public void DoBaseCall(Base b)
			{
				b.BaseMethod(10);
			}

			public Base m_b;
			public static Base ms_b;	
		}			
				
		// test code
		public UseBaseTypesTest2() : base(
			new string[]{"Good.DCall", "Good.TwoCalls", "Good.CrossCalls", 
				"Good.StoreField", "Good.StoreStaticField", "Good.StoreLocal", 
				"Good.Call", "Good.Static", "Good.ValueType", "Good.Reserved", 
				"Good.op_Equality", "Good.add_NameChanged", "Good.Add"},
			
			new string[]{"Bad.BCall", "Bad.TwoCalls", "Bad.ICall", "Bad.CrossCalls",
			"Bad.StoreField", "Bad.StoreStaticField", "Bad.Call", "Bad.Static", 
			"Bad.WriteLine"},

			new string[]{"IInterface.", "Base.", "Derived."})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new UseBaseTypesRule(cache, reporter);
		}
	} 
}

