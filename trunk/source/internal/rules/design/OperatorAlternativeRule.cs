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
using Smokey.Framework.Support;
using System;
using System.Collections.Generic;

namespace Smokey.Internal.Rules
{	
	internal class OperatorAlternativeRule : Rule
	{				
		public OperatorAlternativeRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "D1033")
		{
			m_table.Add("op_Addition", "Add");
			m_table.Add("op_Subtraction", "Subtract");
			m_table.Add("op_Multiply", "Multiply");
			m_table.Add("op_Division", "Divide");
			m_table.Add("op_Modulus", "Mod");
			m_table.Add("op_GreaterThan", "Compare");
			m_table.Add("op_GreaterThanOrEqual", "Compare");
			m_table.Add("op_LessThan", "Compare");
			m_table.Add("op_LessThanOrEqual", "Compare");
			m_table.Add("op_Inequality", "Equals");
			m_table.Add("op_Equality", "Equals");
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitBegin");
			dispatcher.Register(this, "VisitMethod");
			dispatcher.Register(this, "VisitEnd");
		}
				
		public void VisitBegin(BeginType begin)
		{
			m_needsCheck = false;
			m_type = begin.Type;
			m_details = string.Empty;
			
			TypeAttributes vis = m_type.Attributes & TypeAttributes.VisibilityMask;
			if (vis == TypeAttributes.Public || vis == TypeAttributes.NestedPublic || vis == TypeAttributes.NestedFamily || vis == TypeAttributes.NestedFamORAssem)
			{
				Log.DebugLine(this, "-----------------------------------"); 
				Log.DebugLine(this, "{0}", m_type);				

				m_needsCheck = true;
			}
		}		
		
		public void VisitMethod(MethodDefinition method)
		{			
			if (m_needsCheck && m_details.Length == 0)
			{
				MethodAttributes access = method.Attributes & MethodAttributes.MemberAccessMask;
				if (access == MethodAttributes.Public && method.IsSpecialName)
				{
					string require;
					if (m_table.TryGetValue(method.Name, out require))
					{				
						MethodDefinition[] methods = m_type.Methods.GetMethod(require);
						if (methods.Length == 0)
						{
							m_details = method.Name + " requires " + require;				
							Log.DebugLine(this, m_details);				
						}
					}
				}
			}			
		}

		public void VisitEnd(EndType end)
		{
			if (m_needsCheck && m_details.Length > 0)
			{
				Reporter.TypeFailed(m_type, CheckID, m_details);
			}
		}
		
		private bool m_needsCheck;
		private TypeDefinition m_type;
		private string m_details;
		private Dictionary<string, string> m_table = new Dictionary<string, string>();
	}
}

