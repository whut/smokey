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

#if OLD
namespace Smokey.Internal.Rules
{	
	// The idea for this rule comes from John Robbins. See:
	// <http://msdn.microsoft.com/msdnmag/issues/04/09/Bugslayer/>.
	internal sealed class AvoidUnboxingRule : Rule
	{				
		public AvoidUnboxingRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "P1004")
		{
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitBegin");
			dispatcher.Register(this, "VisitUnbox");
			dispatcher.Register(this, "VisitEnd");
		}
				
		public void VisitBegin(BeginMethod begin)
		{
			Log.DebugLine(this, "-----------------------------------"); 
			Log.DebugLine(this, "{0:F}", begin.Info.Instructions);				

			m_offset = -1;
			m_numUnboxes = 0;
		}
		
		public void VisitUnbox(Unbox unbox)
		{
			++m_numUnboxes;
			if (m_offset < 0)
				m_offset = unbox.Untyped.Offset;

			Log.DebugLine(this, "found unbox at {0:X2}", unbox.Untyped.Offset); 
		}

		public void VisitEnd(EndMethod end)
		{
			if (m_numUnboxes > m_maxUnboxes)
			{
				string details = string.Format("found {0} unbox instructions", m_numUnboxes);
				Log.DebugLine(this, details); 
				Reporter.MethodFailed(end.Info.Method, CheckID, m_offset, details);
			}
		}
				
		private int m_offset;
		private int m_numUnboxes;
		private int m_maxUnboxes = Settings.Get("maxUnboxes", 1);
	}
}
#endif
