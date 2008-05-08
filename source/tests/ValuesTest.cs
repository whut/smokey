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
using Mono.Cecil.Cil;
using NUnit.Framework;
using System;
using System.Reflection;
using Smokey.Framework;
using Smokey.Framework.Instructions;
using Smokey.Framework.Support.Advanced;
using Smokey.Framework.Support.Advanced.Values;

namespace Smokey.Tests
{	
	[TestFixture]
	public class ValuesTest : BaseTest
	{	
		#region Test classes
		public class Cases
		{
			public void Linear()
			{
				int v0 = 0;
				Console.WriteLine("first: {0}", v0);

				v0 = 3;
				Console.WriteLine("second: {0}", v0);
			}

			public void MultipleLocals(int a1)
			{
				int v0 = 0;
				int v1 = 1;
				Console.WriteLine("first: {0}", v0);

				if (a1 > 0)
				{
					v0 = 2;
					Console.WriteLine("second: {0}", v0);
				}
				
				v0 = v1;
				Console.WriteLine("last: {0}", v0);
			}

			public void Args(int a1, int a2)
			{
				int v1 = 2;
				Console.WriteLine("first: {0}", a2);
				a1 = 1;

				if (a1 > 0)
				{
					a2 = v1;
					Console.WriteLine("second: {0}", a2);
				}
				
				Console.WriteLine("last: {0}", a2);
			}

			public void This(int a1, object a2)
			{
				int v1 = 2;
				Console.WriteLine("first: {0}", a2);
				a1 = 1;

				if (a1 > 0)
				{
					a2 = v1;
					Console.WriteLine("second: {0}", a2);
				}
				
				a2 = this;
				Console.WriteLine("last: {0}", a2);
			}

			public void Catch(int a1, int a2)
			{
				a1 = 1;
				try
				{
					Console.WriteLine("first: {0}", a2);
				}
				catch (Exception)
				{
					a2 = a1;
					Console.WriteLine("catch: {0}", a2);
				}
			}

			public void Catch2(int a1, object a2)
			{
				a1 = 2;
				try
				{
					Console.WriteLine("first: {0}", a2);
				}
				catch (Exception e)
				{
					a2 = e;
					Console.WriteLine("catch: {0}", a2);
				}
			}

			public void NestedCatch(int a1, object a2)
			{
				a1 = 1;
				try
				{
					Console.WriteLine("first: {0}", a2);
					a1 = 2;
					try
					{
						Console.WriteLine("second: {0}", a2);
					}
					catch (Exception)
					{
						Console.WriteLine("catch1: {0}", a2);
					}
					finally
					{
						a1 = 3;
						Console.WriteLine("finally: {0}", a2);
					}
					Console.WriteLine("third: {0}", a2);
				}
				catch (Exception)
				{
					Console.WriteLine("catch2: {0}", a2);
				}
			}

			public void LoopCatch(int a1, object a2)
			{
				while (--a1 > 0)
				{
					a1 = 2;
					try
					{
						Console.WriteLine("first: {0}", a2);
					}
					catch (Exception e)
					{
						a2 = e;
						Console.WriteLine("catch: {0}", a2);
					}
				}
			}

			public void Ternary(object a1, object a2)
			{
				string v0 = a1 == null ? "first" : "second";
				Console.WriteLine(v0);

				string v1 = null == a2 ? "third" : "fourth";
				Console.WriteLine(v1);
			}

			public void NullLocal(string a1)
			{
				string v0 = a1;
				
				if (v0 == null)
					Console.WriteLine("first: {0}", v0);
				else
					Console.WriteLine("second: {0}", v0);
				
				string v1 = a1;
				if (null != v1)
					Console.WriteLine("third: {0}", v1);
				else
					Console.WriteLine("fourth: {0}", v1);
			}
			
			private void RefArg(ref int a1)
			{
				a1 = 5;
			}

			public void LocalAddress()
			{
				int v0 = 1;
				RefArg(ref v0);
				
				Console.WriteLine("first: {0}", v0);
			}

			public void ArgAddress(int a1)
			{
				a1 = 1;
				RefArg(ref a1);
				
				Console.WriteLine("first: {0}", a1);
			}

			public void StackIndex(int a1)
			{
				Console.WriteLine("first: {0}", a1);

				int v0 = a1.GetHashCode();				
				Console.WriteLine("blah: {0}", a1, "second", v0);
			}
		}
		#endregion
				
		public ValuesTest() 
		{
		}
		
		private int GetIndex(string text)
		{
			foreach (LoadString load in m_instructions.Match<LoadString>())
			{
				if (load.Value.Contains(text))
					return load.Index;
			}
			
			DBC.Fail("Couldn't find {0}", text);
			return -1;
		}
								
		private long? GetArg(int index, int nth)
		{
			long?[] args = m_tracker.State(index).Arguments;
			long? arg = args[nth];
			return arg;
		}
								
		private long? GetLocal(int index, int nth)
		{
			long?[] locals = m_tracker.State(index).Locals;
			long? local = locals[nth];
			return local;
		}
		
		private void DoInit(string methodName)
		{
			string loc = Assembly.GetExecutingAssembly().Location;
			AssemblyDefinition assembly = AssemblyFactory.GetAssembly(loc);
			
			string fullName = string.Format("{0}/{1}", GetType().FullName, "Cases");
			TypeDefinition type = assembly.MainModule.Types[fullName];

			MethodDefinition method = type.Methods.GetMethod(methodName)[0];

			m_instructions = new TypedInstructionCollection(new SymbolTable(), method);	
			m_tracker = new Tracker(m_instructions);
			m_tracker.Analyze();
		}
								
		[Test]
		public void Linear()	
		{
			DoInit("Linear");
			
			int index = GetIndex("first");
			Assert.AreEqual(0, GetLocal(index, 0));
			
			index = GetIndex("second");
			Assert.AreEqual(3, GetLocal(index, 0));
		}
		
		[Test]
		public void MultipleLocals()	
		{
			DoInit("MultipleLocals");
			
			int index = GetIndex("first");
			Assert.AreEqual(0, GetLocal(index, 0));
			Assert.AreEqual(1, GetLocal(index, 1));
			
			index = GetIndex("second");
			Assert.AreEqual(2, GetLocal(index, 0));
			Assert.AreEqual(1, GetLocal(index, 1));
			
			index = GetIndex("last");
			Assert.AreEqual(1, GetLocal(index, 0));
			Assert.AreEqual(1, GetLocal(index, 1));
		}
		
		[Test]
		public void Args()	
		{
			DoInit("Args");
			
			int index = GetIndex("first");
			Assert.IsFalse(GetArg(index, 1).HasValue);
			Assert.IsFalse(GetArg(index, 2).HasValue);
			
			index = GetIndex("second");
			Assert.AreEqual(1, GetArg(index, 1));
			Assert.AreEqual(2, GetArg(index, 2));
			
			index = GetIndex("last");
			Assert.AreEqual(1, GetArg(index, 1));
			Assert.IsFalse(GetArg(index, 2).HasValue);
		}
		
		[Test]
		public void This()	
		{
			DoInit("This");
			
			int index = GetIndex("first");
			Assert.IsFalse(GetArg(index, 1).HasValue);
			Assert.IsFalse(GetArg(index, 2).HasValue);
			
			index = GetIndex("second");
			Assert.AreEqual(1, GetArg(index, 1));
			Assert.AreEqual(1, GetArg(index, 2));	// boxed so value is 1
			
			index = GetIndex("last");
			Assert.AreEqual(1, GetArg(index, 1));
			Assert.AreEqual(1, GetArg(index, 2));
		}
		
		[Test]
		public void Catch()	
		{
			DoInit("Catch");
			
			int index = GetIndex("first");
			Assert.AreEqual(1, GetArg(index, 1));
			Assert.IsFalse(GetArg(index, 2).HasValue);
			
			index = GetIndex("catch");
			Assert.AreEqual(1, GetArg(index, 1));
			Assert.AreEqual(1, GetArg(index, 2));
		}
		
		[Test]
		public void Catch2()	
		{
			DoInit("Catch2");
			
			int index = GetIndex("first");
			Assert.AreEqual(2, GetArg(index, 1));
			Assert.IsFalse(GetArg(index, 2).HasValue);
			
			index = GetIndex("catch");
			Assert.AreEqual(2, GetArg(index, 1));
			Assert.AreEqual(1, GetArg(index, 2));
		}
		
		[Test]
		public void NestedCatch()	
		{
			DoInit("NestedCatch");
			
			int index = GetIndex("first");
			Assert.AreEqual(1, GetArg(index, 1));
			
			index = GetIndex("second");
			Assert.AreEqual(2, GetArg(index, 1));
			
			index = GetIndex("catch1");
			Assert.AreEqual(2, GetArg(index, 1));
			
			index = GetIndex("finally");
			Assert.AreEqual(3, GetArg(index, 1));
			
			index = GetIndex("third");
			Assert.AreEqual(3, GetArg(index, 1));
			
			index = GetIndex("catch2");
			Assert.AreEqual(1, GetArg(index, 1));
		}
		
		[Test]
		public void LoopCatch()	
		{
			DoInit("LoopCatch");
			
			int index = GetIndex("first");
			Assert.AreEqual(2, GetArg(index, 1));
			Assert.IsFalse(GetArg(index, 2).HasValue);
			
			index = GetIndex("catch");
			Assert.AreEqual(2, GetArg(index, 1));
			Assert.AreEqual(1, GetArg(index, 2));
		}
		
		[Test]
		public void Ternary()	
		{
			DoInit("Ternary");
			
			int index = GetIndex("first");
			Assert.AreEqual(0, GetArg(index, 1));
			
			index = GetIndex("second");
			Assert.AreEqual(1, GetArg(index, 1));
			
			index = GetIndex("third");
			Assert.AreEqual(0, GetArg(index, 2));
			
			index = GetIndex("fourth");
			Assert.AreEqual(1, GetArg(index, 2));
		}
		
		[Test]
		public void NullLocal()	
		{
			DoInit("NullLocal");
			
			int index = GetIndex("first");
			Assert.AreEqual(0, GetLocal(index, 0));
			
			index = GetIndex("second");
			Assert.AreEqual(1, GetLocal(index, 0));
			
			index = GetIndex("third");
			Assert.AreEqual(1, GetLocal(index, 1));
			
			index = GetIndex("fourth");
			Assert.AreEqual(0, GetLocal(index, 1));
		}
		
		[Test]
		public void LocalAddress()	
		{
			DoInit("LocalAddress");
			
			int index = GetIndex("first");
			Assert.IsFalse(GetLocal(index, 0).HasValue);
		}
		
		[Test]
		public void ArgAddress()	
		{
			DoInit("ArgAddress");
			
			int index = GetIndex("first");
			Assert.IsFalse(GetArg(index, 1).HasValue);
		}
		
		[Test]
		public void ZStackIndex()	
		{
			DoInit("StackIndex");
			
			int index = GetIndex("second");
			
			int i = m_tracker.GetStackIndex(index, 0);
			Code code = m_instructions[i].Untyped.OpCode.Code;
			Assert.AreEqual(Code.Box, code);
			
			i = m_tracker.GetStackIndex(index, 1);
			code = m_instructions[i].Untyped.OpCode.Code;
			Assert.AreEqual(Code.Ldstr, code);
			
			LoadString load = m_instructions[i] as LoadString;
			Assert.IsTrue(load.Value.Contains("blah"));
		}
		
		private TypedInstructionCollection m_instructions;
		private Tracker m_tracker;
	} 
}
