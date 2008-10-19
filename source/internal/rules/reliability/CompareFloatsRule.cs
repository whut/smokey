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
using Smokey.Framework;
using Smokey.Framework.Instructions;
using Smokey.Framework.Support;

#if OLD
namespace Smokey.Internal.Rules
{	
	internal sealed class CompareFloatsRule : Rule
	{				
		// This is inspired by the Gendarme rule authored by Lukasz Knop
		// although the code is completely different.
		public CompareFloatsRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "R1009")
		{
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitBegin");
			dispatcher.Register(this, "VisitCeq");
			dispatcher.Register(this, "VisitCall");
			dispatcher.Register(this, "VisitEnd");
		}
				
		public void VisitBegin(BeginMethod begin)
		{
			Log.DebugLine(this, "-----------------------------------"); 
			Log.DebugLine(this, "{0:F}", begin.Info.Instructions);				

			m_offset = -1;
			m_info = begin.Info;
		}
		
		public void VisitCeq(Ceq ceq)
		{
			if (m_offset < 0)
			{
				int j = m_info.Tracker.GetStackIndex(ceq.Index, 0);
				int k = m_info.Tracker.GetStackIndex(ceq.Index, 1);
				
				if (j >= 0 && k >= 0)
				{
					if (DoIsFloat(j) && DoIsFloat(k))
					{
						m_offset = ceq.Untyped.Offset;
						Log.DebugLine(this, "found compare at {0:X2}", m_offset);
					}
				}
			}
		}

		public void VisitCall(Call call)
		{
			if (m_offset < 0)
			{
				string name = call.Target.ToString();
				
				if (name == "System.Boolean System.Double::Equals(System.Double)" || name == "System.Boolean System.Single::Equals(System.Single)")
				{
					m_offset = call.Untyped.Offset;
					Log.DebugLine(this, "found equals at {0:X2}", m_offset);
				}
			}
		}

		public void VisitEnd(EndMethod end)
		{
			if (m_offset >= 0)
				Reporter.MethodFailed(end.Info.Method, CheckID, m_offset, string.Empty);
		}
				
		private bool DoIsFloat(int index)
		{			
			TypedInstruction instruction = m_info.Instructions[index];
			TypeReference type = null;

			do
			{
				LoadArg arg = instruction as LoadArg;
				if (arg != null && arg.Arg >= 1)
				{
					type = m_info.Method.Parameters[arg.Arg - 1].ParameterType;
					break;
				}

				LoadLocal local = instruction as LoadLocal;
				if (local != null)
				{
					type = m_info.Method.Body.Variables[local.Variable].VariableType;
					break;
				}

				LoadField field = instruction as LoadField;
				if (field != null)
				{
					type = field.Field.FieldType;
					break;
				}

				LoadStaticField sfield = instruction as LoadStaticField;
				if (sfield != null)
				{
					type = sfield.Field.FieldType;
					break;
				}

				LoadConstantFloat cf = instruction as LoadConstantFloat;
				if (cf != null)
				{
					return cf.Value != double.PositiveInfinity && cf.Value != double.NegativeInfinity;
				}
				
				BinaryOp binary = instruction as BinaryOp;
				if (binary != null)
				{
					int j = m_info.Tracker.GetStackIndex(instruction.Index, 0);
					int k = m_info.Tracker.GetStackIndex(instruction.Index, 1);
					
					if (j >= 0 && k >= 0)
						if (DoIsFloat(j) && DoIsFloat(k))
							return true;
				}
				
				if (instruction.Untyped.OpCode.Code == Code.Conv_R4 || instruction.Untyped.OpCode.Code == Code.Conv_R8 || instruction.Untyped.OpCode.Code == Code.Conv_R_Un)
					return true;

				Call call = instruction as Call;
				if (call != null)
				{
					type = call.Target.ReturnType.ReturnType;
					break;
				}
			}
			while (false);
			
			return type != null && (type.FullName == "System.Single" || type.FullName == "System.Double");
		}

		private MethodInfo m_info;
		private int m_offset;
	}
}
#endif
