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
using Smokey.Framework;
using Smokey.Framework.Instructions;
using Smokey.Framework.Support;

#if OLD
namespace Smokey.Internal.Rules
{	
	internal sealed class ObjectDisposedExceptionRule : Rule
	{				
		public ObjectDisposedExceptionRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "R1036")
		{
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitBegin");
			dispatcher.Register(this, "VisitNew");
			dispatcher.Register(this, "VisitEnd");
		}
				
		public void VisitBegin(BeginMethod begin)
		{
			Log.DebugLine(this, "-----------------------------------"); 
			Log.DebugLine(this, "{0:F}", begin.Info.Instructions);				

			m_offset = -1;
			m_info = begin.Info;
		}
		
		public void VisitNew(NewObj newer)
		{
			if (m_offset < 0 && newer.Ctor.ToString().StartsWith("System.Void System.ObjectDisposedException::.ctor("))
			{
				int numArgs = newer.Ctor.Parameters.Count;
				int index = m_info.Tracker.GetStackIndex(newer.Index, numArgs - 1);
				
				if (index >= 0)
				{
					LoadString arg = m_info.Instructions[index] as LoadString;
					if (arg != null)
					{
						m_offset = newer.Untyped.Offset;
						Log.DebugLine(this, "bad exception at {0:X2}", m_offset); 
					}
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
#endif
