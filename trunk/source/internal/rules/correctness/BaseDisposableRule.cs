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

namespace Smokey.Internal.Rules
{	
	internal sealed class BaseDisposableRule : Rule
	{				
		public BaseDisposableRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "C1005")
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
			m_needsCheck = false;
			m_missingBaseCall = true;
			
			m_type = begin.Info.Type;
			m_method = begin.Info.Method;
			
			if (m_method.Name == "Dispose")
			{		
				if (m_type.BaseImplements("System.IDisposable", Cache))
				{
					if (m_method.Parameters.Count == 1)
					{
						if (m_method.Parameters[0].ParameterType.FullName == "System.Boolean")
						{
							Log.DebugLine(this, "-----------------------------------"); 
							Log.DebugLine(this, "{0:F}", begin.Info.Instructions);			
							
							m_needsCheck = true;
						}
					}	
				}
			}
		}	
		
		public void VisitCall(Call call)
		{
			TypeReference type = m_type.BaseType;

			while (m_needsCheck && m_missingBaseCall && type != null)
			{
				Log.DebugLine(this, "checking {0}", call.Target);	
				
				string name = string.Format("{0}::Dispose(System.Boolean)", type.Name);
				
				if (call.Target.ToString().Contains(name))
					m_missingBaseCall = false;
					
				type = Cache.FindBase(type);
			}
		}

		public void VisitEnd(EndMethod end)
		{
			if (m_needsCheck && m_missingBaseCall)
			{		
				Reporter.MethodFailed(m_method, CheckID, 0, string.Empty);
			}
		}	
		
		private bool m_needsCheck;
		private bool m_missingBaseCall;
		private TypeDefinition m_type;
		private MethodDefinition m_method;
	}
}

