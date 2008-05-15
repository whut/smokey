// Copyright (C) 2008 Jesse Jones
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

using Smokey.Framework;
using Smokey.Framework.Support;
using Smokey.Internal.Rules;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Smokey.Tests
{
	[TestFixture]
	public class ValidateArgs2Test : MethodTest
	{	
		#region Test classes
		internal class Cases
		{
			public int Good1(List<int> list) 
			{ 
				if (list == null)
					throw new ArgumentNullException("list");
								
				return list[list.Count - 1];
			} 

			public int Good2(List<int> list) 
			{ 
				if (list == null)
					return 0;
								
				return list[list.Count - 1];
			} 

			public int Good3(List<int> list) 
			{ 
				DBC.Pre(list != null, "list is null");
								
				return list[list.Count - 1];
			} 

			public void Good4(List<int> list) 
			{ 								
				Console.WriteLine(list);
			} 

			protected int Good5(List<int> list) 
			{ 								
				return list[list.Count - 1];
			} 

			public int Good6(string s) 
			{ 								
				return string.Compare(s, s);
			} 

			public char Good7(string s) 
			{ 			
				if (s != null)
					return s[0];
				else
					return ' ';
			} 

			internal int Bad1(List<int> list) 
			{ 								
				return list[list.Count - 1];
			} 

			public int Bad2(List<int> list) 
			{ 								
				return list.IndexOf(3, 4, 5); 
			} 
		}		
		#endregion
		
		// test code
		public ValidateArgs2Test() : base(
			new string[]{"Cases.Good1", "Cases.Good2", "Cases.Good3", "Cases.Good4", "Cases.Good5", "Cases.Good6", "Cases.Good7"},
			new string[]{"Cases.Bad1", "Cases.Bad2"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new ValidateArgs2Rule(cache, reporter);
		}
	} 
}
