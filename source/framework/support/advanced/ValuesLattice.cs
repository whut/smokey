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
using System;
using System.Collections.Generic;
using System.Text;
using Smokey.Framework;
using Smokey.Framework.Instructions;

namespace Smokey.Framework.Support.Advanced
{	
	// Classes used with DataFlow to track the state of arguments, locals, and
	// the stack.
	namespace Values
	{			
		// Used to compute the state of arguments, locals, and the stack on 
		// entry and exit from BasicBlocks.
		internal sealed class Lattice
		{
			private Lattice()
			{
			}
			
			public Lattice(TypedInstructionCollection instructions, State state)
			{
				DBC.Pre(instructions != null, "instructions is null");

				m_instructions = instructions;
				m_state = state;
			}
			
			// This method is used by the Transform above and later on when we
			// need to compute the state for each instruction in a method.
			public Lattice Transform(int index)
			{
				DBC.Pre(index >= 0 && index < m_instructions.Length, "index is out of range");	// real code would probably use FastPre
					
				long?[] args = DoTransformArgs(index);
				long?[] locals = DoTransformLocals(index);
				List<StackEntry> stack = DoTransformStack(index);
				
				State state = new State(args, locals, stack);
				Log.DebugLine(this, "{0:X2}: {1}", m_instructions[index].Untyped.Offset, state);
				
				return new Lattice(m_instructions, state);	
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
			
			#region Private methods
			private long?[] DoTransformArgs(int index)
			{
				long?[] args = null;

				StoreArg store = m_instructions[index] as StoreArg;
				if (store != null)
				{
					long? newValue = null;
					if (m_state.Stack.Count > 0)			// may be zero if the stack hasn't been inited yet
						newValue = m_state.Stack.Back().Value;
										
					args = new long?[m_state.Arguments.Length];
					m_state.Arguments.CopyTo(args, 0);
					args[store.Argument] = newValue;
				}
				else if (m_instructions[index].Untyped.OpCode.Code == Code.Ldarga || m_instructions[index].Untyped.OpCode.Code == Code.Ldarga_S)
				{
					ParameterDefinition param = m_instructions[index].Untyped.Operand as ParameterDefinition;
					if (param != null)
					{
						int arg;
						if (m_instructions.Method.HasThis)
							arg = param.Sequence;
						else
							arg = param.Sequence - 1;
			
						args = new long?[m_state.Arguments.Length];
						m_state.Arguments.CopyTo(args, 0);
						args[arg] = null;
					}
				}
				else
					args = m_state.Arguments;

				return args;
			}

			private long?[] DoTransformLocals(int index)
			{
				long?[] locals = null;

				StoreLocal store = m_instructions[index] as StoreLocal;
				if (store != null)
				{					
					long? newValue = null;
					if (m_state.Stack.Count > 0)			// may be zero if the stack hasn't been inited yet
						newValue = m_state.Stack.Back().Value;
					
					locals = new long?[m_state.Locals.Length];
					m_state.Locals.CopyTo(locals, 0);
					locals[store.Variable] = newValue;
				}
				else if (m_instructions[index].Untyped.OpCode.Code == Code.Ldloca || m_instructions[index].Untyped.OpCode.Code == Code.Ldloca_S)
				{
					VariableDefinition param = m_instructions[index].Untyped.Operand as VariableDefinition;
					if (param != null)
					{						
						locals = new long?[m_state.Locals.Length];
						m_state.Locals.CopyTo(locals, 0);
						locals[param.Index] = null;
					}
				}
				else
					locals = m_state.Locals;
				
				return locals;
			}

			private List<StackEntry> DoTransformStack(int index)
			{
				List<StackEntry> stack = null;
				
				OpCode code = m_instructions[index].Untyped.OpCode;
				if (code.StackBehaviourPop != StackBehaviour.Pop0 || code.StackBehaviourPush != StackBehaviour.Push0)
				{
					stack = new List<StackEntry>(m_state.Stack);

					if (code.StackBehaviourPop != StackBehaviour.Pop0)
						DoStackPops(stack, index);
	
					if (code.StackBehaviourPush != StackBehaviour.Push0)
						DoStackPushes(stack, index, m_state.Stack);
				}
				else
				{
					stack = m_state.Stack;
				}
								
				return stack;
			}

			private void DoStackPops(List<StackEntry> stack, int index)
			{			
				int count = 0;
				
				TypedInstruction instruction = m_instructions[index];
				switch (instruction.Untyped.OpCode.StackBehaviourPop)
				{
					case StackBehaviour.Pop0:
						DBC.Fail("DoStackPops shouldn't have been called for Pop0");
						break;
						
					case StackBehaviour.Pop1:
					case StackBehaviour.Popi:
					case StackBehaviour.Popref:
						count = 1;
						break;
						
					case StackBehaviour.Pop1_pop1:
					case StackBehaviour.Popi_pop1:
					case StackBehaviour.Popi_popi:
					case StackBehaviour.Popi_popi8:
					case StackBehaviour.Popi_popr4:
					case StackBehaviour.Popi_popr8:
					case StackBehaviour.Popref_pop1:
					case StackBehaviour.Popref_popi:
						count = 2;
						break;
						
					case StackBehaviour.Popi_popi_popi:
					case StackBehaviour.Popref_popi_popi:
					case StackBehaviour.Popref_popi_popi8:
					case StackBehaviour.Popref_popi_popr4:
					case StackBehaviour.Popref_popi_popr8:
					case StackBehaviour.Popref_popi_popref:
						count = 3;
						break;
						
					case StackBehaviour.PopAll:				// leave
						count = stack.Count;
						break;
						
					case StackBehaviour.Varpop:				// call, newobj, ret
						Call call = instruction as Call;
						if (call != null)
						{
							count = call.Target.Parameters.Count + (call.Target.HasThis ? 1 : 0);
						}
						else if (instruction.Untyped.OpCode.Code == Code.Ret)
						{
							count = stack.Count;
						}
						else
						{
							NewObj no = instruction as NewObj;
							DBC.Assert(no != null, "Varpop opcode should be call, ret, or newobj");
							
							count = no.Ctor.Parameters.Count;
						}
						break;
					
					default:
						DBC.Fail("Bad pop state: {0}", instruction.Untyped.OpCode.StackBehaviourPop);
						break;
				}
				
				if (instruction.Untyped.OpCode.Code == Code.Ldelem_Any)
					count = 2;			// cecil has a bug where it says this pops 1 value
				
				// No assert for zero sized stack because we may not have met the block that gives us a stack.
				DBC.Assert(stack.Count == 0 || count <= stack.Count, "{0} at {1:X2} is trying to pop too many items", instruction.Untyped.OpCode.Code, instruction.Untyped.Offset);
				count = Math.Min(count, stack.Count);
				stack.RemoveRange(stack.Count - count, count);
			}

			private void DoStackPushes(List<StackEntry> stack, int index, List<StackEntry> oldStack)
			{		
				long? value;
				
				TypedInstruction instruction = m_instructions[index];
				switch (instruction.Untyped.OpCode.Code)
				{
					case Code.Add:
					case Code.Sub:
					case Code.Mul:
					case Code.Div:
					case Code.Div_Un:
					case Code.Rem:
					case Code.Rem_Un:
					case Code.And:
					case Code.Or:
					case Code.Xor:
					case Code.Shl:
					case Code.Shr:
					case Code.Shr_Un:
					case Code.Neg:
					case Code.Not:
					case Code.Add_Ovf:
					case Code.Add_Ovf_Un:
					case Code.Mul_Ovf:
					case Code.Mul_Ovf_Un:
					case Code.Sub_Ovf:
					case Code.Sub_Ovf_Un:
						stack.Add(new StackEntry(null, index));		// could compute these, but we don't care too much about integer values atm
						break;
			
					case Code.Arglist:			// push non-null
					case Code.Box:	
					case Code.Ldarga:
					case Code.Ldarga_S:
					case Code.Ldelema:
					case Code.Ldflda:
					case Code.Ldftn:
					case Code.Ldind_Ref:
					case Code.Ldloca:
					case Code.Ldloca_S:
					case Code.Ldobj:
					case Code.Ldsflda:
					case Code.Ldstr:
					case Code.Ldtoken:
					case Code.Ldvirtftn:
					case Code.Localloc:
					case Code.Mkrefany:
					case Code.Newarr:
					case Code.Newobj:
					case Code.Refanytype:
					case Code.Refanyval:
					case Code.Unbox:
					case Code.Unbox_Any:
						stack.Add(new StackEntry(1, index));
						break;
			
					case Code.Call:
					case Code.Calli:
					case Code.Callvirt:
						Call call = instruction as Call;
						if (call.Target.ReturnType.ReturnType.ToString() != "System.Void")
						{
							stack.Add(new StackEntry(null, index));
						}
						break;
						
					case Code.Castclass:
						value = oldStack.Back().Value;
						if (value.HasValue && value.Value == 0)
							stack.Add(new StackEntry(0, index));
						else
							stack.Add(new StackEntry(1, index));
						break;
			
					case Code.Ceq:			// push indeterminate
					case Code.Cgt:
					case Code.Cgt_Un:
					case Code.Clt:
					case Code.Clt_Un:
					case Code.Ckfinite:
					case Code.Conv_I1:
					case Code.Conv_I2:
					case Code.Conv_I4:
					case Code.Conv_I8:
					case Code.Conv_U4:
					case Code.Conv_U8:
					case Code.Conv_R4:		
					case Code.Conv_R8:
					case Code.Conv_U2:
					case Code.Conv_U1:
					case Code.Conv_I:
					case Code.Conv_R_Un:
					case Code.Conv_U:
					case Code.Ldelem_I1:
					case Code.Ldelem_U1:
					case Code.Ldelem_I2:
					case Code.Ldelem_U2:
					case Code.Ldelem_I4:
					case Code.Ldelem_U4:
					case Code.Ldelem_I8:
					case Code.Ldelem_I:
					case Code.Ldelem_R4:
					case Code.Ldelem_R8:
					case Code.Ldelem_Ref:
					case Code.Ldelem_Any:
					case Code.Ldind_I1:
					case Code.Ldind_U1:
					case Code.Ldind_I2:
					case Code.Ldind_U2:
					case Code.Ldind_I4:
					case Code.Ldind_U4:
					case Code.Ldind_I8:
					case Code.Ldind_I:
					case Code.Ldind_R4:
					case Code.Ldind_R8:
					case Code.Ldfld:
					case Code.Ldsfld:
					case Code.Ldlen:
					case Code.Sizeof:
						stack.Add(new StackEntry(null, index));
						break;
			
					case Code.Conv_Ovf_I1:		// push previous value
					case Code.Conv_Ovf_U1:
					case Code.Conv_Ovf_I2:
					case Code.Conv_Ovf_U2:
					case Code.Conv_Ovf_I4:
					case Code.Conv_Ovf_U4:
					case Code.Conv_Ovf_I8:
					case Code.Conv_Ovf_U8:
					case Code.Conv_Ovf_I:
					case Code.Conv_Ovf_U:
					case Code.Conv_Ovf_I1_Un:
					case Code.Conv_Ovf_I2_Un:
					case Code.Conv_Ovf_I4_Un:
					case Code.Conv_Ovf_I8_Un:
					case Code.Conv_Ovf_U1_Un:
					case Code.Conv_Ovf_U2_Un:
					case Code.Conv_Ovf_U4_Un:
					case Code.Conv_Ovf_U8_Un:
					case Code.Conv_Ovf_I_Un:
					case Code.Conv_Ovf_U_Un:
						value = oldStack.Back().Value;
						stack.Add(new StackEntry(value, index));
						break;
			
					case Code.Dup:
						value = oldStack.Back().Value;
						stack.Add(new StackEntry(value, index));
						stack.Add(new StackEntry(value, index));
						break;
					
					case Code.Isinst:
						value = oldStack.Back().Value;
						if (value.HasValue && value.Value == 0)
							stack.Add(new StackEntry(0, index));
						else
							stack.Add(new StackEntry(null, index));
						break;
			
					case Code.Ldarg_0:
					case Code.Ldarg_1:
					case Code.Ldarg_2:
					case Code.Ldarg_3:
					case Code.Ldarg:
					case Code.Ldarg_S:
						LoadArg arg = instruction as LoadArg;
						value = m_state.Arguments[arg.Arg];
						stack.Add(new StackEntry(value, index));
						break;
					
					case Code.Ldc_I4_M1:
					case Code.Ldc_I4_0:
					case Code.Ldc_I4_1:
					case Code.Ldc_I4_2:
					case Code.Ldc_I4_3:
					case Code.Ldc_I4_4:
					case Code.Ldc_I4_5:
					case Code.Ldc_I4_6:
					case Code.Ldc_I4_7:
					case Code.Ldc_I4_8:
					case Code.Ldc_I4_S:
					case Code.Ldc_I4:
					case Code.Ldc_I8:
						LoadConstantInt constant = instruction as LoadConstantInt;
						stack.Add(new StackEntry(constant.Value, index));
						break;
			
					case Code.Ldc_R4:			
					case Code.Ldc_R8:
						LoadConstantFloat constant2 = instruction as LoadConstantFloat;
						if (constant2.Value == 0.0)
							stack.Add(new StackEntry(0, index));
						else
							stack.Add(new StackEntry(null, index));
						break;

					case Code.Ldloc_0:
					case Code.Ldloc_1:
					case Code.Ldloc_2:
					case Code.Ldloc_3:
					case Code.Ldloc:
					case Code.Ldloc_S:
						LoadLocal local = instruction as LoadLocal;
						value = m_state.Locals[local.Variable];
						stack.Add(new StackEntry(value, index));
						break;
			
					case Code.Ldnull:					// push null
						stack.Add(new StackEntry(0, index));
						break;
			
					default:
						DBC.Assert(instruction.Untyped.OpCode.StackBehaviourPush == StackBehaviour.Push0, "Expected {0} to push nothing", instruction.Untyped.OpCode);
						break;
				}
			}
			#endregion
							
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
					// indeterminate if one side is indeterminate or if the 
					// values differ.
					long?[] args = DoMeetVars(lhs.m_state.Arguments, rhs.m_state.Arguments);
					long?[] locals = DoMeetVars(lhs.m_state.Locals, rhs.m_state.Locals);
					List<StackEntry> stack = DoMeetStack(lhs.m_state.Stack, rhs.m_state.Stack);
					
					State state = new State(args, locals, stack);
					return new Lattice(lhs.m_instructions, state);	
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
						if (block.Length > 0)
						{
							for (int i = 0; i < block.Length; ++i)
							{
								result = result.Transform(block.First.Index + i);
							}
						}
						else
						{
							long?[] args = lhs.m_state.Arguments;
							long?[] locals = lhs.m_state.Locals;
							List<StackEntry> stack = lhs.m_state.Stack;
	
							if (block.Length == Tracker.CatchBlockLen || block.Length == Tracker.FinallyBlockLen)
							{
								stack = new List<StackEntry>();
								if (block.Length == Tracker.CatchBlockLen)
									stack.Add(new StackEntry(1, -1));		// catch blocks start with the exception on the stack
							}
							else if (block.Length <= Tracker.FirstNullArgBlockLen && block.Length >= Tracker.LastNullArgBlockLen)
							{	
								args = new long?[lhs.m_state.Arguments.Length];
								lhs.m_state.Arguments.CopyTo(args, 0);

								int nth = Tracker.FirstNullArgBlockLen - block.Length;
								args[nth] = 0;
							}
							else if (block.Length <= Tracker.FirstNonNullArgBlockLen && block.Length >= Tracker.LastNonNullArgBlockLen)
							{
								args = new long?[lhs.m_state.Arguments.Length];
								lhs.m_state.Arguments.CopyTo(args, 0);

								int nth = Tracker.FirstNonNullArgBlockLen - block.Length;
								args[nth] = 1;
							}
							else if (block.Length <= Tracker.FirstNullLocalBlockLen && block.Length >= Tracker.LastNullLocalBlockLen)
							{
								locals = new long?[lhs.m_state.Locals.Length];
								lhs.m_state.Locals.CopyTo(locals, 0);

								int nth = Tracker.FirstNullLocalBlockLen - block.Length;
								locals[nth] = 0;
							}
							else if (block.Length <= Tracker.FirstNonNullLocalBlockLen && block.Length >= Tracker.LastNonNullLocalBlockLen)
							{
								locals = new long?[lhs.m_state.Locals.Length];
								lhs.m_state.Locals.CopyTo(locals, 0);

								int nth = Tracker.FirstNonNullLocalBlockLen - block.Length;
								locals[nth] = 1;
							}
							else if (block.Length != 0)
								DBC.Fail("bad block length: {0}", block.Length);
								
							State state = new State(args, locals, stack);
							return new Lattice(lhs.m_instructions, state);	
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
				
				#region Private methods
				private static long?[] DoMeetVars(long?[] lhs, long?[] rhs)
				{
					DBC.Assert(lhs.Length == rhs.Length, "lengths don't match");
					
					long?[] result = new long?[lhs.Length];
					
					for (int i = 0; i < lhs.Length; ++i)
						result[i] = DoMeet(lhs[i], rhs[i]);
					
					return result;
				}
				
				// Unfortunately the stack depths don't always match so we'll
				// merge the top of the stack and set the bottom to indeterminate.
				private static List<StackEntry> DoMeetStack(List<StackEntry> lhs, List<StackEntry> rhs)
				{
					List<StackEntry> result = null;

					if (lhs.Count == rhs.Count)
					{
						result = new List<StackEntry>(lhs.Count);

						for (int i = 0; i < lhs.Count; ++i)
							result.Add(new StackEntry(DoMeet(lhs[i].Value, rhs[i].Value), -1));
					}
					else if (lhs.Count > rhs.Count)
					{
						result = new List<StackEntry>(lhs.Count);

						int delta = lhs.Count - rhs.Count;
						for (int i = 0; i < delta; ++i)
							result.Add(new StackEntry(null, -1));

						for (int i = delta; i < lhs.Count; ++i)
							result.Add(new StackEntry(DoMeet(lhs[i].Value, rhs[i - delta].Value), -1));
					}
					else
					{
						result = new List<StackEntry>(rhs.Count);

						int delta = rhs.Count - lhs.Count;
						for (int i = 0; i < delta; ++i)
							result.Add(new StackEntry(null, -1));

						for (int i = delta; i < lhs.Count; ++i)
							result.Add(new StackEntry(DoMeet(lhs[i - delta].Value, rhs[i].Value), -1));
					}
					
					return result;
				}

				private static long? DoMeet(long? lhs, long? rhs)
				{
					// Preserve zero values because if a value may be null we want to use it
					// so we catch more potential failures. TODO: this is a good idea but it
					// results int too many false positives, for example:
					// string s = null;
					// if (flag)
					//    s = "hey";
					// ...
					// if (flag)
					//    flag = s.Length > 0;
//					if ((lhs.HasValue && lhs.Value == 0) || (rhs.HasValue && rhs.Value == 0))
//						return 0;
						
					if (!lhs.HasValue || !rhs.HasValue)
						return null;
										
					else if (lhs.Value == rhs.Value)
						return lhs;
					
					return null;	
				}		
				#endregion
				
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
	}
}

