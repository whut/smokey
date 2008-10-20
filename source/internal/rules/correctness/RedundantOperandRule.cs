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
using System.Diagnostics;
using Smokey.Framework;
using Smokey.Framework.Instructions;
using Smokey.Framework.Support;

namespace Smokey.Internal.Rules
{	
	internal sealed class RedundantOperandRule : Rule
	{				
		public RedundantOperandRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "C1037")
		{
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitBegin");
			dispatcher.Register(this, "VisitBeginMethod");
			dispatcher.Register(this, "VisitBinaryOp");
			dispatcher.Register(this, "VisitCall");
			dispatcher.Register(this, "VisitCompare");
			dispatcher.Register(this, "VisitEndMethod");
		}
		
		public void VisitBegin(BeginMethods begin)
		{
			Log.DebugLine(this, "-----------------------------------"); 
			Log.DebugLine(this, "{0}", begin.Type);				
		}
		
		public void VisitBeginMethod(BeginMethod begin)
		{
			Log.DebugLine(this, "{0:F}", begin.Info.Instructions);				

			m_info = begin.Info;
			m_offset = -1;
		}
		
		public void VisitBinaryOp(BinaryOp op)
		{
			if (m_offset < 0)
			{
				switch (op.Untyped.OpCode.Code)
				{
					case Code.Div:
					case Code.Div_Un:
					case Code.Rem:
					case Code.Rem_Un:
					case Code.Sub:
					case Code.Sub_Ovf:
					case Code.Sub_Ovf_Un:
					case Code.And:
					case Code.Xor:
					case Code.Or:
						DoCheck(op);
						break;
				}
			}
		}
				
		public void VisitCall(Call call)
		{
			if (m_offset < 0)
			{
				if (call.Target.Parameters.Count == 2)
					if (call.Target.ToString().IndexOf("System.Math::Min(") >= 0 ||
						call.Target.ToString().IndexOf("System.Math::Max(") >= 0 ||
						call.Target.ToString().IndexOf("System.Object::Equals(") >= 0 ||
						call.Target.ToString().IndexOf("System.Object::ReferenceEquals(") >= 0)
						DoCheck(call);
			}
		}
		
		public void VisitCompare(Compare op)
		{
			// Compiler sometimes generates this code to return true.
			if (op.Index >= 2)
			{
				// ldc.i4.0 
				// ldc.i4.0 
				// ceq 
				LoadConstantInt rhs = m_info.Instructions[op.Index - 1] as LoadConstantInt;
				LoadConstantInt lhs = m_info.Instructions[op.Index - 2] as LoadConstantInt;
				if (lhs != null && rhs != null && lhs.Value == 0 && rhs.Value == 0)
					return;
			}
			
			if (m_offset < 0)
				DoCheck(op);
		}
		
		public void VisitEndMethod(EndMethod end)
		{
			if (m_offset >= 0)
			{
				Log.DebugLine(this, "identical operands at {0:X}", m_offset);
				Reporter.MethodFailed(end.Info.Method, CheckID, m_offset, string.Empty);
			}
		}	
		
		private void DoCheck(TypedInstruction binary)
		{
			if (binary.Index >= 2)
			{
				int rhsEnd = binary.Index - 1;
				int rhsStart = rhsEnd - DoGetPushRange(m_info, rhsEnd);

				int lhsEnd = rhsStart - 1;
				int lhsStart = lhsEnd - DoGetPushRange(m_info, lhsEnd);
			
				Log.DebugLine(this, "{0} at {1:X}", binary.Untyped.OpCode.Code, binary.Untyped.Offset);
				Log.DebugLine(this, "   lhs range: [{0:X}, {1:X}]", m_info.Instructions[lhsStart].Untyped.Offset, m_info.Instructions[lhsEnd].Untyped.Offset);
				Log.DebugLine(this, "   rhs range: [{0:X}, {1:X}]", m_info.Instructions[rhsStart].Untyped.Offset, m_info.Instructions[rhsEnd].Untyped.Offset);
				
				bool matches = (rhsEnd - rhsStart) == (lhsEnd - lhsStart);
				for (int i = 0; i <= (rhsEnd - rhsStart) && matches; ++i)
				{
					TypedInstruction lhs = m_info.Instructions[lhsStart + i];
					TypedInstruction rhs = m_info.Instructions[rhsStart + i];
					
					matches = lhs.Untyped.Matches(rhs.Untyped);
				}

				if (matches)
				{
					m_offset = binary.Untyped.Offset;
				}
			}
		}
		
		// Returns the smallest number k such that the instructions in
		// [offset - k, offset] push a single value onto the stack.
		private int DoGetPushRange(MethodInfo info, int offset)
		{
			int delta = DoGetStackDelta(info.Instructions[offset]);
			
			int k = 0;
			while (delta <= 0 && offset - k >= 0)
			{
				++k;
				delta += DoGetStackDelta(info.Instructions[offset - k]);
			}
			
			DBC.Assert(offset - k >= 0, "couldn't find the range");
			
			return k;
		}
		
		private int DoGetStackDelta(TypedInstruction instruction)
		{
			int delta = 0;
			
			delta += DoGetStackCount(instruction, instruction.Untyped.OpCode.StackBehaviourPush);
			delta -= DoGetStackCount(instruction, instruction.Untyped.OpCode.StackBehaviourPop);
//			Log.DebugLine(this, "{0} (delta = {1})", instruction, delta);
						
			return delta;
		}
		
		private int DoGetStackCount(TypedInstruction instruction, StackBehaviour behavior)
		{
			int count = 0;
			
			switch (behavior)
			{
				case StackBehaviour.Pop0:
				case StackBehaviour.Push0:
					break;
					
				case StackBehaviour.Pop1:
				case StackBehaviour.Popi:
				case StackBehaviour.Popref:
				case StackBehaviour.Push1:
				case StackBehaviour.Pushi:
				case StackBehaviour.Pushi8:
				case StackBehaviour.Pushr4:
				case StackBehaviour.Pushr8:
				case StackBehaviour.Pushref:
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
				case StackBehaviour.Push1_push1:
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
					count = int.MaxValue;
					break;
					
				case StackBehaviour.Varpop:				// call, newobj, ret
					Call call = instruction as Call;
					if (call != null)
					{
						count = call.Target.Parameters.Count + (call.Target.HasThis ? 1 : 0);
					}
					else if (instruction.Untyped.OpCode.Code == Code.Ret)
					{
						count = int.MaxValue;
					}
					else
					{
						NewObj no = instruction as NewObj;
						DBC.Assert(no != null, "Varpop opcode should be call, ret, or newobj");
						
						count = no.Ctor.Parameters.Count;
					}
					break;
				
				case StackBehaviour.Varpush:			// call
					Call call2 = instruction as Call;
					DBC.Assert(call2 != null, "Varpush opcode should be call");
					if (call2.Target.ReturnType.ReturnType.FullName != "System.Void")
						count = 1;
					break;

				default:
					DBC.Fail("Bad stack behavior: {0}", behavior);
					break;
			}
						
			return count;
		}
		
		private MethodInfo m_info;
		private int m_offset;
	}
}
