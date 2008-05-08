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
	internal class DisposeScopedObjectsRule : Rule
	{				
		public DisposeScopedObjectsRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "R1004")
		{
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitBegin");
			dispatcher.Register(this, "VisitStoreLocal");
			dispatcher.Register(this, "VisitEnd");
		}
				
		public void VisitBegin(BeginMethod method)
		{
			Log.DebugLine(this, "-----------------------------------"); 
			Log.DebugLine(this, "{0:F}", method.Info.Instructions);				

			m_offset = -1;
			m_info = method.Info;
			m_details = string.Empty;
		}
		
		public void VisitStoreLocal(StoreLocal store)
		{
			if (m_offset < 0)
			{
				int variable = DoGetNewDisposableLocal(store);
				if (variable >= 0)
				{
					List<int> loads = DoGetDisposableLoads(store.Index + 1, variable);
					if (loads.Count > 0)
					{						
						if (DoNeedsDispose(loads, store.Index + 1))
						{
							m_offset = store.Untyped.Offset;
							Log.DebugLine(this, "no dispose call");
						}
					}
				}
			}
		}

		public void VisitEnd(EndMethod method)
		{
			if (m_offset >= 0)
				Reporter.MethodFailed(method.Info.Method, CheckID, m_offset, m_details);
		}
		
		// newobj  System.Void System.IO.StreamReader::.ctor(System.String)
        // stloc.1 V_1
		private int DoGetNewDisposableLocal(StoreLocal store)
		{
			int variable = -1;
			
			NewObj obj = m_info.Instructions[store.Index - 1] as NewObj;
			if (obj != null && obj.Ctor.Name.EndsWith(".ctor"))
			{
				if (IsDisposable.Type(Cache, obj.Ctor.DeclaringType))
				{
					m_details = "Type: " + obj.Ctor.DeclaringType.FullName;
					Log.DebugLine(this, "{0} = new {1}", store.Name, obj.Ctor.DeclaringType.FullName);
					variable = store.Variable;
				}
			}
			
			return variable;
		}

		private bool DoNeedsDispose(List<int> original, int index)
		{
			List<int> loads = new List<int>(original);
			
			// First look at all the call instructions and remove from loads the local
			// loads that are used as this pointers. Also set hasDispose if Dispose was
			// called on the variable in question.
			bool hasDispose = false;
			foreach (Call call in m_info.Instructions.Match<Call>())
			{
				if (call.Target.HasThis)
				{
					int i = m_info.Tracker.GetStackIndex(call.Index, call.Target.Parameters.Count);
					
					if (i >= 0 && loads.Remove(i))
					{
						Log.DebugLine(this, "found load used as this pointer for {0:X2}", call.Untyped.Offset);

						if (call.Target.Parameters.Count == 0)
						{
							if (call.Target.Name == "Dispose" || call.Target.Name == "Close")
							{
								Log.DebugLine(this, "found dispose call at {0:X2}", call.Untyped.Offset);
								hasDispose = true;
							}
						}
					}
				}
			}
			
			// We also need to remove any locals used for null comparisons (a using statement
			// will add one of these).
			loads = loads.RemoveIf(delegate (int i)
			{
				Code code1 = i+1 < m_info.Instructions.Length ? m_info.Instructions[i + 1].Untyped.OpCode.Code : Code.Nop;
				Code code2 = i+2 < m_info.Instructions.Length ? m_info.Instructions[i + 2].Untyped.OpCode.Code : Code.Nop;
				bool remove = (code1 == Code.Brfalse_S || code1 == Code.Brfalse || code1 == Code.Brtrue_S || code1 == Code.Brtrue) ||
								(code1 == Code.Ldnull && (code2 == Code.Beq || code2 == Code.Beq_S));
				
				if (remove)
					Log.DebugLine(this, "found load used for null check1 at {0:X2}", m_info.Instructions[i].Untyped.Offset);

				return remove;
			});
			
			// If we have any locals left we'll assume the code is OK (they may be storing the
			// local somewhere so a dispose isn't needed). But if we consumed all the local
			// references and there is no dispose call we have a problem.
			// TODO: it'd be nice to check methods we call with the local as an argument and
			// see if they actually store the object
#if DEBUG
			if (loads.Count == 0)
				Log.DebugLine(this, "all loads were used as this pointers");
			else
				Log.DebugLine(this, "still {0} loads left", loads.Count);
#endif

			return loads.Count == 0 && !hasDispose;
		}
		
		private List<int> DoGetDisposableLoads(int index, int variable)
		{
			List<int> loads = new List<int>();
			
			for (int i = index; i < m_info.Instructions.Length; ++i)
			{
				LoadLocal load = m_info.Instructions[i] as LoadLocal;
				if (load != null && load.Variable == variable)
				{
					loads.Add(i);
					Log.DebugLine(this, "found load at offset {0:X2}", load.Untyped.Offset);
				}
			}

			return loads;
		}

		private int m_offset;
		private MethodInfo m_info;
		private string m_details;
	}
}

