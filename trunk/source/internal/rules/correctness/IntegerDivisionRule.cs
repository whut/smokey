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
	internal class IntegerDivisionRule : Rule
	{				
		public IntegerDivisionRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "C1009")
		{
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitBegin");
			dispatcher.Register(this, "VisitConvert");
			dispatcher.Register(this, "VisitEnd");
		}
				
		public void VisitBegin(BeginMethod begin)
		{
			Log.DebugLine(this, "-----------------------------------"); 
			Log.DebugLine(this, "{0:F}", begin.Info.Instructions);				

			m_offset = -1;
			m_info = begin.Info;
		}
		
		public void VisitConvert(Conv convert)
		{
			if (m_offset < 0 && convert.Index >= 3)
			{
				switch (convert.Untyped.OpCode.Code)
				{
					case Code.Conv_R4:
					case Code.Conv_R8:
					case Code.Conv_R_Un:
						OpCode prev = m_info.Instructions[convert.Index - 1].Untyped.OpCode;
						if (prev.Code == Code.Div || prev.Code == Code.Div_Un)
						{
							if (IntegerHelpers.IsIntOperand(m_info, convert.Index - 1, 0) && IntegerHelpers.IsIntOperand(m_info, convert.Index - 1, 1))
							{
								m_offset = convert.Untyped.Offset;						
								Log.DebugLine(this, "bad convert at {0:X2}", m_offset);											
							}
						}
						break;	
				}
			}
		}

		public void VisitEnd(EndMethod end)
		{
			if (m_offset >= 0)
			{
				Reporter.MethodFailed(end.Info.Method, CheckID, m_offset, string.Empty);
			}
		}
				
		private int m_offset;
		private MethodInfo m_info;
	}
}
