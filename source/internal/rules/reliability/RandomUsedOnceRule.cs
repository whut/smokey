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
	internal sealed class RandomUsedOnceRule : Rule
	{				
		public RandomUsedOnceRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "R1021")
		{
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			m_dispatcher = dispatcher;

			dispatcher.Register(this, "VisitBegin");
			dispatcher.Register(this, "VisitNew");
			dispatcher.Register(this, "VisitCall");
			dispatcher.Register(this, "VisitEnd");
		}
				
		public void VisitBegin(BeginMethod method)
		{
			Log.DebugLine(this, "-----------------------------------"); 
			Log.DebugLine(this, "{0:F}", method.Info.Instructions);				

			m_offset = -1;
			m_numNew = 0;
			m_numNext = 0;
		}

		public void VisitNew(NewObj obj)
		{
			string name = obj.Ctor.ToString();
			if (name == "System.Void System.Random::.ctor()")
			{
				Log.DebugLine(this, "found new at {0:X2}", obj.Untyped.Offset);
				++m_numNew;
	
				if (m_offset < 0)
					m_offset = obj.Untyped.Offset;
			}
		}

		public void VisitCall(Call call)
		{
			string name = call.Target.ToString();
			if (name.Contains("System.Random::Next"))
			{
				if (m_dispatcher.Looping)
				{
					m_numNext = -10000;
				}
				else if (m_numNew > 0)
				{
					Log.DebugLine(this, "found next at {0:X2}", call.Untyped.Offset);
					++m_numNext;
				}
			}
		}

		public void VisitEnd(EndMethod method)
		{
			if (m_offset >= 0 && m_numNew == 1 && m_numNext == 1)
				Reporter.MethodFailed(method.Info.Method, CheckID, m_offset, string.Empty);
		}
		
		private RuleDispatcher m_dispatcher;
		private int m_offset;
		private int m_numNew;
		private int m_numNext;
	}
}
#endif
