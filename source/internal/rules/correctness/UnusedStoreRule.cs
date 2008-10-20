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
	internal sealed class UnusedStoreRule : Rule
	{				
		public UnusedStoreRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "C1014")
		{
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitBegin");
			dispatcher.Register(this, "VisitStoreArg");
			dispatcher.Register(this, "VisitStoreLocal");
			dispatcher.Register(this, "VisitStoreField");
			dispatcher.Register(this, "VisitStoreStaticField");
			dispatcher.Register(this, "VisitEnd");
		}
		
		public void VisitBegin(BeginMethod begin)
		{
			Log.DebugLine(this, "-----------------------------------"); 
			Log.DebugLine(this, "{0:F}", begin.Info.Instructions);				

			m_offset = -1;
			m_info = begin.Info;
		}
		
		public void VisitStoreArg(StoreArg store)
		{
			if (m_offset < 0 && store.Index > 0)
			{
				StoreArg prev = m_info.Instructions[store.Index - 1] as StoreArg;
				if (prev != null && prev.Argument == store.Argument)
				{
					m_offset = store.Untyped.Offset;						
					Log.DebugLine(this, "Matched at {0:X2}", m_offset);				
				}
			}
		}

		public void VisitStoreLocal(StoreLocal store)
		{
			if (m_offset < 0 && store.Index > 0)
			{
				StoreLocal prev = m_info.Instructions[store.Index - 1] as StoreLocal;
				if (prev != null && prev.Variable == store.Variable)
				{
					m_offset = store.Untyped.Offset;						
					Log.DebugLine(this, "Matched at {0:X2}", m_offset);				
				}
			}
		}

		public void VisitStoreField(StoreField store)
		{
			if (m_offset < 0 && store.Index > 0)
			{
				StoreField prev = m_info.Instructions[store.Index - 1] as StoreField;
				if (prev != null && prev.Field.Name == store.Field.Name)
				{
					m_offset = store.Untyped.Offset;						
					Log.DebugLine(this, "Matched at {0:X2}", m_offset);				
				}
			}
		}

		public void VisitStoreStaticField(StoreStaticField store)
		{
			if (m_offset < 0 && store.Index > 0)
			{
				StoreStaticField prev = m_info.Instructions[store.Index - 1] as StoreStaticField;
				if (prev != null && prev.Field.Name == store.Field.Name)
				{
					m_offset = store.Untyped.Offset;						
					Log.DebugLine(this, "Matched at {0:X2}", m_offset);				
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
						
		private int m_offset;
		private MethodInfo m_info;
	}
}
