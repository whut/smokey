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
	public class ImplicitCastTest : TypeTest
	{	
		// test classes
		internal struct Point
		{
			public Point(int x, int y)
			{
				m_x = x;
				m_y = y;
			}
			
			// cast methods elided
			
			public int X {get {return m_x;}}
			public int Y {get {return m_y;}}
			
			private int m_x;
			private int m_y;
		}			
					
		internal struct Good1
		{
			public Good1(int width, int height)
			{
				m_width = width;
				m_height = height;
			}
			
			public static explicit operator Point(Good1 size)
			{
				return new Point(size.m_width, size.m_height);
			}
						
			public int Width {get {return m_width;}}
			public int Height {get {return m_height;}}
			
			private int m_width;
			private int m_height;
		}			

		internal struct Bad1
		{
			public Bad1(int width, int height)
			{
				m_width = width;
				m_height = height;
			}
			
			public static implicit operator Point(Bad1 size)
			{
				return new Point(size.m_width, size.m_height);
			}
									
			public int Width {get {return m_width;}}
			public int Height {get {return m_height;}}
			
			private int m_width;
			private int m_height;
		}			

		// test code
		public ImplicitCastTest() : base(
			new string[]{"Good1"},
			new string[]{"Bad1"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new ImplicitCastRule(cache, reporter);
		}
	} 
}