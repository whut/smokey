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

using NUnit.Framework;
using System;
using Smokey.Internal;

namespace Smokey.Tests
{
	[TestFixture]
	public class GetOptionsTest : BaseTest
	{		
		[SetUp]
		public void Init()
		{
			m_options = new GetOptions();
		}
		
		[Test]
		public void Has()
		{
			m_options.Add("-value", "desc");
			
			m_options.Parse();
			Assert.IsFalse(m_options.Has("-value"));
			
			m_options.Parse("-value");
			Assert.IsTrue(m_options.Has("-value"));
		}
		
		[Test]
		public void Args()
		{
			m_options.Add("-value=", "desc");	
						
			m_options.Parse("-value=100");
			Assert.AreEqual("100", m_options.Value("-value"));
						
			m_options.Parse("-value:100");
			Assert.AreEqual("100", m_options.Value("-value"));
						
			m_options.Parse("--val:200");
			Assert.AreEqual("200", m_options.Value("-value"));
		}
				
		[Test]
		public void DefaultArg()
		{
			m_options.Add("-value=", "desc");	
			m_options.Add("-time=100", "desc");	
						
			m_options.Parse("-value=10");
			Assert.AreEqual("10", m_options.Value("-value"));
			Assert.AreEqual("100", m_options.Value("-time"));
		}
				
		[Test]
		public void Abreviation1()
		{
			m_options.Add("-panda", "desc");
			m_options.Add("-path=", "desc");
			
			m_options.Parse("-pat=foo", "-pand");
			Assert.IsTrue(m_options.Has("-panda"));
			Assert.IsTrue(m_options.Has("-path"));
			Assert.AreEqual("foo", m_options.Value("-path"));
		}
		
		[Test]
		public void Abreviation2()
		{
			m_options.Add("-panda", "desc");
			m_options.Add("-pancake", "desc");
			m_options.Add("-pandomonium", "desc");
			
			m_options.Parse("-pando");
			Assert.IsTrue(m_options.Has("-pandomonium"));
			
			m_options.Parse("-pancak");
			Assert.IsTrue(m_options.Has("-pancake"));
		}
		
		[Test]
		public void Aliaii()
		{
			m_options.Add("-panda", "-bear", "-animal", "desc");
			m_options.Add("-pancake", "-k", "desc");
			m_options.Add("-pandomonium", "desc");
			
			m_options.Parse("-animal", "-k");
			Assert.IsTrue(m_options.Has("-panda"));
			Assert.IsTrue(m_options.Has("-pancake"));
		}
		
		[Test]
		public void Repeated()
		{
			m_options.Add("-path=", "desc");
			m_options.Add("-xml", "desc");
			m_options.Add("-html", "desc");
			m_options.Add("-data=", "desc");
			
			m_options.Parse("-xml", "-path=alpha", "-html", "-path=beta");
			Assert.IsTrue(m_options.Has("-path"));

			Assert.AreEqual(2, m_options.Values("-path").Length);
			Assert.AreEqual("alpha", m_options.Values("-path")[0]);
			Assert.AreEqual("beta", m_options.Values("-path")[1]);

			Assert.AreEqual(0, m_options.Values("-data").Length);
		}
		
		[Test]
		public void Operands()
		{
			m_options.Add("-path=", "desc");
			m_options.Add("-xml", "-k", "desc");
			m_options.Add("-html", "desc");
			
			m_options.Parse("-xml", "-path=foo", "alpha", "beta", "-html");
			Assert.IsTrue(m_options.Has("-xml"));
			Assert.IsTrue(m_options.Has("-path"));
			Assert.IsTrue(m_options.Has("-html"));

			Assert.AreEqual(2, m_options.Operands.Length);
			Assert.AreEqual("alpha", m_options.Operands[0]);
			Assert.AreEqual("beta", m_options.Operands[1]);
		}
		
		[Test]
		public void DashDash()
		{
			m_options.Add("-path=", "desc");
			m_options.Add("-xml", "-k", "desc");
			m_options.Add("-html", "desc");
			
			m_options.Parse("-xml", "-path=foo", "--", "alpha", "beta", "-html");
			Assert.IsTrue(m_options.Has("-xml"));
			Assert.IsTrue(m_options.Has("-path"));
			Assert.IsFalse(m_options.Has("-html"));

			Assert.AreEqual(3, m_options.Operands.Length);
			Assert.AreEqual("alpha", m_options.Operands[0]);
			Assert.AreEqual("beta", m_options.Operands[1]);
			Assert.AreEqual("-html", m_options.Operands[2]);
		}
		
		[Test]
		[ExpectedException(typeof(MalformedCommandLineException))]
		public void BadAbreviation()
		{
			m_options.Add("-panda", "desc");
			m_options.Add("-pancake", "desc");
			m_options.Add("-pandomonium", "desc");
			
			m_options.Parse("-pand");
		}
		
		[Test]
		[ExpectedException(typeof(MalformedCommandLineException))]
		public void BadOption()
		{
			m_options.Add("-value", "desc");	
						
			m_options.Parse("-valee");
		}
		
		[Test]
		[ExpectedException(typeof(MalformedCommandLineException))]
		public void BadArg1()
		{
			m_options.Add("-value", "desc");	
						
			m_options.Parse("-value=100");
		}
		
		[Test]
		[ExpectedException(typeof(MalformedCommandLineException))]
		public void BadArg2()
		{
			m_options.Add("-value=", "desc");	
						
			m_options.Parse("-value");
		}
		
		[Test]
		[ExpectedException(typeof(MalformedCommandLineException))]
		public void AmbiguousOption1()
		{
			m_options.Add("-panda", "desc");
			m_options.Add("-pancake", "desc");
			m_options.Add("-pandomonium", "desc");
			m_options.Add("-pan", "desc");

			m_options.Parse("-panda");
		}
		
		[Test]
		[ExpectedException(typeof(MalformedCommandLineException))]
		public void AmbiguousOption2()
		{
			m_options.Add("-panda", "-bare", "desc");
			m_options.Add("-pancake", "-bare-naked", "desc");

			m_options.Parse("-panda");
		}
		
		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void BadHas()
		{
			m_options.Add("-panda", "-bare", "desc");
			m_options.Add("-pancake", "desc");

			m_options.Parse("-panda");
			m_options.Has("-xxx");
		}
		
		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void BadValue()
		{
			m_options.Add("-panda", "-bare", "desc");
			m_options.Add("-pancake", "desc");

			m_options.Parse("-panda");
			m_options.Value("-xxx");
		}
		
		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void BadValues()
		{
			m_options.Add("-panda", "-bare", "desc");
			m_options.Add("-pancake", "desc");

			m_options.Parse("-panda");
			m_options.Values("-xxx");
		}
		
		private GetOptions m_options;
	} 
}
