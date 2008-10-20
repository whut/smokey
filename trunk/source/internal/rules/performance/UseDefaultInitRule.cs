// Copyright (C) 2007 Jesse Jones
//
// Authors:
//	Jb Evain <jbevain@gmail.com>
//	Jesse Jones <jesjones@mindspring.com>
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

namespace Smokey.Internal.Rules
{	
	internal sealed class UseDefaultInitRule : Rule
	{				
		public UseDefaultInitRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "P1011")
		{
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitBegin");
			dispatcher.Register(this, "VisitStore");
			dispatcher.Register(this, "VisitInit");
			dispatcher.Register(this, "VisitEnd");
		}
				
		public void VisitBegin(BeginMethod method)
		{
			m_needsCheck = method.Info.Method.IsConstructor && !method.Info.Type.IsValueType;
			m_offset = -1;
			m_info = method.Info;
			
			if (m_needsCheck)
			{
				Log.DebugLine(this, "-----------------------------------"); 
				Log.DebugLine(this, "{0:F}", method.Info.Instructions);				
			}
		}
		
        // ldarg.0   this
        // ldc.i4.0   
        // stfld     System.Int32 Smokey.Tests.UseDefaultInitTest/BadCase1::m_int
		public void VisitStore(StoreField store)
		{
			if (m_needsCheck && m_offset < 0)
			{
				long? value = m_info.Tracker.GetStack(store.Index, 0);
				LoadConstantFloat flt = m_info.Instructions[store.Index - 1] as LoadConstantFloat;
				if ((value.HasValue && value.Value == 0) || (flt != null && flt.Value == 0.0))
				{
					Log.DebugLine(this, "found zero value at index {0}", store.Index - 1); 
					int index = m_info.Tracker.GetStackIndex(store.Index, 1);
					if (index >= 0)
					{
						LoadArg load = m_info.Instructions[index] as LoadArg;
						if (load != null)
							Log.DebugLine(this, "found load at index {0}", index); 
						if (load != null && load.Arg == 0)
						{
							m_offset = store.Untyped.Offset;
							Log.DebugLine(this, "found zero field store at {0:X2}", m_offset); 
						}
					}
				}
			}
		}

        // ldarg.0  this
        // ldflda   Smokey.Tests.UseDefaultInitTest/Struct Smokey.Tests.UseDefaultInitTest/BadCase3::m_struct  
        // initobj  Smokey.Tests.UseDefaultInitTest/Struct
		public void VisitInit(InitObj init)
		{
			if (m_needsCheck && m_offset < 0)
			{
				if (init.Type.IsValueType)
				{
					LoadFieldAddress field = m_info.Instructions[init.Index - 1] as LoadFieldAddress;
					if (field != null)
					{
						LoadArg arg = m_info.Instructions[init.Index - 2] as LoadArg;
						if (arg != null && arg.Arg == 0)
						{
							m_offset = init.Untyped.Offset;
							Log.DebugLine(this, "found default ctor call at {0:X2}", m_offset); 
						}
					}
				}
			}
		}

		public void VisitEnd(EndMethod method)
		{
			if (m_offset >= 0)
			{
				Reporter.MethodFailed(method.Info.Method, CheckID, m_offset, string.Empty);
			}
		}
		
		private MethodInfo m_info;
		private bool m_needsCheck;
		private int m_offset;
	}
}

