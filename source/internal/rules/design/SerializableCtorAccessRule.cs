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
	internal sealed class SerializableCtorAccessRule : Rule
	{				
		public SerializableCtorAccessRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "D1029")
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
			m_correct = true;
			m_type = begin.Type;
			
			if (begin.Type.TypeOrBaseImplements("System.Runtime.Serialization.ISerializable", Cache))
			{
				if (!begin.Type.IsInterface)
				{
					if (!begin.Type.IsSubclassOf("System.Delegate", Cache) && !begin.Type.IsSubclassOf("System.MulticastDelegate", Cache))
					{
						Log.DebugLine(this, "-----------------------------------"); 
						Log.DebugLine(this, "checking {0}", begin.Type);				
			
						m_needsCheck = true;
					}
				}
			}
		}
		
		public void VisitMethod(MethodDefinition method)
		{						
			if (m_needsCheck)
			{
				if (method.IsConstructor && method.Parameters.Count == 2)
				{
					if (method.Parameters[0].ParameterType.FullName == "System.Runtime.Serialization.SerializationInfo" &&
						method.Parameters[1].ParameterType.FullName == "System.Runtime.Serialization.StreamingContext")
					{						
						MethodAttributes attrs = method.Attributes & MethodAttributes.MemberAccessMask;
						Log.DebugLine(this, "found ctor, access: {0}", attrs); 
						
						if (m_type.IsSealed)
							m_correct = attrs == MethodAttributes.Private;
						else
							m_correct = attrs == MethodAttributes.Family;
					}
				}
			}
		}

		public void VisitEnd(EndType end)
		{			
			if (m_needsCheck && !m_correct)
			{
				Reporter.TypeFailed(end.Type, CheckID, string.Empty);
			}
		}
		
		private TypeDefinition m_type;
		private bool m_needsCheck;
		private bool m_correct;
	}
}

