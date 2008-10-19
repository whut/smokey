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
using Smokey.Framework;
using Smokey.Framework.Instructions;
using Smokey.Framework.Support;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;

#if OLD
namespace Smokey.Internal.Rules
{	
	internal sealed class DoubleCheckedLockingRule : Rule
	{				
		public DoubleCheckedLockingRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "C1029")
		{
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitBegin");
			dispatcher.Register(this, "VisitBranch");
			dispatcher.Register(this, "VisitCall");
			dispatcher.Register(this, "VisitRet");
			dispatcher.Register(this, "VisitEnd");
		}
		
		public void VisitBegin(BeginMethod begin)
		{	
			m_needsCheck = false;
			m_foundSingle = false;
			m_foundRet = false;
			m_foundGuard = false;
			m_numLocks = 0;
			m_field = null;
			
			if (begin.Info.Method.IsStatic)
			{
				if (begin.Info.Method.ReturnType.ReturnType.FullName != "System.Void")
				{
					Log.DebugLine(this, "-----------------------------------"); 
					Log.DebugLine(this, "{0:F}", begin.Info.Instructions);				

					m_needsCheck = true;
					m_info = begin.Info;
				}
			}
		}
		
		public void VisitBranch(ConditionalBranch branch)	
		{
			if (m_needsCheck && !m_foundSingle && branch.Index >= 2 && m_numLocks == 1)
			{
				// Look for the if within the lock:
				//    call       System.Void System.Threading.Monitor::Enter(System.Object)
				//    ldsfld     field
				//    brtrue     2A
				if (branch.Untyped.OpCode.Code == Code.Brtrue || branch.Untyped.OpCode.Code == Code.Brtrue_S)
				{
					LoadStaticField load = m_info.Instructions[branch.Index - 1] as LoadStaticField;
					Call call = m_info.Instructions[branch.Index - 2] as Call;
					
					if (load != null && call != null && call.Target.ToString() == "System.Void System.Threading.Monitor::Enter(System.Object)")
					{
						m_field = load.Field;
						Log.DebugLine(this, "found candidate lock at {0:X2}", branch.Untyped.Offset);
						
						// If we found the inner if then we need to look for a new
						// and a store within the if,
						if (DoFindNew(branch.Index + 1, branch.Target.Index - branch.Index - 1))
						{
							m_foundSingle = true;
							
							// and a second guard before the lock.
							if (DoFindGuard(call.Index - 1))
							{
								m_foundGuard = true;
							}
						}
					}
				}
			}
		}
		
		// match:
		// 		ldsfld     field
		// 		ret        
		public void VisitRet(End end)	
		{
			if (m_needsCheck && !m_foundRet && m_foundSingle)
			{
				if (end.Untyped.OpCode.Code == Code.Ret && end.Index > 1)
				{
					LoadStaticField load = m_info.Instructions[end.Index - 1] as LoadStaticField;
					if (load != null && load.Field == m_field)
					{
						Log.DebugLine(this, "found return at {0:X2}", end.Untyped.Offset);
						m_foundRet = true;
					}
				}
			}
		}

		public void VisitCall(Call call)	
		{
			if (m_needsCheck)
			{
				if (call.Target.ToString() == "System.Void System.Threading.Monitor::Enter(System.Object)")
				{
					Log.DebugLine(this, "found lock at {0:X2}", call.Untyped.Offset);
					++m_numLocks;
				}
			}
		}

		public void VisitEnd(EndMethod end)
		{
			if (m_numLocks == 1 && m_foundSingle && m_foundRet && !m_foundGuard)
			{
				Reporter.MethodFailed(end.Info.Method, CheckID, 0, string.Empty);
			}
		}
								
		// Find the following in the count instructions starting at index:
		//		newobj     xxx
		//		stsfld     field
		private bool DoFindNew(int index, int count)
		{
			Log.DebugLine(this, "checking {0} instructions starting at {1:X2} for a new", count, m_info.Instructions[index].Untyped.Offset);

			for (int i = index; i < index + count; ++i)
			{
				StoreStaticField store = m_info.Instructions[i] as StoreStaticField;
				if (store != null && store.Field == m_field)
				{
					NewObj newer = m_info.Instructions[i - 1] as NewObj;
					if (newer != null)
					{
						Log.DebugLine(this, "found a new at {0:X2}", store.Untyped.Offset);
						return true;
					}
				}
			}
			
			return false;
		}
		
		// Find the following working backwards from index:
		// 		ldsfld     field
		// 		brtrue     36
		private bool DoFindGuard(int index)	
		{
			while (index > 0)
			{
				ConditionalBranch branch = m_info.Instructions[index] as ConditionalBranch;
				if (branch != null && (branch.Untyped.OpCode.Code == Code.Brtrue || branch.Untyped.OpCode.Code == Code.Brtrue_S))
				{
					LoadStaticField load = m_info.Instructions[index - 1] as LoadStaticField;
					if (load != null && load.Field == m_field)
					{
						Log.DebugLine(this, "found the guard at {0:X2}", branch.Untyped.Offset);
						return true;
					}
				}
				--index;
			}
			
			return false;
		}

		private MethodInfo m_info;
		private bool m_needsCheck;
		private bool m_foundSingle;
		private bool m_foundRet;
		private bool m_foundGuard;
		private int m_numLocks;
		private FieldReference m_field;
	}
}
