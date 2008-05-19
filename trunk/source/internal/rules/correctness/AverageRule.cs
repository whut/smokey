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
	internal sealed class AverageRule : Rule
	{				
		public AverageRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "C1011")
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
		
		// 00: ldarg.0    a
		// 01: ldarg.1    b
		// 02: add.ovf    
		// 03: ldc.i4.2   
		// 04: div 
		public void VisitBinary(BinaryOp binary)	
		{
			do
			{
				if (m_offset >= 0)
					break;
					
				int i = binary.Index;
				if (i < 4)
					break;

				if (!DoMatch(i, Code.Div, Code.Div_Un) || !DoMatch(i - 1, Code.Ldc_I4_2))
					if (!DoMatch(i, Code.Shr, Code.Shr_Un) || !DoMatch(i - 1, Code.Ldc_I4_1))
						break;

				if (!DoMatch(i - 2, Code.Add, Code.Add_Ovf))
					break;

				if (!IntegerHelpers.IsIntOperand(m_info, i - 2, 0) || !IntegerHelpers.IsIntOperand(m_info, i - 2, 1))
					break;

				m_offset = binary.Untyped.Offset;						
			}
			while (false);
		}

		public void VisitEnd(EndMethod end)
		{
			if (m_offset >= 0)
			{
				Reporter.MethodFailed(end.Info.Method, CheckID, m_offset, string.Empty);
			}
		}
		
		private bool DoMatch(int i, Code c1)
		{
			Code code = m_info.Instructions[i].Untyped.OpCode.Code;
			return code == c1;
		}
		
		private bool DoMatch(int i, Code c1, Code c2)
		{
			Code code = m_info.Instructions[i].Untyped.OpCode.Code;
			return code == c1 || code == c2;
		}
				
		private int m_offset;
		private MethodInfo m_info;
	}
}
