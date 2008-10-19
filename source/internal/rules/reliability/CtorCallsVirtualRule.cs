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

#if OLD
namespace Smokey.Internal.Rules
{	
	internal sealed class CtorCallsVirtualRule : Rule
	{				
		public CtorCallsVirtualRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "R1005")
		{
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitBegin");
		}
				
		public void VisitBegin(BeginMethod begin)
		{
			if (begin.Info.Method.IsConstructor && !begin.Info.Type.IsSealed && !begin.Info.Method.IsStatic)
			{		
				Log.DebugLine(this, "-----------------------------------"); 
				Log.DebugLine(this, "{0:F}", begin.Info.Instructions);				
	
				m_details = string.Empty;
				m_tested.Clear();
				int offset = DoCallsVirtual(Cache, begin.Info);
				
				if (offset >= 0)
				{
					m_details = "Calls: " + m_details;			
					Log.DebugLine(this, m_details); 
					Reporter.MethodFailed(begin.Info.Method, CheckID, offset, m_details);
				}
			}
		}
		
		private int DoCallsVirtual(AssemblyCache cache, MethodInfo info)
		{
			int offset = -1;
			if (m_tested.IndexOf(info.Method.ToString()) >= 0)
				return offset;
				
			m_tested.Add(info.Method.ToString());

			Log.DebugLine(this, "checking:");
			Log.Indent();
			
			// If the class isn't sealed then,
			if (info.Method.Body != null && info.Instructions != null)
			{
				Log.DebugLine(this, "{0:F}", info.Instructions);

				// loop through every instruction,
				for (int i = 0; i < info.Instructions.Length && offset < 0; ++i)
				{
					// if it's a call,
					Call call = info.Instructions[i] as Call;
					if (call != null)
					{	
						// then we have a problem if we're calling a virtual method
						// on our instance,
						MethodInfo targetInfo = cache.FindMethod(call.Target);
						if (targetInfo != null)
						{
							if (call.IsThisCall(Cache, info, i))
							{
								if (targetInfo.Method.IsVirtual && !targetInfo.Method.IsFinal)
								{
									Log.DebugLine(this, "{0} is virtual", call.Target);
									{
										m_details = call.Target.ToString();			
										offset = call.Untyped.Offset;
										Log.DebugLine(this, "found virtual call at {0:X2}", offset);
									}
								}
								else
								{
									// or we're calling one of our methods which calls a virtual method
									// on our instance.
									offset = DoCallsVirtual(cache, targetInfo);
								}
							}
						}
					}
				}
			}
			Log.Unindent();
			
			if (offset >= 0)
				m_details = info.Method + " -> " + Environment.NewLine + "       " + m_details;			
			
			return offset;
		}

		private string m_details;		
		private List<string> m_tested = new List<string>();
	}
}
#endif
