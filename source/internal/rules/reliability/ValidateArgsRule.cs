// Copyright (C) 2008 Jesse Jones
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
using Smokey.Framework.Instructions;
using Smokey.Framework.Support;

using System;
using System.Collections.Generic;

namespace Smokey.Internal.Rules
{	
	internal abstract class BaseValidateArgsRule : Rule
	{				
		public BaseValidateArgsRule(AssemblyCache cache, IReportViolations reporter, string checkID) 
			: base(cache, reporter, checkID)
		{
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitBegin");
			dispatcher.Register(this, "VisitBranch");
			dispatcher.Register(this, "VisitCeq");
			dispatcher.Register(this, "VisitCall");
			dispatcher.Register(this, "VisitEnd");
		}
				
		public void VisitBegin(BeginMethod begin)	
		{
			m_offset = -1;
			m_info = begin.Info;
			m_badArg = null;
			m_needsCheck = OnNeedsCheck(m_info.Type) && OnNeedsCheck(m_info.Type, m_info.Method);
			m_table.Clear();
			
			if (m_needsCheck)
			{
				Log.DebugLine(this, "-----------------------------------"); 
				Log.DebugLine(this, "{0:F}", begin.Info.Instructions);	
				
				foreach (ParameterDefinition p in m_info.Method.Parameters)
				{
					if (!p.ParameterType.IsValueType)
					{
						Log.DebugLine(this, "{0} is a reference type", p.Name); 
						m_table.Add(p.Name, false);
					}
				}
			}
			
			if (m_table.Count == 0)
				m_needsCheck = false;
		}
		
		// ldarg.1    list
		// brtrue     11
		public void VisitBranch(ConditionalBranch branch)
		{
			if (m_needsCheck && m_offset < 0)
			{
				if (branch.Untyped.OpCode.Code == Code.Brtrue || branch.Untyped.OpCode.Code == Code.Brtrue_S || branch.Untyped.OpCode.Code == Code.Brfalse || branch.Untyped.OpCode.Code == Code.Brfalse_S)
				{
					LoadArg load = m_info.Instructions[branch.Index - 1] as LoadArg;
					if (load != null)
					{
						m_table[load.Name] = true;
						Log.DebugLine(this, "found a compare at {0:X2}", branch.Untyped.Offset); 
					}
				}
			}
		}

		// ldarg.1    list
		// ldnull     
		// ceq 
		public void VisitCeq(Ceq ceq)
		{
			if (m_needsCheck && m_offset < 0)
			{
				LoadNull load1 = m_info.Instructions[ceq.Index - 1] as LoadNull;
				LoadArg load2 = m_info.Instructions[ceq.Index - 2] as LoadArg;

				if (load1 == null && load2 == null)
				{
					load1 = m_info.Instructions[ceq.Index - 2] as LoadNull;
					load2 = m_info.Instructions[ceq.Index - 1] as LoadArg;
				}

				if (load1 != null && load2 != null)
				{
					m_table[load2.Name] = true;
					Log.DebugLine(this, "found a compare at {0:X2}", ceq.Untyped.Offset); 
				}
			}
		}

		public void VisitCall(Call call)
		{
			if (m_needsCheck && m_offset < 0 && call.Target.HasThis)
			{
				int numArgs = call.Target.Parameters.Count;
				int index = m_info.Tracker.GetStackIndex(call.Index, numArgs);
				
				if (index >= 0)
				{
					Log.DebugLine(this, "found this arg at {0:X2}", m_info.Instructions[index].Untyped.Offset); 
					LoadArg load = m_info.Instructions[index] as LoadArg;
					
					if (load != null)
					{
						bool found;
						if (m_table.TryGetValue(load.Name, out found))
						{
							if (!found)
							{
								m_offset = call.Untyped.Offset;
								m_badArg = load.Name;
								Log.DebugLine(this, "bad call at {0:X2}", m_offset); 
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
				string details = "Arg: " + m_badArg;
				Log.DebugLine(this, details);
				Reporter.MethodFailed(end.Info.Method, CheckID, m_offset, string.Empty);
			}
		}
				
		protected virtual bool OnNeedsCheck(TypeDefinition type) 
		{
			return true;
		}
		
		protected virtual bool OnNeedsCheck(TypeDefinition type, MethodDefinition method) 
		{
			return true;
		}
		
		private bool m_needsCheck;
		private int m_offset;
		private string m_badArg;
		private MethodInfo m_info;
		private Dictionary<string, bool> m_table = new Dictionary<string, bool>();
	}

	internal class ValidateArgs1Rule : BaseValidateArgsRule
	{				
		public ValidateArgs1Rule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "R1034")
		{
		}
		
		protected override bool OnNeedsCheck(TypeDefinition type) 
		{
			return type.IsPublic || type.IsNestedPublic;
		}
				
		protected override bool OnNeedsCheck(TypeDefinition type, MethodDefinition method) 
		{
			return method.IsPublic;
		}
	}

	internal class ValidateArgs2Rule : BaseValidateArgsRule
	{				
		public ValidateArgs2Rule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "R1035")
		{
		}
		
		protected override bool OnNeedsCheck(TypeDefinition type) 
		{
			return !type.IsNestedPrivate && !type.IsNestedFamily && !type.IsNestedFamilyAndAssembly;
		}
				
		protected override bool OnNeedsCheck(TypeDefinition type, MethodDefinition method) 
		{
			if (type.IsPublic || type.IsNestedPublic)
				return method.IsAssembly;
			else
				return method.IsPublic || method.IsAssembly;
		}
	}
}

