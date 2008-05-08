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
	internal class UnprotectedEventRule : Rule
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
		
		public void VisitCall(Call call)
		{
			if (m_offset < 0)
			{
				string name = call.Target.ToString();
				if (name.StartsWith("System.Void System.EventHandler") && name.Contains("::Invoke("))
				{
		Log.DebugLine(this, "found event call");				
					// Get the index of the instruction that loaded the this argument of the
					// Invoke call.
					int i = m_info.Tracker.GetStackIndex(call.Index, 2);
					if (i >= 0)
					{
						// 18: ldarg.0    this
						// 19: ldfld      System.EventHandler Smokey.Tests.UnprotectedEventTest/Good1::Event1
						// 1E: brfalse    34
						// 23: ldarg.0    this
						// 24: ldfld      System.EventHandler Smokey.Tests.UnprotectedEventTest/Good1::Event1
						// 29: ldarg.0    this
						// 2A: ldsfld     System.EventArgs System.EventArgs::Empty
						// 2F: callvirt   System.Void System.EventHandler::Invoke(System.Object,System.EventArgs)

		Log.DebugLine(this, "first arg is loaded at {0:X2}", m_info.Instructions[i].Untyped.Offset);				
						// We expect it to be a ldfld instruction and the type should be
						// System.EventHandler.
						LoadField field1 = m_info.Instructions[i] as LoadField;
						if (field1 != null && field1.Field.FieldType.ToString().StartsWith("System.EventHandler"))
						{
		Log.DebugLine(this, "field1 is ok");				
							// Typically it will be preceded by a load this, a branch, and
							// a load of the event.
							Load load = m_info.Instructions[i - 1] as Load;
							ConditionalBranch branch = m_info.Instructions[i - 2] as ConditionalBranch;
							LoadField field2 = m_info.Instructions[i - 3] as LoadField;
							
							// If not we have a problem.
							if (load == null || branch == null || field2 == null)
							{
								m_offset = call.Untyped.Offset;						
								Log.DebugLine(this, "no if test for event at {0:X2}", m_offset);				
							}
							else if (load != null && branch != null && field2 != null)
							{
								// If we do have the guard it must be using the correct field.
								if (field1.Field.Name != field2.Field.Name)
								{
									m_offset = call.Untyped.Offset;						
									Log.DebugLine(this, "if test but wrong event at {0:X2}", m_offset);				
								}
							}
						}
						else
						{
							// 18: ldsfld     System.EventHandler Smokey.Tests.UnprotectedEventTest/Good1::Event3
							// 1D: brfalse    32
							// 22: ldsfld     System.EventHandler Smokey.Tests.UnprotectedEventTest/Good1::Event3
							// 27: ldarg.0    this
							// 28: ldsfld     System.EventArgs System.EventArgs::Empty
							// 2D: callvirt   System.Void System.EventHandler::Invoke(System.Object,System.EventArgs)

							// Or it may be a be a ldsfld instruction.
							LoadStaticField sfield1 = m_info.Instructions[i] as LoadStaticField;
							if (sfield1 != null && sfield1.Field.FieldType.ToString().StartsWith("System.EventHandler"))
							{
		Log.DebugLine(this, "sfield1 is ok " + sfield1.Field.FieldType);				
								// Typically it will be preceded by a branch and a load of the event.
								ConditionalBranch sbranch = m_info.Instructions[i - 1] as ConditionalBranch;
								LoadStaticField sfield2 = m_info.Instructions[i - 2] as LoadStaticField;
								
								// If not we have a problem.
								if (sbranch == null || sfield2 == null)
								{
									m_offset = call.Untyped.Offset;						
									Log.DebugLine(this, "no if test for event at {0:X2}", m_offset);				
								}
								else if (sbranch != null && sfield2 != null)
								{
									// If we do have the guard it must be using the correct field.
									if (sfield1.Field.Name != sfield2.Field.Name)
									{
										m_offset = call.Untyped.Offset;						
										Log.DebugLine(this, "if test but wrong event at {0:X2}", m_offset);				
									}
								}
							}
							else
							{
								// 29: ldloc.0    V_0
								// 2A: brfalse    3B
								// 2F: ldloc.0    V_0
								// 30: ldarg.0    this
								// 31: ldsfld     System.EventArgs System.EventArgs::Empty
								// 36: callvirt   System.Void System.EventHandler::Invoke(System.Object,System.EventArgs)

								// Or it may be a be a ldloc instruction (if handlers are stored
								// in a table for example).
								LoadLocal local1 = m_info.Instructions[i] as LoadLocal;
								if (local1 != null && local1.Type.ToString().StartsWith("System.EventHandler"))
								{
			Log.DebugLine(this, "local1 is ok " + local1.Type);				
									// Typically it will be preceded by a branch and a load of the event.
									ConditionalBranch branch2 = m_info.Instructions[i - 1] as ConditionalBranch;
									LoadLocal local2 = m_info.Instructions[i - 2] as LoadLocal;
									
									// If not we have a problem.
									if (branch2 == null || local2 == null)
									{
										m_offset = call.Untyped.Offset;						
										Log.DebugLine(this, "no if test for event at {0:X2}", m_offset);				
									}
									else if (branch2 != null && local2 != null)
									{
										// If we do have the guard it must be using the correct local.
										if (local1.Variable != local2.Variable)
										{
											m_offset = call.Untyped.Offset;						
											Log.DebugLine(this, "if test but wrong event at {0:X2}", m_offset);				
										}
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
		
		private int m_offset;
		private MethodInfo m_info;
	}
}
