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
using Smokey.Framework;
using Smokey.Framework.Support;
using Smokey.Internal.Rules;

#if OLD
namespace Smokey.Tests
{
	[TestFixture]
	public class MalformedFormatStringTest : MethodTest
	{	
		#region Test classes
		private class GoodCases
		{
			public string InOrder(int x, int y)
			{
				return string.Format("x = {0}, y = {1}", x, y);
			}

			public string OuOfOrder(int x, int y)
			{
				return string.Format("x = {1,3:X2}, y = {0}", x, y);
			}		

			public string Repeated(int x, int y)
			{
				return string.Format("x = {0}, y = {1}, giggles = {0}", x, y);
			}		

			public void Params(int x, int y)
			{				
				Log.InfoLine(true, "{0} {1} {2} {3} {4}", x, y, x, y, y);
			}
			
			private static string Localize(string s)
			{
				return s;
			}

			public string Localized(int x)
			{
				return string.Format(Localize("x = {0}"), x);
			}		
		}
				
		private class BadCases
		{
			public string TooManyFormats(int x, int y)
			{
				return string.Format("x = {0}, y = {1}{2}", x, y);
			}

			public string TooFewFormats(int x, int y)
			{
				return string.Format("x = {0}, y = ", x, y);
			}

			public string BogusFormat(int x, int y)
			{
				return string.Format("x = {0}, y = {1)", x, y);
			}

			public string SkippedFormat(int x, int y)
			{
				return string.Format("x = {1}, y = {2}", x, y);
			}

			public void BadLog(int x, int y)
			{
				Log.InfoLine(true, "x = {0}, y = ", x, y);
			}

			public void Params1(int x, int y)
			{				
				Log.InfoLine(true, "{0} {1} {2} {3} {4}", x, y, x, y, y, x);
			}

			public void Params2(int x, int y)
			{				
				Log.InfoLine(true, "{0} {1} {2} {3} {4}", x, y, x, y);
			}
		}
		#endregion
		
		// test code
		public MalformedFormatStringTest() : base(
			new string[]{"GoodCases.InOrder", "GoodCases.OuOfOrder", 
			"GoodCases.Repeated", "GoodCases.Params", "GoodCases.Localized"},
			
			new string[]{"BadCases.TooManyFormats", "BadCases.TooFewFormats", "BadCases.BogusFormat", 
			"BadCases.SkippedFormat", "BadCases.BadLog", "BadCases.Params1", "BadCases.Params2"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new MalformedFormatStringRule(cache, reporter);
		}
	} 
}
#endif
