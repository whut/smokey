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
using System;
using Smokey.Framework;
using Smokey.Framework.Instructions;
using Smokey.Framework.Support;
using Smokey.Framework.Support.Advanced;

namespace Smokey.Internal.Rules
{	
	internal class BadExitRule : Rule
	{				
		public BadExitRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "D1039")
		{
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitBegin");
			dispatcher.Register(this, "VisitCall");
			dispatcher.Register(this, "VisitGraph");
		}
						
		public void VisitBegin(BeginMethod begin)
		{
			Log.DebugLine(this, "-----------------------------------"); 
			Log.DebugLine(this, "{0:F}", begin.Info.Instructions);				

			m_method = begin.Info.Method;
		}
		
		public void VisitCall(Call call)
		{				
			if (call.Target.ToString().Contains("System.Windows.Forms.Application::Run("))
			{
				m_callsRun = true;
			}
			else if (call.Target.ToString().Contains("System.Environment::Exit("))
			{
				m_callsExit = true;
				m_details += m_method + " ";
			}
		}
				
		public void VisitGraph(CallGraph graph)
		{
			if (m_callsRun && m_callsExit)
			{
				m_details = "Calls Exit: " + m_details;
				Log.DebugLine(this, m_details); 
				Reporter.AssemblyFailed(Cache.Assembly, CheckID, m_details);
			}
		}

		MethodDefinition m_method;
		private bool m_callsRun;
		private bool m_callsExit;
		private string m_details = string.Empty;
	}
}

