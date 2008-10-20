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
using Smokey.Framework;
using Smokey.Framework.Instructions;
using Smokey.Framework.Support;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Smokey.Internal.Rules
{	
	internal sealed class BadRegExRule : Rule
	{				
		public BadRegExRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "C1015")
		{
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitBegin");
			dispatcher.Register(this, "VisitNew");
			dispatcher.Register(this, "VisitCall");
			dispatcher.Register(this, "VisitEnd");
		}
		
		public void VisitBegin(BeginMethod begin)
		{
			Log.DebugLine(this, "-----------------------------------"); 
			Log.DebugLine(this, "{0:F}", begin.Info.Instructions);				

			m_offset = -1;
			m_info = begin.Info;
			m_details = string.Empty;
		}
		
		public void VisitNew(NewObj obj)	
		{
			if (m_offset < 0)
			{
				if (obj.Ctor.ToString() == "System.Void System.Text.RegularExpressions.Regex::.ctor(System.String)")
				{
					LoadString prev = m_info.Instructions[obj.Index - 1] as LoadString;
					if (prev != null && !DoIsValidRegEx(prev.Value))
					{
						m_offset = obj.Untyped.Offset;
					}
				}
			}
		}

		public void VisitCall(Call call)	
		{
			if (m_offset < 0)
			{
				if (Array.IndexOf(m_targets1, call.Target.ToString()) >= 0)
				{
					LoadString prev = m_info.Instructions[call.Index - 1] as LoadString;
					if (prev != null && !DoIsValidRegEx(prev.Value))
					{
						m_offset = call.Untyped.Offset;
					}
				}
				else if (Array.IndexOf(m_targets2, call.Target.ToString()) >= 0)
				{
					LoadString prev = m_info.Instructions[call.Index - 2] as LoadString;
					if (prev != null && !DoIsValidRegEx(prev.Value))
					{
						m_offset = call.Untyped.Offset;
					}
				}
			}
		}

		public void VisitEnd(EndMethod end)
		{
			if (m_offset >= 0)
			{
				Reporter.MethodFailed(end.Info.Method, CheckID, m_offset, m_details);
			}
		}
		
		private bool DoIsValidRegEx(string s)
		{
			bool valid = false;
			Log.DebugLine(this, "checking {0}", s);				
			
			try
			{
				Unused.Value = new Regex(s);
				valid = true;
			}
			catch (Exception ex)
			{
				m_details = ex.Message;
			}
			
			return valid;
		}
						
		private int m_offset;
		private MethodInfo m_info;
		private string m_details;
		private string[] m_targets1 = new string[]
		{
			"System.Boolean System.Text.RegularExpressions.Regex::IsMatch(System.String,System.String)",
			"System.Text.RegularExpressions.Match System.Text.RegularExpressions.Regex::Match(System.String,System.String)",
			"System.Text.RegularExpressions.MatchCollection System.Text.RegularExpressions.Regex::Matches(System.String,System.String)",
			"System.String[] System.Text.RegularExpressions.Regex::Split(System.String,System.String)",
		};
		private string[] m_targets2 = new string[]
		{
			"System.String System.Text.RegularExpressions.Regex::Replace(System.String,System.String,System.String)",
		};
	}
}
