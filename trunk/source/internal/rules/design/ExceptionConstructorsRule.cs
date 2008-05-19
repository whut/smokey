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
using Smokey.Framework.Support;

namespace Smokey.Internal.Rules
{		
	internal sealed class ExceptionConstructorsRule : Rule
	{				
		public ExceptionConstructorsRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "D1015")
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
			m_needsCheck = begin.Type.IsSubclassOf("System.Exception", Cache);
						
			if (m_needsCheck)
			{
				Log.DebugLine(this, "-----------------------------------"); 
				Log.DebugLine(this, "{0}", begin.Type);				

				m_foundDefault = false;
				m_foundInner = false;
				m_foundSerialized = false;
			}
		}
		
		public void VisitMethod(MethodDefinition method)
		{			
			if (m_needsCheck && method.IsConstructor)
			{
				Log.DebugLine(this, "{0}", method);		
				
				if (method.Parameters.Count == 0)
				{
					Log.DebugLine(this, "   found default ctor");				
					m_foundDefault = true;
				}
				
				for (int i = 0; i < method.Parameters.Count && !m_foundInner; ++i)
				{		
					if (method.Parameters[i].ParameterType.FullName == "System.Exception")
					{
						Log.DebugLine(this, "   found inner ctor");				
						m_foundInner = true;
					}
				}
				
				if (method.Parameters.Count == 2)
				{
					if (method.Parameters[0].ParameterType.FullName == "System.Runtime.Serialization.SerializationInfo")
					{
						if (method.Parameters[1].ParameterType.FullName == "System.Runtime.Serialization.StreamingContext")
						{
							Log.DebugLine(this, "   found serialize ctor");				
							m_foundSerialized = true;
						}
					}
				}
			}
		}

		public void VisitEnd(EndType end)
		{
			if (m_needsCheck)
			{
				if (!m_foundDefault || !m_foundInner || !m_foundSerialized)
				{
					Reporter.TypeFailed(end.Type, CheckID, string.Empty);
				}
			}
		}
		
		private bool m_needsCheck;
		private bool m_foundDefault;
		private bool m_foundInner;
		private bool m_foundSerialized;
	}
}

