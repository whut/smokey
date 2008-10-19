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
	internal sealed class AttributePropertiesRule : Rule
	{				
		public AttributePropertiesRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "D1009")
		{
		}
		
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitBegin");
			dispatcher.Register(this, "VisitField");
			dispatcher.Register(this, "VisitMethod");
			dispatcher.Register(this, "VisitEnd");
		}
				
		public void VisitBegin(BeginType begin)
		{
			m_needsCheck = begin.Type.IsSubclassOf("System.Attribute", Cache);
				Log.DebugLine(this, "{0}", begin.Type);	
			
			if (m_needsCheck)
			{
				Log.DebugLine(this, "-----------------------------------"); 
				Log.DebugLine(this, "{0}", begin.Type);	
				
				m_required.Clear();
				m_optional.Clear();
				m_getters.Clear();
				m_setters.Clear();
			}
		}
		
		public void VisitField(FieldDefinition field)
		{
			if (m_needsCheck)
			{
				if ((field.Attributes & FieldAttributes.Static) == 0)
				{
					if ((field.Attributes & FieldAttributes.FieldAccessMask) != FieldAttributes.Public)
					{
						Log.DebugLine(this, "found field {0}", field.Name);	
						m_optional.Add(field.Name);
					}
				}
			}
		}
								
		// It's tricky to know if the ctor is seting a field with an argument without
		// all the MethodInfo machinery so we'll be conservative and say that any
		// field set in a ctor is a required field.
		public void VisitMethod(MethodDefinition method)
		{
			if (m_needsCheck && method.Body != null)
			{
				if (method.IsConstructor)
				{
					for (int i = 2; i < method.Body.Instructions.Count; ++i)
					{
						if (method.Body.Instructions[i].OpCode.Code == Code.Stfld)
						{
							FieldReference field = (FieldReference) method.Body.Instructions[i].Operand;
							Log.DebugLine(this, "ctor is setting field {0}", field.Name);
							
							if (m_optional.Remove(field.Name))
								m_required.Add(field.Name);
						}
					}
				}
				else if (method.SemanticsAttributes == MethodSemanticsAttributes.Getter)
				{
					for (int i = 0; i < method.Body.Instructions.Count; ++i)
					{
						if (method.Body.Instructions[i].OpCode.Code == Code.Ldfld)
						{
							FieldReference field = (FieldReference) method.Body.Instructions[i].Operand;
							Log.DebugLine(this, "getter for field {0}", field.Name);
							
							m_getters.Add(field.Name);
						}
					}
				}
				else if (method.SemanticsAttributes == MethodSemanticsAttributes.Setter)
				{
					for (int i = 0; i < method.Body.Instructions.Count; ++i)
					{
						if (method.Body.Instructions[i].OpCode.Code == Code.Stfld)
						{
							FieldReference field = (FieldReference) method.Body.Instructions[i].Operand;
							Log.DebugLine(this, "setter for field {0}", field.Name);
							
							m_setters.Add(field.Name);
						}
					}
				}
			}
		}
				
		public void VisitEnd(EndType end)
		{
			if (m_needsCheck)
			{
				string details = string.Empty;
				
				foreach (string name in m_optional)
				{
					if (m_getters.IndexOf(name) < 0 && m_setters.IndexOf(name) < 0)
						details = string.Format("Optional field {0} needs a getter and a setter. {1}", name, details);

					else if (m_getters.IndexOf(name) < 0 )
						details = string.Format("Optional field {0} needs a getter. {1}", name, details);

					else if (m_setters.IndexOf(name) < 0)
						details = string.Format("Optional field {0} needs a setter. {1}", name, details);
				}
				
				foreach (string name in m_required)
				{
					if (m_getters.IndexOf(name) < 0)
						details = string.Format("Required field {0} needs a getter. {1}", name, details);
				}
				
				if (details.Length > 0)
				{
					Log.DebugLine(this, details);
					Reporter.TypeFailed(end.Type, CheckID, details);
				}
			}
		}
				
		private bool m_needsCheck;
		private List<string> m_required = new List<string>();
		private List<string> m_optional = new List<string>();

		private List<string> m_getters = new List<string>();
		private List<string> m_setters = new List<string>();
	}
}
#endif
