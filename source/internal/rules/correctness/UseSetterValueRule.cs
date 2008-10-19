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

#if OLD
namespace Smokey.Internal.Rules
{	
	internal sealed class UseSetterValueRule : Rule
	{				
		public UseSetterValueRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "C1022")
		{
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitBegin");
			dispatcher.Register(this, "VisitStore");
			dispatcher.Register(this, "VisitLoad");
			dispatcher.Register(this, "VisitLoadAddress");
			dispatcher.Register(this, "VisitEnd");
		}
		
		public void VisitBegin(BeginMethod begin)
		{
			Log.DebugLine(this, "-----------------------------------"); 
			Log.DebugLine(this, "{0:F}", begin.Info.Instructions);				

			m_needsCheck = begin.Info.Method.Name.StartsWith("set_");
			m_found1 = false;
			m_found2 = false;
		}
		
		public void VisitStore(StoreArg store)
		{
			if (m_needsCheck)
			{
				if (store.Argument == 1 && !m_found1)
				{
					m_found1 = true;
					Log.DebugLine(this, "found value1 use at {0:X2}", store.Untyped.Offset);				
				}
				else if (store.Argument == 2 && !m_found2)
				{
					m_found2 = true;
					Log.DebugLine(this, "found value2 use at {0:X2}", store.Untyped.Offset);				
				}
			}
		}

		public void VisitLoad(LoadArg load)
		{
			if (m_needsCheck)
			{
				if (load.Arg == 1 && !m_found1)
				{
					m_found1 = true;
					Log.DebugLine(this, "found value1 use at {0:X2}", load.Untyped.Offset);				
				}
				else if (load.Arg == 2 && !m_found2)
				{
					m_found2 = true;
					Log.DebugLine(this, "found value2 use at {0:X2}", load.Untyped.Offset);				
				}
			}
		}

		public void VisitLoadAddress(LoadArgAddress load)
		{
			if (m_needsCheck)
			{
				if (load.Arg == 1 && !m_found1)
				{
					m_found1 = true;
					Log.DebugLine(this, "found value1 use at {0:X2}", load.Untyped.Offset);				
				}
				else if (load.Arg == 2 && !m_found2)
				{
					m_found2 = true;
					Log.DebugLine(this, "found value2 use at {0:X2}", load.Untyped.Offset);				
				}
			}
		}

		public void VisitEnd(EndMethod end)
		{
			if (m_needsCheck)
			{
				if (end.Info.Method.Name == "set_Item" && end.Info.Method.Parameters.Count == 2)
				{
					if (!m_found1 || !m_found2)
						Reporter.MethodFailed(end.Info.Method, CheckID, 0, string.Empty);
				}
				else
				{
					if (!m_found1)
						Reporter.MethodFailed(end.Info.Method, CheckID, 0, string.Empty);
				}
			}
		}
		
		private bool m_needsCheck;
		private bool m_found1;
		private bool m_found2;
	}
}
#endif
