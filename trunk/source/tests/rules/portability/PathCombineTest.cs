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

#if OLD
namespace Smokey.Tests
{
	[TestFixture]
	public class PathCombineTest : MethodTest
	{	
		#region Test cases
		private class Cases
		{
			public void Good(string path)
			{
				Console.WriteLine(path + "hello world");
				Console.WriteLine("/hello/world");
				Console.WriteLine(@"C:\Program Files");
				Console.WriteLine(@"C:\Windows\Foo");
				Console.WriteLine(path + "var tmp");
				Console.WriteLine(path + "</head>");
				Console.WriteLine(path + "file://foo");
			}

			public string Bad1(string path)
			{
				return path + "/foo";
			}

			public string Bad2(string path)
			{
				return "foo\\" + path;
			}

			public void Bad3(string path)
			{
				Console.WriteLine(path + "/" + path);
			}

			public void Bad4(string path)
			{
				path += "/foo";
				Console.WriteLine(path);
			}

			public void Bad5(object o)
			{
				string path = "/foo/" + o + "/bar";
				Console.WriteLine(path);
			}
		}
		#endregion
		
		// test code
		public PathCombineTest() : base( 
			new string[]{"Cases.Good"},
			new string[]{"Cases.Bad1", "Cases.Bad2", "Cases.Bad3", "Cases.Bad4", "Cases.Bad5"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new PathCombineRule(cache, reporter);
		}
	} 
}
#endif
