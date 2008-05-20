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
	internal sealed class UnprotectedEventRule : Rule
	{				
		public UnprotectedEventRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "C1023")
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
		
		// 		18: ldarg.0    this
		// 19: ldfld      System.EventHandler Good1::Event1					3) Guard using same load instruction
		// 1E: brfalse    34
		// 
		// 		23: ldarg.0    this
		// 24: ldfld      System.EventHandler Good1::Event1					2) Load of this argument
		// 		29: ldarg.0    this
		// 		2A: ldsfld     System.EventArgs System.EventArgs::Empty
		// 
		// 2F: callvirt   System.Void System.EventHandler::Invoke(System.Object,System.EventArgs)	1) Call to invoke
		public void VisitCall(Call call)
		{
			if (m_offset < 0)
			{
				// 1) See if invoke was called.
				string name = call.Target.ToString();
				if (name.StartsWith("System.Void System.EventHandler") && name.Contains("::Invoke(") && call.Target.Parameters.Count == 2)
				{
					Log.DebugLine(this, "found event invoke at {0:X2}", call.Untyped.Offset);				

					// 2) Get the instruction used to load the this argument for invoke.
					int i = m_info.Tracker.GetStackIndex(call.Index, 2);
					if (i >= 0)
					{
						Load load1 = m_info.Instructions[i] as Load;
						if (load1 != null)
						{
							Log.DebugLine(this, "event target is loaded at {0:X2}", load1.Untyped.Offset);				

							// 3) Backup until we find a conditional branch. If it's not brfalse or
							// not using the same instruction as in step 2 then we have a problem.
							bool foundGuard = false;
							
							int j = load1.Index - 1;
							while (j > 0 && !foundGuard)
							{
								ConditionalBranch branch = m_info.Instructions[j] as ConditionalBranch;
								if (branch != null)
								{
									if (branch.Untyped.OpCode.Code == Code.Brfalse || branch.Untyped.OpCode.Code == Code.Brfalse_S)
									{
										Load load2 = m_info.Instructions[j - 1] as Load;
										if (load2 != null)
										{
											if (load2.Untyped.OpCode.Code == load1.Untyped.OpCode.Code && load2.Untyped.Operand == load1.Untyped.Operand)
											{
												Log.DebugLine(this, "found guard at {0:X2}", branch.Untyped.Offset);				
												foundGuard = true;
											}
										}
									}
								}
								
								--j;
							}
							
							if (!foundGuard)
							{
								m_offset = call.Untyped.Offset;						
								Log.DebugLine(this, "no guard for event at {0:X2}", m_offset);				
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
		
		private int m_offset;
		private MethodInfo m_info;
	}
}
