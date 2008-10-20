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
using Smokey.Framework.Instructions;
using Smokey.Internal;
using System;
using System.Collections.Generic;
//using System.Diagnostics;
using System.Text;

namespace Smokey.Framework.Support.Advanced
{			
	/// <summary>Finds the basic blocks and wires them together based on the control flow.</summary>
	public class ControlFlowGraph 
	{				
		public ControlFlowGraph(TypedInstructionCollection instructions)
		{		
			DBC.Pre(instructions != null, "instructions is null");
			Profile.Start("ControlFlowGraph ctor");
			
			// If we have instructions then,
			if (instructions.Length > 0)
			{
				// get all of the wired up basic blocks,
				Dictionary<int, BasicBlock> blocks = DoGetWiredBlocks(instructions);
				m_numBlocks = blocks.Count;
					
				// if the very first instruction is a try block then we'll have 
				// mutiple roots,
				foreach (TryCatch tc in instructions.TryCatchCollection)
				{
					if (tc.Try.Index == 0)
					{
						m_roots = new BasicBlock[tc.Catchers.Count + 1];
						m_roots[0] = blocks[0];
	
						for (int i = 0; i < tc.Catchers.Count; ++i)
						{
							CatchBlock cb = tc.Catchers[i];
							m_roots[i + 1] = blocks[cb.Index];
						}					
					}
				}
				
				// otherwise the first instruction is our root.
				if (m_roots == null)
					m_roots = new BasicBlock[]{blocks[0]};
				
				DoInvariantRoots(instructions, blocks, m_roots);
			}
			else
				m_roots = new BasicBlock[0];			
			
			Profile.Stop("ControlFlowGraph ctor");
		}
		
		/// <summary>Returns the number of blocks in the method.</summary>
		public int Length	
		{
			[DisableRule("D1032", "UnusedMethod")]	
			get {return m_numBlocks;}
		}
		
		/// <summary>Returns the root blocks. The length will normally be one, but may be larger if the
		/// method's first instruction starts a try block, or zero if the method has no body.</summary>
		public BasicBlock[] Roots	{get {return m_roots;}}
						
		#region Private methods
		private Dictionary<int, BasicBlock> DoGetWiredBlocks(TypedInstructionCollection instructions)
		{
			DBC.Pre(instructions.Length > 0, "Can't get a root if there are no instructions");

			m_branchTargets = new List<int>();
			Dictionary<int, BasicBlock> blocks = DoGetUnwiredBlocks(instructions);	// index -> block
			
			foreach (KeyValuePair<int, BasicBlock> entry in blocks)
				DoWireBlock(instructions, blocks, entry.Value);
				
			return blocks;
		}
		
		private Dictionary<int, BasicBlock> DoGetUnwiredBlocks(TypedInstructionCollection instructions)
		{
			// First get a list of all the instructions which start a block.
			List<int> leaders = DoGetLeaders(instructions);
			
			// Sort them so we can efficiently pop them off the list.
			leaders.Sort();
			Log.TraceLine(this, "Leaders: {0}", DoTargetsToString(instructions, leaders));
			leaders.Reverse();
			
			// And form the basic blocks by starting at the first leader and
			// accumulating instructions until we hit another leader.
			Dictionary<int, BasicBlock> blocks = new Dictionary<int, BasicBlock>();

			while (leaders.Count > 0)
			{
				int first = leaders[leaders.Count - 1];
				leaders.RemoveAt(leaders.Count - 1);
				
				BasicBlock block;
				if (leaders.Count > 0)
				{
					int last = leaders[leaders.Count - 1];
					DBC.Assert(first < last, "Leader {0} should be before {1}", first, last);
					
					block = new BasicBlock(instructions[first], instructions[last - 1]);
				}
				else
				{
					block = new BasicBlock(instructions[first], instructions[instructions.Length - 1]);
				}

				blocks.Add(first, block);
				Log.DebugLine(this, "Added block: {0}", block);
			}
			
			return blocks;
		}
				
		private List<int> DoGetLeaders(TypedInstructionCollection instructions)
		{
			// The leaders will be:   
			List<int> leaders = new List<int>();
			
			// 1) the first instruction in the method,
			leaders.Add(0);
			
			for (int index = 0; index < instructions.Length; ++index)
			{
				TypedInstruction instruction = instructions[index];

				// 2) the targets of branches,
				Branch branch = instruction as Branch;
				if (branch != null)
				{
					m_branchTargets.Add(branch.Target.Index);
				}
				else
				{
					Switch swtch = instruction as Switch;
					if (swtch != null)
					{
						foreach (TypedInstruction i in swtch.Targets)
							m_branchTargets.Add(i.Index);
					}
				}
				
				if (index > 0)
				{
					// 3) and instructions starting try, catch or finally blocks,
					TryCatch tc = instructions.TryCatchCollection.HandlerStartsAt(index);
					if (tc != null)
						leaders.Add(index);

					// 4) the instructions following branches or end instructions.
					else if (instructions[index - 1] is Branch || instructions[index - 1] is Switch || instructions[index - 1] is End)
						leaders.Add(index);
				}
			}
			
			foreach (int target in m_branchTargets)
			{
				if (leaders.IndexOf(target) < 0)
					leaders.Add(target);					
			}
			
			return leaders;
		}
		
		private static void DoWireCatches(TypedInstructionCollection instructions, List<BasicBlock> nextBlocks, Dictionary<int, BasicBlock> blocks, int targetIndex)
		{
			foreach (TryCatch tc in instructions.TryCatchCollection)
			{
				if (tc.Try.Index == targetIndex)
				{
					foreach (CatchBlock cb in tc.Catchers)
						nextBlocks.Add(blocks[cb.Index]);

					if (tc.Fault.Length > 0 && tc.Catchers.Count == 0)
						nextBlocks.Add(blocks[tc.Fault.Index]);
				}
			}
		}
				
		private void DoWireBlock(TypedInstructionCollection instructions, Dictionary<int, BasicBlock> blocks, BasicBlock block)
		{
			var nextBlocks = new List<BasicBlock>();
			
			if (block.First.Index == 0)
				DoWireCatches(instructions, nextBlocks, blocks, 0);

			do
			{
				ConditionalBranch conditional = block.Last as ConditionalBranch;
				if (conditional != null)
				{
					nextBlocks.Add(blocks[conditional.Index + 1]);
					nextBlocks.Add(blocks[conditional.Target.Index]);
					
					DoWireCatches(instructions, nextBlocks, blocks, conditional.Index + 1);
					DoWireCatches(instructions, nextBlocks, blocks, conditional.Target.Index);
					break;
				}

				UnconditionalBranch unconditional = block.Last as UnconditionalBranch;
				if (unconditional != null)
				{
					nextBlocks.Add(blocks[unconditional.Target.Index]);
					DoWireCatches(instructions, nextBlocks, blocks, unconditional.Target.Index);
					
					if (block.Last.Untyped.OpCode.Code == Code.Leave || block.Last.Untyped.OpCode.Code == Code.Leave_S)
						DoAddFinally(instructions, nextBlocks, blocks, block);
					break;
				}

				Switch swtch = block.Last as Switch;
				if (swtch != null)
				{
					foreach (TypedInstruction t in swtch.Targets)
					{
						nextBlocks.Add(blocks[t.Index]);
						DoWireCatches(instructions, nextBlocks, blocks, t.Index);
					}
					nextBlocks.Add(blocks[swtch.Index + 1]);
					DoWireCatches(instructions, nextBlocks, blocks, swtch.Index + 1);
					break;
				}

				End end = block.Last as End;
				if (end != null)
				{
					DoAddFinally(instructions, nextBlocks, blocks, block);
					break;
				}
				
				DoWireCatches(instructions, nextBlocks, blocks, block.Last.Index + 1);
				nextBlocks.Add(blocks[block.Last.Index + 1]);
			} 
			while (false);
							
			block.Next = nextBlocks.ToArray();
			Log.DebugLine(this, "Wired block: {0}", block);
		}
		
		// Before leaving a try or catch block we need to transfer control to 
		// the finally block (if present).
		private static void DoAddFinally(TypedInstructionCollection instructions, List<BasicBlock> nextBlocks, Dictionary<int, BasicBlock> blocks, BasicBlock block)
		{
			TryCatch candidate = null;
			
			for (int i = 0; i < instructions.TryCatchCollection.Length; ++i)
			{
				TryCatch tc = instructions.TryCatchCollection[i];
				if (tc.Finally.Length > 0)
				{
					if (block.First.Index >= tc.Try.Index && block.Last.Index < tc.Try.Index + tc.Try.Length)
					{
						if (candidate == null || tc.Try.Length < candidate.Try.Length)
							candidate = tc;
					}
					else
					{
						for (int j = 0; j < tc.Catchers.Count; ++j)
						{
							if (block.First.Index >= tc.Catchers[j].Index && block.Last.Index < tc.Catchers[j].Index + tc.Catchers[j].Length)
							{
								if (candidate == null || tc.Try.Length < candidate.Try.Length)
									candidate = tc;
							}
						}
					}
				}
				else if (tc.Fault.Length > 0)
				{
					for (int j = 0; j < tc.Catchers.Count; ++j)
					{
						if (block.First.Index >= tc.Catchers[j].Index && block.Last.Index < tc.Catchers[j].Index + tc.Catchers[j].Length)
						{					
							block.Fault = blocks[tc.Fault.Index];
							DoWireCatches(instructions, nextBlocks, blocks, tc.Fault.Index);
							return;
						}
					}
				}
			}	
			
			if (candidate != null)
			{
				block.Finally = blocks[candidate.Finally.Index];
				DoWireCatches(instructions, nextBlocks, blocks, candidate.Finally.Index);
			}
		}

		[System.Diagnostics.Conditional("DEBUG")]
		private void DoInvariantRoots(TypedInstructionCollection instructions, Dictionary<int, BasicBlock> blocks, BasicBlock[] roots)
		{	
			List<BasicBlock> deadBlocks = null, reachable = null;
			
			try
			{
				// Get all of the blocks reachable from roots,
				reachable = new List<BasicBlock>();
				foreach (BasicBlock b in roots)
					DoGetReachableBlocks(reachable, b);
				
				int sum = 0;
				for (int i = 0; i < reachable.Count; ++i)
				{
					BasicBlock b1 = reachable[i];
					Log.TraceLine(this, "reachable{0} = {1}", i, b1);
					sum += b1.Last.Index - b1.First.Index + 1;
					
					// they should be sane,
					DBC.Assert(b1.First.Index <= b1.Last.Index, "block {0} has bad indexes", b1);
					DBC.Assert(b1.Next != null, "block {0} has no next blocks array", b1);				
					
					// and they should all be disjoint.
					for (int j = i + 1; j < reachable.Count; ++j)
					{
						BasicBlock b2 = reachable[j];
						DBC.Assert(b1.First.Index < b2.First.Index || b1.First.Index > b2.Last.Index, "block {0} intersects block {1}", b1, b2);
						DBC.Assert(b1.Last.Index < b2.First.Index || b1.Last.Index > b2.Last.Index, "block {0} intersects block {1}", b1, b2);
					}
				}
				
				// And the total number of instructions in them should equal the number of
				// instructions in instructions plus any dead blocks.
				deadBlocks = DoGetDeadBlocks(instructions, blocks, reachable);
				
				int deadCount = 0;
				foreach (BasicBlock b in deadBlocks)
				{
					deadCount += b.Last.Index - b.First.Index + 1;
					Log.TraceLine(this, "Block {0} is dead code", b);
				}
				
				DBC.Assert(sum + deadCount == instructions.Length, "blocks cover {0} instructions, but there are {1} instructions", sum, instructions.Length);
			}
			catch (AssertException e)
			{
				Log.ErrorLine(this, "Assert: {0}", e.Message);
				if (reachable != null && deadBlocks != null)
					Log.ErrorLine(this, "Missed: {0}", DoGetMissedOffsets(instructions, reachable, deadBlocks));
				Log.ErrorLine(this, "Instructions:");
				Log.ErrorLine(this, "{0:F}", instructions);
				Log.ErrorLine(this, "Blocks are:");
				foreach (BasicBlock b in blocks.Values)
				{
					Log.ErrorLine(this, "{0}", b);
					Log.ErrorLine(this);
				}
				throw;
			}
		}
		
		private static string DoGetMissedOffsets(TypedInstructionCollection instructions, List<BasicBlock> reachable, List<BasicBlock> deadBlocks)
		{
			// Get a list of all the offsets in the method.
			List<int> offsets = new List<int>(instructions.Length);
			
			foreach (TypedInstruction instruction in instructions)
				offsets.Add(instruction.Untyped.Offset);
				
			// Remove the visited instructions.
			foreach (BasicBlock block in reachable)
				DoRemoveOffsets(instructions, offsets, block);
				
			// Remove the dead code.
			foreach (BasicBlock block in deadBlocks)
				DoRemoveOffsets(instructions, offsets, block);
				
			// Anything left is code we should have visited but didn't.
			return ListExtensions.Accumulate(offsets, string.Empty, (s, e) => s + e.ToString("X2") + " ");
		}
		
		private static void DoRemoveOffsets(TypedInstructionCollection instructions, List<int> offsets, BasicBlock block)
		{
			if (block.Length > 0)
			{
				for (int i = block.First.Index; i <= block.Last.Index; ++i)
				{
					Unused.Value = offsets.Remove(instructions[i].Untyped.Offset);
				}
			}
		}
		
		private List<BasicBlock> DoGetDeadBlocks(TypedInstructionCollection instructions, Dictionary<int, BasicBlock> blocks, List<BasicBlock> reachable)
		{
			List<BasicBlock> deadBlocks = new List<BasicBlock>();
			
			// If the block isn't the target of a branch and it's preceded by an end
			// instruction it's dead code. This can happen if a catch block throws.
			foreach (KeyValuePair<int, BasicBlock> entry in blocks)
			{
				BasicBlock b = entry.Value;
				int i = b.First.Index;
				
				if (m_branchTargets.IndexOf(i) < 0)
				{
					if (instructions.TryCatchCollection.HandlerStartsAt(i) == null)
						if (i > 0 && (instructions[i - 1] is End || instructions[i - 1] is UnconditionalBranch))
							deadBlocks.Add(b);
				}
			}
			
			// If a leave is dead code then its target is dead as well. 
			List<BasicBlock> deaderBlocks = new List<BasicBlock>();
			foreach (BasicBlock b in deadBlocks)
			{
				if (b.Length > 0)
				{
					for (int i = b.First.Index; i <= b.Last.Index; ++i)
					{
						if (instructions[i].Untyped.OpCode.Code == Code.Leave || instructions[i].Untyped.OpCode.Code == Code.Leave_S)
						{
							UnconditionalBranch branch = (UnconditionalBranch) instructions[i];
							if (reachable.IndexOf(blocks[branch.Target.Index]) < 0 && deaderBlocks.IndexOf(blocks[branch.Target.Index]) < 0)
								deaderBlocks.Add(blocks[branch.Target.Index]);
						}
					}
				}
			}
			deadBlocks.AddRange(deaderBlocks);
			
			return deadBlocks;
		}

		[System.Diagnostics.Conditional("DEBUG")]
		private static void DoGetReachableBlocks(List<BasicBlock> blocks, BasicBlock block)
		{
			if (blocks.IndexOf(block) < 0)
			{
				blocks.Add(block);
				
				foreach (BasicBlock b in block.Next)
					DoGetReachableBlocks(blocks, b);
					
				if (block.Finally != null)
					DoGetReachableBlocks(blocks, block.Finally);
				else if (block.Fault != null)
					DoGetReachableBlocks(blocks, block.Fault);
			}
		}
		
		private static string DoTargetsToString(TypedInstructionCollection instructions, List<int> targets)
		{
			StringBuilder builder = new StringBuilder();
			
			builder.Append('[');
			for (int i = 0; i < targets.Count; ++i)
			{
				int index = targets[i];
				TypedInstruction instruction = instructions[index];
				
				builder.Append(instruction.Untyped.Offset.ToString("X2"));
				if (i + 1 < targets.Count)
					builder.Append(", ");
			}
			builder.Append(']');
			
			return builder.ToString();
		}
		#endregion
				
		#region Fields
		private BasicBlock[] m_roots;
		private int m_numBlocks;
		
		private List<int> m_branchTargets;
		#endregion
	}
}

