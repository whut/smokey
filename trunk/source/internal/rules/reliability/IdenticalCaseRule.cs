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

namespace Smokey.Internal.Rules
{	
	internal class IdenticalCaseRule : Rule
	{				
		public IdenticalCaseRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "R1023")
		{
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitBegin");
			dispatcher.Register(this, "VisitSwitch");
			dispatcher.Register(this, "VisitEnd");
		}
				
		public void VisitBegin(BeginMethod method)
		{
			Log.DebugLine(this, "-----------------------------------"); 
			Log.DebugLine(this, "{0:F}", method.Info.Instructions);				

			m_offset = -1;
			m_info = method.Info;
		}

		// 08: switch     26 35 53 44 44 44 
		// 21: br         53
		// 26: ldstr      "zero"
		// 2B: call       System.Void System.Console::WriteLine(System.String)
		// 30: br         53
		// 35: ldstr      "zero"
		// 3A: call       System.Void System.Console::WriteLine(System.String)
		// 3F: br         53
		// 44: ldstr      "zero"
		// 49: call       System.Void System.Console::WriteLine(System.String)
		// 4E: br         53
		// 53: ret                    
		public void VisitSwitch(Switch swtch)
		{
			if (m_offset < 0)
			{
				UnconditionalBranch unconditional = m_info.Instructions[swtch.Index + 1] as UnconditionalBranch;
				if (unconditional != null && unconditional.Target.Index > unconditional.Index)
				{
					TypedInstruction last = unconditional.Target;
					
					List<TypedInstruction> cases = new List<TypedInstruction>();
					foreach (TypedInstruction ti in swtch.Targets)
					{
						if (ti.Index != last.Index)
							if (cases.IndexOf(ti) < 0)
								cases.Add(ti);
					}
					cases.Add(last);
					
					cases.Sort((lhs, rhs) => lhs.Index.CompareTo(rhs.Index));

					for (int i = 0; i < cases.Count - 1 && m_offset < 0; ++i)
					{
						TypedInstruction ti1 = cases[i];
						int len1 = cases[i + 1].Index - ti1.Index - 1;
			
						for (int j = i + 1; j < cases.Count - 1 && m_offset < 0; ++j)
						{
							TypedInstruction ti2 = cases[j];
							int len2 = cases[j + 1].Index - ti2.Index - 1;
							if (len1 == len2)
							{
								if (DoMatches(ti1.Index, ti2.Index, len1))
								{
									Log.DebugLine(this, "{0:X2} matches {1:X2}", ti1.Untyped.Offset, ti2.Untyped.Offset);
									m_offset = swtch.Untyped.Offset;
								}
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
			DBC.Assert(count >= 0, "count is negative ({0})", count);
			
			bool match = true;
			Log.DebugLine(this, "checking {0} instructions at {1:X2} and {2:X2}", count, m_info.Instructions[base1].Untyped.Offset, m_info.Instructions[base2].Untyped.Offset);
			
			for (int i = 0; i < count && match; ++i)
			{				
				Instruction i1 = m_info.Instructions[base1 + i].Untyped;
				Instruction i2 = m_info.Instructions[base2 + i].Untyped;
				
				if (!i1.Matches(i2))
				{
					match = false;
					Log.DebugLine(this, "   {0} != {1}", m_info.Instructions[base1 + i], m_info.Instructions[base2 + i]);
				}
			}
			
			if (match)
			 Log.DebugLine(this, "   matches");				
			
			return match;
		}

		private MethodInfo m_info;
		private int m_offset;
	}
}

