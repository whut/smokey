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
	internal class BaseSerializable1Rule : Rule
	{				
		public BaseSerializable1Rule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "R1017")
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
			
			if (m_method.IsConstructor)
			{		
				TypeAttributes vis = m_type.Attributes & TypeAttributes.VisibilityMask;
				if (vis == TypeAttributes.Public || vis == TypeAttributes.NestedPublic || vis == TypeAttributes.NestedFamily || vis == TypeAttributes.NestedFamORAssem)
				{
					if (m_type.BaseImplements("System.Runtime.Serialization.ISerializable", Cache))
					{
						if (m_method.Parameters.Count == 2)
						{
							if (m_method.Parameters[0].ParameterType.FullName == "System.Runtime.Serialization.SerializationInfo" &&
								m_method.Parameters[1].ParameterType.FullName == "System.Runtime.Serialization.StreamingContext")
							{
								Log.DebugLine(this, "-----------------------------------"); 
								Log.DebugLine(this, "{0:F}", begin.Info.Instructions);			
								
								m_needsCheck = true;
							}
						}	
					}
				}
			}
		}	
		
		public void VisitCall(Call call)
		{
			if (m_needsCheck && m_missingBaseCall)
			{
				Log.DebugLine(this, "checking {0}", call.Target);	
				
				string name = m_type.BaseType.Name;
				name += "::.ctor(System.Runtime.Serialization.SerializationInfo,System.Runtime.Serialization.StreamingContext)";
				
				if (call.Target.ToString().Contains(name))
					m_missingBaseCall = false;
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

