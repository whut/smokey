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
using Smokey.Framework.Support;

#if OLD
namespace Smokey.Internal.Rules
{	
	internal sealed class ArithmeticButNoEqualsRule : Rule
	{				
		public ArithmeticButNoEqualsRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "D1055")
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
			
			m_hasAdd = false;
			m_hasMinus = false;
			m_hasEquals = false;
		}
		
		public void VisitMethod(MethodDefinition method)
		{								
			if (method.IsPublic)
			{
				Log.DebugLine(this, "{0}", method);
				if (method.Name == "op_Addition")
				{
					m_hasAdd = true;
				}
				else if (method.Name == "op_Subtraction")
				{
					m_hasMinus = true;
				}
				else if (method.Reuses("System.Boolean", "Equals", "System.Object"))
				{
					m_hasEquals = true;
				}
			}
		}
		
		public void VisitEnd(EndType end)
		{		
			if (m_hasAdd && m_hasMinus && !m_hasEquals)
			{
				Reporter.TypeFailed(end.Type, CheckID, string.Empty);
			}
		}
		
		private bool m_hasAdd;
		private bool m_hasMinus;
		private bool m_hasEquals;
	}
}
#endif
