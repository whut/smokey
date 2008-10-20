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
	[TestFixture]
	public class IdenticalBranchTest : MethodTest
	{	
		#region Test Classes
		private class Cases
		{
			public int Good1(string s)
			{
				if (s == "hey")
					Console.WriteLine("pos");
				else
					Console.WriteLine("zero");
					
				int n = 0;
				if (s == "hey")
					++n;
				else
					--n;
					
				return n;
			}
			
			public void Bad1(string s)
			{
				if (s == "hey")
					Console.WriteLine("pos");
				else
					Console.WriteLine("pos");
			}
			
			public int Bad2(string s)
			{
				int n = 0;
				if (s == "hey")
					++n;
				else
					++n;
					
				return n;
			}
			
			public void Bad3(string s)
			{
				if (s == "hey")
				{
					s += "hmm";
					Console.WriteLine(s);
				}
				else
				{
					s += "hmm";
					Console.WriteLine(s);
				}
			}
			
			public void Bad4(string s)
			{
				if (s == "hey")
				{
					if (s.Length > 0)
						s += m_data;
					Console.WriteLine(s);
				}
				else
				{
					if (s.Length > 0)
						s += m_data;
					Console.WriteLine(s);
				}
			}
			
			public void Bad5(string s, string t)
			{
				string a = s + t;
				string b = t + "hmm";
				
				if (a == "hey")
				{
					Console.WriteLine(a + b);
				}
				else
				{
					Console.WriteLine(a + b);
				}
			}
			
			private string m_data = "gah";
		}		
		#endregion
		
		// test code
		public IdenticalBranchTest() : base(
			new string[]{"Cases.Good1"},
			new string[]{"Cases.Bad1", "Cases.Bad2", "Cases.Bad3", "Cases.Bad4", "Cases.Bad5"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new IdenticalBranchRule(cache, reporter);
		}
	} 
}

