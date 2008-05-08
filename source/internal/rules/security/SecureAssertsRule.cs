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
	internal class SecureAssertsRule : Rule
	{				
		public SecureAssertsRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "S1010")
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
			
			m_offset = -1;
		}
		
		public void VisitCall(Call call)
		{
			if (m_offset < 0)
			{
				if (call.Target.Name == "Assert" && call.Target.Parameters.Count == 0)
				{
					Log.DebugLine(this, "   calls assert method at {0:X2}", call.Untyped.Offset);		
					
					TypeReference type = call.Target.GetDeclaredIn(Cache);
					if (type != null && type.FullName == "System.Security.IStackWalk")
					{
						Log.DebugLine(this, "   assert is defined in IStackWalk");		

						m_offset = call.Untyped.Offset;
					}
				}
			}
		}

		public void VisitEnd(EndMethod end)
		{
			if (m_offset < 0 && DoHasAssert(end.Info.Method.SecurityDeclarations))
				m_offset = 0;
				
			if (m_offset >= 0)
			{
				if (!DoHasDemand(end.Info.Type.SecurityDeclarations) && !DoHasDemand(end.Info.Method.SecurityDeclarations))
				{
					Reporter.MethodFailed(end.Info.Method, CheckID, m_offset, string.Empty);
				}
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

		private bool DoHasDemand(SecurityDeclarationCollection secs)
		{
			foreach (SecurityDeclaration dec in secs)
			{
				if (dec.Action == SecurityAction.NonCasDemand || dec.Action == SecurityAction.NonCasLinkDemand || dec.Action == SecurityAction.Demand || dec.Action == SecurityAction.LinkDemand)	// TODO: may be a mono or cecil bug here, doesn't seem like we should have to check NonCasDemand
				{
					Log.DebugLine(this, "   has a demand attr");
					return true;
				}
			}
			
			return false;
		}

		private int m_offset;
	}
}

