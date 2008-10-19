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
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Smokey.Framework.Support;
using Smokey.Internal.Rules;

#if OLD
namespace Smokey.Tests
{
	[TestFixture]
	public class TemplateMethodTest : MethodTest
	{	
		public class Cases
		{	
			public virtual void Good1()
			{
				WriteHeader();
				WriteFooter();
				WriteWatermark();
				WriteContents();
			}
			
			public virtual void Good2()
			{
				Console.WriteLine("prolog1");
				Console.WriteLine("prolog2");
				Console.WriteLine("prolog3");
				Console.WriteLine("prolog4");

				WriteHeader();
				WriteFooter();
				WriteWatermark();
				WriteContents();

				Console.WriteLine("epilog1");
				Console.WriteLine("epilog2");
				Console.WriteLine("epilog3");
				Console.WriteLine("epilog4");
			}
			
			public virtual bool Good3(string begin)
			{		
				bool needs = false;
				
				if (begin[0] != 'x' && begin.Length == 0)
					if (Equals(begin))
						needs = true;
				
				return needs;
			}

			public virtual void Bad1()
			{
				Console.WriteLine("header1");
				Console.WriteLine("header2");
				Console.WriteLine("header3");
				Console.WriteLine("header4");
				Console.WriteLine("header5");

				Console.WriteLine("watermark1");
				Console.WriteLine("watermark2");
				Console.WriteLine("watermark3");
				Console.WriteLine("watermark4");
				Console.WriteLine("watermark5");

				Console.WriteLine("contents1");
				Console.WriteLine("contents2");
				Console.WriteLine("contents3");
				Console.WriteLine("contents4");
				Console.WriteLine("contents5");

				Console.WriteLine("header1");
				Console.WriteLine("header2");
				Console.WriteLine("header3");
				Console.WriteLine("header4");
				Console.WriteLine("header5");

				Console.WriteLine("footer1");
				Console.WriteLine("footer2");
				Console.WriteLine("footer3");
				Console.WriteLine("footer4");
				Console.WriteLine("footer5");

				Console.WriteLine("watermark1");
				Console.WriteLine("watermark2");
				Console.WriteLine("watermark3");
				Console.WriteLine("watermark4");
				Console.WriteLine("watermark5");

				Console.WriteLine("contents1");
				Console.WriteLine("contents2");
				Console.WriteLine("contents3");
				Console.WriteLine("contents4");
				Console.WriteLine("contents5");

				Console.WriteLine("watermark1");
				Console.WriteLine("watermark2");
				Console.WriteLine("watermark3");
				Console.WriteLine("watermark4");
				Console.WriteLine("watermark5");

				Console.WriteLine("contents1");
				Console.WriteLine("contents2");
				Console.WriteLine("contents3");
				Console.WriteLine("contents4");
				Console.WriteLine("contents5");

				Console.WriteLine("header1");
				Console.WriteLine("header2");
				Console.WriteLine("header3");
				Console.WriteLine("header4");
				Console.WriteLine("header5");

				Console.WriteLine("footer1");
				Console.WriteLine("footer2");
				Console.WriteLine("footer3");
				Console.WriteLine("footer4");
				Console.WriteLine("footer5");

				Console.WriteLine("watermark1");
				Console.WriteLine("watermark2");
				Console.WriteLine("watermark3");
				Console.WriteLine("watermark4");
				Console.WriteLine("watermark5");

				Console.WriteLine("header1");
				Console.WriteLine("header2");
				Console.WriteLine("header3");
				Console.WriteLine("header4");
				Console.WriteLine("header5");
			}
			
			public virtual void Bad2()
			{
				Console.WriteLine("header1");
				Console.WriteLine("header2");
				Console.WriteLine("header3");
				Console.WriteLine("header4");
				Console.WriteLine("header5");

				Console.WriteLine("footer1");
				Console.WriteLine("footer2");
				Console.WriteLine("footer3");
				Console.WriteLine("footer4");
				Console.WriteLine("footer5");

				Console.WriteLine("watermark1");
				Console.WriteLine("watermark2");
				Console.WriteLine("watermark3");
				Console.WriteLine("watermark4");
				Console.WriteLine("watermark5");

				Console.WriteLine("contents1");
				Console.WriteLine("contents2");
				Console.WriteLine("contents3");
				Console.WriteLine("contents4");
				Console.WriteLine("contents5");

				Console.WriteLine("footer1");
				Console.WriteLine("footer2");
				Console.WriteLine("footer3");
				Console.WriteLine("footer4");
				Console.WriteLine("footer5");

				Console.WriteLine("footer2");
				Console.WriteLine("footer3");
				Console.WriteLine("footer4");
				Console.WriteLine("footer5");

				Console.WriteLine("watermark1");
				Console.WriteLine("watermark2");
				Console.WriteLine("watermark3");
				Console.WriteLine("watermark4");
				Console.WriteLine("watermark5");

				Console.WriteLine("contents1");
				Console.WriteLine("contents2");
				Console.WriteLine("contents3");
				Console.WriteLine("contents4");
				Console.WriteLine("contents5");

				Console.WriteLine("watermark1");
				Console.WriteLine("watermark2");
				Console.WriteLine("watermark3");
				Console.WriteLine("watermark4");
				Console.WriteLine("watermark5");

				Console.WriteLine("watermark1");
				Console.WriteLine("watermark2");
				Console.WriteLine("watermark3");
				Console.WriteLine("watermark4");
				Console.WriteLine("watermark5");

				Console.WriteLine("contents1");
				Console.WriteLine("contents2");
				Console.WriteLine("contents3");
				Console.WriteLine("contents4");
				Console.WriteLine("contents5");

				Console.WriteLine("footer1");
				Console.WriteLine("footer2");
				Console.WriteLine("footer3");
				Console.WriteLine("footer4");
				Console.WriteLine("footer5");

				Console.WriteLine("footer2");
				Console.WriteLine("footer3");
				Console.WriteLine("footer4");
				Console.WriteLine("footer5");

				Console.WriteLine("watermark1");
				Console.WriteLine("watermark2");
				Console.WriteLine("watermark3");
				Console.WriteLine("watermark4");
				Console.WriteLine("watermark5");

				Console.WriteLine("contents1");
				Console.WriteLine("contents2");
				Console.WriteLine("contents3");
				Console.WriteLine("contents4");
				Console.WriteLine("contents5");

				Console.WriteLine("contents1");
				Console.WriteLine("contents2");
				Console.WriteLine("contents3");
				Console.WriteLine("contents4");
				Console.WriteLine("contents5");

				WriteContents();
			}
			
			public virtual void Bad3()
			{
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
				WriteFixed();
			}
			
			public virtual void WriteHeader()
			{
				Console.WriteLine("header1");
				Console.WriteLine("header2");
				Console.WriteLine("header3");
				Console.WriteLine("header4");
				Console.WriteLine("header5");
			}
			
			public virtual void WriteFooter()
			{
				Console.WriteLine("footer1");
				Console.WriteLine("footer2");
				Console.WriteLine("footer3");
				Console.WriteLine("footer4");
				Console.WriteLine("footer5");
			}
			
			public virtual void WriteWatermark()
			{
				Console.WriteLine("watermark1");
				Console.WriteLine("watermark2");
				Console.WriteLine("watermark3");
				Console.WriteLine("watermark4");
				Console.WriteLine("watermark5");
			}
			
			public virtual void WriteContents()
			{
				Console.WriteLine("contents1");
				Console.WriteLine("contents2");
				Console.WriteLine("contents3");
				Console.WriteLine("contents4");
				Console.WriteLine("contents5");
			}
			
			public void WriteFixed()
			{
				Console.WriteLine("fixed1");
				Console.WriteLine("fixed2");
				Console.WriteLine("fixed3");
				Console.WriteLine("fixed4");
				Console.WriteLine("fixed5");
			}
		}			

		public class Derived : Cases
		{	
			public override void Good1()
			{
				Console.WriteLine("header1");
				Console.WriteLine("header2");
				Console.WriteLine("header3");
				Console.WriteLine("header4");
				Console.WriteLine("header5");

				Console.WriteLine("footer1");
				Console.WriteLine("footer2");
				Console.WriteLine("footer3");
				Console.WriteLine("footer4");
				Console.WriteLine("footer5");

				Console.WriteLine("watermark1");
				Console.WriteLine("watermark2");
				Console.WriteLine("watermark3");
				Console.WriteLine("watermark4");
				Console.WriteLine("watermark5");

				Console.WriteLine("contents1");
				Console.WriteLine("contents2");
				Console.WriteLine("contents3");
				Console.WriteLine("contents4");
				Console.WriteLine("contents5");

				Console.WriteLine("header1");
				Console.WriteLine("header2");
				Console.WriteLine("header3");
				Console.WriteLine("header4");
				Console.WriteLine("header5");
			}
		}
		
		// test code
		public TemplateMethodTest() : base(
			new string[]{"Cases.Good1", "Cases.Good2", "Cases.Good3", "Derived.Good1"},
			new string[]{"Cases.Bad1", "Cases.Bad2", "Cases.Bad3"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new TemplateMethodRule(cache, reporter);
		}
	} 
}
#endif
