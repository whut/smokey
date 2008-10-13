// Copyright (C) 2008 Jesse Jones
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
	internal sealed class UnusualMonitor1Rule : Rule
	{				
		public UnusualMonitor1Rule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "R1027")
		{
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitBegin");
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

		public void VisitCall(Call call)
		{
			if (m_offset < 0 && call.Target.ToString() == "System.Boolean System.Threading.Monitor::Wait(System.Object)")
			{
				int conditionalStart = -1;
				int conditionalEnd = -1;
				
				if (DoMatch1a(call.Index + 2))
				{
					Log.DebugLine(this, "matched loop with ignore");
					
					conditionalStart = call.Index + 3;
					conditionalEnd = DoFindBranch(conditionalStart, call.Index - 2);
				}			
				else if (DoMatch1b(call.Index + 1))
				{
					Log.DebugLine(this, "matched loop with pop");

					conditionalStart = call.Index + 2;
					conditionalEnd = DoFindBranch(conditionalStart, call.Index - 2);
				}
				else if (DoMatch2a(call.Index + 2))
				{
					Log.DebugLine(this, "matched static loop with ignore");

					conditionalStart = call.Index + 3;
					conditionalEnd = DoFindBranch(conditionalStart, call.Index - 1);
				}
				else if (DoMatch2b(call.Index + 1))
				{
					Log.DebugLine(this, "matched static loop with pop");

					conditionalStart = call.Index + 2;
					conditionalEnd = DoFindBranch(conditionalStart, call.Index - 1);
				}
				else
				{
					m_offset = call.Untyped.Offset;
					Log.DebugLine(this, "   call at {0:X2} doesn't match the pattern", m_offset);
				}
				
				if (m_offset < 0)
				{
					if (conditionalEnd < 0)
					{
						m_offset = call.Untyped.Offset;
						Log.DebugLine(this, "   missing backward branch at {0:X2}", m_offset);
					}
					else 
					{
						Log.DebugLine(this, "   checking conditional section from [{0:X2}, {1:X2})", m_info.Instructions[conditionalStart].Untyped.Offset, m_info.Instructions[conditionalEnd].Untyped.Offset);
	
						// The code in the conditional part should all be boolean expression
						// type stuff. So, we'll test for some common instructions which should
						// not appear in a boolean expression;
						for (int index = conditionalStart; index < conditionalEnd; ++index)
						{
							Call c = m_info.Instructions[index] as Call;
							if (c != null && c.Target.ReturnType.ReturnType.ToString() == "System.Void")
							{
								m_offset = call.Untyped.Offset;
								Log.DebugLine(this, "   found a bad call in the conditional section at {0:X2}", c.Untyped.Offset);
								break;
							}
	
							Store s = m_info.Instructions[index] as Store;
							if (s != null)
							{
								m_offset = call.Untyped.Offset;
								Log.DebugLine(this, "   found a store in the conditional section at {0:X2}", s.Untyped.Offset);
								break;
							}
						}
					}
				}
			}
		}

		public void VisitEnd(EndMethod end)
		{
			if (m_offset >= 0)
				Reporter.MethodFailed(end.Info.Method, CheckID, m_offset, string.Empty);
		}
		
		private int DoFindBranch(int index, int target)
		{
			while (index < m_info.Instructions.Length)
			{
				UnconditionalBranch ub = m_info.Instructions[index] as UnconditionalBranch;
				if (ub != null)
					return -1;

				Branch branch = m_info.Instructions[index++] as Branch;
				if (branch != null && branch.Target.Index == target)
				{
					Log.DebugLine(this, "   found backward branch at {0:X2}", branch.Untyped.Offset);
					return branch.Index;
				}
			}
			
			return -1;
		}
		
		// 0F: br         29
		// 14: Ldarg.0    this
		// 15: ldfld      System.Object Smokey.Tests.UnusualMonitor1Test/Good1::m_lock
		// 1A: call       System.Boolean System.Threading.Monitor::Wait(System.Object)
		// 1F: box        System.Boolean
		// 24: call       System.Void *(System.Object)
		public bool DoMatch1a(int index)
		{
			bool match = false;

			do
			{
				if (index - 5 < 0 || index >= m_info.Instructions.Length)
					break;

				TypedInstruction instruction = m_info.Instructions[index - 0];
				Code code = instruction.Untyped.OpCode.Code;
				if (code != Code.Call && code != Code.Callvirt)
					break;
				if (!((Call) instruction).Target.ToString().StartsWith("System.Void ") || !((Call) instruction).Target.ToString().EndsWith("(System.Object)"))
					break;

				instruction = m_info.Instructions[index - 1];
				code = instruction.Untyped.OpCode.Code;
				if (code != Code.Box)
					break;

				instruction = m_info.Instructions[index - 2];
				code = instruction.Untyped.OpCode.Code;
				if (code != Code.Call && code != Code.Callvirt)
					break;
				if (((Call) instruction).Target.ToString() != "System.Boolean System.Threading.Monitor::Wait(System.Object)")
					break;

				instruction = m_info.Instructions[index - 3];
				code = instruction.Untyped.OpCode.Code;
				if (code != Code.Ldfld)
					break;

				instruction = m_info.Instructions[index - 4];
				code = instruction.Untyped.OpCode.Code;
				if (code != Code.Ldarg_0)
					break;

				instruction = m_info.Instructions[index - 5];
				code = instruction.Untyped.OpCode.Code;
				if (code != Code.Br_S && code != Code.Br)
					break;

				match = true;
			}
			while (false);

			return match;
		}

		// 0F: br         24
		// 14: Ldarg.0    this
		// 15: ldfld      System.Object Smokey.Tests.UnusualMonitor1Test/Good1::m_lock
		// 1A: call       System.Boolean System.Threading.Monitor::Wait(System.Object)
		// 1F: pop
		public bool DoMatch1b(int index)
		{
			bool match = false;

			do
			{
				if (index - 4 < 0 || index >= m_info.Instructions.Length)
					break;

				TypedInstruction instruction = m_info.Instructions[index - 0];
				Code code = instruction.Untyped.OpCode.Code;
				if (code != Code.Pop)
					break;

				instruction = m_info.Instructions[index - 1];
				code = instruction.Untyped.OpCode.Code;
				if (code != Code.Call && code != Code.Callvirt)
					break;
				if (((Call) instruction).Target.ToString() != "System.Boolean System.Threading.Monitor::Wait(System.Object)")
					break;

				instruction = m_info.Instructions[index - 2];
				code = instruction.Untyped.OpCode.Code;
				if (code != Code.Ldfld)
					break;

				instruction = m_info.Instructions[index - 3];
				code = instruction.Untyped.OpCode.Code;
				if (code != Code.Ldarg_0)
					break;

				instruction = m_info.Instructions[index - 4];
				code = instruction.Untyped.OpCode.Code;
				if (code != Code.Br_S && code != Code.Br)
					break;

				match = true;
			}
			while (false);

			return match;
		}

		// 0E: br         27
		// 13: ldsfld     System.Object Smokey.Tests.UnusualMonitor1Test/Good5::m_lock
		// 18: call       System.Boolean System.Threading.Monitor::Wait(System.Object)
		// 1D: box        System.Boolean
		// 22: call       System.Void Smokey.Internal.Ignore::set_Value(System.Object)
		public bool DoMatch2a(int index)
		{
			bool match = false;

			do
			{
				if (index - 4 < 0 || index >= m_info.Instructions.Length)
					break;

				TypedInstruction instruction = m_info.Instructions[index - 0];
				Code code = instruction.Untyped.OpCode.Code;
				if (code != Code.Call && code != Code.Callvirt)
					break;
				if (((Call) instruction).Target.ToString().IndexOf("Ignore::set_Value") < 0 && ((Call) instruction).Target.ToString().IndexOf("Unused::set_Value") < 0)
					break;

				instruction = m_info.Instructions[index - 1];
				code = instruction.Untyped.OpCode.Code;
				if (code != Code.Box)
					break;

				instruction = m_info.Instructions[index - 2];
				code = instruction.Untyped.OpCode.Code;
				if (code != Code.Call && code != Code.Callvirt)
					break;
				if (((Call) instruction).Target.ToString() != "System.Boolean System.Threading.Monitor::Wait(System.Object)")
					break;

				instruction = m_info.Instructions[index - 3];
				code = instruction.Untyped.OpCode.Code;
				if (code != Code.Ldsfld)
					break;

				instruction = m_info.Instructions[index - 4];
				code = instruction.Untyped.OpCode.Code;
				if (code != Code.Br_S && code != Code.Br)
					break;

				match = true;
			}
			while (false);

			return match;
		}

		// 0E: br         1E
		// 13: ldsfld     System.Object Smokey.Tests.UnusualMonitor1Test/Good6::m_lock
		// 18: call       System.Boolean System.Threading.Monitor::Wait(System.Object)
		// 1D: pop
		public bool DoMatch2b(int index)
		{
			bool match = false;

			do
			{
				if (index - 3 < 0 || index >= m_info.Instructions.Length)
					break;

				TypedInstruction instruction = m_info.Instructions[index - 0];
				Code code = instruction.Untyped.OpCode.Code;
				if (code != Code.Pop)
					break;

				instruction = m_info.Instructions[index - 1];
				code = instruction.Untyped.OpCode.Code;
				if (code != Code.Call && code != Code.Callvirt)
					break;
				if (((Call) instruction).Target.ToString() != "System.Boolean System.Threading.Monitor::Wait(System.Object)")
					break;

				instruction = m_info.Instructions[index - 2];
				code = instruction.Untyped.OpCode.Code;
				if (code != Code.Ldsfld)
					break;

				instruction = m_info.Instructions[index - 3];
				code = instruction.Untyped.OpCode.Code;
				if (code != Code.Br_S && code != Code.Br)
					break;

				match = true;
			}
			while (false);

			return match;
		}

		private int m_offset;
		private MethodInfo m_info;
	}
}

