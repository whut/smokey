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
using Smokey.Framework.Support.Advanced;
using Smokey.Framework.Support.Advanced.Values;

namespace Smokey.Internal.Rules
{		
	internal sealed class EqualsRequiresNullCheckRule : Rule
	{				
		public EqualsRequiresNullCheckRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "MS1020")
		{
		}
						
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitBegin");	
			dispatcher.Register(this, "VisitReturn");	
			dispatcher.Register(this, "VisitCast");	
			dispatcher.Register(this, "VisitInstruction");	
			dispatcher.Register(this, "VisitEnd");
		}
				
		public void VisitBegin(BeginMethod begin)
		{		
			m_needsCheck = false;
			m_offset = -1;
			m_details = string.Empty;
			
			if (!begin.Info.Method.DeclaringType.IsValueType)	// structs can't be null so they're ok
			{
				if (DoIsEquals(begin.Info.Method))
				{
					m_tracker = new Tracker(begin.Info.Instructions);
					m_tracker.InitArg(1, 0);		// 0 is the this ptr
					m_tracker.Analyze(begin.Info.Graph);

					m_needsCheck = true;
				}
				else if (DoIsOperatorEquals(begin.Info.Method))
				{
					m_tracker = new Tracker(begin.Info.Instructions);
					m_tracker.InitArg(0, 0);		// static method so no this pointer
					m_tracker.InitArg(1, 0);
					m_tracker.Analyze(begin.Info.Graph);
					
					m_needsCheck = true;
				}
			}
			
			if (m_needsCheck)
			{
				Log.DebugLine(this, "-----------------------------------"); 
				Log.DebugLine(this, "{0:F}", begin.Info.Instructions);				
				
				m_foundEarlyReturn = false;
				m_info = begin.Info;
			}
		}
		
		// In general an Equals method will be either completely wrong or correct.
		// But correct methods often have a lot of control flow with early returns
		// which make them difficult to analyze. So, we'll be conservative and
		// consider a method OK if it contains an early return or is check.
		public void VisitReturn(End end)
		{
			if (m_needsCheck && !m_foundEarlyReturn && end.Index < m_info.Instructions.Length - 1 && m_offset < 0)
			{
				m_foundEarlyReturn = true;
				Log.DebugLine(this, "early return {0:X2}", end.Untyped.Offset); 
			}
		}

		public void VisitCast(CastClass cast)
		{
			if (m_needsCheck && !m_foundEarlyReturn && m_offset < 0)
			{
				if (cast.Untyped.OpCode.Code == Code.Isinst)
				{
					if (m_info.Instructions[cast.Index + 1].Untyped.OpCode.Code == Code.Brfalse || 
						m_info.Instructions[cast.Index + 1].Untyped.OpCode.Code == Code.Brfalse_S)
					{
						m_foundEarlyReturn = true;
						Log.DebugLine(this, "isinst check {0:X2}", cast.Untyped.Offset); 
					}
				}
			}
		}

		public void VisitInstruction(TypedInstruction instruction)
		{
			if (m_needsCheck && !m_foundEarlyReturn && m_offset < 0)
			{
				m_offset = NullCheck.Check<EqualsRequiresNullCheckRule>(instruction, m_tracker, out m_details);
			}
		}

		public void VisitEnd(EndMethod end)
		{
			if (m_offset >= 0 && !m_foundEarlyReturn)
				Reporter.MethodFailed(end.Info.Method, CheckID, m_offset, m_details);

			m_tracker = null;
		}
		
		// Return true if the method matches one of:
		// 		public override bool Equals(object)
		// 		public bool Equals(xxx), where xxx is not a value type
		private static bool DoIsEquals(MethodDefinition method)
		{
			bool equals = false;
			
			if (method.Name == "Equals")
			{
				if (method.Parameters.Count == 1)
				{
					if (method.IsVirtual && !method.IsNewSlot)
					{
						equals = method.Parameters[0].ParameterType.FullName == "System.Object";
					}
					else if (!method.IsVirtual && !method.IsStatic)
					{
						if (method.Parameters[0].ParameterType.FullName == method.DeclaringType.FullName)
							if (!method.Parameters[0].ParameterType.IsValueType)
								equals = true;
					}
				}
			}
			
			return equals;
		}
		
		// Return true if the method matches:
		// 		public static bool operator==(xxx, xxx), where xxx is not a value type
		private static bool DoIsOperatorEquals(MethodDefinition method)
		{
			bool equals = false;
			
			if (method.Name == "op_Equality")
			{
				if (method.Parameters.Count == 2)
				{
					if (method.IsStatic)
					{
						if (method.Parameters[0].ParameterType.FullName == method.Parameters[1].ParameterType.FullName)
							if (method.Parameters[0].ParameterType.FullName == method.DeclaringType.FullName)
								if (!method.Parameters[0].ParameterType.IsValueType)
								equals = true;
					}
				}
			}
			
			return equals;
		}
		
		private MethodInfo m_info;
		private Tracker m_tracker;
		private int m_offset;
		private bool m_needsCheck;
		private bool m_foundEarlyReturn;
		private string m_details;
	}
}

