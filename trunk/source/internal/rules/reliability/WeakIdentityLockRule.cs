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
	internal sealed class WeakIdentityLockRule : Rule
	{				
		public WeakIdentityLockRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "R1013")
		{
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitBegin");
			dispatcher.Register(this, "VisitCall");
			dispatcher.Register(this, "VisitEnd");
		}
				
		public void VisitBegin(BeginMethod begin)
		{
			Log.DebugLine(this, "-----------------------------------"); 
			Log.DebugLine(this, "{0:F}", begin.Info.Instructions);				

			m_offset = -1;
			m_details = string.Empty;
			m_info = begin.Info;
		}

		// ldarg.0  this
        // ldfld    System.String Smokey.Tests.WeakIdentityLockTest/Cases::m_string
        // stloc.0  V_0
        // ldloc.0  V_0
        // call     System.Void System.Threading.Monitor::Enter(System.Object)
        //
		// ldarg.0  this
        // ldfld    System.String Smokey.Tests.WeakIdentityLockTest/Cases::m_string
        // call     System.Void System.Threading.Monitor::Enter(System.Object)
		public void VisitCall(Call call)
		{
			if (m_offset < 0)
			{
				string name = call.Target.ToString();
				if (name.Contains("System.Threading.Monitor::Enter(System.Object)"))
				{
					Log.DebugLine(this, "found lock at {0:X2}", call.Untyped.Offset);
					
					int index = call.Index - 1;
					if (DoStoresLocal(index))
						index -= 2;
						
					FieldReference field = DoGetField(index);
					if (field != null)
					{
						if (Array.IndexOf(m_badTypes, field.FieldType.FullName) >= 0)
						{
							m_offset = call.Untyped.Offset;
							m_details = "Field: " + field.Name;
							Log.DebugLine(this, "lock on weak identity at: {0:X2}", m_offset);
						}
					}
				}
			}
		}

		public void VisitEnd(EndMethod end)
		{
			if (m_offset >= 0)
				Reporter.MethodFailed(end.Info.Method, CheckID, m_offset, m_details);
		}
		
        // stloc.0  V_0
        // ldloc.0  V_0
		private bool DoStoresLocal(int index)
		{
			bool found = false;
			
			if (index > 0)
			{
				LoadLocal load = m_info.Instructions[index] as LoadLocal;
				StoreLocal store = m_info.Instructions[index - 1] as StoreLocal;
				
				found = load != null && store != null && load.Variable == store.Variable;
			}
			
			return found;
		}

		// ldarg.0  this
        // ldfld    System.String Smokey.Tests.WeakIdentityLockTest/Cases::m_string
		private FieldReference DoGetField(int index)
		{
			FieldReference field = null;
			
			if (index > 0)
			{
				LoadField load = m_info.Instructions[index] as LoadField;
				LoadArg arg = m_info.Instructions[index - 1] as LoadArg;
				
				if (arg != null && load != null && arg.Arg == 0)
					field = load.Field;
			}
			
			return field;
		}

		private MethodInfo m_info;
		private int m_offset;
		private string m_details;
		private string[] m_badTypes = new string[]{
			"System.MarshalByRefObject",
			"System.ExecutionEngineException",
			"System.OutOfMemoryException",
			"System.StackOverflowException",
			"System.String",
			"System.Reflection.MemberInfo",
			"System.Reflection.ParameterInfo",
			"System.Threading.Thread",
		};
	}
}
#endif
