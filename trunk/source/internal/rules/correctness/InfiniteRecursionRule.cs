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
	internal sealed class InfiniteRecursionRule : Rule
	{				
		public InfiniteRecursionRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "C1001")
		{
		}
						
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitBegin");	
			dispatcher.Register(this, "VisitReturn");	
			dispatcher.Register(this, "VisitBranch");	
			dispatcher.Register(this, "VisitCall");	
			dispatcher.Register(this, "VisitEnd");
		}
				
		public void VisitBegin(BeginMethod method)
		{		
			Log.DebugLine(this, "-----------------------------------"); 
			Log.DebugLine(this, "{0:F}", method.Info.Instructions);				
			
			m_hasBranch = false;
			m_info = method.Info;
			m_offset = -1;
		}
				
		// We no longer have the machinery in place to do this right so what we'll
		// do is catch the most common failures which tend to be simple mistakes
		// like recursive properties. We'll do this by failing if we have a recursive
		// call and no early returns or branches.
		public void VisitReturn(End end)
		{
			if (!m_hasBranch && end.Index < m_info.Instructions.Length - 1)
			{
				m_hasBranch = true;
				Log.DebugLine(this, "early return at {0:X2}", end.Untyped.Offset); 
			}
		}

		public void VisitBranch(Branch branch)
		{
			if (!m_hasBranch)
			{
				m_hasBranch = true;
				Log.DebugLine(this, "branch at {0:X2}", branch.Untyped.Offset); 
			}
		}

		public void VisitCall(Call call)
		{
			if (m_offset < 0)
			{
				if (m_info.Method.MetadataToken == call.Target.MetadataToken)
				{
					m_offset = call.Untyped.Offset;
					Log.DebugLine(this, "found recursive call at {0}", m_offset);
				}
			}
		}

		public void VisitEnd(EndMethod method)
		{
			if (m_offset >= 0 && !m_hasBranch)
				Reporter.MethodFailed(method.Info.Method, CheckID, m_offset, string.Empty);
		}
		
		private MethodInfo m_info;
		private int m_offset;
		private bool m_hasBranch;
	}
}
#endif