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
using System.Security;
using System.Collections.Generic;
using Smokey.Framework;
using Smokey.Framework.Instructions;
using Smokey.Framework.Support;
using Smokey.Framework.Support.Advanced;

namespace Smokey.Internal.Rules
{		
	internal sealed class TransparentAssertRule2 : Rule
	{				
		public TransparentAssertRule2(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "S1014")
		{
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitAssembly");
			dispatcher.Register(this, "VisitBegin");
			dispatcher.Register(this, "VisitCall");
			dispatcher.Register(this, "VisitEnd");
			dispatcher.Register(this, "VisitType");
			dispatcher.Register(this, "VisitFini");
		}
		
		public void VisitAssembly(AssemblyDefinition assembly)
		{
			m_details = string.Empty;
			m_needsCheck = false;
			
			foreach (CustomAttribute attr in assembly.CustomAttributes)
			{
				if (attr.Constructor.DeclaringType.Name == "SecurityCriticalAttribute")
				{
					if (attr.ConstructorParameters.Count == 0)
					{
						m_needsCheck = true;
						break;
					}
				}
			}
		}

		public void VisitType(TypeDefinition type)
		{			
			if (m_needsCheck && DoHasAssert(type.SecurityDeclarations))
			{
				if (type.CustomAttributes.Has("SecurityCriticalAttribute"))
				{
					m_details += type.FullName + " ";
				}
			}
		}
		
		public void VisitBegin(BeginMethod begin)
		{
			if (m_needsCheck)
			{
				m_type = begin.Info.Type;
				m_method = begin.Info.Method;
				m_failed = false;
				
				if (DoHasAssert(m_method.SecurityDeclarations))
				{
					if (!m_method.CustomAttributes.Has("SecurityCriticalAttribute") && !m_type.CustomAttributes.Has("SecurityCriticalAttribute"))
					{
						m_failed = true;
					}
				}
			}
		}

		public void VisitCall(Call call)
		{
			if (m_needsCheck && !m_failed)
			{
				if (call.Target.Name == "Assert" && call.Target.Parameters.Count == 0)
				{					
					TypeReference type = call.Target.GetDeclaredIn(Cache);
					if (type != null && type.FullName == "System.Security.IStackWalk")
					{
						if (!m_method.CustomAttributes.Has("SecurityCriticalAttribute") && !m_type.CustomAttributes.Has("SecurityCriticalAttribute"))
						{
							m_failed = true;
						}
					}
				}
			}
		}

		public void VisitEnd(EndMethod end)
		{
			if (m_needsCheck && m_failed)
				m_details += end.Info.Method.ToString() + " ";
		}
		
		public void VisitFini(EndTesting end)
		{
			Unused.Arg(end);
			
			if (m_details.Length > 0)
			{
				m_details = "Asserting: " + m_details;
				Reporter.AssemblyFailed(Cache.Assembly, CheckID, m_details);
			}
		}
	
		private bool DoHasAssert(SecurityDeclarationCollection secs)
		{
			foreach (SecurityDeclaration dec in secs)
			{
				if (dec.Action == SecurityAction.Assert)	
				{
					Log.DebugLine(this, "   has an assert attr");
					return true;
				}
			}
			
			return false;
		}

		private bool m_needsCheck;
		private string m_details = string.Empty;
		
		private TypeDefinition m_type;
		private MethodDefinition m_method;
		private bool m_failed;
	}
}
