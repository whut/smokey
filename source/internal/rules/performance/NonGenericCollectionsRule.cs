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
using Smokey.Framework.Support;

#if OLD
namespace Smokey.Internal.Rules
{	
	internal sealed class NonGenericCollectionsRule : Rule
	{				
		public NonGenericCollectionsRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "P1007")
		{
			Runtime = TargetRuntime.NET_2_0;
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitBegin");
			dispatcher.Register(this, "VisitField");
			dispatcher.Register(this, "VisitEnd");
		}
				
		public void VisitBegin(BeginType begin)
		{						
			Log.DebugLine(this, "-----------------------------------"); 
			Log.DebugLine(this, "checking {0}", begin.Type);				

			m_details = string.Empty;
		}

		public void VisitField(FieldDefinition field)
		{						
			if (Array.IndexOf(m_bad, field.FieldType.FullName) >= 0)
			{
				m_details += string.Format("{0} ", field.FieldType.Name);
				Log.DebugLine(this, "found {0}", field.FieldType.FullName);
			}
		}

		public void VisitEnd(EndType end)
		{			
			if (m_details.Length > 0)
			{
				m_details = string.Format("Uses {0}", m_details);
				Reporter.TypeFailed(end.Type, CheckID, m_details);
			}
		}
		
		private string m_details;
		private string[] m_bad = new string[]{
			"System.Collections.ArrayList",
			"System.Collections.Hashtable",
			"System.Collections.Queue",
			"System.Collections.SortedList",
			"System.Collections.Stack"};
	}
}
#endif
