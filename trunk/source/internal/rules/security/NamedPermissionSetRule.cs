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

#if OLD
namespace Smokey.Internal.Rules
{		
	internal sealed class NamedPermissionSetRule : Rule
	{				
		public NamedPermissionSetRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "S1019")
		{
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitBegin");
			dispatcher.Register(this, "VisitNew");
			dispatcher.Register(this, "VisitEnd");
		}
				
		public void VisitBegin(BeginMethod begin)
		{
			Log.DebugLine(this, "-----------------------------------"); 
			Log.DebugLine(this, "{0:F}", begin.Info.Instructions);				

			m_offset = -1;
			m_info = begin.Info;
		}
		
		public void VisitNew(NewObj obj)
		{
			if (m_offset < 0)
			{
				if (obj.Ctor.ToString().StartsWith("System.Void System.Security.NamedPermissionSet::.ctor(System.String"))
				{
					int numArgs = obj.Ctor.Parameters.Count;
					int entry = numArgs - 1;
					int index = m_info.Tracker.GetStackIndex(obj.Index, entry);
				
					if (index >= 0)
					{
						LoadString load = m_info.Instructions[index] as LoadString;
						if (load != null && Array.IndexOf(m_bad, load.Value) >= 0)
						{
							m_offset = load.Untyped.Offset;
							Log.DebugLine(this, "found bad code at {0:X2}", m_offset); 
						}
					}
				}
			}
		}

		public void VisitEnd(EndMethod method)
		{
			if (m_offset >= 0)
			{
				Reporter.MethodFailed(method.Info.Method, CheckID, m_offset, string.Empty);
			}
		}

		private int m_offset;	
		private MethodInfo m_info;
		private string[] m_bad = new string[]{
			"FullTrust", "SkipVerification", "Execution", "Nothing", "LocalIntranet", 
			"Internet", "Everything"};
	}
}
#endif
