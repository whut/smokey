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
			m_entryPoints.Clear();
			m_safeTypes.Clear();
			m_requiredSafeTypes.Clear();
			
			m_disabled = assembly.CustomAttributes.HasDisableRule(CheckID);
		}
		
		public void VisitType(TypeDefinition type)
		{
			Unused.Value = type;

			if (!m_disabled)
				Log.DebugLine(this, "-----------------------------------"); 
		}
		
		public void VisitMethod(BeginMethod begin)
		{
			if (!m_disabled)
			{
				Log.DebugLine(this, "{0}", begin.Info.Method);	
								
				string root = DoGetRootName(begin.Info.Method);
				MethodState state = new MethodState
				{
					Method = begin.Info.Method,
					IsThreadRoot = root != null,
					IsThreadSafe = DoIsMarkedThreadSafe(begin.Info.Method),
					IsExternal = begin.Info.Method.ExternallyVisible(Cache),
				};
				m_methods.Add(begin.Info.Method, state);
				
				if (root != null)
				{
					m_knownRoots.Add(state, root);
				}
				else if (state.IsThreadSafe && state.IsExternal)
				{
					m_knownRoots.Add(state, "*");
				}
				else
				{
					if (Cache.Assembly.EntryPoint == begin.Info.Method)
					{
						m_entryPoints.Add(begin.Info.Method);
					}
					else if (state.IsExternal)
					{
						Log.TraceLine(this, "Externally visible but not marked as a thread:");
						Log.TraceLine(this, "   {0}", begin.Info.Method);
						m_entryPoints.Add(begin.Info.Method);
					}
				}
				
				if (DoTypeIsMarkedThreadSafe(begin.Info.Type))
					if (m_safeTypes.IndexOf(begin.Info.Type) < 0)
						m_safeTypes.Add(begin.Info.Type);
				
				if (DoMethodIsMarkedThreadSafe(begin.Info.Method))
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
				details += DoCheckForBadSafe(roots);
				details += DoCheckForBadSafeTypes();
								
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
				details += "Not decorated with a thread attribute:" + Environment.NewLine;

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
				DoSetChains(entry.Value, entry.Key.Method, graph, new List<MethodDefinition>{entry.Key.Method}, entry.Key.IsThreadSafe && entry.Key.IsExternal, 0);
			
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
		
		private string DoCheckForBadSafe(IEnumerable<MethodReference> roots)
		{
			string details = string.Empty;		
			
			foreach (MethodState state in m_safeMethods)	
			{
				if (!state.IsCalledFromMultiple && !state.IsExternal)	
					if (!roots.Any(r => r == state.Method))
						details += "   " + state.Method + Environment.NewLine;
			}
			
			if (details.Length > 0)
				details = "Marked as safe, but called from a single thread: " + Environment.NewLine + details;
												
			return details;
		}
		
//		private List<TypeReference> m_safeTypes = new List<TypeReference>();
//		private List<TypeReference> m_requiredSafeTypes = new List<TypeReference>();

		private string DoCheckForBadSafeTypes()
		{
			string details = string.Empty;	
			
			var bad = m_safeTypes.Except(m_requiredSafeTypes);
			
			foreach (var type in bad)	
			{
				details += "   " + type.FullName + Environment.NewLine;
			}
			
			if (details.Length > 0)
				details = "Types marked as safe, but all methods are called from a single thread: " + Environment.NewLine + details;
												
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
							
							if (m_requiredSafeTypes.IndexOf(state.Method.DeclaringType) < 0)
								m_requiredSafeTypes.Add(state.Method.DeclaringType);
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
					if (!state.IsThreadRoot && !state.IsThreadSafe)
						unmarked.Add(state.Method);
				}
			}
			
			return unmarked;
		}
		
		private string DoGetRootName(MethodDefinition method)
		{								
			foreach (CustomAttribute attr in method.CustomAttributes)
			{	
				if (attr.Constructor.ToString().Contains("ThreadRootAttribute"))
				{
					if (attr.ConstructorParameters.Count > 0)
					{
						return attr.ConstructorParameters[0] as string;
					}
				}
			}
			
			return null;
		}
		
		private bool DoMethodIsMarkedThreadSafe(MethodDefinition method)
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
				
		private bool DoTypeIsMarkedThreadSafe(TypeReference type)
		{
			foreach (CustomAttribute attr in type.CustomAttributes)
			{	
				if (attr.Constructor.ToString().Contains("ThreadSafe"))
				{
					return true;
				}
			}
			
			return false;
		}
				
		private bool DoIsMarkedThreadSafe(MethodDefinition method)
		{								
			if (DoMethodIsMarkedThreadSafe(method))
				return true;

			if (DoTypeIsMarkedThreadSafe(method.DeclaringType))
				return true;
							
			return false;
		}
		#endregion
		
		#region Private Types -------------------------------------------------
		private sealed class MethodState
		{
			public MethodDefinition Method {get; set;}
		
			public bool IsThreadRoot {get; set;}
			public bool IsThreadSafe {get; set;}
			public bool IsExternal {get; set;}
			
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
		
		private List<MethodDefinition> m_entryPoints = new List<MethodDefinition>();

		private List<TypeReference> m_safeTypes = new List<TypeReference>();
		private List<TypeReference> m_requiredSafeTypes = new List<TypeReference>();
		#endregion
	}
}


