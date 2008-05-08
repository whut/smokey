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

namespace Smokey.Internal.Rules
{	
	internal class GuiUsesConsoleRule : Rule
	{				
		public GuiUsesConsoleRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "D1048")
		{
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitAsembly");
			dispatcher.Register(this, "VisitBegin");
			dispatcher.Register(this, "VisitCall");
			dispatcher.Register(this, "VisitEnd");	
		}
				
		// Note that we cannot rely on -target and AssemblyKind because 
		// -target:exe maps to AssemblyKind.Console.
		public void VisitAsembly(AssemblyDefinition assembly)
		{
			m_gui = assembly.IsGui();
			Log.DebugLine(this, "{0} needsCheck: {1}, kind: {2}", assembly.Name.Name, m_gui, assembly.Kind);
		}
				
		public void VisitBegin(BeginMethod begin)
		{
			m_needsCheck = m_gui && !begin.Info.Method.CustomAttributes.Has("ConditionalAttribute");
			m_offset = -1;

			if (m_needsCheck)
			{
				Log.DebugLine(this, "-----------------------------------"); 
				Log.DebugLine(this, "{0:F}", begin.Info.Instructions);				
			}
		}
				
		public void VisitCall(Call call)
		{	
			if (m_needsCheck && m_offset < 0)
			{
				string name = call.Target.ToString();
			
				if (name.Contains("System.Console::Write(") || name.Contains("System.Console::WriteLine("))
				{
					m_offset = call.Untyped.Offset;
					Log.DebugLine(this, "found call at {0:X2}", m_offset);
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
		
		private bool m_gui;
		private bool m_needsCheck;
		private int m_offset;
	}
}

