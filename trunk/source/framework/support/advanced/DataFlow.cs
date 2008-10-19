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

//#define LOG_BLOCKS

using Smokey.Framework.Instructions;
using Smokey.Internal;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

#if OLD
namespace Smokey.Framework.Support.Advanced
{			
	/// <summary>Used with <c>DataFlow</c> to represent arbitrary state and the methods
	/// operating on the state.</summary>
	///
	/// <remarks>LATTICE represents the state of a basic block. Because we don't have 
	/// CRTP in C# we use the virtual methods in this class to avoid downcasting
	/// from a base lattice to the derived lattices.</remarks>
	public abstract class LatticeFunctions<LATTICE> 
	{
		/// <summary>Note that this should function should be closed, commutative,
		/// and associative.</summary>
		public abstract LATTICE Meet(LATTICE lhs, LATTICE rhs);

		/// <summary>Unique value such that Meet(any, Top) == Top.</summary>
		public abstract LATTICE Top {get;}

		/// <summary>Given a lattice representing the state upon entry to the block
		/// return a new lattice representing the state on exit from the block.</summary>
		public abstract LATTICE Transform(LATTICE lhs, BasicBlock block);

		/// <summary>Returns true if the two lattices are different.</summary>
		public abstract bool DiffersFrom(LATTICE lhs, LATTICE rhs);
	}
	
	/// <summary>Base class used for logging.</summary>
	public abstract class BaseDataFlow
	{
	}
	
	/// <summary>General purpose class used to track how data flows through a method.</summary>
	///
	/// <remarks>For example, ValuesState uses this to determine the values for arguments,
	/// local variables, and the stack at each instruction in a method.</remarks>
	public class DataFlow<LATTICE> : BaseDataFlow
	{	
		public DataFlow(TypedInstructionCollection instructions, BasicBlock[] roots)
		{	
			DBC.Pre(instructions != null, "instructions is null");
			DBC.Pre(roots.Length > 0, "no roots");
			Log.DebugLine(this, "{0:F}", instructions);
			
			if (instructions.Method.Body.Variables.Count < 256)
			{
				if (roots.Length == 1)
					m_entry = roots[0];
				else
					m_entry = new BasicBlock(0, roots);
				
				DoAddReachableToWorkList(m_entry);	
				DoAddPredecessors(null, m_entry);
								
				// The algorithm is much faster if predecessor blocks appear first.
				m_workList.Sort(delegate (BasicBlock lhs, BasicBlock rhs)
				{				
					return lhs.SafeIndex.CompareTo(rhs.SafeIndex);
				});	
	
#if LOG_BLOCKS
				DoLogWorkList();
				DoLogPredecessors();
#endif
			}
			else
			{
				// Methods with a very large number of local variables can cause dataflow
				// to take a very long time so we skip them.
				m_skipped = true;
				Log.InfoLine(this, "skipping DataFlow for {0} (it has {1} locals)", instructions.Method, instructions.Method.Body.Variables.Count);			
			}
		}
		
		/// <summary>Returns lattices for the start of all blocks reachable from the roots.
		/// Note that this may return zero or negative length blocks.</summary>
		public Dictionary<BasicBlock, LATTICE> Analyze(LatticeFunctions<LATTICE> functions, LATTICE initialState)
		{
			Dictionary<BasicBlock, LATTICE> inputStates;
			
			if (m_workList.Count > 0)
			{
				Profile.Start("Dataflow Analyze");
				inputStates = DoAnalyze(functions, initialState);
				Profile.Stop("Dataflow Analyze");
			}
			else
				inputStates = new Dictionary<BasicBlock, LATTICE>();
				
			return inputStates;
		}
		
		/// <summary>Returns true if we skipped processing this method (because it's
		/// too complex).</summary>
		public bool Skipped
		{
			get {return m_skipped;}
		}

		#region Private Methods -----------------------------------------------
		// This is a standard iterative data-flow analysis algorithm. See Advanced
		// Compiler Design and Implementation by Muchnick for example.
		private Dictionary<BasicBlock, LATTICE> DoAnalyze(LatticeFunctions<LATTICE> functions, LATTICE initialState)
		{
			// Remove the first block from the work list since we know it's value.
			Unused.Value = m_workList.Remove(m_entry);
					
			// Initialize the input states for all of the blocks. For the first
			// block we are given the state. For the other blocks we use the 
			// top (zero) value.
			var inputStates = new Dictionary<BasicBlock, LATTICE>();
			inputStates.Add(m_entry, initialState);
			
			foreach (BasicBlock block in m_workList)
				inputStates.Add(block, functions.Top);
			
			// Initialize the output states for all of the blocks. 
			var outputStates = new Dictionary<BasicBlock, LATTICE>();
			outputStates.Add(m_entry, functions.Transform(initialState, m_entry));
			
			foreach (BasicBlock block in m_workList)
				outputStates.Add(block, functions.Top);

			// While we have blocks to process,
			while (m_workList.Count > 0)
			{
				// pop the first block,
				BasicBlock block = m_workList[0];
				m_workList.RemoveAt(0);
				
				Log.DebugLine(this, "processing block {0:S}", block);
				Log.Indent();
				
				// and for every predecessor,
				LATTICE totalEffect = functions.Top;
				foreach (BasicBlock pred in m_predecessors[block])
				{
					// meet the current state of our block with the output state 
					// of the predecessor block,
					totalEffect = functions.Meet(totalEffect, outputStates[pred]);
					Log.DebugLine(this, "meeting {0:S}, out state: {1}", pred, outputStates[pred]);
					Log.DebugLine(this, "meet state: {0}", totalEffect);
				}
					
				// and if the value changes queue it up again.
				if (functions.DiffersFrom(totalEffect, inputStates[block]))
				{
					Log.DebugLine(this, "queuing up the block again");

					inputStates[block] = totalEffect;
					outputStates[block] = functions.Transform(totalEffect, block);
					
					m_workList.Add(block);
				}
				Log.Unindent();
			}	
			
			return inputStates;
		}
				
		private void DoAddReachableToWorkList(BasicBlock block)
		{
			if (m_workList.IndexOf(block) < 0)
			{
				m_workList.Add(block);
								
				foreach (BasicBlock b in block.Next)
					DoAddReachableToWorkList(b);
				
				if (block.Finally != null)
					DoAddReachableToWorkList(block.Finally);
				else if (block.Fault != null)
					DoAddReachableToWorkList(block.Fault);
			}
		}
		
		private void DoAddPredecessors(BasicBlock pred, BasicBlock block)
		{
			List<BasicBlock> preds;
			if (!m_predecessors.TryGetValue(block, out preds))
			{
				preds = new List<BasicBlock>();
				m_predecessors[block] = preds;
			}
			
			if (preds.IndexOf(pred) < 0)
			{	
				if (pred != null)
					preds.Add(pred);
					
				if (block.Finally != null)
				{
					DoAddPredecessors(block, block.Finally);

					foreach (BasicBlock b in block.Next)
					{
						DoAddPredecessors(block.Finally, b);
					}
				}
				else if (block.Fault != null)
				{
					DoAddPredecessors(block, block.Fault);

					foreach (BasicBlock b in block.Next)
					{
						DoAddPredecessors(block.Fault, b);
					}
				}
				else
				{
					foreach (BasicBlock b in block.Next)
					{
						DoAddPredecessors(block, b);
					}
				}
			}
		}
						
#if LOG_BLOCKS
		[Conditional("DEBUG")]
		private void DoLogWorkList()
		{
			Log.DebugLine(this, "Blocks:");
			Log.Indent();
			foreach (BasicBlock b in m_workList)
			{
				Log.DebugLine(this, b.ToString());
			}
			Log.Unindent();
		}
		
		[Conditional("DEBUG")]
		private void DoLogPredecessors()
		{
			Log.DebugLine(this, "Predecessors:");
			Log.Indent();
			foreach (KeyValuePair<BasicBlock, List<BasicBlock>> entry in m_predecessors)
			{
				StringBuilder builder = new StringBuilder();
				
				builder.Append(entry.Key.ToString("S", null));
				builder.Append(" <= ");
				
				foreach (BasicBlock b in entry.Value)
				{
					builder.Append(b.ToString("S", null));
					builder.Append(' ');
				}
				
				Log.DebugLine(this, builder.ToString());
			}
			Log.Unindent();
		}
#endif
		#endregion
				
		#region Fields --------------------------------------------------------
		private BasicBlock m_entry;
		private List<BasicBlock> m_workList = new List<BasicBlock>();
		private Dictionary<BasicBlock, List<BasicBlock>> m_predecessors = new Dictionary<BasicBlock, List<BasicBlock>>();
		private bool m_skipped;
		#endregion
	}
}
#endif
