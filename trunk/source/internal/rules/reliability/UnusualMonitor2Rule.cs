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
	internal class UnusualMonitor2Rule : Rule
	{				
		public UnusualMonitor2Rule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "R1028")
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
			if (m_offset < 0)
			{
				if (call.Target.ToString().StartsWith("System.Boolean System.Threading.Monitor::Wait(System.Object,"))
				{
					if (!DoFindBranch(call.Index + 1))
					{
						m_offset = call.Untyped.Offset;
						Log.DebugLine(this, "   missing backward branch at {0:X2}", m_offset);
					}
				}
			}
		}

		public void VisitEnd(EndMethod end)
		{
			if (m_offset >= 0)
				Reporter.MethodFailed(end.Info.Method, CheckID, m_offset, string.Empty);
		}
		
		private bool DoFindBranch(int startIndex)
		{
			int index = startIndex;
			while (index < m_info.Instructions.Length)
			{
				Branch branch = m_info.Instructions[index++] as Branch;
				if (branch != null && branch.Target.Index < startIndex)
				{
					Log.DebugLine(this, "   found backward branch at {0:X2}", branch.Untyped.Offset);
					return true;
				}
			}
			
			return false;
		}
		
		private int m_offset;
		private MethodInfo m_info;
	}
}

