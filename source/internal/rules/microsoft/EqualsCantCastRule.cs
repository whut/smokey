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
	internal abstract class EqualsCantCastRule : Rule
	{				
		protected EqualsCantCastRule(AssemblyCache cache, IReportViolations reporter, string checkID) 
			: base(cache, reporter, checkID)
		{
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitBegin");	
			dispatcher.Register(this, "VisitCall");	
			dispatcher.Register(this, "VisitCast");	
			dispatcher.Register(this, "VisitUnbox");	
			dispatcher.Register(this, "VisitEnd");
		}
				
		protected virtual bool OnNeedsCheck(BeginMethod begin)
		{		
			bool needs = false;
			
			if (begin.Info.Method.Body != null && begin.Info.Method.Body.ExceptionHandlers.Count == 0)
				if (DoIsEquals(begin.Info.Method))
					needs = true;
			
			return needs;
		}
		
		public void VisitBegin(BeginMethod begin)
		{		
			m_needsCheck = OnNeedsCheck(begin);
			m_offset = -1;
			
			if (m_needsCheck)
			{
				Log.DebugLine(this, "-----------------------------------"); 
				Log.DebugLine(this, "{0:F}", begin.Info.Instructions);				
				
				m_foundTypeCheck = false;
				m_info = begin.Info;

				m_tracker = new Tracker(m_info.Instructions);
				m_tracker.InitArg(1, -33);		// give arg1 a magic value so we can track it
				m_tracker.Analyze(m_info.Graph);
			}
		}
		
		public void VisitUnbox(Unbox unbox)
		{
			if (m_needsCheck && !m_foundTypeCheck && m_offset < 0)
			{
				// If we find a box to our struct type, and it's using our 
				// argument, then we have a problem.
				if (unbox.FromType.FullName == m_info.Instructions.Method.DeclaringType.FullName)
				{
					if (m_tracker.GetStack(unbox.Index, 0) == -33)
					{
						m_offset = unbox.Untyped.Offset;
						Log.DebugLine(this, "found unbox at {0:X2}", m_offset); 
					}
				}
			}
		}

		public void VisitCall(Call call)
		{
			if (m_needsCheck && !m_foundTypeCheck)
			{
				// If the method has a GetType call we'll assume that it isn't 
				// completely silly.
				if (call.Target.ToString().Contains("Object::GetType()"))
				{
					if (m_tracker.GetStack(call.Index, 0) == -33)
					{
						m_foundTypeCheck = true;
						Log.DebugLine(this, "found GetType call at {0:X2}", call.Untyped.Offset); 
					}
				}
			}
		}

		public void VisitCast(CastClass cast)
		{
			if (m_needsCheck && !m_foundTypeCheck)
			{
				// If the method has a isinst check we'll assume that it isn't 
				// completely silly.
				if (cast.Untyped.OpCode.Code == Code.Isinst)
				{
					if (cast.ToType.FullName == m_info.Method.DeclaringType.FullName)
					{
						if (m_tracker.GetStack(cast.Index, 0) == -33)
						{
							m_foundTypeCheck = true;
							Log.DebugLine(this, "found isinst call at {0:X2}", cast.Untyped.Offset); 
						}
					}
				}
			
				// If we find a cast to our class type, and it's using our argument, 
				// then we have a problem.
				else if (m_offset < 0)
				{
					if (cast.ToType.FullName == m_info.Method.DeclaringType.FullName)
					{
						if (m_tracker.GetStack(cast.Index, 0) == -33)
						{
							m_offset = cast.Untyped.Offset;
							Log.DebugLine(this, "found cast at {0:X2}", m_offset); 
						}
					}
				}
			}
		}

		public void VisitEnd(EndMethod end)
		{
			if (m_offset >= 0 && !m_foundTypeCheck)
				Reporter.MethodFailed(end.Info.Method, CheckID, m_offset, string.Empty);

			m_tracker = null;
		}

		private static bool DoIsEquals(MethodDefinition method)
		{
			bool equals = false;
			
			if (method.Name == "Equals")
			{		
				if (method.Parameters.Count == 1)	
				{
					if (method.IsVirtual && !method.IsNewSlot)
						equals = method.Parameters[0].ParameterType.FullName == "System.Object";
				}
			}
			
			return equals;
		}
		
		private MethodInfo m_info;
		private Tracker m_tracker;
		private int m_offset;
		private bool m_needsCheck;
		private bool m_foundTypeCheck;
	}

	internal class EqualsCantCastRule1 : EqualsCantCastRule
	{				
		public EqualsCantCastRule1(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "MS1004")
		{
		}
		
		protected override bool OnNeedsCheck(BeginMethod method)
		{		
			bool needs = false;
			
			if (!method.Info.Method.DeclaringType.IsValueType)	// classes and struct equals are implemeneted differently so we'll provide two forms of help
				needs = base.OnNeedsCheck(method);
			
			return needs;
		}
	}

	internal class EqualsCantCastRule2 : EqualsCantCastRule
	{				
		public EqualsCantCastRule2(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "MS1005")
		{
		}
		
		protected override bool OnNeedsCheck(BeginMethod method)
		{		
			bool needs = false;
			
			if (method.Info.Method.DeclaringType.IsValueType)
				needs = base.OnNeedsCheck(method);
			
			return needs;
		}
	}
}

