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
using Mono.Cecil.Metadata;
using System;
using System.Security;
using System.Collections.Generic;
using Smokey.Framework;
using Smokey.Framework.Instructions;
using Smokey.Framework.Support;
using Smokey.Framework.Support.Advanced;

namespace Smokey.Internal.Rules
{		
	internal class PartitionAssemblyRule : Rule
	{				
		public PartitionAssemblyRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "S1015")
		{
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitAssembly");
			dispatcher.Register(this, "VisitGraph");
		}
		
		public void VisitAssembly(AssemblyDefinition assembly)
		{
			m_mixed = false;
			
			foreach (CustomAttribute attr in assembly.CustomAttributes)
			{
				if (attr.Constructor.DeclaringType.Name == "SecurityCriticalAttribute")
				{
					if (attr.ConstructorParameters.Count == 0)
					{
						m_mixed = true;
						break;
					}
				}
			}
		}

		public void VisitGraph(CallGraph graph)
		{
			m_graph = graph;
			
			// If the assembly is mixed transparent/critical then we may have critical
			// methods.
			if (m_mixed)
			{
				List<string> lines = new List<string>();
				
				// So, for each method,
				foreach (KeyValuePair<MethodReference, List<MethodReference>> entry in graph.Entries())
				{
					MethodInfo caller = Cache.FindMethod(entry.Key);
					if (caller != null)
					{
						// if it's public,
						MethodAttributes access = caller.Method.Attributes & MethodAttributes.MemberAccessMask;
						if (access == MethodAttributes.Public)
						{
							// and transparent,
							if (!caller.Method.CustomAttributes.Has("SecurityCriticalAttribute"))
							{
								// then fail if it calls a non-public critical method.
								string line = DoIsBad(caller.Method, entry.Value, 1);
								if (line.Length > 0)
								{
									lines.Add(line);
								}
							}
						}
					}
				}
				
				if (lines.Count > 0)
				{
					string details = string.Join(Environment.NewLine, lines.ToArray());
//					Console.WriteLine(details);
					Reporter.AssemblyFailed(Cache.Assembly, CheckID, details);
				}
			}
		}
		
		private string DoIsBad(MethodDefinition caller, List<MethodReference> targets, int depth)
		{			
			if (depth > 10)
				return string.Empty;
				
			foreach (MethodReference t in targets)
			{
				MethodInfo target = Cache.FindMethod(t);
				if (target != null)
				{
					MethodAttributes access = target.Method.Attributes & MethodAttributes.MemberAccessMask;				
					if (access != MethodAttributes.Public)
					{
						if (target.Method.CustomAttributes.Has("SecurityCriticalAttribute"))
						{
							if (!target.Method.CustomAttributes.Has("SecurityTreatAsSafeAttribute"))
							{
								return string.Format("{0} -> {1}", caller, target.Method);
							}
						}
					
						List<MethodReference> calls = m_graph.GetCalls(t);
						if (calls != null)
						{
							string sub = DoIsBad(target.Method, calls, depth + 1);
							if (sub.Length > 0)
								return string.Format("{0} -> {1}", caller, sub);
						}
					}
				}
			}
			
			return string.Empty;
		}

		private bool m_mixed;
		private CallGraph m_graph;
	}
}
