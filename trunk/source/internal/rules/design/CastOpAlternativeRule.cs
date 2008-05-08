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
	internal class CastOpAlternativeRule : Rule
	{				
		public CastOpAlternativeRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "D1034")
		{
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
			m_failed = false;
			
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
			if (m_needsCheck && !m_failed)
			{
				MethodAttributes access = method.Attributes & MethodAttributes.MemberAccessMask;
				if (access == MethodAttributes.Public && method.IsSpecialName)
				{
					if (method.Name == "op_Explicit" || method.Name == "op_Implicit")
					{	
						string name = method.ReturnType.ReturnType.Name;
						if (!DoHas("From" + name + "Type"))
						{
							m_failed = true;
						}
						else if (!DoHas("To" + name + "Type"))
						{
							m_failed = true;
						}
					}
				}
			}			
		}

		public void VisitEnd(EndType end)
		{
			if (m_needsCheck && m_failed)
			{
				Reporter.TypeFailed(m_type, CheckID, string.Empty);
			}
		}
		
		private bool DoHas(string name)
		{
			MethodDefinition[] methods = m_type.Methods.GetMethod(name);
			return methods.Length > 0;
		}
		
		private bool m_needsCheck;
		private TypeDefinition m_type;
		private bool m_failed;
	}
}

