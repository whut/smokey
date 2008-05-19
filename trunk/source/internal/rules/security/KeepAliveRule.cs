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
	internal sealed class KeepAliveRule : Rule
	{				
		public KeepAliveRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "S1003")
		{
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitBegin");
			dispatcher.Register(this, "VisitField");
			dispatcher.Register(this, "VisitMethod");
			dispatcher.Register(this, "VisitEnd");
		}
		
		public void VisitBegin(BeginType begin)
		{
			MethodDefinition finalizer = begin.Type.Methods.GetMethod("Finalize", Type.EmptyTypes);
			m_hasFinalizer = finalizer != null;
			m_hasIntPtr = false;
			m_hasKeepAlive = false;
			
			if (m_hasFinalizer)
			{
				Log.DebugLine(this, "-----------------------------------"); 
				Log.DebugLine(this, "checking {0}", begin.Type);
			}
		}
		
		public void VisitField(FieldDefinition field)
		{
			if (m_hasFinalizer && !m_hasIntPtr)
			{
				string name = field.FieldType.FullName;
				if (name == "System.IntPtr" || name == "System.UIntPtr")
				{
					Log.DebugLine(this, "{0} is an IntPtr", field.Name);
					m_hasIntPtr = true;
				}
			}
		}
		
		public void VisitMethod(MethodDefinition method)
		{
			if (m_hasFinalizer && m_hasIntPtr && !m_hasKeepAlive && method.Body != null)
			{
				InstructionCollection instructions = method.Body.Instructions;
				
				for (int i = 1; i < instructions.Count && !m_hasKeepAlive; ++i)
				{
					if (instructions[i].OpCode.Code == Code.Call || instructions[i].OpCode.Code == Code.Callvirt)
					{
						MethodReference target = (MethodReference) instructions[i].Operand;							
						if (target.ToString() == "System.Void System.GC::KeepAlive(System.Object)")
						{
							m_hasKeepAlive = true;
						}
					}
				}
			}
		}
		
		public void VisitEnd(EndType end)
		{
			if (m_hasFinalizer && m_hasIntPtr && !m_hasKeepAlive)
			{			
				Reporter.TypeFailed(end.Type, CheckID, string.Empty);
			}
		}
				
		private bool m_hasFinalizer;
		private bool m_hasIntPtr;
		private bool m_hasKeepAlive;
	}
}

