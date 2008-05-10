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
using Smokey.Framework.Support.Advanced.Values;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Smokey.Internal.Rules
{	
	internal class InconsistentPropertyRule : Rule
	{				
		public InconsistentPropertyRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "C1028")
		{
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitBegin");
			dispatcher.Register(this, "VisitMethod");
			dispatcher.Register(this, "VisitRet");
			dispatcher.Register(this, "VisitStore");
			dispatcher.Register(this, "VisitEnd");
		}
				
		public void VisitBegin(BeginMethods begin)
		{
			Log.DebugLine(this, "-----------------------------------"); 
			Log.DebugLine(this, "{0}", begin.Type);				

			m_getters.Clear();
			m_setters.Clear();
			m_getFields.Clear();
			m_setFields.Clear();
			
			foreach (PropertyDefinition prop in begin.Type.Properties)
			{
				if (prop.GetMethod != null && prop.SetMethod != null)
				{
					if (prop.SetMethod.Parameters.Count == 1)	// ignore indexers
					{
						if (!prop.GetMethod.CustomAttributes.HasDisableRule("C1028"))
						{
							m_getters.Add(prop.GetMethod, prop);
							m_setters.Add(prop.SetMethod, prop);
		
							m_getFields.Add(prop.GetMethod, new List<FieldReference>());
							m_setFields.Add(prop.SetMethod, new List<FieldReference>());
						}
					}
				}
			}
		}
				
		public void VisitMethod(BeginMethod begin)
		{		
			m_info = null;
			m_inGetter = m_getters.ContainsKey(begin.Info.Method);
			m_inSetter = m_setters.ContainsKey(begin.Info.Method);
			
			if (m_inGetter || m_inSetter)
			{
				Log.DebugLine(this, "-----------------------------------"); 
				Log.DebugLine(this, "{0:F}", begin.Info.Instructions);		
				
				m_info = begin.Info;
			}
		}
		
		public void VisitRet(End end)	
		{
			if (m_inGetter && end.Untyped.OpCode.Code == Code.Ret && end.Index > 0)
			{
				LoadField load = m_info.Instructions[end.Index - 1] as LoadField;
				if (load != null)
				{
					List<FieldReference> fields = m_getFields[m_info.Method];
					if (fields.IndexOf(load.Field) < 0)
					{
						Log.DebugLine(this, "{0} is used at {1:X2}", load.Field.Name, end.Untyped.Offset);											
						fields.Add(load.Field);
					}
				}
			}
		}

		public void VisitStore(StoreField store)	
		{
			if (m_inSetter)
			{
				LoadArg load = m_info.Instructions[store.Index - 1] as LoadArg;
				if (load != null && load.Arg == 1)
				{
					List<FieldReference> fields = m_setFields[m_info.Method];
					if (fields.IndexOf(store.Field) < 0)
					{
						Log.DebugLine(this, "{0} is used at {1:X2}", store.Field.Name, store.Untyped.Offset);											
						fields.Add(store.Field);
					}
				}
			}
		}

		public void VisitEnd(EndMethods end)
		{
			foreach (KeyValuePair<MethodDefinition, PropertyDefinition> entry in m_getters)
			{
				List<FieldReference> gets = m_getFields[entry.Key];
				List<FieldReference> sets = m_setFields[entry.Value.SetMethod];
				
				if (sets.Count > 0)
				{
					var d = gets.Except(sets);
					if (d.Count() > 0)			// if gets has elements not in sets we have a problem
					{
						foreach (FieldReference f in d)
							Log.DebugLine(this, "{0} is a bad get", f.Name);
							
						Reporter.MethodFailed(entry.Key, CheckID, 0, string.Empty);
					}
				}
			}
		}
						
		private MethodInfo m_info;
		private Dictionary<MethodDefinition, PropertyDefinition> m_getters = new Dictionary<MethodDefinition, PropertyDefinition>();
		private Dictionary<MethodDefinition, PropertyDefinition> m_setters = new Dictionary<MethodDefinition, PropertyDefinition>();

		private Dictionary<MethodDefinition, List<FieldReference>> m_getFields = new Dictionary<MethodDefinition, List<FieldReference>>();
		private Dictionary<MethodDefinition, List<FieldReference>> m_setFields = new Dictionary<MethodDefinition, List<FieldReference>>();

		private bool m_inGetter;
		private bool m_inSetter;
	}
}
