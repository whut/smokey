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
using Smokey.Framework.Support.Advanced;
using System;
using System.Collections.Generic;

#if OLD
namespace Smokey.Internal.Rules
{	
	internal sealed class CircularReferenceRule : Rule
	{				
		public CircularReferenceRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "D1041")
		{
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitBegin");
			dispatcher.Register(this, "VisitField");
			dispatcher.Register(this, "VisitMethod");
			dispatcher.Register(this, "VisitEnd");
			dispatcher.Register(this, "VisitFini");
		}
						
		public void VisitBegin(BeginType begin)
		{
			Log.DebugLine(this, "-----------------------------------"); 
			Log.DebugLine(this, "checking {0}", begin.Type);				

			m_needsCheck = !begin.Type.IsCompilerGenerated();
			if (m_needsCheck)
			{
				m_refs = new List<TypeReference>();
				
				TypeReference super = begin.Type.BaseType;
				while (super != null && super.FullName != "System.Object")
				{
					DoAdd(super);
					
					TypeDefinition t = Cache.FindType(super);
					super = t != null ? t.BaseType : null;
				}
			}
		}
		
		public void VisitField(FieldDefinition field)
		{
			if (m_needsCheck)
				DoAdd(field.FieldType);
		}
		
		public void VisitMethod(MethodDefinition method)
		{
			if (m_needsCheck)
			{
				DoAdd(method.ReturnType.ReturnType);
				foreach (ParameterDefinition p in method.Parameters)
				{
					DoAdd(p.ParameterType);
				}
				foreach (GenericParameter g in method.GenericParameters)
				{
					DoAdd(g);
				}
				if (method.Body != null)
				{
					foreach (VariableDefinition v in method.Body.Variables)
					{
						DoAdd(v.VariableType);
					}
				}
			}
		}
		
		public void VisitEnd(EndType end)
		{
			if (m_needsCheck)
				m_table.Add(end.Type, m_refs);
		}
						
		public void VisitFini(EndTesting end)
		{
			Unused.Value = end;
			
			foreach (KeyValuePair<TypeReference, List<TypeReference>> entry in m_table)
			{
				TypeReference type = entry.Key;
				string details = string.Empty;
				foreach (TypeReference r in entry.Value)
				{
					if (string.Compare(type.FullName, r.FullName) < 0)	// tie-breaker which prevents us from reporting the same error twice
					{
						List<TypeReference> refs = null;
						if (m_table.TryGetValue(r, out refs) && refs.IndexOf(type) >= 0)
						{
							details = string.Format("{0}{1} ", details, r);
						}
					}
				}
				
				if (details.Length > 0)
				{
					details = string.Format("{0} <-> {1} ", type.FullName, details);
					Log.DebugLine(this, details); 
		
					TypeDefinition t = Cache.FindType(type);
					if (t != null)
						Reporter.TypeFailed(t, CheckID, details);
				}
			}
		}
		
		private void DoAdd(TypeReference type)
		{
			if (m_refs.IndexOf(type) < 0)
			{
				m_refs.Add(type);
				Log.DebugLine(this, "   adding {0}", type.FullName); 
			}
		}

		private bool m_needsCheck;
		private List<TypeReference> m_refs;
		private Dictionary<TypeReference, List<TypeReference>> m_table = new Dictionary<TypeReference, List<TypeReference>>();
	}
}
#endif
