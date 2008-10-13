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
using System;
using System.Collections.Generic;
using Smokey.Framework;
using Smokey.Framework.Instructions;
using Smokey.Framework.Support;
using Smokey.Framework.Support.Advanced;

namespace Smokey.Internal.Rules
{	
	internal sealed class ThreadNameRule : Rule
	{				
		public ThreadNameRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "D1061")
		{
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitBegin");
			dispatcher.Register(this, "VisitNewObj");
			dispatcher.Register(this, "VisitCall");
			dispatcher.Register(this, "VisitEnd");
		}
						
		public void VisitBegin(BeginMethod begin)
		{
			Log.DebugLine(this, "-----------------------------------"); 
			Log.DebugLine(this, "{0:F}", begin.Info.Instructions);				

			m_newOffsets.Clear();
			m_setCount = 0;
		}
		
		public void VisitNewObj(NewObj newobj)
		{								
			if (newobj.Ctor.ToString().StartsWith("System.Void System.Threading.ThreadStart::.ctor("))
			{
				m_newOffsets.Add(newobj.Untyped.Offset);
				Log.DebugLine(this, "   found new at: {0:X2}", newobj.Untyped.Offset);
			}
		}
				
		public void VisitCall(Call call)
		{			
			if (call.Target.ToString().StartsWith("System.Void System.Threading.Thread::set_Name("))
			{
				Log.DebugLine(this, "   found set at: {0:X2}", call.Untyped.Offset);
				++m_setCount;
			}
		}
				
		public void VisitEnd(EndMethod end)
		{
			if (m_newOffsets.Count > m_setCount)
			{
				int offset = m_newOffsets.Count == 1 ? m_newOffsets[0] : 0;
				Reporter.MethodFailed(end.Info.Method, CheckID, offset, string.Empty);
			}
		}

		private List<int> m_newOffsets = new List<int>();
		private int m_setCount;
	}
}

