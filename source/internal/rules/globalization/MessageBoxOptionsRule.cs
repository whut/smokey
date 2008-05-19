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
using System.IO;
using System.Collections.Generic;
using Smokey.Framework;
using Smokey.Framework.Instructions;
using Smokey.Framework.Support;

namespace Smokey.Internal.Rules
{	
	internal sealed class MessageBoxOptionsRule : Rule
	{				
		public MessageBoxOptionsRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "G1001")
		{
			m_enabled = Settings.Get("*localized*", "true") == "true";
			Log.TraceLine(this, "enabled: {0}", m_enabled);			
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			if (m_enabled)
			{
				dispatcher.Register(this, "VisitBegin");
				dispatcher.Register(this, "VisitCall");
			}
		}
								
		public void VisitBegin(BeginMethod begin)
		{
			Log.DebugLine(this, "-----------------------------------"); 
			Log.DebugLine(this, "{0:F}", begin.Info.Instructions);				

			m_method = begin.Info.Method;
		}
		
		public void VisitCall(Call call)
		{
			if (call.Target.ToString().Contains("System.Windows.Forms.MessageBox::Show"))
			{
				if (!call.ToString().Contains("System.Windows.Forms.MessageBoxOptions"))
				{
					Reporter.MethodFailed(m_method, CheckID, call.Untyped.Offset, string.Empty);
				}
			}
		}
		
		private bool m_enabled;
		private MethodDefinition m_method;
	}
}
