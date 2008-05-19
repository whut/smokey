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
using System;
using Smokey.Framework;
using Smokey.Framework.Instructions;
using Smokey.Framework.Support;

namespace Smokey.Internal.Rules
{	
	internal sealed class SealedProtectedRule : Rule
	{				
		public SealedProtectedRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "D1053")
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
			Log.DebugLine(this, "-----------------------------------"); 
			Log.DebugLine(this, "{0}", begin.Type);				

			m_type = begin.Type;
			m_name = null;
			m_needsCheck = m_type.IsSealed;
		}
		
		public void VisitField(FieldDefinition field)
		{				
			if (m_needsCheck && m_name == null)
			{
				if (field.IsFamilyAndAssembly || field.IsFamily || field.IsFamilyOrAssembly)
				{
					m_name = field.Name;
				}
			}
		}

		public void VisitMethod(MethodDefinition method)
		{				
			if (m_needsCheck && m_name == null)
			{
				if (method.IsFamilyAndAssembly || method.IsFamily || method.IsFamilyOrAssembly)
				{
					TypeReference tr = method.GetDeclaredIn(Cache);
					if (tr != null && tr == m_type)
					{
						m_name = method.Name;
					}
				}
			}
		}

		public void VisitEnd(EndType end)
		{
			if (m_name != null)
			{
				string details = "Member: " + m_name;
				Log.DebugLine(this, details);
				Reporter.TypeFailed(m_type, CheckID, details);
			}
		}
		
		private bool m_needsCheck;
		private TypeDefinition m_type;
		private string m_name;
	}
}

