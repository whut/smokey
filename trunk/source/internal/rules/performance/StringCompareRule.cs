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
	internal sealed class StringCompareRule : Rule
	{				
		public StringCompareRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "P1015")
		{
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitBegin");
			dispatcher.Register(this, "VisitCall");
			dispatcher.Register(this, "VisitEnd");
		}
				
		public void VisitBegin(BeginMethod method)
		{
			Log.DebugLine(this, "-----------------------------------"); 
			Log.DebugLine(this, "{0:F}", method.Info.Instructions);				

			m_offset = -1;
			m_info = method.Info;
		}
		
		public void VisitCall(Call call)
		{
			if (m_offset < 0)
			{
				if (call.Target.ToString().Contains("System.String::Equals") || 
					call.Target.ToString().Contains("System.String::op_Equality") ||
					call.Target.ToString().Contains("System.String::op_Inequality"))
				{
					if (DoIsEmptyString(call, 0) || DoIsEmptyString(call, 1))
					{
						m_offset = call.Untyped.Offset;
						Log.DebugLine(this, "found bad equals at {0:X2}", m_offset);				
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
		
		private bool DoIsEmptyString(Call call, int nth)
		{
			bool empty = false;
			
			int index = m_info.Tracker.GetStackIndex(call.Index, nth);
			if (index >= 0)
			{
				LoadString str = m_info.Instructions[index] as LoadString;
				if (str != null)
					empty = str.Value.Length == 0;
				else
				{
					LoadStaticField field = m_info.Instructions[index] as LoadStaticField;
					if (field != null)
					{
						empty = field.Field.DeclaringType.FullName == "System.String" && field.Field.Name == "Empty";
					}
				}
			}
			
			return empty;
		}
		
		private MethodInfo m_info;
		private int m_offset;
	}
}
#endif
