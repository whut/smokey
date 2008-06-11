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
using System.Text;

namespace Smokey.Internal.Rules
{	
	internal sealed class UnusedFieldRule : Rule
	{				
		public UnusedFieldRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "D1050")
		{
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitType");
			dispatcher.Register(this, "VisitField");
			dispatcher.Register(this, "VisitLoadField");
			dispatcher.Register(this, "VisitLoadFieldAddr");
			dispatcher.Register(this, "VisitLoadStaticField");
			dispatcher.Register(this, "VisitLoadStaticFieldAddress");
			dispatcher.Register(this, "VisitFini");
		}
		
		private enum State 
		{
			Defined, 			// field is internal but hasn't been used yet
			Used, 				// field is internal and has been used
			Referenced			// field has been used, but we don't know if it's internal
		};
		
		public void VisitType(BeginType begin)
		{
			m_needsCheck = !begin.Type.IsCompilerGenerated();
			m_type = begin.Type;
		}
		
		public void VisitField(FieldDefinition field)
		{
			if (m_needsCheck && field.IsAssembly && m_type.IsClass)
//			if (m_needsCheck && field.IsAssembly && m_type.IsClass && !m_type.FullName.Contains("PrivateImplementationDetails"))
			{
				if (!m_type.CustomAttributes.HasDisableRule("D1050"))
				{
					TypeAttributes attrs = m_type.Attributes;
					if (!m_type.IsValueType || (attrs & TypeAttributes.LayoutMask) == TypeAttributes.AutoLayout)
					{
						if (m_fields.ContainsKey(field))
						{
							DBC.Assert(m_fields[field] == State.Referenced, "state is {0}", m_fields[field]);
							m_fields[field] = State.Used;
						}
						else
						{
							m_fields.Add(field, State.Defined);
						}
					}
				}
			}
		}
		
		public void VisitLoadField(LoadField load)
		{
			if (m_needsCheck)
			{
				State state;
				if (m_fields.TryGetValue(load.Field, out state))
				{
					if (state == State.Defined)
						m_fields[load.Field] = State.Used;
				}
				else
				{
					m_fields.Add(load.Field, State.Referenced);
				}
			}
		}
				
		public void VisitLoadFieldAddr(LoadFieldAddress load)
		{
			if (m_needsCheck)
			{
				State state;
				if (m_fields.TryGetValue(load.Field, out state))
				{
					if (state == State.Defined)
						m_fields[load.Field] = State.Used;
				}
				else
				{
					m_fields.Add(load.Field, State.Referenced);
				}
			}
		}
				
		public void VisitLoadStaticField(LoadStaticField load)
		{
			if (m_needsCheck)
			{
				State state;
				if (m_fields.TryGetValue(load.Field, out state))
				{
					if (state == State.Defined)
						m_fields[load.Field] = State.Used;
				}
				else
				{
					m_fields.Add(load.Field, State.Referenced);
				}
			}
		}
				
		public void VisitLoadStaticFieldAddress(LoadStaticFieldAddress load)
		{
			if (m_needsCheck)
			{
				State state;
				if (m_fields.TryGetValue(load.Field, out state))
				{
					if (state == State.Defined)
						m_fields[load.Field] = State.Used;
				}
				else
				{
					m_fields.Add(load.Field, State.Referenced);
				}
			}
		}
				
		public void VisitFini(EndTesting end)
		{
			Unused.Arg(end);
			
			if (m_needsCheck)
			{
				List<string> fields = new List<string>();
				
				foreach (KeyValuePair<FieldReference, State> entry in m_fields)
				{
					if (entry.Value == State.Defined)
						fields.Add(entry.Key.ToString());
				}
				
				if (fields.Count > 0)
				{
					string details = "Fields: " + string.Join(Environment.NewLine, fields.ToArray());
					Log.DebugLine(this, details);
					Reporter.AssemblyFailed(Cache.Assembly, CheckID, details);
				}
			}
		}
		
		private bool m_needsCheck;
		private TypeDefinition m_type;
		private Dictionary<FieldReference, State> m_fields = new Dictionary<FieldReference, State>();
	}
}

