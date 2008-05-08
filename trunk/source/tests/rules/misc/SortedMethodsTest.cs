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
using System.Reflection;
using System.Runtime.Serialization;
using Smokey.Framework.Support;
using Smokey.Internal.Rules;

namespace Smokey.Tests
{
	[TestFixture]
	public class SortedMethodsTest : TypeTest
	{	
		#region Test cases
		internal class Good1		
		{
			public void A1()
			{
			}
		}

		internal class Good2	
		{
			public void A2()
			{
			}

			public void A1()
			{
			}
		}

		internal class Good3
		{
			public void A1()
			{
			}

			public void A3()
			{
			}

			public void A2()
			{
			}
		}

		internal class Good4
		{
			public void A1()
			{
			}

			public void A4()
			{
			}

			public void A3()
			{
			}

			public void A2()
			{
			}
		}

		internal class Good5
		{
			public void A1()
			{
			}

			public void A2()
			{
			}

			public void A3()
			{
			}

			public void A4()
			{
			}

			public void A5()
			{
			}
		}

		internal class Good6
		{
			public void A1()
			{
				A3();
			}

			private void A3()
			{
			}

			public void A2()
			{
			}

			public void A4()
			{
			}

			public void A5()
			{
			}

			public void A6()
			{
			}
		}

		internal class Good7
		{
			public void A3()
			{
			}

			public void A1()
			{
			}

			public void A2()
			{
			}

			public void A5()
			{
			}

			public void A4()
			{
			}
		}

		internal class Good8
		{
			public Good8()
			{
			}
			
			public void Register() 
			{
			}
					
			public void VisitBegin()
			{		
			}
					
			public void VisitReturn()
			{
			}
	
			public void VisitBranch()
			{
			}
	
			public void VisitCall()
			{
			}
	
			public void VisitEnd()
			{
			}
		}

		internal class Good9
		{
			public Good9()
			{
			}
			
			public void Alpha() 
			{
			}
			
			public void Moop()
			{		
			}
					
			public bool Shazaam {get {return false;}}
					
			public void Toggle()
			{
			}
	
			public void VisitCall()
			{
			}
	
			public void VisitEnd()
			{
			}
	
			public void VisitReturn()
			{
			}
		}

		internal class Bad1		
		{
			public void A1()
			{
			}

			public void A11()
			{
			}

			public void A3()
			{
			}

			public void A2()
			{
			}

			public void A4()
			{
			}

			public void A5()
			{
			}
		}

		internal class Bad2
		{
			protected void A1()
			{
			}

			protected void A11()
			{
			}

			protected void A3()
			{
			}

			protected void A2()
			{
			}

			protected void A4()
			{
			}

			protected void A5()
			{
			}
		}

		internal class Bad3
		{
			public Bad3()
			{
			}
			
			public void Alpha() 
			{
			}
			
			public bool Shazaam {get {return false;}}
					
			public void Beta()
			{		
			}
					
			public void VisitBranch()
			{
			}
	
			public void VisitCall()
			{
			}
	
			public void VisitEnd()
			{
			}
	
			public void VisitReturn()
			{
			}
		}

		internal class Bad4
		{			
			public event EventHandler Alpha;
			
			public bool this[int index]
			{
				get {return Alpha != null;}
				set {}
			}
			
			public static bool operator<(Bad4 lhs, Bad4 rhs)
			{
				return false;
			}

			public static bool operator>(Bad4 lhs, Bad4 rhs)
			{
				return false;
			}
			
			public static explicit operator int(Bad4 value)
			{
				return 0;
			}

			public void aa1() 
			{
			}
			
			public void aa2() 
			{
			}
			
			public void aa3() 
			{
			}
			
			public void aa4() 
			{
			}
			
			public void aa6() 
			{
			}
			
			public void aa5() 
			{
			}
			
			public void aa7() 
			{
			}
		}
		#endregion
		
		// test code
		public SortedMethodsTest() : base(
			new string[]{"Good1", "Good2", "Good3", "Good4", 
			"Good5", "Good6", "Good7", "Good8", "Good9"},
			new string[]{"Bad1", "Bad2", "Bad3", "Bad4"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new SortedMethodsRule(cache, reporter);
		}
	} 
}
