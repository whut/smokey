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
//using Mono.Cecil.Cil;
using Smokey.Framework;
using Smokey.Framework.Instructions;
using Smokey.Framework.Support;
//using Smokey.Framework.Support.Advanced;	
using System;
//using System.Collections.Generic;
//using System.Linq;

namespace Smokey.Internal.Rules
{	
	internal sealed class ThreadAbortRule : Rule
	{				
		public ThreadAbortRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "R1040")
		{
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitBegin");
			dispatcher.Register(this, "VisitCall");
			dispatcher.Register(this, "VisitEnd");
		}
				
		public void VisitBegin(BeginMethod method)
		{
			Log.DebugLine(this, "-----------------------------------"); 
			Log.DebugLine(this, "{0:F}", method.Info.Instructions);				

			m_offset = -1;
		}
		
		public void VisitCall(Call call)
		{
			if (m_offset < 0)
			{
				if (call.Target.ToString() == "System.Void System.Threading.Thread::Abort()")
				{	
					m_offset = call.Untyped.Offset;
				}
				else if (call.Target.ToString() == "System.Void System.Threading.Thread::Abort(System.Object)")
				{	
					m_offset = call.Untyped.Offset;
				}
			}
		}

		public void VisitEnd(EndMethod method)
		{
			if (m_offset >= 0)
				Reporter.MethodFailed(method.Info.Method, CheckID, m_offset, string.Empty);
		}
				
		private int m_offset;
	}
}


