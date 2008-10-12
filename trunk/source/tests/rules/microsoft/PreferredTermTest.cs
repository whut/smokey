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
using Smokey.Framework.Support;
using Smokey.Internal.Rules;

namespace Smokey.Tests
{
	[TestFixture]
	public class PreferredTermTest : MethodTest
	{	
		#region Test classes
		public class GoodCases
		{								
			public bool IsEnterpriseServices()
			{
				return true;
			}			

			public bool IsCanceled()
			{
				return IsCancelled();
			}			

			private bool IsCancelled()
			{
				return true;
			}			
		}
				
		public class BadCases
		{
			public bool IsCancelled()
			{
				return true;
			}			

			public bool Indices()
			{
				return true;
			}			

			public bool LogOut3()
			{
				return true;
			}			

			public bool SignOff_foo()
			{
				return true;
			}			

			public bool _Writeable()
			{
				return true;
			}			

			public bool Alpha(bool writeable)
			{
				return true;
			}			

			public bool Beta(bool theIndices)
			{
				return true;
			}			

			public bool Gamma(bool aLogOut3)
			{
				return true;
			}			
		}				
		#endregion
		
		// test code
		public PreferredTermTest() : base(
			new string[]{"GoodCases.IsEnterpriseServices", "GoodCases.IsCanceled", "GoodCases.IsCancelled"},
			
			new string[]{"BadCases.IsCancelled", "BadCases.Indices", "BadCases.LogOut3", "BadCases.SignOff_foo", "BadCases._Writeable", "BadCases.Alpha", "BadCases.Beta", "BadCases.Gamma"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new PreferredTermRule(cache, reporter);
		}
	} 
}