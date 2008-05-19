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

using System;
using System.Collections.Generic;
using Smokey.Framework;
using Smokey.Framework.Instructions;
using Smokey.Framework.Support.Advanced;

namespace Smokey.Tests
{	
	// Used by DataFlowTest to track changes to V_0.
	internal sealed class V0
	{	
		// State of V_O at a particular point in the method.
		internal struct State	
		{
			public State(long? value)
			{
				m_value = value;
			}
			
			public bool HasValue	
			{
				get {return m_value.HasValue;}
			}
						
			public long Value	
			{
				get 
				{
					DBC.Pre(m_value.HasValue, "V_0 has no value");
					return m_value.Value;
				}
			}
							
			public override string ToString()
			{
				if (m_value.HasValue)
					return "V_0 = " + m_value.Value;
				else
					return "V_0 = indeterminate";
			}

			#region Equality
			public override bool Equals(object rhsObj)
			{
				if (rhsObj == null)					
					return false;
				
				if (GetType() != rhsObj.GetType()) 
					return false;
			
				State rhs = (State) rhsObj;					
				return this == rhs;
			}
				
			public bool Equals(State rhs)
			{					
				return this == rhs;
			}

			public static bool operator==(State lhs, State rhs)
			{
				if (lhs.m_value.HasValue && rhs.m_value.HasValue)
					return lhs.m_value.Value == rhs.m_value.Value;

				else if (!lhs.m_value.HasValue && !rhs.m_value.HasValue)
					return true;
				
				return false;
			}
			
			public static bool operator!=(State lhs, State rhs)
			{
				return !(lhs == rhs);
			}
			
			public override int GetHashCode()
			{
				return m_value.GetHashCode();
			}
			#endregion
			
			private readonly long? m_value;
		}
		
		// Used to compute the state of V_0 on entry and exit from BasicBlocks.
		internal sealed class Lattice
		{
			private Lattice()
			{
			}
			
			public Lattice(TypedInstructionCollection instructions, State state)
			{
				m_instructions = instructions;
				m_state = state;
			}
			
			// This method is used by the Transform above and later on when we
			// need to compute the state for each instruction in a method.
			public Lattice Transform(int index)
			{
				DBC.Pre(index >= 0 && index < m_instructions.Length, "index is oor");	// real code would probably use FastPre
	
				StoreLocal store = m_instructions[index] as StoreLocal;
				if (store != null)
				{
					LoadConstantInt load = m_instructions[index - 1] as LoadConstantInt;	// test code so we only track constant loads
					if (load != null)
						return new Lattice(m_instructions, new State(load.Value));	
					else
						return new Lattice(m_instructions, Indeterminate);	
				}
				
				return this;
			}
						
			public State State 
			{
				get {return m_state;}
			}
			
			public bool IsTop
			{
				get {return m_instructions == null;}
			}
			
			public override string ToString()
			{
				return m_state.ToString();
			}
							
			public static readonly State Indeterminate = new State();

			#region Functions class
			internal sealed class Functions : LatticeFunctions<Lattice>
			{
				public override Lattice Meet(Lattice lhs, Lattice rhs)
				{
					DBC.Pre(lhs != null, "lhs is null");
					DBC.Pre(rhs != null, "rhs is null");
					
					// If one side is top then return the other side.
					if (lhs.IsTop)
						return rhs;	
					else if (rhs.IsTop)
						return lhs;
						
					// Otherwise we have to meet the two sides.	The result is
					// indeterminate if one side is indeterminate or if the values
					// differ.
					if (!lhs.m_state.HasValue && !rhs.m_state.HasValue)	// strictly speaking this case is not necessary but it saves us an allocation
						return lhs;
						
					else if (!lhs.m_state.HasValue || !rhs.m_state.HasValue)
						return new Lattice(lhs.m_instructions, Indeterminate);
										
					else if (lhs.m_state.Value == rhs.m_state.Value)
						return lhs;
					
					return new Lattice(lhs.m_instructions, Indeterminate);	
				}
		
				public override Lattice Top
				{
					get {return ms_top;}
				}
		
				public override Lattice Transform(Lattice lhs, BasicBlock block)
				{
					DBC.Pre(lhs != null, "lhs is null");
					DBC.Pre(block != null, "block is null");
		
					Lattice result = lhs;
					if (!lhs.IsTop)
					{
						for (int i = 0; i < block.Length; ++i)
						{
							result = result.Transform(block.First.Index + i);
						}
					}
					
					return result;
				}
		
				public override bool DiffersFrom(Lattice lhs, Lattice rhs)
				{
					DBC.Pre(lhs != null, "lhs is null");
					DBC.Pre(rhs != null, "rhs is null");
						
					if (lhs.IsTop || rhs.IsTop)			// if either side is indeterminate then we cant say the two sides are equal
						return true;
					else					
						return lhs.m_state != rhs.m_state;
				}
				
				#region Fields
				private static Lattice ms_top = new Lattice();
				#endregion
			}
			#endregion

			#region Fields
			private readonly TypedInstructionCollection m_instructions;	// top if m_instructions == null
			private readonly State m_state;						// valid only if !m_top
			#endregion
		}

		// Returns the state of V_0 for every instruction in the method.
		internal sealed class Tracker
		{
			public Tracker(TypedInstructionCollection instructions)
			{
				var graph = new ControlFlowGraph(instructions);			
				var initialState = new Lattice(instructions, new State(0));
		
				var data = new DataFlow<Lattice>(instructions, graph.Roots);
				Dictionary<BasicBlock, Lattice> lattices = data.Analyze(new Lattice.Functions(), initialState);
				
				m_states = new State[instructions.Length];
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
				
				for (int index = 0; index < instructions.Length; ++index)
					Log.DebugLine(this, "{0:X2}: {1}", instructions[index].Untyped.Offset, m_states[index]);
			}
			
			//- Returns the state of V_0 before the instruction at index executes.
			public State State(int index)
			{
				DBC.Pre(index >= 0 && index < m_states.Length, "index oor");
				return m_states[index];
			}
									
			#region Fields
			private State[] m_states;
			#endregion
		}
	}
}

