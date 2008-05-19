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
	internal sealed class PermitDenySecurityRule : Rule
	{				
		public PermitDenySecurityRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "S1005")
		{
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitBegin");
			dispatcher.Register(this, "VisitCall");
			dispatcher.Register(this, "VisitEnd");
		}
				
		public void VisitBegin(BeginMethod begin)
		{
			Log.DebugLine(this, "-----------------------------------"); 
			Log.DebugLine(this, "{0:F}", begin.Info.Instructions);		
			
			m_offset = DoHasBadDemand(begin.Info.Method) ? 0 : -1;
		}
		
		public void VisitCall(Call call)
		{
			if (m_offset < 0)
			{
				if (call.Target.ToString().Contains("System.Void System.Security.CodeAccessPermission::Deny"))
					m_offset = call.Untyped.Offset;

				else if (call.Target.ToString().Contains("System.Void System.Security.CodeAccessPermission::PermitOnly"))
					m_offset = call.Untyped.Offset;
			}
		}

		public void VisitEnd(EndMethod end)
		{
			if (m_offset >= 0)
			{
				Reporter.MethodFailed(end.Info.Method, CheckID, m_offset, string.Empty);
			}
		}
		
		private bool DoHasBadDemand(MethodDefinition method)
		{
			foreach (SecurityDeclaration dec in method.SecurityDeclarations)
			{
				if (dec.Action == SecurityAction.PermitOnly || dec.Action == SecurityAction.Deny)
				{
					Log.DebugLine(this, "   {0} has a bad demand attr", method);
					return true;
				}
			}
			
			return false;
		}

		private int m_offset;
	}
}

