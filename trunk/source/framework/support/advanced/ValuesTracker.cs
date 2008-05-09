// Copyright (C) 2007-2008 Jesse Jones
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

using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Text;
using Smokey.Framework.Instructions;

namespace Smokey.Framework.Support.Advanced
{	
	namespace Values
	{	
		/// <summary>Returns the state of arguments, locals, and the stack for every 
		/// instruction in the method.</summary>
		public class Tracker
		{
			public Tracker(TypedInstructionCollection instructions)
			{
				DBC.Pre(instructions != null, "instructions is null");

				m_instructions = instructions;
				m_initialState = DoGetInitialState(m_instructions);
			}
			
			public void InitArg(int arg, long? value)
			{
				DBC.Pre(arg >= 0, "argument is negative");

				m_initialState.State.Arguments[arg] = value;
			}
			
			[DisableRule("D1032", "UnusedMethod")]
			public void Analyze()
			{
				var graph = new ControlFlowGraph(m_instructions);		
				Analyze(graph);
			}
			
			public void Analyze(ControlFlowGraph graph)
			{				
				DBC.Pre(graph != null, "graph is null");

				Profile.Start("Splicing");
				var visited = new List<BasicBlock>();
				foreach (BasicBlock root in graph.Roots)
					DoSpliceHandlers(m_instructions, root, visited);

				visited.Clear();
				foreach (BasicBlock root in graph.Roots)
					DoSpliceNullCheck(m_instructions, root, visited);
				var data = new DataFlow<Lattice>(m_instructions, graph.Roots);
				m_skipped = data.Skipped;
				Profile.Stop("Splicing");
				
				var functions = new Lattice.Functions();
				Dictionary<BasicBlock, Lattice> lattices = data.Analyze(functions, m_initialState);
				
				Profile.Start("Post Transform");
				m_states = new State[m_instructions.Length];
				foreach (var entry in lattices)
				{
					BasicBlock block = entry.Key;
					if (block.Length > 0)
					{
						Lattice lattice = entry.Value;
						
						for (int index = block.First.Index; index <= block.Last.Index; ++index)	// it'd be nice to assert that every index was set, but methods often have dead code so it's a little difficult
						{
							m_states[index] = lattice.State;
							lattice = lattice.Transform(index);
						}
					}
				}
				Profile.Stop("Post Transform");
				
				for (int index = 0; index < m_instructions.Length; ++index)
				{
					Log.DebugLine(this, "{0:X2}: {1}", m_instructions[index].Untyped.Offset, m_states[index]);
				}
			}
			
			// Returns the state of arguments, locals, and the stack before the 
			// instruction at index executes.
			[DisableRule("D1032", "UnusedMethod")]
			public State State(int index)
			{
				DBC.Pre(index >= 0 && index < m_states.Length, "index out of range");
				return m_states[index];
			}
						
			// The stack is kind of annoying so we'll provide helpers.
			public long? GetStack(int index, int nth)
			{
				DBC.FastPre(index >= 0 && index < m_states.Length, "index out of range");

				List<StackEntry> stack = m_states[index].Stack;
				if (stack == null)
				{
					if (!m_skipped)
					{						
						Log.WarningLine(this, "stack was null at {0:X2}", m_instructions[index].Untyped.Offset);
						Log.DebugLine(this, "{0:F}", m_instructions);
					}
					return null;
				}
				
				DBC.FastPre(nth >= 0 && nth < stack.Count, "nth out of range");
				
				return stack[stack.Count - nth - 1].Value;
			}
			
			// Returns -1 if there isn't one instruction that sets the stack entry.
			public int GetStackIndex(int index, int nth)
			{
				DBC.FastPre(index >= 0 && index < m_states.Length, "index out of range");
				DBC.FastPre(nth >= 0, "nth out of range");

				List<StackEntry> stack = m_states[index].Stack;
				if (stack == null)
				{
					if (!m_skipped)
					{						
						Log.WarningLine(this, "stack was null at {0:X2}", m_instructions[index].Untyped.Offset);
						Log.DebugLine(this, "{0:F}", m_instructions);
					}
					return -1;
				}
				
				if (nth < stack.Count)
					return stack[stack.Count - nth - 1].Index;
				else
					return -1;
			}
			
			// Lengths of special blocks we splice in. Used by Lattice.Functions.Transform. 
			public static readonly int CatchBlockLen   = -1;
			public static readonly int FinallyBlockLen = -2;
			
			public static readonly int FirstNullArgBlockLen = -1000;		// A_0
			public static readonly int LastNullArgBlockLen  = -1999;		// A_999

			public static readonly int FirstNonNullArgBlockLen = -2000;		// A_0
			public static readonly int LastNonNullArgBlockLen  = -2999;		// A_999
			
			public static readonly int FirstNullLocalBlockLen = -3000;		// A_0
			public static readonly int LastNullLocalBlockLen  = -3999;		// A_999

			public static readonly int FirstNonNullLocalBlockLen = -4000;	// A_0
			public static readonly int LastNonNullLocalBlockLen  = -4999;	// A_999
			
			#region Private Methods -------------------------------------------
			// Catch and finally blocks require special handling to reset their stacks
			// on entry. We can't really do this in meet without breaking commutativity
			// and transform is too late. So what we do is splice in a special zero
			// length block right before the handler and when it is transformed reset
			// the stack.
			private void DoSpliceHandlers(TypedInstructionCollection instructions, BasicBlock block, List<BasicBlock> visited)
			{
				if (visited.IndexOf(block) < 0)
				{
					visited.Add(block);
					
					for (int i = 0; i < block.Next.Length; ++i)
					{
						BasicBlock next = block.Next[i];
						
						if (next.Length > 0)
						{
							TryCatch tc = instructions.TryCatchCollection.HandlerStartsAt(next.First.Index);
							if (tc != null && tc.Try.Index != next.First.Index)
							{
								BasicBlock splice = null;
								if (tc.Finally.Length > 0 && tc.Finally.Index == next.First.Index)
									splice = new BasicBlock(FinallyBlockLen, new BasicBlock[1]);
								else if (tc.Fault.Length > 0 && tc.Fault.Index == next.First.Index)
									splice = new BasicBlock(FinallyBlockLen, new BasicBlock[1]);
								else
									splice = new BasicBlock(CatchBlockLen, new BasicBlock[1]);
								splice.Next[0] = next;
								
								block.Next[i] = splice;
							}
						}
						
						DoSpliceHandlers(instructions, next, visited);
					}
					
					if (block.Finally != null)
						DoSpliceHandlers(instructions, block.Finally, visited);
					else if (block.Fault != null)
						DoSpliceHandlers(instructions, block.Fault, visited);
				}
			}
						
            // This is another case where the lattice meet and transform paradigm 
            // doesn't work so well. What we want to do is set locals and arguments
            // based on the result of null pointer comparisons. Doing this in meet 
            // would be really ugly and would break commutativity so we handle it
            // by splicing in a special block to each edge leading out from the branch.
			private void DoSpliceNullCheck(TypedInstructionCollection instructions, BasicBlock block, List<BasicBlock> visited)
			{
				if (visited.IndexOf(block) < 0)
				{
					visited.Add(block);
					
					if (block.Next.Length == 2)
						DoTrySpliceNull(instructions, block);
					
					for (int i = 0; i < block.Next.Length; ++i)
						DoSpliceNullCheck(instructions, block.Next[i], visited);
					
					if (block.Finally != null)
						DoSpliceNullCheck(instructions, block.Finally, visited);
					else if (block.Fault != null)
						DoSpliceNullCheck(instructions, block.Fault, visited);
				}
			}
			
			private static void DoTrySpliceNull(TypedInstructionCollection instructions, BasicBlock block)
			{
				TypedInstruction instruction = instructions[block.Last.Index];
				Code code = instruction.Untyped.OpCode.Code;
				
				if (code == Code.Brtrue || code == Code.Brtrue_S || code == Code.Brfalse || code == Code.Brfalse_S)
				{
					DoSpliceTrue(instructions, block);
				}
				else if (code == Code.Beq || code == Code.Beq_S || code == Code.Bne_Un || code == Code.Bne_Un_S)
				{
					DoSpliceEq(instructions, block);
				}
			}
						
			private static void DoSpliceEq(TypedInstructionCollection instructions, BasicBlock block)
			{				
				int index = block.Last.Index;
				
                // ldloc.0 V_0
				// ldnull     
                // beq     2F
				LoadArg arg = null;
				LoadLocal local = null;
				if (instructions[index - 1].Untyped.OpCode.Code == Code.Ldnull)
				{
					arg = instructions[block.Last.Index - 2] as LoadArg;
					local = instructions[block.Last.Index - 2] as LoadLocal;
				}
				else if (instructions[index - 2].Untyped.OpCode.Code == Code.Ldnull)
				{
					arg = instructions[block.Last.Index - 1] as LoadArg;
					local = instructions[block.Last.Index - 1] as LoadLocal;
				}

				if (arg != null || local != null)
				{
					TypedInstruction instruction = instructions[block.Last.Index];
					Code code = instruction.Untyped.OpCode.Code;
					
					if (code == Code.Bne_Un || code == Code.Bne_Un_S)
					{
						if (arg != null)
							DoSpliceNull(block, FirstNullArgBlockLen - arg.Arg, FirstNonNullArgBlockLen - arg.Arg);
	
						else 
							DoSpliceNull(block, FirstNullLocalBlockLen - local.Variable, FirstNonNullLocalBlockLen - local.Variable);
					}
					else
					{
						if (arg != null)
							DoSpliceNull(block, FirstNonNullArgBlockLen - arg.Arg, FirstNullArgBlockLen - arg.Arg);
	
						else 
							DoSpliceNull(block, FirstNonNullLocalBlockLen - local.Variable, FirstNullLocalBlockLen - local.Variable);
					}
				}
			}
						
			private static void DoSpliceTrue(TypedInstructionCollection instructions, BasicBlock block)
			{				
				int index = block.Last.Index;
	
				// ldarg.1 a1
				// brtrue  10	(not taken is the first branch)
				LoadArg arg = instructions[index - 1] as LoadArg;
				LoadLocal local = instructions[index - 1] as LoadLocal;

				if (arg != null || local != null)
				{
					TypedInstruction instruction = instructions[index];
					Code code = instruction.Untyped.OpCode.Code;
	
					if (code == Code.Brtrue || code == Code.Brtrue_S)
					{
						if (arg != null)
							DoSpliceNull(block, FirstNullArgBlockLen - arg.Arg, FirstNonNullArgBlockLen - arg.Arg);
	
						else 
							DoSpliceNull(block, FirstNullLocalBlockLen - local.Variable, FirstNonNullLocalBlockLen - local.Variable);
					}
					else 
					{
						if (arg != null)
							DoSpliceNull(block, FirstNonNullArgBlockLen - arg.Arg, FirstNullArgBlockLen - arg.Arg);
	
						else 
							DoSpliceNull(block, FirstNonNullLocalBlockLen - local.Variable, FirstNullLocalBlockLen - local.Variable);
					}
				}
			}
						
			private static void DoSpliceNull(BasicBlock block, int branch0Len, int branch1Len)
			{
				BasicBlock splice0 = new BasicBlock(branch0Len, new BasicBlock[1]);
				BasicBlock splice1 = new BasicBlock(branch1Len, new BasicBlock[1]);
				
				splice0.Next[0] = block.Next[0];
				splice1.Next[0] = block.Next[1];
				
				block.Next[0] = splice0;
				block.Next[1] = splice1;
			}
			
			private static Lattice DoGetInitialState(TypedInstructionCollection instructions)
			{
				// Get the arguments.
				int numArgs = instructions.Method.Parameters.Count + 1;
	
				long?[] args = new long?[numArgs];
				for (int i = 0; i < numArgs; ++i)
					if (instructions.Method.HasThis && i == 0)
						args[i] = 1;
					else
						args[i] = null;
					
				// Get the locals.
				int numLocals = instructions.Method.Body.Variables.Count;
				long? value = 0;
				if (!instructions.Method.Body.InitLocals)
					value = null;

				long?[] locals = new long?[numLocals];
				for (int i = 0; i < numLocals; ++i)
					locals[i] = value;

				// Get the stack.
				List<StackEntry> stack = new List<StackEntry>();

				return new Lattice(instructions, new State(args, locals, stack));
			}
			#endregion
									
			#region Fields ----------------------------------------------------
			private TypedInstructionCollection m_instructions;
			private Lattice m_initialState;
			private State[] m_states;
			private bool m_skipped;
			#endregion
		}
	}
}

