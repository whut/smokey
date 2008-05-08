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
	public class AvoidEmptyInterfacesTest : TypeTest
	{	
		public delegate void Handler(object o, EventArgs e);

		// test classes
		public interface IGood1
		{
			void Stuff();
		}			
		
		public interface IGood2
		{
			int Count {get;}
		}			
		
		public interface IGood3
		{
			event Handler Event;
		}			
		
		public interface IGood4 : IGood1, IGood2
		{
		}

		public class Good5
		{
		}			
		
		public interface IBad1
		{
		}			

		public interface IBad2 : IGood2
		{
		}			
		
		// test code
		public AvoidEmptyInterfacesTest() : base(
			new string[]{"IGood1", "IGood2", "IGood3", "IGood4", "Good5"},
			new string[]{"IBad1", "IBad2"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new AvoidEmptyInterfacesRule(cache, reporter);
		}
	} 
}