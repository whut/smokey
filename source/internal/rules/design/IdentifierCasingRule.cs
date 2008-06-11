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
using Smokey.Framework;
using Smokey.Framework.Support;
using System;
using System.Collections.Generic;

namespace Smokey.Internal.Rules
{	
	internal sealed class IdentifierCasingRule : Rule
	{				
		public IdentifierCasingRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "D1059")
		{
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitBegin");
			dispatcher.Register(this, "VisitMethod");
			dispatcher.Register(this, "VisitEnd");
			dispatcher.Register(this, "VisitType");
			dispatcher.Register(this, "VisitFini");
		}
		
		public void VisitBegin(BeginType begin)
		{								
			m_methods.Clear();
		}
		
		public void VisitMethod(MethodDefinition method)
		{								
			m_methods.Add(method.Name);
			
			List<string> args = new List<string>();
			foreach (ParameterDefinition p in method.Parameters)
				args.Add(p.Name);

			List<string> bad = new List<string>();
			DoGetMatches(bad, args, "Arguments: ");

			if (bad.Count == 1)
			{
				Log.DebugLine(this, bad[0]);
				Reporter.MethodFailed(method, CheckID, 0, bad[0]);
			}
		}
		
		public void VisitEnd(EndType end)
		{								
			List<string> bad = new List<string>();
			DoGetMatches(bad, m_methods, "Methods: ");

			if (bad.Count == 1)
			{
				Log.DebugLine(this, bad[0]);
				Reporter.TypeFailed(end.Type, CheckID, bad[0]);
			}
		}
		
		public void VisitType(TypeDefinition type)
		{								
			m_types.Add(type.FullName);
			
			if (m_namespaces.IndexOf(type.Namespace) < 0)
				m_namespaces.Add(type.Namespace);
		}
		
		public void VisitFini(EndTesting end)
		{		
			List<string> bad = new List<string>();
			DoGetMatches(bad, m_namespaces, "Namespaces: ");
			DoGetMatches(bad, m_types, "Types: ");
			
			if (bad.Count > 0)
			{
				string details = string.Join(Environment.NewLine, bad.ToArray());
				Log.DebugLine(this, details);
				Reporter.AssemblyFailed(Cache.Assembly, CheckID, details);
			}
		}
		
		// line := prefix + badName1/badName2 + badName3/badName4/badName5
		private void DoGetMatches(List<string> bad, List<string> candidates, string prefix)
		{
			string line = prefix;
			
			List<string> found = new List<string>();
			for (int i = 0; i < candidates.Count; ++i)
			{
				string si = candidates[i].ToLower();
				string entry = candidates[i];
				
				if (found.IndexOf(si) < 0)
				{
					for (int j = i + 1; j < candidates.Count; ++j)
					{
						string sj = candidates[j].ToLower();
						if (si == sj && candidates[i] != candidates[j])
						{
							entry += "/" + candidates[j];
							found.Add(si);
						}
					}
				}
				
				if (entry != candidates[i])
					line += entry + " ";
			}
			
			if (line != prefix)
				bad.Add(line);
		}
		
		private List<string> m_methods = new List<string>();
		private List<string> m_types = new List<string>();
		private List<string> m_namespaces = new List<string>();
	}
}

