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
using Smokey.Framework;
using Smokey.Framework.Instructions;
using Smokey.Framework.Support;

namespace Custom
{	
	internal sealed class LogStartsWithStringRule : Rule
	{				
		// The checkID must match the id in the xml. Note that multiple classes can
		// share the same checkID.
		public LogStartsWithStringRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "CU1002 - example custom rules")
		{
        }
		
		// This is where you register the types that you want to visit. You can
		// visit AssemblyDefinition, TypeDefinition, MethodDefinition, the types
		// within the method, etc. See RuleDispatcher for the complete list.
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitBegin");
			dispatcher.Register(this, "VisitCall");
			dispatcher.Register(this, "VisitEnd");
		}
		
		// This is pretty typical for methods that visit instructions: first
		// we visit BeginMethod so that we can reset our state. Then we visit
		// the instructions (call and callvirt in our case). And, finally we
		// visit EndMethod to report the results.
		public void VisitBegin(BeginMethod begin)
		{			
			Log.DebugLine(this, "-----------------------------------"); 
			Log.DebugLine(this, "{0:F}", begin.Info.Instructions);				

			m_offset = -1;
			m_info = begin.Info;
		}
		
		// This is where we do our real work.
		public void VisitCall(Call call)
		{
			// First we'll check to see if we've already found a bad log. If we
			// have we can skip the work below (and also avoid setting m_offset
			// to point to a later log).
			if (m_offset < 0)
			{
				// Then we check to see if the target of the call is a Smokey
				// log method.
				if (DoIsLogCall(call.Target))
				{
					Log.DebugLine(this, "found log call at {0:X2}", call.Untyped.Offset); 
	
					// If it is we need to find the type of the first argument.
					// To do this we find the number of arguments used in the log
					// call and then use the Tracker to find the index of the
					// instruction that pushed the first argument onto the stack.
					// Note that the top of the stack is at zero (which is why we
					// subtract one from Parameters.Count).
					int nth = call.Target.Parameters.Count - 1;
					int index = m_info.Tracker.GetStackIndex(call.Index, nth);
					
					// If we found an index (we won't if the stack entry is set
					// from multiple code paths which is fairly rare),
					if (index >= 0)
					{
						// then we need to figure out the type of the object on the
						// stack. The tracker tracks values, not types, so we can't
						// use it, but all we really care about for this rule are
						// string literals so we can simply see if it's a ldstr
						// instruction.
						if (m_info.Instructions[index].Untyped.OpCode.Code == Code.Ldstr)
						{
							// If it is a ldstr then we need to record the offset
							// of the bad call to Smokey.Log.
							m_offset = call.Untyped.Offset;						
							Log.DebugLine(this, "first argument (at {0:X2}) is a string", m_offset); 
						}
					}
				}
			}
		}

		// Once RuleDispatcher is done visiting the method it will visit EndMethod.
		// We'll report failures here (this rule is simple enough that we could 
		// eliminate this method and report failures in VisitCall).
		public void VisitEnd(EndMethod end)
		{
			if (m_offset >= 0)
			{
				Reporter.MethodFailed(end.Info.Method, CheckID, m_offset, string.Empty);
			}
		}
		
		private static bool DoIsLogCall(MethodReference target)
		{
			string name = target.ToString();
			
			if (name.Contains("Smokey.Framework.Log::ErrorLine"))
				return true;
			
			else if (name.Contains("Smokey.Framework.Log::WarningLine"))
				return true;
			
			else if (name.Contains("Smokey.Framework.Log::InfoLine"))
				return true;
			
			else if (name.Contains("Smokey.Framework.Log::TraceLine"))
				return true;
			
			else if (name.Contains("Smokey.Framework.Log::DebugLine"))
				return true;

			return false;
		}
				
		private int m_offset;
		private MethodInfo m_info;
	}
}

