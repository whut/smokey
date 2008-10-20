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
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Smokey.Framework.Support;
using Smokey.Internal.Rules;

namespace Smokey.Tests
{
	public static class DoubleExtensions
	{
		//- Returns true if the two floating point numbers are close to each other.
		public static bool NearlyEquals(this double x, double y)
		{
			return x.NearlyEquals(y, 1.0e-6);
		}
	
		//- Comparing floating point numbers for equality is tricky because the 
		//- limited precision of the hardware introduces small errors so that two 
		//- numbers that should compare equal don't. So what we do is consider the 
		//- numbers equal if their difference is less than some epsilon value. But 
		//- choosing this epsilon is also tricky because if the numbers are large 
		//- epsilon should also be large, and if the numbers are very small the 
		//- epsilon must be even smaller. To get around this we'll use a technique 
		//- from Knuth's "Seminumerical Algorithms" section 4.2.2 and scale epsilon 
		//- by the exponent of one of the specified numbers. 
		public static bool NearlyEquals(this double x, double y, double epsilon)
		{
			Debug.Assert(epsilon >= 0.0);
						
			// Infinity is an exact value so we can use equals with it.
			if (double.IsInfinity(x) || double.IsInfinity(y))
				return x == y;
			
			// Knuth recommends scaling by the exponent of the smallest number.
			// C has standard functions for breaking apart floating point numbers
			// (frexp) and scaling them (ldexp), but .NET does not. So, we'll
			// scale by a percentage of the smallest value which should give
			// us very similar results.
			double scaling = 0.75 * Math.Min(Math.Abs(x), Math.Abs(y));
				
			// If we of our numbers is exactly zero then we cannot use an
			// extremely small scaled epsilon or numbers will never compare
			// equal to zero. So, we do like Knuth, and use whatever epsilion
			// was passed in.
			double delta = scaling != 0.0 ? epsilon * scaling : epsilon;
			
			// If the difference between the numbers is smaller than the scaled
			// epsilon we'll consider the numbers to be equal.
			double difference = Math.Abs(x - y);
			bool equal = difference <= delta;
			
			return equal;
		}
	}

	[TestFixture]
	public class CompareFloatsTest : MethodTest
	{	
		#region Test classes
		private class Cases
		{
			public Cases(double x)
			{
				m_x = x;
			}
			
			public double X
			{
				get {return m_x;}
			}
			
			public double GetX()
			{
				return m_x;
			}
			
			public static double GetPI()
			{
				return 3.1415;
			}
			
			public bool GoodInts(int x, int y)
			{
				return x == y;
			}
						
			public bool GoodFloats(double x, double y)
			{
				return Math.Abs(x - y) < 0.001;
			}
			
			public bool GoodInfinity1(double x)
			{
				return x == double.PositiveInfinity;
			}
			
			public bool GoodInfinity2(double x)
			{
				return x == float.NegativeInfinity;
			}
			
			public bool GoodNearlyEquals(double x, double y)
			{
				return x.NearlyEquals(y);
			}
			
			public bool BadFloats(float x, float y)
			{
				return x == y;
			}
			
			public bool BadDoubles(double x, double y)
			{
				return x == y;
			}
			
			public static bool BadStaticFloats(double x, double y)
			{
				return x == y;
			}
			
			public bool BadCall(double x, double y)
			{
				return GetX() == y;
			}
			
			public bool BadStaticCall(double x, double y)
			{
				return GetPI() == y;
			}
			
			public bool BadProperty(double y)
			{
				return X == y;
			}
			
			public static bool BadArith(double x, double y)
			{
				double x1 = x + y;
				double x2 = x * y;
				return x1 == x2;
			}
			
			public static bool BadArith2(double x, double y)
			{
				return (x + y) == (x * y);
			}
			
			public static bool BadArith3(double x, double y)
			{
				return (x + y / 2.0) == (x * y + x - y * 2.12);
			}
			
			public bool BadField(double y)
			{
				return m_x == y;
			}
			
			public static bool BadStaticField(double y)
			{
				return ms_pi == y;
			}
			
			public bool BadInt(int y)
			{
				return m_x == y;
			}
			
			public bool BadInt2(int x, double y)
			{
				return x == y;
			}
			
			public bool BadEquals(double x, double y)
			{
				return x.Equals(y);
			}
			
			public bool BadNotEqual(double x, double y)
			{
				return x != y;
			}
						
			private double m_x;
			private static double ms_pi = 3.1415;
		}		
		#endregion
		
		// test code
		public CompareFloatsTest() : base(
			new string[]{"Cases.GoodInts", "Cases.GoodFloats", "Cases.GoodInfinity1", 
			"Cases.GoodInfinity2", "Cases.GoodNearlyEquals"},
			
			new string[]{"Cases.BadFloats", "Cases.BadDoubles", "Cases.BadProperty", 
			"Cases.BadStaticFloats", "Cases.BadCall", "Cases.BadStaticCall", 
			"Cases.BadArith2", "Cases.BadArith", "Cases.BadArith3", "Cases.BadField", "Cases.BadInt", 
			"Cases.BadInt2", 
			"Cases.BadEquals", "Cases.BadStaticField", "Cases.BadNotEqual"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new CompareFloatsRule(cache, reporter);
		}

		[Test]
		public void NearlyEquals()
		{
			double zero = 0.0;
			double one = 1.0;
			double two = 2.0;
			double huge1 = Math.PI * 1.0e80;
			double huge2 = Math.E * 1.0e80;
			double tiny1 = Math.PI * 1.0e-80;
			double tiny2 = Math.E * 1.0e-80;

			Assert.IsTrue(one.NearlyEquals(one));
			Assert.IsTrue(one.NearlyEquals(one - double.Epsilon));
			Assert.IsTrue(one.NearlyEquals(1.001, 0.01));
			Assert.IsTrue(huge1.NearlyEquals(huge1));
			Assert.IsTrue(huge1.NearlyEquals(huge1 + 100.0));
			Assert.IsTrue(tiny1.NearlyEquals(tiny1));
			Assert.IsTrue(tiny1.NearlyEquals(tiny1 - double.Epsilon));
			Assert.IsTrue(double.PositiveInfinity.NearlyEquals(double.PositiveInfinity));
			Assert.IsTrue(double.NegativeInfinity.NearlyEquals(double.NegativeInfinity));
			Assert.IsTrue(zero.NearlyEquals(zero));
			Assert.IsTrue(zero.NearlyEquals(-zero));
			Assert.IsTrue(zero.NearlyEquals(tiny1));
			
			Assert.IsFalse(one.NearlyEquals(two));
			Assert.IsFalse(one.NearlyEquals(-one));
			Assert.IsFalse(one.NearlyEquals(1.001, 0.0001));
			Assert.IsFalse(huge1.NearlyEquals(huge2));
			Assert.IsFalse(one.NearlyEquals(huge2));
			Assert.IsFalse(tiny1.NearlyEquals(tiny2));
			Assert.IsFalse(tiny1.NearlyEquals(one));
			Assert.IsFalse(huge1.NearlyEquals(double.PositiveInfinity));
			Assert.IsFalse(double.NaN.NearlyEquals(double.NaN));
			Assert.IsFalse(one.NearlyEquals(double.NaN));
			Assert.IsFalse(double.PositiveInfinity.NearlyEquals(double.NaN));
			Assert.IsFalse(double.NaN.NearlyEquals(one));
			Assert.IsFalse(one.NearlyEquals(zero));
			Assert.IsFalse(zero.NearlyEquals(one));
			Assert.IsFalse(zero.NearlyEquals(huge1));
		}
	} 
}

