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
	internal sealed class LockThisRule : Rule
	{				
		public LockThisRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "MS1008")
		{
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitBegin");
			dispatcher.Register(this, "VisitCall");
			dispatcher.Register(this, "VisitStore");
			dispatcher.Register(this, "VisitEnd");
		}
				
		public void VisitBegin(BeginMethod method)
		{
			m_offset = -1;
			m_needsCheck = false;
			if (!method.Info.Method.IsStatic && method.Info.Method.HasThis)
				if ((method.Info.Method.Attributes & MethodAttributes.MemberAccessMask) != MethodAttributes.Private)
					m_needsCheck = true;
			
			if (m_needsCheck)
			{
				Log.DebugLine(this, "-----------------------------------"); 
				Log.DebugLine(this, "{0:F}", method.Info.Instructions);				

				m_info = method.Info;
				m_thisLocals.Clear();
			}
		}
				
		// ldloc.0  V_0
        // call     System.Void System.Threading.Monitor::Enter(System.Object)
		public void VisitCall(Call call)
		{
			if (m_needsCheck && m_offset < 0)
			{
				if (call.Target.ToString() == "System.Void System.Threading.Monitor::Enter(System.Object)")
				{
					LoadLocal local = m_info.Instructions[call.Index - 1] as LoadLocal;
					if (local != null && m_thisLocals.IndexOf(local.Variable) >= 0)
					{
						m_offset = call.Untyped.Offset;
						Log.DebugLine(this, "found lock(this) at {0:X2}", m_offset);
					}
				}
			}
		}

        // ldarg.0  this
        // stloc.0  V_0
		public void VisitStore(StoreLocal store)
		{
			if (m_needsCheck && m_offset < 0)
			{
				LoadArg load = m_info.Instructions[store.Index - 1] as LoadArg;
				if (load != null && load.Arg == 0)
				{
					m_thisLocals.Add(store.Variable);
					Log.DebugLine(this, "{0} = this at {1:X2}", store.Name, store.Untyped.Offset);
				}
				else
					Unused.Value = m_thisLocals.Remove(store.Variable);
			}
		}

		public void VisitEnd(EndMethod method)
		{
			if (m_offset >= 0)
				Reporter.MethodFailed(method.Info.Method, CheckID, m_offset, string.Empty);
		}
				
		private bool m_needsCheck;
		private int m_offset;
		private MethodInfo m_info;
		private List<int> m_thisLocals = new List<int>();
	}
}
#endif
