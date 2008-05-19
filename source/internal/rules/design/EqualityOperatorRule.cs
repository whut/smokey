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
	internal sealed class EqualityOperatorRule : Rule
	{				
		public EqualityOperatorRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "D1036")
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
			Log.DebugLine(this, "{0}", begin.Type);				

			m_type = begin.Type;
			m_needsCheck = false;
		}		
		
		public void VisitMethod(MethodDefinition method)
		{			
			Log.DebugLine(this, "{0}", method.Name);				

			if (!m_type.IsNestedPrivate && m_type.IsValueType && !m_needsCheck && method.IsVirtual)
			{
				MethodAttributes vtable = method.Attributes & MethodAttributes.VtableLayoutMask;
				if (vtable == MethodAttributes.ReuseSlot)
				{
					if (method.Name == "Equals")
					{
						m_needsCheck = true;
					}
				}
			}			
		}

		public void VisitEnd(EndType end)
		{
			if (m_needsCheck)
			{
				MethodDefinition[] methods = m_type.Methods.GetMethod("op_Equality");
				if (methods.Length == 0)
				{
					Log.DebugLine(this, "no ==");				
					Reporter.TypeFailed(m_type, CheckID, string.Empty);
				}
			}
		}
				
		private bool m_needsCheck;
		private TypeDefinition m_type;
	}
}

