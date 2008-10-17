	
	<Violation checkID = "C1039" severity = "Error" breaking = "false">
		<Translation lang = "en" typeName = "NullResult" category = "Correctness">			
			<Cause>
			A method calls another method which sometimes returns null, but the first
			method uses the result without checking for null.
			</Cause>
		
			<Description>
			Currently this rule will only fire if the second method is in the assembly
			being tested and the method result is being used as the target of a method call.
			</Description>
	
			<Fix>
			Add a check for a null result.
			</Fix>
		</Translation>
	</Violation>	

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
using System.Diagnostics;
using Smokey.Framework;
using Smokey.Framework.Instructions;
using Smokey.Framework.Support;

namespace Smokey.Internal.Rules
{	
	internal sealed class NullResultRule : Rule
	{				
		public NullResultRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "C1039")
		{
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitBegin");
			dispatcher.Register(this, "VisitMethod");
			dispatcher.Register(this, "VisitStore");
			dispatcher.Register(this, "VisitCall");
			dispatcher.Register(this, "VisitBranch");
			dispatcher.Register(this, "VisitRet");
			dispatcher.Register(this, "VisitEnd");
		}
		
		public void VisitBegin(BeginTesting begin)
		{
			Log.DebugLine(this, "+++++++++++++++++++++++++++++++++++++");
			
			m_nulls.Clear();
			m_candidates.Clear();
		}
		
		public void VisitMethod(BeginMethod begin)
		{
			Log.DebugLine(this, "{0:F}", begin.Info.Instructions);			
			
			m_info = begin.Info;
			m_potentialNulls.Clear();
		}

		public void VisitCall(Call call)
		{
			// If we're calling a method which uses a local which may be null as the
			// this argument then the call is a candidate for a null dereference.
			// TODO: would also be nice to check for the result used as an argument
			// to methods which don't accept a null argument
			if (call.Target.HasThis)
			{
				int index = call.GetThisIndex(m_info);
				if (index >= 0)
				{
					LoadLocal load = m_info.Instructions[index] as LoadLocal;
					if (load != null && m_potentialNulls.ContainsKey(load.Variable))
					{
						Log.DebugLine(this, "{0:X2}'s has potential null at {1:X2}", call.Untyped.Offset, load.Untyped.Offset);	
						m_candidates[m_info.Method] = m_potentialNulls[load.Variable];
					}
					else
						Log.DebugLine(this, "{0:X2}'s this arg is at {1:X2}", call.Untyped.Offset, m_info.Instructions[index].Untyped.Offset);	
				}
			}
		}

		public void VisitStore(StoreLocal store)
		{
			// If we're calling a method which returns a reference type then save
			// the local the result is stuffed into.
			MethodReference target = null;
			
			Call call = m_info.Instructions[store.Index - 1] as Call;
			if (call != null)
			{
				TypeDefinition rtype = Cache.FindType(call.Target.ReturnType.ReturnType);
				if (rtype != null && !rtype.IsValueType)
				{
					Log.DebugLine(this, "{0:X2} stores reference result", call.Untyped.Offset);	
					target = call.Target;
				}
			}
			
			if (target != null)
				m_potentialNulls[store.Variable] = target;
			else
				Unused.Value = m_potentialNulls.Remove(store.Variable);
		}

		public void VisitBranch(Branch branch)
		{
			m_potentialNulls.Clear();
		}
		
		public void VisitRet(End end)
		{
			m_potentialNulls.Clear();
			
			// If we found a null return then we know this method returns null.
			// TODO: would be nice to also check for a ReturnsNullAttribute on the method.
			if (end.Untyped.OpCode.Code == Code.Ret && end.Index > 0)
			{
				LoadNull load = m_info.Instructions[end.Index - 1] as LoadNull;
				if (load != null && m_nulls.IndexOf(m_info.Method) < 0)
				{
					Log.DebugLine(this, "{0} returns null", m_info.Method);	
					m_nulls.Add(m_info.Method);
				}
			}
		}
		
		public void VisitEnd(EndTesting end)
		{
			string details = string.Empty;
			
			foreach (var entry in m_candidates)
			{
				if (m_nulls.IndexOf(entry.Value) >= 0)
					details += entry.Key + " uses " + entry.Value + " result" + Environment.NewLine;
			}
			
			if (details.Length > 0)
				details = "Methods:" + Environment.NewLine + details;
			
			details = details.Trim();
			if (details.Length > 0)
			{
				Log.DebugLine(this, "{0}", details);
				Reporter.AssemblyFailed(Cache.Assembly, CheckID, details);
			}
		}
		
		private MethodInfo m_info;
		private Dictionary<int, MethodReference> m_potentialNulls = new Dictionary<int, MethodReference>();
		
		private Dictionary<MethodReference, MethodReference> m_candidates = new Dictionary<MethodReference, MethodReference>();
		private List<MethodReference> m_nulls = new List<MethodReference>();
	}
}
