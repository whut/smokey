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
using System.Collections.Generic;
using System.Reflection;
using Smokey.Framework;
using Smokey.Framework.Support;
using Smokey.Framework.Support.Advanced;
using Smokey.Framework.Instructions;

namespace Smokey.Tests
{	
	[TestFixture]
	public class ControlFlowGraphTest : BaseTest
	{	
		#region Test classes
		private class Cases
		{
			~Cases()
			{
			}

			public void Linear(int x)
			{
				int y = x + x;
				Console.WriteLine("sum = {0}", y);
			}

			public void OneIf(int x)
			{
				if (x > 0)
					Console.WriteLine("first: {0}", x);
				else
					Console.WriteLine("second: {0}", x+x);
			}

			public void TwoIfs(int x)
			{				
				if (x > 0)
				{
					Console.WriteLine("first: {0}", x);
					if (x > 1)
						Console.WriteLine("second: {0}", x+x);
				}
				Console.WriteLine("end");
			}

			public void Switch(int x)
			{				
				switch (x)
				{
					case 1:
						Console.WriteLine("1");
						break;
						
					case 2:
						Console.WriteLine("2");
						break;
						
					case 3:
						Console.WriteLine("3");
						break;
						
					case 4:
						Console.WriteLine("4");
						break;
						
					default:
						Console.WriteLine("end cases");
						break;
				}
				
				Console.WriteLine("end");
			}

			public void Try(int x)
			{				
				Console.WriteLine("start");

				try
				{
					if (x <= 0)
						throw new ArgumentException("x isn't positive");
						
					Console.WriteLine("all good");
				}
				catch (Exception)
				{
					Console.WriteLine("all bad");
				}
			}

			public void Finally(int x)
			{				
				Console.WriteLine("start");

				try
				{
					if (x <= 0)
						throw new ArgumentException("x isn't positive");
						
					Console.WriteLine("all good");
				}
				catch (Exception)
				{
					Console.WriteLine("all bad");
				}
				finally
				{
					Console.WriteLine("clean up");
				}
			}
			
			public void While(int x)
			{
				while (x > 0)
				{
					if (x < 0)
					{
						Console.WriteLine("bailing");
						break;
					}
					
					Console.WriteLine("x: {0}", x);
				}
			}
			
			public void Complex(int x)
			{
				switch (x)
				{
					case 0:
						if (x > 0)
							Complex(x - 1);
						break;
						
					case 1:
						if (x < 0)
							Complex(x + 1);
						break;
				}
				
				if (x == 100)
					Complex(100);
			} 

			public void TrashStackTrace()
			{
				try
				{
					Console.WriteLine("Run code");
				}
				catch (ApplicationException e)
				{
					Console.WriteLine(e.Message);
					throw e;	
				}
			}

			public void Coverage(Assembly assembly)
			{
				Console.WriteLine("loading rules from {0}", assembly.FullName);
				
				Type[] types = assembly.GetTypes();
				foreach (Type t in types)
				{
					if (t.IsClass && typeof(Rule).IsAssignableFrom(t))	
					{
						try
						{
							object o = Activator.CreateInstance(t);
							Console.WriteLine("added rule {0}", o);
						}
						catch (AssertException)
						{
							throw;		
						}
						catch (Exception e)
						{
							Console.WriteLine("Couldn't instantiate {0}: {1}", t.FullName, e.Message);
						}
					}
				}
	
				Log.InfoLine((Rule) null, "added rules from {0}", assembly.FullName);
			}

			public void NestedExceptions(int x)
			{				
				Console.WriteLine("start");

				try
				{
					try
					{
						if (x <= 0)
							throw new ArgumentException("nested try1");
					}
					catch (Exception)
					{
						Console.WriteLine("nested catch1");
						throw;
					}
					
					try
					{
						if (x >= 0)
							throw new ArgumentException("nested try2");
					}
					catch (Exception)
					{
						Console.WriteLine("nested catch2");
						throw;
					}
					
					Console.WriteLine("all good");
				}
				catch (Exception)
				{
					Console.WriteLine("outer catch");
				}
				finally
				{
					Console.WriteLine("clean up");
				}
			}

			public void NestedFinally(int x)
			{		
				try
				{
					Console.WriteLine("start");
	
					try
					{
						Console.WriteLine("try");
					}
					finally
					{
						Console.WriteLine("finally1");
					}
				}
				catch (Exception)
				{
					Console.WriteLine("outer catch");
				}
				finally
				{
					Console.WriteLine("finally2");
				}
			}
		}
		#endregion
				
		public ControlFlowGraphTest() 
		{
			string loc = Assembly.GetExecutingAssembly().Location;
			m_assembly = AssemblyFactory.GetAssembly(loc);
		}
								
		private TypeDefinition DoGetType(string name)
		{			
			string fullName = string.Format("{0}/{1}", GetType().FullName, name);
			TypeDefinition type = m_assembly.MainModule.Types[fullName];
			
			return type;
		}
								
		// Linear code has one block.
		[Test]
		public void Linear()	
		{
			TypeDefinition type = DoGetType("Cases");
			MethodDefinition method = type.Methods.GetMethod("Linear")[0];

			TypedInstructionCollection instructions = new TypedInstructionCollection(new SymbolTable(), method);			
			Log.DebugLine((ControlFlowGraph) null, "-----------------------------------"); 
			Log.DebugLine((ControlFlowGraph) null, "{0:F}", instructions);
			
			ControlFlowGraph graph = new ControlFlowGraph(instructions);
			Assert.AreEqual(1, graph.Roots.Length);
			Log.DebugLine((ControlFlowGraph) null, "\n{0:T}", graph.Roots[0]);
			Assert.AreEqual(1, graph.Length);

			Assert.AreEqual(0, graph.Roots[0].First.Index);
			Assert.AreEqual(instructions.Length - 1, graph.Roots[0].Last.Index);
		}

		// If with an else has 4 blocks.
		[Test]
		public void OneIf()	
		{
			TypeDefinition type = DoGetType("Cases");
			MethodDefinition method = type.Methods.GetMethod("OneIf")[0];

			TypedInstructionCollection instructions = new TypedInstructionCollection(new SymbolTable(), method);			
			Log.DebugLine((ControlFlowGraph) null, "-----------------------------------"); 
			Log.DebugLine((ControlFlowGraph) null, "{0:F}", instructions);
			
			ControlFlowGraph graph = new ControlFlowGraph(instructions);
			Assert.AreEqual(1, graph.Roots.Length);
			Log.DebugLine((ControlFlowGraph) null, "\n{0:T}", graph.Roots[0]);
			Assert.AreEqual(4, graph.Length);
		}
								
		// Two ifs should have 4 blocks.
		[Test]
		public void TwoIfs()	
		{
			TypeDefinition type = DoGetType("Cases");
			MethodDefinition method = type.Methods.GetMethod("TwoIfs")[0];

			TypedInstructionCollection instructions = new TypedInstructionCollection(new SymbolTable(), method);			
			Log.DebugLine((ControlFlowGraph) null, "-----------------------------------"); 
			Log.DebugLine((ControlFlowGraph) null, "{0:F}", instructions);
			
			ControlFlowGraph graph = new ControlFlowGraph(instructions);
			Assert.AreEqual(1, graph.Roots.Length);
			Log.DebugLine((ControlFlowGraph) null, "\n{0:T}", graph.Roots[0]);
			Assert.AreEqual(4, graph.Length);
		}
								
		// Switch should have 8 blocks.
		[Test]
		public void Switch()	
		{
			TypeDefinition type = DoGetType("Cases");
			MethodDefinition method = type.Methods.GetMethod("Switch")[0];

			TypedInstructionCollection instructions = new TypedInstructionCollection(new SymbolTable(), method);			
			Log.DebugLine((ControlFlowGraph) null, "-----------------------------------"); 
			Log.DebugLine((ControlFlowGraph) null, "{0:F}", instructions);
			
			ControlFlowGraph graph = new ControlFlowGraph(instructions);
			Assert.AreEqual(1, graph.Roots.Length);
			Log.DebugLine((ControlFlowGraph) null, "\n{0:T}", graph.Roots[0]);
			Assert.AreEqual(8, graph.Length);
		}
								
		// Try statement should have 6 blocks.
		[Test]
		public void Try()	
		{
			TypeDefinition type = DoGetType("Cases");
			MethodDefinition method = type.Methods.GetMethod("Try")[0];

			TypedInstructionCollection instructions = new TypedInstructionCollection(new SymbolTable(), method);			
			Log.DebugLine((ControlFlowGraph) null, "-----------------------------------"); 
			Log.DebugLine((ControlFlowGraph) null, "{0:F}", instructions);
			
			ControlFlowGraph graph = new ControlFlowGraph(instructions);
			Assert.AreEqual(1, graph.Roots.Length);
			Log.DebugLine((ControlFlowGraph) null, "\n{0:T}", graph.Roots[0]);
			Assert.AreEqual(6, graph.Length);
		}
								
		// Finally statement should have 7 blocks.
		[Test]
		public void Finally()	
		{
			TypeDefinition type = DoGetType("Cases");
			MethodDefinition method = type.Methods.GetMethod("Finally")[0];

			TypedInstructionCollection instructions = new TypedInstructionCollection(new SymbolTable(), method);			
			Log.DebugLine((ControlFlowGraph) null, "-----------------------------------"); 
			Log.DebugLine((ControlFlowGraph) null, "{0:F}", instructions);
			
			ControlFlowGraph graph = new ControlFlowGraph(instructions);
			Assert.AreEqual(1, graph.Roots.Length);
			Log.DebugLine((ControlFlowGraph) null, "\n{0:T}", graph.Roots[0]);
			Assert.AreEqual(7, graph.Length);
		}
							
		// While statement should have 6 blocks.
		[Test]
		public void While()	
		{
			TypeDefinition type = DoGetType("Cases");
			MethodDefinition method = type.Methods.GetMethod("While")[0];

			TypedInstructionCollection instructions = new TypedInstructionCollection(new SymbolTable(), method);			
			Log.DebugLine((ControlFlowGraph) null, "-----------------------------------"); 
			Log.DebugLine((ControlFlowGraph) null, "{0:F}", instructions);
			
			ControlFlowGraph graph = new ControlFlowGraph(instructions);
			Assert.AreEqual(1, graph.Roots.Length);
			Log.DebugLine((ControlFlowGraph) null, "\n{0:T}", graph.Roots[0]);
			Assert.AreEqual(6, graph.Length);
		}
							
		// Complex should have 12 blocks.
		[Test]
		public void Complex()	
		{
			TypeDefinition type = DoGetType("Cases");
			MethodDefinition method = type.Methods.GetMethod("Complex")[0];

			TypedInstructionCollection instructions = new TypedInstructionCollection(new SymbolTable(), method);			
			Log.DebugLine((ControlFlowGraph) null, "-----------------------------------"); 
			Log.DebugLine((ControlFlowGraph) null, "{0:F}", instructions);
			
			ControlFlowGraph graph = new ControlFlowGraph(instructions);
			Assert.AreEqual(1, graph.Roots.Length);
			Log.DebugLine((ControlFlowGraph) null, "\n{0:T}", graph.Roots[0]);
			Assert.AreEqual(12, graph.Length);
		}
							
		// Finalize should have 3 blocks.
		[Test]
		public void Finalize1()	
		{
			TypeDefinition type = DoGetType("Cases");
			MethodDefinition method = type.Methods.GetMethod("Finalize")[0];

			TypedInstructionCollection instructions = new TypedInstructionCollection(new SymbolTable(), method);			
			Log.DebugLine((ControlFlowGraph) null, "-----------------------------------"); 
			Log.DebugLine((ControlFlowGraph) null, "{0:F}", instructions);
			
			ControlFlowGraph graph = new ControlFlowGraph(instructions);
			Assert.AreEqual(1, graph.Roots.Length);
			Log.DebugLine((ControlFlowGraph) null, "\n{0:T}", graph.Roots[0]);
			Assert.AreEqual(3, graph.Length);
		}

		[Test]
		public void TrashStackTrace()	
		{
			TypeDefinition type = DoGetType("Cases");
			MethodDefinition method = type.Methods.GetMethod("TrashStackTrace")[0];

			TypedInstructionCollection instructions = new TypedInstructionCollection(new SymbolTable(), method);			
			Log.DebugLine((ControlFlowGraph) null, "-----------------------------------"); 
			Log.DebugLine((ControlFlowGraph) null, "{0:F}", instructions);
			
			ControlFlowGraph graph = new ControlFlowGraph(instructions);
			Assert.AreEqual(2, graph.Roots.Length);
			Log.DebugLine((ControlFlowGraph) null, "Root 1:\n{0:T}", graph.Roots[0]);
			Log.DebugLine((ControlFlowGraph) null, "Root 2:\n{0:T}", graph.Roots[1]);
		}	

		[Test]
		public void Coverage()	
		{
			TypeDefinition type = DoGetType("Cases");
			MethodDefinition method = type.Methods.GetMethod("Coverage")[0];

			TypedInstructionCollection instructions = new TypedInstructionCollection(new SymbolTable(), method);			
			Log.DebugLine((ControlFlowGraph) null, "-----------------------------------"); 
			Log.DebugLine((ControlFlowGraph) null, "{0:F}", instructions);
			
			ControlFlowGraph graph = new ControlFlowGraph(instructions);
			Assert.AreEqual(1, graph.Roots.Length);	
		}

		[Test]
		public void NestedExceptions()	
		{
			TypeDefinition type = DoGetType("Cases");
			MethodDefinition method = type.Methods.GetMethod("NestedExceptions")[0];

			TypedInstructionCollection instructions = new TypedInstructionCollection(new SymbolTable(), method);			
			Log.DebugLine((ControlFlowGraph) null, "-----------------------------------"); 
			Log.DebugLine((ControlFlowGraph) null, "{0:F}", instructions);
			
			ControlFlowGraph graph = new ControlFlowGraph(instructions);
			Assert.AreEqual(1, graph.Roots.Length);
		}

		[Test]
		public void ZNestedFinally()	
		{
			TypeDefinition type = DoGetType("Cases");
			MethodDefinition method = type.Methods.GetMethod("NestedFinally")[0];

			TypedInstructionCollection instructions = new TypedInstructionCollection(new SymbolTable(), method);			
			Log.DebugLine((ControlFlowGraph) null, "-----------------------------------"); 
			Log.DebugLine((ControlFlowGraph) null, "{0:F}", instructions);
			
			ControlFlowGraph graph = new ControlFlowGraph(instructions);
			Assert.AreEqual(2, graph.Roots.Length);
		}

		private AssemblyDefinition m_assembly;
	} 
}
