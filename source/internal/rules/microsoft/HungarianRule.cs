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
using Smokey.Framework.Support;
using Smokey.Framework.Instructions;

namespace Smokey.Internal.Rules
{	
	internal class HungarianRule : Rule
	{				
		[DisableRule("C1000", "StringSpelling")]	// ignore the weird hungarian strings
		public HungarianRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "MS1030")
		{
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitBegin");
			dispatcher.Register(this, "VisitMethod");
			dispatcher.Register(this, "VisitStore");
			dispatcher.Register(this, "VisitEnd");
		}
				
		public void VisitBegin(BeginMethods begin)
		{						
			Log.DebugLine(this, "-----------------------------------"); 
			Log.DebugLine(this, "checking {0}", begin.Type);				

			m_names.Clear();

			foreach (FieldDefinition f in begin.Type.Fields)
			{
				DoAdd(f.Name);
			}
		}
				
		public void VisitMethod(BeginMethod begin)
		{						
			foreach (ParameterDefinition p in begin.Info.Method.Parameters)
			{
				DoAdd(p.Name);
			}
		}
				
		public void VisitStore(StoreLocal store)
		{						
			if (store.RealName)
				DoAdd(store.Name);
			else
				Log.DebugLine(this, "no local names");				
	}
				
		public void VisitEnd(EndMethods end)
		{						
			if (m_names.Count >= 5)
			{	
				m_names.Sort();
				string details = "Names: " + string.Join(" ", m_names.ToArray());
				Log.DebugLine(this, details);
				
				Reporter.TypeFailed(end.Type, CheckID, details);
			}
		}
		
		private void DoAdd(string name)
		{
			if (DoIsHungarian(name))
			{
				if (m_names.IndexOf(name) < 0)
				{
					Log.DebugLine(this, "adding {0}", name);				
					m_names.Add(name);
				}
			}
		}
		
		private bool DoIsHungarian(string name)
		{
			if (name.StartsWith("ms_"))
				name = name.Substring(3);
			else if (name.StartsWith("m_"))
				name = name.Substring(2);
			else if (name.StartsWith("_"))
				name = name.Substring(1);
					
			foreach (string prefix in m_prefixes)
			{
				if (name.Length >= prefix.Length + 1)
				{
					if (name.StartsWith(prefix) && char.IsUpper(name[prefix.Length]))
						return true;
				}
			}
			
			return false;
		}
		
		private List<string> m_names = new List<string>();
		private string[] m_prefixes = new string[]
		{
			"b",
			"c",
			"ch",
			"d",
			"dw",
			"f",
			"h",
			"hwnd",
			"i",
			"k",
			"l",
			"lp",
			"lpsz",
			"e",
			"p",
			"pch",
			"psz",
			"rgsz",
			"str",
			"sz",
			"u",
			"w",
		};
	}
}

