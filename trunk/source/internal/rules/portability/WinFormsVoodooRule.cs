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

namespace Smokey.Internal.Rules
{	
	internal sealed class WinFormsVoodooRule : Rule
	{				
		public WinFormsVoodooRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "PO1009")
		{
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitCall");
			dispatcher.Register(this, "VisitFini");
		}
				
		public void VisitCall(Call call)
		{
			if (!m_foundRun && call.Target.ToString().Contains("System.Windows.Forms.Application::Run"))
			{
				Log.DebugLine(this, "found Application::Run call");
				m_foundRun = true;
			}
			else if (!m_foundStyle && call.Target.ToString().Contains("System.Windows.Forms.Application::EnableVisualStyles"))
			{
				Log.DebugLine(this, "found Application::EnableVisualStyles call");
				m_foundStyle = true;
			}
			else if (!m_foundCompatible && call.Target.ToString().Contains("System.Windows.Forms.Application::SetCompatibleTextRenderingDefault"))
			{
				Log.DebugLine(this, "found Application::SetCompatibleTextRenderingDefault call");
				m_foundCompatible = true;
			}
		}
				
		public void VisitFini(EndTesting end)
		{		
			Unused.Arg(end);
			
			if (m_foundRun)
			{
				if (!m_foundStyle || !m_foundCompatible)
				{
					Reporter.AssemblyFailed(Cache.Assembly, CheckID, string.Empty);
				}
			}
		}
		
		private bool m_foundRun;
		private bool m_foundStyle;
		private bool m_foundCompatible;
	}
}

