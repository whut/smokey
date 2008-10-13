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
using System.Linq;

namespace Smokey.Internal.Rules
{	
	internal sealed class ThreadSafeAttrRule : Rule
	{				
		public ThreadSafeAttrRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "R1039")
		{
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			m_dispatcher = dispatcher;

			dispatcher.Register(this, "VisitAssembly");
			dispatcher.Register(this, "VisitType");
			dispatcher.Register(this, "VisitMethod");
			dispatcher.Register(this, "VisitCalls");
		}
		
		public void VisitAssembly(AssemblyDefinition assembly)
		{
			m_methods.Clear();				// need to do this for unit tests
			m_knownRoots.Clear();
			m_safeMethods.Clear();
			m_singles.Clear();
			m_multies.Clear();
			m_entryPoints.Clear();
			
			m_disabled = assembly.CustomAttributes.HasDisableRule(CheckID);
		}
		
		public void VisitType(TypeDefinition type)
		{
			if (!m_disabled)
				Log.DebugLine(this, "-----------------------------------"); 
		}
		
		// TODO:
		// get disable working
		public void VisitMethod(BeginMethod begin)
		{
			if (!m_disabled)
			{
				Log.DebugLine(this, "{0}", begin.Info.Method);	
				
				string root1 = DoGetSingleRootName(begin.Info.Method);
				string root2 = DoGetMultiRootName(begin.Info.Method);
				MethodState state = new MethodState(begin.Info.Method, root1 != null, root2 != null);
				m_methods.Add(begin.Info.Method, state);
				
				if (root1 != null)
				{
					m_knownRoots.Add(state, root1);
					if (m_singles.IndexOf(root1) < 0)
						m_singles.Add(root1);
				}
				else if (root2 != null)
				{
					m_knownRoots.Add(state, root2);
					if (m_multies.IndexOf(root2) < 0)
						m_multies.Add(root2);
				}
				else
				{
					if (Cache.Assembly.EntryPoint == begin.Info.Method)
					{
						m_entryPoints.Add(begin.Info.Method);
					}
					else if (begin.Info.Method.ExternallyVisible(Cache))
					{
						Log.TraceLine(this, "Externally visible but not marked as a threat root:");
						Log.TraceLine(this, "   {0}", begin.Info.Method);
						m_entryPoints.Add(begin.Info.Method);
					}
				}
				
				if (DoIsMarkedThreadSafe(begin.Info.Method))
					m_safeMethods.Add(state);
			}
		}
		
		public void VisitCalls(CallGraph graph)
		{
			if (!m_disabled)
			{
				string details = string.Empty;
							
				IEnumerable<MethodReference> roots = m_dispatcher.ClassifyMethod.ThreadRoots();
				
				details += DoCheckForUnmarkedRoots(roots);
				if (m_knownRoots.Count > 0)
					details += DoCheckForUnsafeMethods(graph);
				details += DoCheckForBadSafe();
				
				var both = m_singles.Intersect(m_multies);
				if (both.Any())
				{
					details += "Roots marked as both single and multi:" + Environment.NewLine;
					foreach (string root in both)
						details += "   " + root + Environment.NewLine;
				}
				
				details = details.Trim();
				if (details.Length > 0)
				{
					Log.DebugLine(this, "Details: {0}", details);
					Reporter.AssemblyFailed(Cache.Assembly, CheckID, details);
				}
			}
		}
		
		#region Private Methods -----------------------------------------------
		private string DoCheckForUnmarkedRoots(IEnumerable<MethodReference> roots)
		{
			string details = string.Empty;
									
			List<MethodDefinition> unmarkedRoots = DoGetUnmarkedRoots(roots);
			if (unmarkedRoots.Any())
			{
				details += "Not decorated with a thread root attribute:" + Environment.NewLine;

				foreach (MethodDefinition method in unmarkedRoots)
				{
					details += "   " + method.ToString() + Environment.NewLine;
				}
			}
			
			return details;
		}
		
		private string DoCheckForUnsafeMethods(CallGraph graph)	
		{
			foreach (var entry in m_knownRoots)
				DoSetChains(entry.Value, entry.Key.Method, graph, new List<MethodDefinition>{entry.Key.Method}, entry.Key.MarkedAsMulti, 0);
			
			foreach (MethodDefinition m in m_entryPoints)
				DoSetChains("Main", m, graph, new List<MethodDefinition>{m}, false, 0);
		
			string details = string.Empty;			
			foreach (MethodState state in m_methods.Values)
			{
				if (state.IsCalledFromMultiple && !DoIsMarkedThreadSafe(state.Method))
				{
					details += state.GetCallChains("   ") + Environment.NewLine;
				}
			}
			
			if (details.Length > 0)
				details = "Not marked as thread safe: " + Environment.NewLine + details;
												
			return details;
		}
		
		private string DoCheckForBadSafe()
		{
			string details = string.Empty;		
			
			foreach (MethodState state in m_safeMethods)	
			{
				if (!state.IsCalledFromMultiple)
					details += "   " + state.Method + Environment.NewLine;
			}
			
			if (details.Length > 0)
				details = "Marked as unsafe, but called from a single thread: " + Environment.NewLine + details;
												
			return details;
		}
		
		private void DoSetChains(string root, MethodDefinition caller, CallGraph graph, List<MethodDefinition> chain, bool multiple, int depth)
		{
//			if (depth > 12)			// this can't be too large or the rule takes a very long time to run
//				return false;
			
			// For each method the caller calls,
			IEnumerable<MethodReference> callees = graph.GetCalls(caller);
			if (callees != null)
			{
				foreach (MethodReference callee in callees)
				{
					// if it's a method in the assembly we're testing,
					MethodState state;
					if (m_methods.TryGetValue(callee, out state))
					{	
						// and we haven't already called it from our root,
						if (!state.IsCalledFrom(root))
						{
							// then update it's call chain and update all the methods it calls.
							List<MethodDefinition> schain = new List<MethodDefinition>(chain.Count + 1);
							schain.AddRange(chain);
							schain.Add(state.Method);
														
							state.SetCallChain(root, schain, multiple);
							DoSetChains(root, state.Method, graph, schain, multiple, depth + 1);
						}
					}
				}
			}
		}
		
		private List<MethodDefinition> DoGetUnmarkedRoots(IEnumerable<MethodReference> roots)
		{
			List<MethodDefinition> unmarked = new List<MethodDefinition>();
			
			foreach (MethodReference mref in roots)
			{
				MethodState state;
				if (m_methods.TryGetValue(mref, out state))
				{
					if (!state.MarkedAsSingle && !state.MarkedAsMulti)
						unmarked.Add(state.Method);
				}
			}
			
			return unmarked;
		}
		
		private string DoGetSingleRootName(MethodDefinition method)
		{								
			foreach (CustomAttribute attr in method.CustomAttributes)
			{	
				if (attr.Constructor.ToString().Contains("ThreadSingleRootAttribute"))
				{
					if (attr.ConstructorParameters.Count > 0)
					{
						return attr.ConstructorParameters[0] as string;
					}
				}
			}
			
			return null;
		}
		
		private string DoGetMultiRootName(MethodDefinition method)
		{								
			foreach (CustomAttribute attr in method.CustomAttributes)
			{	
				if (attr.Constructor.ToString().Contains("ThreadMultiRootAttribute"))
				{
					if (attr.ConstructorParameters.Count > 0)
					{
						return attr.ConstructorParameters[0] as string;
					}
				}
			}
			
			return null;
		}
		
		private bool DoIsMarkedThreadSafe(MethodDefinition method)
		{								
			foreach (CustomAttribute attr in method.CustomAttributes)
			{	
				if (attr.Constructor.ToString().Contains("ThreadSafe"))
				{
					return true;
				}
			}
			
			return false;
		}
		#endregion
		
		#region Private Types -------------------------------------------------
		private sealed class MethodState
		{
			public MethodState(MethodDefinition method, bool single, bool multi)
			{
				Method = method;
				MarkedAsSingle = single;
				MarkedAsMulti = multi;
			}
			
			public MethodDefinition Method {get; private set;}
		
			public bool MarkedAsSingle {get; private set;}
			public bool MarkedAsMulti {get; private set;}
			
			public bool IsCalledFrom(string root)
			{
				return m_chain.ContainsKey(root);
			}
			
			public bool IsCalledFromMultiple
			{
				get {return m_multiple || m_chain.Count > 1;}
			}
	
			public string GetCallChains(string prefix)
			{
				string result = prefix + Method + Environment.NewLine;
				
				foreach (var entry in m_chain)
				{
					result += prefix + prefix;
					result += string.Join(" -> ", (entry.Value.Select(m => m.ToString()).ToArray()));
					result += Environment.NewLine;
				}
				
				return result;
			}
			
			public void SetCallChain(string root, List<MethodDefinition> chain, bool multiple)
			{
				m_chain.Add(root, chain);
				
				if (multiple)
					m_multiple = true;
			}
			
			private Dictionary<string, List<MethodDefinition>> m_chain = new Dictionary<string, List<MethodDefinition>>();
			private bool m_multiple;
		}
		#endregion

		#region Fields --------------------------------------------------------
		private bool m_disabled;
		private RuleDispatcher m_dispatcher;	
		private Dictionary<MethodReference, MethodState> m_methods = new Dictionary<MethodReference, MethodState>();
		private Dictionary<MethodState, string> m_knownRoots = new Dictionary<MethodState, string>();
		private List<MethodState> m_safeMethods = new List<MethodState>();
		private List<string> m_singles = new List<string>();
		private List<string> m_multies = new List<string>();
		
		private List<MethodDefinition> m_entryPoints = new List<MethodDefinition>();
		#endregion
	}
}


