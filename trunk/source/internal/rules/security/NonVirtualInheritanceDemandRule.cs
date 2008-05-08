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
	internal class NonVirtualInheritanceDemandRule : Rule
	{				
		public NonVirtualInheritanceDemandRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "S1017")
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
			Log.DebugLine(this, "-----------------------------------"); 
			Log.DebugLine(this, "checking {0}", begin.Type);
		
			m_details = string.Empty;
		}

		public void VisitMethod(MethodDefinition method)
		{
			if (DoHasInheritanceDemand(method.SecurityDeclarations))
			{
				if (!method.IsVirtual)
				{
					Log.DebugLine(this, "{0} is not overridable", method.Name);
					m_details = m_details + " " + method;
				}
			}
		}
		
		public void VisitEnd(EndType end)
		{
			if (m_details.Length > 0)
			{
				Reporter.TypeFailed(end.Type, CheckID, m_details);
			}
		}
		
		private static bool DoHasInheritanceDemand(SecurityDeclarationCollection decs)
		{
			foreach (SecurityDeclaration dec in decs)
			{
				if (dec.Action == SecurityAction.InheritDemand || dec.Action == SecurityAction.NonCasInheritance)
				{
					return true;
				}
			}
			
			return false;
		}
		
		private string m_details;
	}
}

