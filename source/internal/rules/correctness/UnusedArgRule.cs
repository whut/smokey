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
	internal sealed class UnusedArgRule : Rule
	{				
		public UnusedArgRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "C1030")
		{
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitBegin");
			dispatcher.Register(this, "VisitLoad");
			dispatcher.Register(this, "VisitLoadAddr");
			dispatcher.Register(this, "VisitEnd");
		}
		
		public void VisitBegin(BeginMethod begin)
		{
			Log.DebugLine(this, "-----------------------------------"); 
			Log.DebugLine(this, "{0:F}", begin.Info.Instructions);				

			m_needsCheck = begin.Info.Method.Body != null && begin.Info.Method.Body.Instructions.Count > 1;
			m_args.Clear();

			if (m_needsCheck)
			{
				for (int i = 0; i < begin.Info.Method.Parameters.Count; ++i)
				{
					ParameterReference p = begin.Info.Method.Parameters[i];
					Log.DebugLine(this, "adding '{0}'", p.Name);				
				
					m_args.Add(p.Name);
				}
			}
		}
		
		public void VisitLoad(LoadArg load)
		{
			if (m_needsCheck)
			{
				Log.DebugLine(this, "removing '{0}'", load.Name);				
				Ignore.Value = m_args.Remove(load.Name);
			}
		}

		public void VisitLoadAddr(LoadArgAddress load)
		{
			if (m_needsCheck)
			{
				Log.DebugLine(this, "removing '{0}'", load.Name);				
				Ignore.Value = m_args.Remove(load.Name);
			}
		}

		public void VisitEnd(EndMethod end)
		{
			if (m_args.Count > 0)
			{
				string details = "Args: " + string.Join(" ", m_args.ToArray());
				Log.DebugLine(this, "{0}", details);				

				Reporter.MethodFailed(end.Info.Method, CheckID, 0, details);
			}
		}
						
		private List<string> m_args = new List<string>();
		private bool m_needsCheck;
	}
}
