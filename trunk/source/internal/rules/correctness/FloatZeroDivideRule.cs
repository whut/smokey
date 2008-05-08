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
using Smokey.Framework.Support.Advanced.Values;

namespace Smokey.Internal.Rules
{	
	internal class FloatZeroDivideRule : Rule
	{				
		public FloatZeroDivideRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "C1025")
		{
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitBegin");
			dispatcher.Register(this, "VisitBinary");
			dispatcher.Register(this, "VisitEnd");
		}
				
		public void VisitBegin(BeginMethod begin)
		{
			Log.DebugLine(this, "-----------------------------------"); 
			Log.DebugLine(this, "{0:F}", begin.Info.Instructions);				

			m_offset = -1;
			m_info = begin.Info;
		}
		
		public void VisitBinary(BinaryOp op)	
		{
			if (m_offset < 0 && (op.Untyped.OpCode.Code == Code.Div || op.Untyped.OpCode.Code == Code.Div_Un))
			{
				do
				{
					LoadConstantFloat load1 = m_info.Instructions[op.Index - 1] as LoadConstantFloat;
					if (load1 != null)
					{
						if (load1.Value == 0.0)
						{
							m_offset = op.Untyped.Offset;						
							Log.DebugLine(this, "zero constant denominator at {0:X2}", m_offset);											
						}
						break;
					}

					LoadLocal load2 = m_info.Instructions[op.Index - 1] as LoadLocal;
					if (load2 != null)
					{
						if (DoIsFloatType(load2.Type))
						{
							State state = m_info.Tracker.State(op.Index - 1);
							long? value = state.Locals[load2.Variable];
							if (value.HasValue && value.Value == 0)
							{
								m_offset = op.Untyped.Offset;						
								Log.DebugLine(this, "zero local denominator at {0:X2}", m_offset);											
							}
						}
						break;
					}
					
					LoadArg load3 = m_info.Instructions[op.Index - 1] as LoadArg;
					if (load3 != null)
					{
						if (DoIsFloatType(load3.Type))
						{
							State state2 = m_info.Tracker.State(op.Index - 1);
							long? value2 = state2.Arguments[m_info.Method.HasThis ? load3.Arg : load3.Arg-1];
							if (value2.HasValue && value2.Value == 0)
							{
								m_offset = op.Untyped.Offset;						
								Log.DebugLine(this, "zero arg denominator at {0:X2}", m_offset);											
							}
						}
						break;
					}
				}
				while (false);
			}
		}

		public void VisitEnd(EndMethod end)
		{
			if (m_offset >= 0)
			{
				Reporter.MethodFailed(end.Info.Method, CheckID, m_offset, string.Empty);
			}
		}
		
		private bool DoIsFloatType(TypeReference type)
		{
			switch (type.FullName)
			{
				case "System.Single":
				case "System.Double":
					return true;
			}
			
			return false;
		}
				
		private int m_offset;
		private MethodInfo m_info;
	}
}
