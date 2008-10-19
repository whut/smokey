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

#if OLD
namespace Smokey.Internal.Rules
{		
	internal sealed class TransparentAssertRule1 : Rule
	{				
		public TransparentAssertRule1(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "S1013")
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
			m_needsCheck = assembly.CustomAttributes.Has("SecurityTransparentAttribute");
		}

		public void VisitType(TypeDefinition type)
		{
			if (m_needsCheck && DoHasAssert(type.SecurityDeclarations))
			{
				m_details += type.FullName + " ";
			}
		}
		
		public void VisitBegin(BeginMethod begin)
		{
			if (m_needsCheck)
			{
				m_failed = DoHasAssert(begin.Info.Method.SecurityDeclarations);
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
						m_failed = true;
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
			Unused.Value = end;
			
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

		private string m_details = string.Empty;
		private bool m_failed;
		private bool m_needsCheck;
	}
}
#endif
