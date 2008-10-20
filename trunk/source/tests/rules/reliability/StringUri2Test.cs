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
	public class StringUri2Test : TypeTest
	{	
		#region Test classes
		private class Good1			// type is Uri
		{
			public Uri Uri
			{
				get {return m_value;}
				set {m_value = value;}
			}
			
			private Uri m_value;
		}

		private class Good2			// name isnt uri, url, etc
		{
			public string Urx
			{
				get {return m_value;}
				set {m_value = value;}
			}
			
			private string m_value;
		}

		private class Bad1
		{
			public string Uri
			{
				get {return m_value;}
				set {m_value = value;}
			}
			
			private string m_value;
		}

		private class Bad2
		{
			public string MyUrn
			{
				get {return m_value;}
				set {m_value = value;}
			}
			
			private string m_value;
		}

		private class Bad3
		{
			public string url
			{
				get {return m_value;}
				set {m_value = value;}
			}
			
			private string m_value;
		}
		#endregion
		
		// test code
		public StringUri2Test() : base(
			new string[]{"Good1", "Good2"},
			new string[]{"Bad1", "Bad2", "Bad3"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new StringUri2Rule(cache, reporter);
		}
	} 
}

