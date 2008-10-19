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
	internal sealed class CompareToRule : Rule
	{				
		public CompareToRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "D1021")
		{
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitBegin");
			dispatcher.Register(this, "VisitMethod");
			dispatcher.Register(this, "VisitEnd");
		}
				
		public void VisitBegin(BeginType begin)
		{
			Log.DebugLine(this, "-----------------------------------"); 
			Log.DebugLine(this, "{0}", begin.Type);		
							
			m_needsCheck = !begin.Type.IsInterface && begin.Type.TypeImplements("System.IComparable");
			if (m_needsCheck)
			{
				m_foundCompare = false;
				m_foundEquals = false;
			}
		}
		
		public void VisitMethod(MethodDefinition method)
		{
			if (m_needsCheck)
			{
				if (method.Reuses("System.Boolean", "Equals", "System.Object"))
				{
					Log.DebugLine(this, "{0}", method); 
					m_foundEquals = true;
				}
				else if (method.Matches("System.Int32", "CompareTo", method.DeclaringType.FullName))
				{
					Log.DebugLine(this, "{0}", method); 
					m_foundCompare = true;
				}
			}
		}

		public void VisitEnd(EndType end)
		{
			if (m_needsCheck && m_foundCompare && !m_foundEquals)
			{
				Reporter.TypeFailed(end.Type, CheckID, string.Empty);
			}
		}
				
		private bool m_needsCheck;
		private bool m_foundCompare;
		private bool m_foundEquals;
	}
}
#endif
