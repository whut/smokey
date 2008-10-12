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
using Smokey.Framework.Instructions;
using Smokey.Framework.Support.Advanced;
using Smokey.Internal;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Smokey.Framework.Support
{				
	/// <summary>Rules use this to register the objects they want to visit.</summary>
	///
	/// <remarks><para>Objects are visited in the following order:
	/// BeginTesting				the begin objects are synthesized objects used to demarcate processing
	///	   AssemblyDefinition
	///    ModuleDefinition			visited one or more times
	///    BeginTypes
	///       BeginType				visited once for each type in the assembly
	///          TypeDefinition		
	///          FieldDefinition	these are visited once for each corresponding member in the type
	///          EventDefinition	
	///          PropertyDefinition	
	///          MethodDefinition	
	///       EndType
	///    EndTypes
	///    BeginMethods				
	///       BeginMethod			visited for each method defined in the assembly
	///          each instruction	used to visit a TypedInstruction subclass
	///       EndMethod
	///    EndMethods
	///    CallGraph
	/// EndTesting</para>
	///
	/// <para>To preserve efficiency rules should not do any iteration themselves.
	/// Note that instructions are visited based on the control flow graph in 
	/// depth-first order. Instructions are visited only once (including those in 
	/// finally or fault blocks).</para></remarks>
	[DisableRule("D1041", "CircularReference")]	// Smokey.Framework.Support.Rule <-> Smokey.Framework.Support.RuleDispatcher  
	[DisableRule("D1045", "GodClass")]
	public class RuleDispatcher
	{										
#if TEST
		internal RuleDispatcher()
		{
			m_classifier.Register(this);
		}		
#endif

		internal RuleDispatcher(IReportViolations reporter)
		{
			m_reporter = reporter;
			m_classifier.Register(this);
		}
		
		// Note that using reflection is roughly 6x faster than using a thunk.
		// It'd be slicker to register the methods via attributes but that would
		// be quite a bit slower...
		public void Register<T>(T rule, string method) where T: Rule
		{
#if DEBUG
			Rule r;
			if (m_registered.TryGetValue(rule.CheckID, out r))
				DBC.Assert(r == rule, "multiple rules have id {0}", rule.CheckID);
			else
				m_registered.Add(rule.CheckID, rule);
#endif

			Register(rule, method, rule.GetType().Name, rule.CheckID);
		}
			
		internal void Register(object rule, string method, string name, string checkID) 
		{
			DBC.Pre(rule != null, "rule is null");
			DBC.Pre(!string.IsNullOrEmpty(method), "method is null or empty");
			DBC.Pre(!string.IsNullOrEmpty(name), "name is null or empty");
			DBC.Pre(!string.IsNullOrEmpty(checkID), "checkID is null or empty");

	        System.Reflection.MethodInfo mi = rule.GetType().GetMethod(method, BindingFlags.Public | BindingFlags.Instance);
	        DBC.Pre(mi != null, "{0} isn't a public instance method of {1}", method, rule.GetType());
//	        Console.WriteLine("binding to {0}.{1}", rule.GetType(), method);
//    	    UntypedCallback callback = (UntypedCallback) Delegate.CreateDelegate(typeof(UntypedCallback), rule, mi);

			ParameterInfo[] pis = mi.GetParameters();
			DBC.Pre(pis.Length == 1, "{0}::{1} should only have one argument", rule.GetType().Name, method);
			Type type = pis[0].ParameterType;

			List<RuleCallback> callbacks;
			if (!m_callbackTable.TryGetValue(type, out callbacks))
			{
				callbacks = new List<RuleCallback>();
				m_callbackTable.Add(type, callbacks);
			}

			callbacks.Add(new RuleCallback(name, checkID, rule, mi));
		}
			
		internal Dictionary<string, List<string>> ExcludedNames	
		{
			set {m_excludedNames = value;}
		}
				
		internal void Dispatch(AssemblyDefinition assembly)
		{			
			m_excluding.Clear();
			DoVisit(assembly);
			foreach (ModuleDefinition module in assembly.Modules)
				DoVisit(module);
		}
						
		internal void Dispatch(BeginTesting x)
		{			
			DoVisit(x);
		}
						
		internal void Dispatch(EndTesting x)
		{			
			DoVisit(x);
		}
						
		internal void Dispatch(BeginTypes types)
		{			
			m_excluding.Clear();
			DoVisit(types);
		}
						
		internal void Dispatch(EndTypes types)
		{			
			m_excluding.Clear();
			DoVisit(types);
		}
						
		internal void Dispatch(BeginMethods methods)
		{			
			DoVisit(methods);
		}
						
		internal void Dispatch(EndMethods methods)
		{			
			DoVisit(methods);
		}
						
		internal void Dispatch(TypeDefinition type)
		{			
			DoSetExcluding(type.FullName);
				
			BeginType begin = new BeginType();
			begin.Type = type;
			DoVisit(begin);
			
			DoVisit(type);
			foreach (FieldDefinition field in type.Fields)
				DoVisit(field);
			
			foreach (EventDefinition evt in type.Events)
				DoVisit(evt);
			
			foreach (PropertyDefinition prop in type.Properties)
				DoVisit(prop);
			
			foreach (MethodDefinition method in type.Constructors)
				DoVisit(method);
			
			foreach (MethodDefinition method in type.Methods)
				DoVisit(method);
			
			EndType end = new EndType();
			end.Type = type;
			DoVisit(end);
		}
						
		[System.Diagnostics.Conditional("TEST")]
		[DisableRule("D1032", "UnusedMethod")]
		internal void Dispatch(MethodDefinition method)
		{			
//			Log.ErrorLine(this, "visiting {0}", method);
			DoSetExcluding(method.ToString());
			DoVisit(method);
		}
		
		[System.Diagnostics.Conditional("TEST")]
		[DisableRule("D1032", "UnusedMethod")]
		internal void Dispatch(EventDefinition evt)
		{			
			DoVisit(evt);
		}
		
		internal void Dispatch(MethodInfo info)
		{			
			DoSetExcluding(info.Method.ToString());
				
			BeginMethod begin = new BeginMethod();
			begin.Info = info;
			DoVisit(begin);
						
			m_visited.Clear();
			if (info.Method.Body != null)
			{
				for (int i = 0; i < info.Graph.Roots.Length; ++i)
				{
					if (m_visited.IndexOf(info.Graph.Roots[i]) < 0)
						DoVisit(info.Instructions, info.Graph.Roots[i], info.Instructions.Length - 1, 0, int.MinValue);
				}						
			}
			
			EndMethod end = new EndMethod();
			end.Info = info;
			DoVisit(end);
		}
		
		internal void DispatchCallGraph()
		{			
			m_excluding.Clear();
			DoVisit(m_callGraph);
		}
						
		/// <summary>Returns true if we're visiting an instruction within a loop.</summary>
		public bool Looping
		{
			get {return m_looping;}
		}
						
		public ClassifyMethod ClassifyMethod
		{
			get {return m_classifier;}
		}
						
		#region Private Members -----------------------------------------------
		private void DoVisit(TypedInstructionCollection instructions, BasicBlock block, int lastIndex, int prevIndex, int loopIndex)
		{
			DBC.Assert(m_visited.IndexOf(block) < 0, "{0} has already been visited", block);
			
			if (block.Length < 0 || block.First.Index <= lastIndex)
			{
//				Log.DebugLine(this, "visiting {0}", block);
				
				if (block.Length > 0)
				{				
					if (block.First.Index < prevIndex)		
						if (prevIndex > loopIndex)
							loopIndex = prevIndex;
					
					if (block.First.Index > loopIndex)
						loopIndex = int.MinValue;
//					Log.DebugLine(this, "prevIndex: {0}, loopIndex: {1}", prevIndex, loopIndex);

					// Visit each instruction in the block,
					m_visited.Add(block);
					for (int index = block.First.Index; index <= block.Last.Index; ++index)
					{
						TypedInstruction instruction = instructions[index];
						if (instruction.Untyped.OpCode.Code == Code.Call || instruction.Untyped.OpCode.Code == Code.Callvirt)
						{
							MethodReference target = (MethodReference) instruction.Untyped.Operand;
							m_callGraph.Add(instructions.Method, target);
						}
						
						m_looping = index <= loopIndex;
						DoVisit(instruction);
					}
					
					// the finally block if we have one,
					if (block.Finally != null && m_visited.IndexOf(block.Finally) < 0)
						DoVisit(instructions, block.Finally, lastIndex, block.Last.Index, int.MinValue);
					
					// the fault block if we have one,
					else if (block.Fault != null && m_visited.IndexOf(block.Fault) < 0)
						DoVisit(instructions, block.Fault, lastIndex, block.Last.Index, int.MinValue);
				}
				
				// and each block this block leads to.
				for (int index = 0; index < block.Next.Length; ++index)
				{
					if (m_visited.IndexOf(block.Next[index]) < 0)
						if (block.Length > 0)
							DoVisit(instructions, block.Next[index], lastIndex, block.Last.Index, loopIndex);
						else
							DoVisit(instructions, block.Next[index], lastIndex, prevIndex, loopIndex);
				}
			}
		}
		
		private void DoVisit(object obj)
		{
			List<RuleCallback> callbacks = DoGetCallbacks(obj);	// if store rule name with untyped might be able to profile rules

			m_args[0] = obj;
			
			for (int i = 0; i < callbacks.Count; ++i)
			{
				RuleCallback rule = callbacks[i];

				Profile.Start(rule.Name);
				try
				{
//					Console.WriteLine("visiting {0} for {1} {2}", rule.Name, obj, obj.GetType());

					if (m_excluding.IndexOf(rule.CheckID) < 0)
					{
						rule.Method.Invoke(rule.Instance, m_args);		
					}
//					Console.WriteLine("   done");
				}
				catch (DllNotFoundException dll)
				{
					string details = string.Format("Couldn't load {0}", dll.Message);
					DoRemoveFailedRules(rule.CheckID);
					DoReportFailure(details, dll);
				}
				catch (Exception e)
				{
					string details = string.Format("{0} failed visiting {1}.{2}", rule.Name, obj, Environment.NewLine);

					Exception ee = e;
					while (ee != null)
					{
						details += string.Format("--------- {0} Exception{1}", ee == e ? "Outer" : "Inner", Environment.NewLine);
						details += string.Format("{0}", ee.Message);
						details += string.Format("{0}", ee.StackTrace);
	
						ee = ee.InnerException;
					}

					Log.InfoLine(this, details);
					
					DoRemoveFailedRules(rule.CheckID);
					DoReportFailure(details, e);
				}
				finally
				{
					Profile.Stop(rule.Name);
				}
			}
		}
		
		// If a rule fails we want to report the failure and then continue on 
		// without it. So, if a rule fails we'll remove all the callbacks here
		// so we don't get spammed with errors.
		private void DoRemoveFailedRules(string checkID)
		{
			RuleCallback prototype = new RuleCallback("?", checkID, null, null);
			
			foreach (KeyValuePair<Type, List<RuleCallback>> entry in m_callbackTable)
			{
				Ignore.Value = entry.Value.Remove(prototype);
			}
		}
		
		private void DoReportFailure(string details, Exception e)
		{
			if (m_reporter != null)
			{
				m_reporter.AssemblyFailed(null, "C1004", details);
			}
			else
			{
				Log.ErrorLine(this, details);
				throw e;
			}
		}
		
		private List<RuleCallback> DoGetCallbacks(object obj)
		{
			List<RuleCallback> result = new List<RuleCallback>();

			Type type = obj.GetType();
			while (type != typeof(object))		// need to allow people to register base instruction types
			{
				List<RuleCallback> callbacks;
				if (m_callbackTable.TryGetValue(type, out callbacks))
					result.AddRange(callbacks);
				
				type = type.BaseType;
			}
			
			return result;
		}
		
		private void DoSetExcluding(string name)
		{
			m_excluding.Clear();

			foreach (KeyValuePair<string, List<string>> entry in m_excludedNames)
			{
				if (name.Contains(entry.Key))
				{
//					Console.Error.WriteLine("excluding {0} for {1}", string.Join(" ", entry.Value.ToArray()), name);
					m_excluding.AddRange(entry.Value);
				}
			}
		}
		#endregion
		
		#region Private Types -------------------------------------------------
		private struct RuleCallback
		{
			public readonly string Name;
			public readonly string CheckID;
			public readonly object Instance;
			public readonly System.Reflection.MethodInfo Method;
			
			public RuleCallback(string name, string checkID, object instance, System.Reflection.MethodInfo method)
			{
				Instance = instance;
				Method = method;
				Name = name;
				CheckID = checkID;
			}

			public override bool Equals(object rhsObj)
			{
				if (rhsObj == null)						
					return false;
				
				if (GetType() != rhsObj.GetType()) 
					return false;
			
				RuleCallback rhs = (RuleCallback) rhsObj;					
				return CheckID == rhs.CheckID;
			}
							
			public static bool operator==(RuleCallback lhs, RuleCallback rhs)
			{
				return lhs.CheckID == rhs.CheckID;
			}
			
			public static bool operator!=(RuleCallback lhs, RuleCallback rhs)
			{
				return !(lhs == rhs);
			}    
	
			public override int GetHashCode()
			{
				return CheckID.GetHashCode();
			}				
		}
		#endregion
				
		#region Fields --------------------------------------------------------
		private IReportViolations m_reporter;
		private Dictionary<string, List<string>> m_excludedNames = new Dictionary<string, List<string>>();	// name -> checkIDs
		private List<string> m_excluding = new List<string>();

		private Dictionary<Type, List<RuleCallback>> m_callbackTable = new Dictionary<Type, List<RuleCallback>>();
		private List<BasicBlock> m_visited = new List<BasicBlock>();
		private bool m_looping;
		private CallGraph m_callGraph = new CallGraph();
		private ClassifyMethod m_classifier = new ClassifyMethod();
		
		private object[] m_args = new object[1];
#if DEBUG
			private Dictionary<string, Rule> m_registered = new Dictionary<string, Rule>();
#endif
		#endregion
	}
}


