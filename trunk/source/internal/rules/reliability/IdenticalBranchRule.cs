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
	internal sealed class IdenticalBranchRule : Rule
	{				
		public IdenticalBranchRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "R1022")
		{
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitBegin");
			dispatcher.Register(this, "VisitBranch");
			dispatcher.Register(this, "VisitEnd");
		}
				
		public void VisitBegin(BeginMethod method)
		{
			Log.DebugLine(this, "-----------------------------------"); 
			Log.DebugLine(this, "{0:F}", method.Info.Instructions);				

			m_offset = -1;
			m_info = method.Info;
		}

		// 10: brfalse    14
		// 11: ldstr      "pos"
		// 12: call       System.Void System.Console::WriteLine(System.String)
		// 13: br         16
		// 
		// 14: ldstr      "pos"
		// 15: call       System.Void System.Console::WriteLine(System.String)
		// 16: ret 
		public void VisitBranch(ConditionalBranch conditional)
		{
			if (m_offset < 0)
			{
				// If it's a forward conditional branch,
				if (conditional.Target.Index > conditional.Index)		
				{
					// and the not-taken case ends with a forward unconditional branch,
					UnconditionalBranch unconditional = m_info.Instructions[conditional.Target.Index - 1] as UnconditionalBranch;
					if (unconditional != null && unconditional.Target.Index > unconditional.Index)
					{
						// then we have a contruct that resembles an if statement so we'll
						// compare the code in the two branches.
						int num1 = unconditional.Index - conditional.Index - 1;
						int num2 = unconditional.Target.Index - conditional.Target.Index;
						Log.DebugLine(this, "not taken at {0:X2} has {1} instructions, taken has {2} instructions", conditional.Untyped.Offset, num1, num2);				
						
						if (num1 == num2 && conditional.Target.Index + num2 <= m_info.Instructions.Length)
						{
							if (DoMatches(conditional.Index + 1, conditional.Target.Index, num1))
							{
								Log.DebugLine(this, "matches");
								m_offset = conditional.Untyped.Offset;
							}
						}
					}
				}
			}
		}

		public void VisitEnd(EndMethod method)
		{
			if (m_offset >= 0)
				Reporter.MethodFailed(method.Info.Method, CheckID, m_offset, string.Empty);
		}
		
		private bool DoMatches(int base1, int base2, int count)
		{			
			bool match = true;
			
			for (int i = 0; i < count && match; ++i)
			{				
				Instruction i1 = m_info.Instructions[base1 + i].Untyped;
				Instruction i2 = m_info.Instructions[base2 + i].Untyped;
				
				if (!i1.Matches(i2))
				{
					match = false;
					Log.DebugLine(this, "{0} != {1}", m_info.Instructions[base1 + i], m_info.Instructions[base2 + i]);
				}
			}
			
			return match;
		}

		private MethodInfo m_info;
		private int m_offset;
	}
}
#endif
