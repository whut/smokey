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
	internal class GodClassRule : Rule
	{				
		public GodClassRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "D1045")
		{
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitBegin");
			dispatcher.Register(this, "VisitCall");
			dispatcher.Register(this, "VisitNew");
			dispatcher.Register(this, "VisitEnd");
		}
		
		public void VisitBegin(BeginMethods begin)
		{
			Log.DebugLine(this, "-----------------------------------"); 
			Log.DebugLine(this, "{0}", begin.Type.FullName);

			m_type = begin.Type;
			m_types.Clear();
		}
				
		public void VisitCall(Call call)
		{	
			if (call.Target.DeclaringType != m_type)
			{
				string name = call.Target.DeclaringType.FullName;
			
				if (m_types.IndexOf(name) < 0)
				{
					Log.DebugLine(this, "   {0}", name);
					m_types.Add(name);
				}
			}
		}
		
		public void VisitNew(NewObj obj)
		{	
			if (obj.Ctor.DeclaringType != m_type)
			{
				string name = obj.Ctor.DeclaringType.FullName;
			
				if (m_types.IndexOf(name) < 0)
				{
					Log.DebugLine(this, "   {0}", name);
					m_types.Add(name);
				}
			}
		}
		
		public void VisitEnd(EndMethods end)
		{
			Log.DebugLine(this, "   {0} types", m_types.Count);
			if (m_types.Count >= 40)
			{
				string details = string.Format("references {0} types", m_types.Count);
				Reporter.TypeFailed(end.Type, CheckID, details);
			}
		}
		
		private TypeReference m_type;
		private List<string> m_types = new List<string>();
	}
}

