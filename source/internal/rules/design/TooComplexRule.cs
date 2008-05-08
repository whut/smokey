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
	internal class TooComplexRule : Rule
	{				
		public TooComplexRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "D1002")
		{
			m_maxBranches = Settings.Get("maxBranches", 40);
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitBegin");
			dispatcher.Register(this, "VisitBranch");
			dispatcher.Register(this, "VisitEnd");
		}
				
		public void VisitBegin(BeginMethod begin)
		{
			Log.DebugLine(this, "-----------------------------------"); 
			Log.DebugLine(this, "{0:F}", begin.Info.Instructions);				

			m_count = 0;
		}
		
		// What we want to do here is identify methods that are too complex to 
		// be readily understandable. So, we don't care about length since large 
		// straight-line methods are ugly but not hard to understand. Nor do we 
		// want to over-count switch statements because they are well structured 
		// and easy to understand.
		public void VisitBranch(Branch branch)
		{
			++m_count;
			Log.DebugLine(this, "found branch at {0:X2}", branch.Untyped.Offset); 
		}

		public void VisitEnd(EndMethod end)
		{
			if (m_count > m_maxBranches)
			{
				Log.DebugLine(this, "found {0} branches", m_count); 
				Reporter.MethodFailed(end.Info.Method, CheckID, 0, string.Empty);
			}
		}
				
		private int m_count;
		private int m_maxBranches;
	}
}

