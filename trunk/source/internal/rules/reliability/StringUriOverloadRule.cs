// Copyright (C) 2008 Jesse Jones
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
using Smokey.Framework.Support.Advanced;

using System;
using System.Collections.Generic;

namespace Smokey.Internal.Rules
{	
	internal class StringUriOverloadRule : Rule
	{				
		public StringUriOverloadRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "R1030")
		{
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitBegin");
			dispatcher.Register(this, "VisitMethod");
			dispatcher.Register(this, "VisitCall");
			dispatcher.Register(this, "VisitFini");
		}
				
		public void VisitBegin(BeginType begin)
		{
			Log.DebugLine(this, "-----------------------------------"); 
			Log.DebugLine(this, "{0}", begin.Type);				
			
			foreach (MethodDefinition method in begin.Type.Methods)
			{
				int index = DoGetUriArg(method);
				if (index >= 0)
				{
					MethodDefinition str = DoFindString(begin.Type, method, index);
					if (str != null)
					{
						Log.DebugLine(this, "{0} is a candidate (index = {1})", method.Name, index);
						m_table.Add(str, new Entry(method));
					}
				}
			}
		}
		
		public void VisitMethod(BeginMethod begin)
		{			
			if (m_table.TryGetValue(begin.Info.Method, out m_entry))
			{
				Log.DebugLine(this, "{0}", begin.Info.Method);
				Log.DebugLine(this, "{0:F}", begin.Info.Instructions);
			}
		}

		public void VisitCall(Call call)
		{
			if (m_entry != null && !m_entry.CallsUri)
			{
				if (call.Target == m_entry.Uri)
				{
					Log.DebugLine(this, "   call to uri method at {0:X2}", call.Untyped.Offset);											
					m_entry.CallsUri = true;
				}
			}
		}

		public void VisitFini(EndTesting end)
		{			
			foreach (KeyValuePair<MethodDefinition, Entry> entry in m_table)
			{
				if (!entry.Value.CallsUri)
				{
					string details = "Method: " + entry.Key.Name;
					Log.DebugLine(this, details);
					
					TypeDefinition type = Cache.FindType(entry.Key.DeclaringType);
					Reporter.TypeFailed(type, CheckID, details);
				}
			}
		}
		
		private int DoGetUriArg(MethodDefinition method)
		{
			for (int index = 0; index < method.Parameters.Count; ++index)
			{
				ParameterDefinition p = method.Parameters[index];
				
				if (p.ParameterType.FullName == "System.Uri")
				{
					return index;
				}
			}
			
			return -1;
		}
		
		private MethodDefinition DoFindString(TypeDefinition type, MethodDefinition uri, int index)
		{
			foreach (MethodDefinition candidate in type.Methods)
			{
				if (candidate != uri)
				{
					if (candidate.Name == uri.Name && candidate.Parameters.Count == uri.Parameters.Count)
					{
						bool match = true;
	
						for (int i = 0; i < candidate.Parameters.Count && match; ++i)
						{
							ParameterDefinition ps = candidate.Parameters[i];
							
							if (i == index)
							{
								match = ps.ParameterType.FullName == "System.String";
							}
							else
							{
								ParameterDefinition pu = uri.Parameters[i];
								match = ps.ParameterType.FullName == pu.ParameterType.FullName;
							}
						}
						
						if (match)
							return candidate;
					}
				}
			}
			
			return null;
		}
		
		private class Entry
		{
			public Entry(MethodDefinition uri)
			{
				m_uri = uri;
				m_callsUri = false;
			}
			
			public MethodDefinition Uri
			{
				get {return m_uri;}
			}
			
			public bool CallsUri
			{
				get {return m_callsUri;}
				set {m_callsUri = value;}
			}
			
			private MethodDefinition m_uri;
			private bool m_callsUri;
		}
				
		private Entry m_entry;
		private Dictionary<MethodDefinition, Entry> m_table = new Dictionary<MethodDefinition, Entry>();	// string method -> entry
	}
}

