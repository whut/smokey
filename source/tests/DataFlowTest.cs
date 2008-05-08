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
using Smokey.Framework;
using Smokey.Framework.Support.Advanced;
using Smokey.Framework.Instructions;

namespace Smokey.Tests
{	
	[TestFixture]
	public class DataFlowTest : BaseTest
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

			public void OneIf(int a1)
			{
				int v0 = 0;
				Console.WriteLine("first: {0}", v0);
				
				if (a1 > 0)
				{
					v0 = 2;
					Console.WriteLine("second: {0}", a1);
				}
				else
				{
					v0 = 3;
					Console.WriteLine("third: {0}", a1);
				}

				Console.WriteLine("last: {0}", a1);
			}

			public void TwoIfs(int a1)
			{
				int v0 = 0;
				Console.WriteLine("first: {0}", v0);
				
				if (a1 > 0)
				{
					v0 = 2;
					Console.WriteLine("second: {0}", a1);
				}
				else
				{
					v0 = 3;
					Console.WriteLine("third: {0}", a1);
				}

				Console.WriteLine("fourth: {0}", a1);

				if (a1 > 100)
				{
					v0 = 5;
					Console.WriteLine("fifth: {0}", a1);
				}
			
				Console.WriteLine("last: {0}", a1);
			}

			public void Switch(int a1)
			{
				int v0 = 0;

				switch (a1)
				{
					case 1:
						v0 = 1;
						Console.WriteLine("first: {0}", v0);
						break;
						
					case 2:
						v0 = 2;
						Console.WriteLine("second: {0}", v0);
						break;
						
					case 3:
						v0 = 3;
						Console.WriteLine("third: {0}", v0);
						break;
						
					default:
						v0 = 10;
						Console.WriteLine("default: {0}", v0);
						break;
				}

				Console.WriteLine("last: {0}", a1);
			}

			public void For(int a1)
			{
				int v0 = 0;
				
				for (int i = 0; i < a1; ++i)
				{
					Console.WriteLine("first: {0}", v0);

					if (i == 10)
					{
						v0 = 2;
						Console.WriteLine("second: {0}", v0);
					}
				}

				Console.WriteLine("last: {0}", a1);
			}

			public void DoWhile(int a1)
			{
				int v0 = 0;
				
				do
				{
					Console.WriteLine("first: {0}", v0);

					if (a1 == 10)
					{
						v0 = 2;
						Console.WriteLine("second: {0}", v0);
					}
				}
				while (--a1 > 100);

				Console.WriteLine("last: {0}", a1);
			}

			public void Catch1(int a1)
			{
				int v0 = 0;
				
				try
				{
					v0 = 2;
					Console.WriteLine("first: {0}", v0);
				}
				catch (Exception)
				{
					Console.WriteLine("catch: {0}", a1);
				}

				Console.WriteLine("last: {0}", a1);
			}

			public void Catch2(int a1)
			{
				int v0 = 0;
				
				try
				{
					v0 = 2;
					Console.WriteLine("first: {0}", v0);
				}
				catch (Exception)
				{
					Console.WriteLine("catch: {0}", a1);
					throw;
				}

				Console.WriteLine("last: {0}", a1);
			}

			public void Finally(int a1)
			{
				int v0 = 0;
				
				try
				{
					v0 = 2;
					Console.WriteLine("first: {0}", v0);
				}
				catch (Exception)
				{
					v0 = 3;
					Console.WriteLine("catch: {0}", a1);
					throw;
				}
				finally
				{
					v0 = 2;
					Console.WriteLine("finally: {0}", a1);
				}

				Console.WriteLine("last: {0}", a1);
			}
		}
		#endregion
				
		public DataFlowTest() 
		{
			string loc = Assembly.GetExecutingAssembly().Location;
			m_assembly = AssemblyFactory.GetAssembly(loc);
		}
		
		private int DoGetIndex(TypedInstructionCollection instructions, string text)
		{
			foreach (LoadString load in instructions.Match<LoadString>())
			{
				if (load.Value.Contains(text))
					return load.Index;
			}
			
			DBC.Fail("Couldn't find {0}", text);
			return -1;
		}
								
		private TypeDefinition DoGetType(string name)
		{			
			string fullName = string.Format("{0}/{1}", GetType().FullName, name);
			TypeDefinition type = m_assembly.MainModule.Types[fullName];
			
			return type;
		}
								
		[Test]
		public void Linear()	
		{
			TypeDefinition type = DoGetType("Cases");
			MethodDefinition method = type.Methods.GetMethod("Linear")[0];

			var instructions = new TypedInstructionCollection(new SymbolTable(), method);	
			var tracker = new V0.Tracker(instructions);
			
			int index = DoGetIndex(instructions, "first");
			Assert.AreEqual(0, tracker.State(index).Value);
			
			index = DoGetIndex(instructions, "second");
			Assert.AreEqual(3, tracker.State(index).Value);
		}
								
		[Test]
		public void OneIf()	
		{
			TypeDefinition type = DoGetType("Cases");
			MethodDefinition method = type.Methods.GetMethod("OneIf")[0];

			var instructions = new TypedInstructionCollection(new SymbolTable(), method);	
			var tracker = new V0.Tracker(instructions);
			
			int index = DoGetIndex(instructions, "first");
			Assert.AreEqual(0, tracker.State(index).Value);
			
			index = DoGetIndex(instructions, "second");
			Assert.AreEqual(2, tracker.State(index).Value);
			
			index = DoGetIndex(instructions, "third");
			Assert.AreEqual(3, tracker.State(index).Value);
			
			index = DoGetIndex(instructions, "last");
			Assert.IsFalse(tracker.State(index).HasValue);
		}
		
		[Test]
		public void TwoIfs()	
		{
			TypeDefinition type = DoGetType("Cases");
			MethodDefinition method = type.Methods.GetMethod("TwoIfs")[0];

			var instructions = new TypedInstructionCollection(new SymbolTable(), method);	
			var tracker = new V0.Tracker(instructions);
						
			int index = DoGetIndex(instructions, "second");
			Assert.AreEqual(2, tracker.State(index).Value);
			
			index = DoGetIndex(instructions, "third");
			Assert.AreEqual(3, tracker.State(index).Value);
			
			index = DoGetIndex(instructions, "fourth");
			Assert.IsFalse(tracker.State(index).HasValue);
			
			index = DoGetIndex(instructions, "fifth");
			Assert.AreEqual(5, tracker.State(index).Value);
			
			index = DoGetIndex(instructions, "last");
			Assert.IsFalse(tracker.State(index).HasValue);
		}
		
		[Test]
		public void Switch()	
		{
			TypeDefinition type = DoGetType("Cases");
			MethodDefinition method = type.Methods.GetMethod("Switch")[0];

			var instructions = new TypedInstructionCollection(new SymbolTable(), method);	
			var tracker = new V0.Tracker(instructions);
						
			int index = DoGetIndex(instructions, "second");
			Assert.AreEqual(2, tracker.State(index).Value);
			
			index = DoGetIndex(instructions, "third");
			Assert.AreEqual(3, tracker.State(index).Value);
			
			index = DoGetIndex(instructions, "default");
			Assert.AreEqual(10, tracker.State(index).Value);
			
			index = DoGetIndex(instructions, "last");
			Assert.IsFalse(tracker.State(index).HasValue);
		}
		
		[Test]
		public void For()	
		{
			TypeDefinition type = DoGetType("Cases");
			MethodDefinition method = type.Methods.GetMethod("For")[0];

			var instructions = new TypedInstructionCollection(new SymbolTable(), method);	
			var tracker = new V0.Tracker(instructions);
						
			int index = DoGetIndex(instructions, "first");
			Assert.IsFalse(tracker.State(index).HasValue);
			
			index = DoGetIndex(instructions, "second");
			Assert.AreEqual(2, tracker.State(index).Value);
						
			index = DoGetIndex(instructions, "last");
			Assert.IsFalse(tracker.State(index).HasValue);
		}
		
		[Test]
		public void DoWhile()	
		{
			TypeDefinition type = DoGetType("Cases");
			MethodDefinition method = type.Methods.GetMethod("DoWhile")[0];

			var instructions = new TypedInstructionCollection(new SymbolTable(), method);	
			var tracker = new V0.Tracker(instructions);
						
			int index = DoGetIndex(instructions, "first");
			Assert.IsFalse(tracker.State(index).HasValue);
			
			index = DoGetIndex(instructions, "second");
			Assert.AreEqual(2, tracker.State(index).Value);
						
			index = DoGetIndex(instructions, "last");
			Assert.IsFalse(tracker.State(index).HasValue);
		}
		
		[Test]
		public void Catch1()	
		{
			TypeDefinition type = DoGetType("Cases");
			MethodDefinition method = type.Methods.GetMethod("Catch1")[0];

			var instructions = new TypedInstructionCollection(new SymbolTable(), method);	
			var tracker = new V0.Tracker(instructions);
						
			int index = DoGetIndex(instructions, "first");
			Assert.AreEqual(2, tracker.State(index).Value);
			
			index = DoGetIndex(instructions, "catch");
			Assert.AreEqual(0, tracker.State(index).Value);
						
			index = DoGetIndex(instructions, "last");
			Assert.IsFalse(tracker.State(index).HasValue);
		}
		
		[Test]
		public void Catch2()	
		{
			TypeDefinition type = DoGetType("Cases");
			MethodDefinition method = type.Methods.GetMethod("Catch2")[0];

			var instructions = new TypedInstructionCollection(new SymbolTable(), method);	
			var tracker = new V0.Tracker(instructions);
						
			int index = DoGetIndex(instructions, "first");
			Assert.AreEqual(2, tracker.State(index).Value);
			
			index = DoGetIndex(instructions, "catch");
			Assert.AreEqual(0, tracker.State(index).Value);
						
			index = DoGetIndex(instructions, "last");
			Assert.AreEqual(2, tracker.State(index).Value);
		}
		
		[Test]
		public void ZFinally()	
		{
			TypeDefinition type = DoGetType("Cases");
			MethodDefinition method = type.Methods.GetMethod("Finally")[0];

			var instructions = new TypedInstructionCollection(new SymbolTable(), method);	
			var tracker = new V0.Tracker(instructions);
						
			int index = DoGetIndex(instructions, "first");
			Assert.AreEqual(2, tracker.State(index).Value);
			
			index = DoGetIndex(instructions, "catch");
			Assert.AreEqual(3, tracker.State(index).Value);
						
			index = DoGetIndex(instructions, "finally");
			Assert.AreEqual(2, tracker.State(index).Value);
			
			index = DoGetIndex(instructions, "last");
			Assert.AreEqual(2, tracker.State(index).Value);
		}

		private AssemblyDefinition m_assembly;
	} 
}
