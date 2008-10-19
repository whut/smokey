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

#if OLD
namespace Smokey.Internal.Rules
{	
	internal sealed class DefaultExceptionCtorRule : Rule
	{				
		public DefaultExceptionCtorRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "MS1028")
		{
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitBegin");	
			dispatcher.Register(this, "VisitNewObj");	
			dispatcher.Register(this, "VisitEnd");
		}
						
		public void VisitBegin(BeginMethod begin)
		{		
			m_needsCheck = true;
			m_offset = -1;
			
			if (m_needsCheck)
			{
				Log.DebugLine(this, "-----------------------------------"); 
				Log.DebugLine(this, "{0:F}", begin.Info.Instructions);				
			}
		}
		
		public void VisitNewObj(NewObj newobj)
		{
			if (m_needsCheck && m_offset < 0)
			{
				string name = newobj.Ctor.DeclaringType.FullName;
				Log.DebugLine(this, "newobj.Ctor.DeclaringType: {0}", name);
				
				if (name == "System.ArgumentException" || 
					name == "System.ArgumentNullException" || 
					name == "System.ArgumentOutOfRangeException" || 
					name == "System.DuplicateWaitObjectException")
				{
					if (newobj.Ctor.Parameters.Count == 0)
					{
						m_offset = newobj.Untyped.Offset;											
						Log.DebugLine(this, "   bad call at {0:X2}", m_offset);
					}
				}
			}
		}

		public void VisitEnd(EndMethod end)
		{
			if (m_offset >= 0)
				Reporter.MethodFailed(end.Info.Method, CheckID, m_offset, string.Empty);
		}
		
		private int m_offset;
		private bool m_needsCheck;
	}
}
#endif
