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
using System.Diagnostics;
using Smokey.Framework;
using Smokey.Framework.Instructions;
using Smokey.Framework.Support;

namespace Smokey.Internal.Rules
{	
	internal sealed class UnrelatedEqualsRule : Rule
	{				
		public UnrelatedEqualsRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "C1019")
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
			m_info = begin.Info;
		}
		
		public void VisitCall(Call call)
		{
			if (m_offset < 0)
			{
				if (call.Target.ToString().Contains("::Equals(") && call.Target.Parameters.Count == 1)
				{
					TypeReference t1 = DoGetType(call.Index, 0);
					TypeReference t2 = DoGetType(call.Index, 1);
		
					if (t1 != null && t2 != null)
					{	
						if (t1.FullName != "System.Object" && t2.FullName != "System.Object")
						{
							if (!t1.IsSubclassOf(t2, Cache) && !t2.IsSubclassOf(t1, Cache) && t1.FullName != t2.FullName)
							{
								TypeDefinition td1 = Cache.FindType(t1);
								TypeDefinition td2 = Cache.FindType(t2);
								
								if (td1 == null || !td1.TypeOrBaseImplements(t2.FullName, Cache))
								{
									if (td2 == null || !td2.TypeOrBaseImplements(t1.FullName, Cache))
									{					
										m_offset = call.Untyped.Offset;						
										Log.DebugLine(this, "Matched at {0:X2}", m_offset);				
									}
								}
							}
						}
					}
				}
			}
		}

		public void VisitEnd(EndMethod end)
		{
			if (m_offset >= 0)
			{
				Reporter.MethodFailed(end.Info.Method, CheckID, m_offset, string.Empty);
			}
		}
						
		public TypeReference DoGetType(int index, int nth)
		{
			TypeReference type = null;
			
			int i = m_info.Tracker.GetStackIndex(index, nth);
			if (i >= 0)
			{
				do	// TODO: would be nice to do something with load constant instructions as well
				{
					LoadArg arg = m_info.Instructions[i] as LoadArg;
					if (arg != null && arg.Arg >= 1)
					{
						ParameterDefinition p = m_info.Method.Parameters[arg.Arg - 1];
						type = p.ParameterType;
						break;
					}

					LoadField field = m_info.Instructions[i] as LoadField;
					if (field != null)
					{
						type = field.Field.FieldType;
						break;
					}

					LoadLocal local = m_info.Instructions[i] as LoadLocal;
					if (local != null)
					{
						VariableDefinition v = m_info.Method.Body.Variables[local.Variable];
						type = v.VariableType;
						break;
					}

					LoadStaticField sfield = m_info.Instructions[i] as LoadStaticField;
					if (sfield != null)
					{
						type = sfield.Field.FieldType;
						break;
					}
					
					Box box = m_info.Instructions[i] as Box;
					if (box != null)
					{
						type = box.Type;
						break;
					}
				} 
				while (false);	
			}
			
			return type;
		}

		private int m_offset;
		private MethodInfo m_info;
	}
}
