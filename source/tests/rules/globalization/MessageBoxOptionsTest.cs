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
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Smokey.Framework.Support;
using Smokey.Internal.Rules;

namespace Smokey.Tests
{
	[TestFixture]
	public class MessageBoxOptionsTest : MethodTest
	{	
		#region Test classes
		private class Cases
		{
			public void GoodCase1(Control owner, string text, string caption)
			{
				MessageBoxOptions options = 0;
				if (CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft)
					options |= MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign;
				
				MessageBox.Show(
					owner, 
					text, 
					caption,
					MessageBoxButtons.OK, 
					MessageBoxIcon.Information, 
					MessageBoxDefaultButton.Button1, 
					options);
			}
					
			public void GoodCase2(string text, string caption)
			{
				Console.WriteLine(text + caption);
			}
					
			public void BadCase1(string text, string caption)
			{				
				MessageBox.Show(text, caption);
			}
		}		
		#endregion
		
		// test code
		public MessageBoxOptionsTest() : base(
			new string[]{"Cases.GoodCase1", "Cases.GoodCase2"},
			new string[]{"Cases.BadCase1"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new MessageBoxOptionsRule(cache, reporter);
		}
	} 
}
