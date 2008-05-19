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
using System.IO;
using System.Collections.Generic;
using Smokey.Framework;
using Smokey.Framework.Instructions;
using Smokey.Framework.Support;

namespace Smokey.Internal.Rules
{	
	internal sealed class ExitCodeRule : Rule
	{				
		public ExitCodeRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "PO1007")
		{
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitBegin");
			dispatcher.Register(this, "VisitCall");
			dispatcher.Register(this, "VisitRet");
			dispatcher.Register(this, "VisitStore");
			dispatcher.Register(this, "VisitEnd");
		}
				
		public void VisitBegin(BeginMethod begin)
		{
			Log.DebugLine(this, "-----------------------------------"); 
			Log.DebugLine(this, "{0:F}", begin.Info.Instructions);				

			m_offset = -1;
			m_info = begin.Info;
			m_isMain = Cache.Assembly.EntryPoint == begin.Info.Method;
			m_stores.Clear();
			m_candidateLoads.Clear();
		}
		
		public void VisitRet(End end)
		{
			if (m_isMain && m_offset < 0 && end.Index > 0)
			{
				if (end.Untyped.OpCode.Code == Code.Ret)
				{
					Log.DebugLine(this, "checking {0}", end);	
					DoCheckLoad(end);
				}
			}
		}

		public void VisitCall(Call call)
		{
			if (m_offset < 0 && call.Index > 0)
			{
				if (call.Target.ToString() == "System.Void System.Environment::Exit(System.Int32)")
				{					
					DoCheckLoad(call);
				}
				else if (call.Target.ToString() == "System.Void System.Environment::set_ExitCode(System.Int32)")
				{					
					DoCheckLoad(call);
				}
			}
		}
		
		public void VisitStore(StoreLocal store)
		{
			if (store.Index > 0)
			{
				LoadConstantInt load = m_info.Instructions[store.Index - 1] as LoadConstantInt;
				if (load != null)
				{
					if (load.Value < 0 || load.Value > 255)
					{
						Log.DebugLine(this, "store at {0:X2}", store.Untyped.Offset); 
						
						if (m_stores.IndexOf(store.Variable) < 0)
						{
							m_stores.Add(store.Variable);
						}
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
			else 
			{
				foreach (int v in m_candidateLoads)
				{
					if (m_stores.IndexOf(v) >= 0)
					{
						Reporter.MethodFailed(end.Info.Method, CheckID, 0, string.Empty);
						return;
					}
				}
			}
		}
								
		private void DoCheckLoad(TypedInstruction instruction)
		{
			LoadConstantInt load = m_info.Instructions[instruction.Index - 1] as LoadConstantInt;
			if (load != null)
			{
				if (load.Value < 0 || load.Value > 255)
				{
					m_offset = instruction.Untyped.Offset;						
					Log.DebugLine(this, "bad exit at {0:X2}", m_offset); 
				}
			}

			LoadLocal local = m_info.Instructions[instruction.Index - 1] as LoadLocal;
			if (local != null)
			{
				Log.DebugLine(this, "{0} at {1:X2}", instruction.Untyped.OpCode, instruction.Untyped.Offset); 
				if (m_candidateLoads.IndexOf(local.Variable) < 0)
				{
					m_candidateLoads.Add(local.Variable);
				}
			}
		}

		private int m_offset;
		private MethodInfo m_info;
		private bool m_isMain;
		private List<int> m_stores = new List<int>();
		private List<int> m_candidateLoads = new List<int>();
	}
}
