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
	internal sealed class RecursiveLock2Rule : Rule
	{				
		public RecursiveLock2Rule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "R1038")
		{
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitBegins");
			dispatcher.Register(this, "VisitBegin");
			dispatcher.Register(this, "VisitCall");
			dispatcher.Register(this, "VisitEnds");
			dispatcher.Register(this, "VisitCallGraph");
		}
				
		public void VisitBegins(BeginMethods begin)
		{
			Log.DebugLine(this, "-----------------------------------"); 
			Log.DebugLine(this, "{0}", begin.Type);				

			m_current = new Entry();
		}
		
		public void VisitBegin(BeginMethod begin)
		{
			Log.DebugLine(this, "---------------"); 
			Log.DebugLine(this, "{0:F}", begin.Info.Instructions);				

			m_info = begin.Info;
		}
		
		public void VisitCall(Call call)
		{
			DoSaveCalls(call);
			DoSaveExternalCalls(call);
		}

		public void VisitEnds(EndMethods end)
		{
			m_table.Add(end.Type, m_current);			
			m_current = null;
		}
		
		public void VisitCallGraph(CallGraph graph)
		{
			foreach (KeyValuePair<TypeDefinition, Entry> entry1 in m_table)
			{
				foreach (KeyValuePair<MethodDefinition, List<MethodReference>> entry2 in entry1.Value.Calls)
				{
					foreach (MethodReference method in entry2.Value)
					{
						List<string> chain = new List<string>();
						chain.Add(entry2.Key.ToString());
						
						string name = string.Empty;
						if (DoCallsExternal(method, entry1.Value, graph, chain, 0, ref name))
						{
							string details = "Field: " + name + Environment.NewLine;
							details += "Calls: " + string.Join(" =>" + Environment.NewLine + "       ", chain.ToArray());
							Log.DebugLine(this, details);
							
							Reporter.TypeFailed(entry1.Key, CheckID, details);
						}
					}
				}
			}
		}
		
		[DisableRule("D1047", "TooManyArgs")]
		private bool DoCallsExternal(MethodReference method, Entry entry, CallGraph graph, List<string> chain, int depth, ref string name)
		{
			if (depth > 8)			// this can't be too large or the rule takes a very long time to run
				return false;
			
			chain.Add(method.ToString());
			if (entry.ExternalCalls.ContainsKey(method))
			{
				name = entry.ExternalCalls[method];
				return true;
			}
			
			foreach (MethodReference candidate in graph.Calls(method))
			{
				if (DoCallsExternal(candidate, entry, graph, chain, depth + 1, ref name))
					return true;
			}
			chain.RemoveAt(chain.Count - 1);
			
			return false;
		}
						
		private void DoSaveCalls(Call call)
		{
			// If the call acquires a lock,
			if (DoMatchLock1(call.Index) || DoMatchLock2(call.Index))
			{				
				// and the lock is immediately followed by a try/finally block then,
				TryCatch tc = m_info.Instructions.TryCatchCollection.HandlerStartsAt(call.Index + 1);
				if (tc != null && tc.Finally.Length > 0)
				{
					Log.DebugLine(this, "found a lock at {0:X2}", call.Untyped.Offset);

					// iterate over each instruction in the try block,
					for (int index = call.Index + 1; index < call.Index + tc.Try.Length + 1; ++index)
					{
						Call call2 = m_info.Instructions[index] as Call;
						if (call2 != null)
						{
							List<MethodReference> methods;
							
							// record each method we call while holding the lock,
							if (!m_current.Calls.TryGetValue(m_info.Method, out methods))
							{
								methods = new List<MethodReference>();
								m_current.Calls.Add(m_info.Method, methods);
							}
							
							methods.Add(call2.Target);
							Log.DebugLine(this, "{0} is called under a lock", call2.Target.Name);
							
							// and check to see if we're making a direct call to external code.
							string name = string.Empty;
							if (DoCallsExternalCode(call2, ref name))
							{
								string details = "Field: " + name;
								Log.DebugLine(this, details + " (direct external call)");								
								Reporter.TypeFailed(m_info.Type, CheckID, details);
							}
						}
					}
				}
			}
		}
		
		private void DoSaveExternalCalls(Call call)
		{
			if (!m_current.ExternalCalls.ContainsKey(m_info.Method))
			{
				string name = string.Empty;
				if (DoCallsExternalCode(call, ref name))
				{
					Log.DebugLine(this, "external call at {0:X2}", call.Untyped.Offset);
					m_current.ExternalCalls[m_info.Method] = name;	// we just care that the method calls external code, we don't really care which field it uses if it uses more than one
				}
			}
		}
		
		private bool DoCallsExternalCode(Call call, ref string name)
		{
			if (call.Target.Name == "Invoke" && call.Target.HasThis)
			{
				int i = call.GetThisIndex(m_info);
				Log.DebugLine(this, "found invoke call at {0:X2}", call.Untyped.Offset);
				
				if (i >= 0)
				{
					LoadField load = m_info.Instructions[i] as LoadField;
					if (load != null)
					{
						Log.DebugLine(this, "   this pointer is at {0:X2}", load.Untyped.Offset);
						if (load.Field.FieldType.IsSameOrSubclassOf("System.Delegate", Cache))
						{
							Log.DebugLine(this, "   and is a delegate type");
							name = load.Field.ToString();
							return true;
						}
					}
				}
			}
			
			return false;
		}
				
		// 00: ldsfld     System.Object EvilDoer.BadLocker2::lock1
		// 05: stloc.N    V_0
		// 06: ldloc.N    V_0
		// 07: call       System.Void System.Threading.Monitor::Enter(System.Object)
		private bool DoMatchLock1(int index)
		{
			bool match = false;

			do
			{
				if (index - 3 < 0 || index >= m_info.Instructions.Length)
					break;

				TypedInstruction instruction = m_info.Instructions[index - 0];
				Code code = instruction.Untyped.OpCode.Code;
				if (code != Code.Call && code != Code.Callvirt)
					break;
				if (((Call) instruction).Target.ToString() != "System.Void System.Threading.Monitor::Enter(System.Object)")
					break;

				instruction = m_info.Instructions[index - 1];
				code = instruction.Untyped.OpCode.Code;
				if (code != Code.Ldloc_S && code != Code.Ldloc && code != Code.Ldloc_0 && code != Code.Ldloc_1 && code != Code.Ldloc_2 && code != Code.Ldloc_3)
					break;
				int varN = ((LoadLocal) instruction).Variable;

				instruction = m_info.Instructions[index - 2];
				code = instruction.Untyped.OpCode.Code;
				if (code != Code.Stloc_S && code != Code.Stloc && code != Code.Stloc_0 && code != Code.Stloc_1 && code != Code.Stloc_2 && code != Code.Stloc_3)
					break;
				if (varN != ((StoreLocal) instruction).Variable)
					break;

				instruction = m_info.Instructions[index - 3];
				code = instruction.Untyped.OpCode.Code;
				if (code != Code.Ldsfld)
					break;

				match = true;
			}
			while (false);

			return match;
		}

		// 00: Ldarg.0    this
		// 01: ldfld      System.Object EvilDoer.BadLocker1::lock1
		// 06: stloc.N    V_0
		// 07: ldloc.N    V_0
		// 08: call       System.Void System.Threading.Monitor::Enter(System.Object)
		private bool DoMatchLock2(int index)
		{
			bool match = false;

			do
			{
				if (index - 4 < 0 || index >= m_info.Instructions.Length)
					break;

				TypedInstruction instruction = m_info.Instructions[index - 0];
				Code code = instruction.Untyped.OpCode.Code;
				if (code != Code.Call && code != Code.Callvirt)
					break;
				if (((Call) instruction).Target.ToString() != "System.Void System.Threading.Monitor::Enter(System.Object)")
					break;

				instruction = m_info.Instructions[index - 1];
				code = instruction.Untyped.OpCode.Code;
				if (code != Code.Ldloc_S && code != Code.Ldloc && code != Code.Ldloc_0 && code != Code.Ldloc_1 && code != Code.Ldloc_2 && code != Code.Ldloc_3)
					break;
				int varN = ((LoadLocal) instruction).Variable;

				instruction = m_info.Instructions[index - 2];
				code = instruction.Untyped.OpCode.Code;
				if (code != Code.Stloc_S && code != Code.Stloc && code != Code.Stloc_0 && code != Code.Stloc_1 && code != Code.Stloc_2 && code != Code.Stloc_3)
					break;
				if (varN != ((StoreLocal) instruction).Variable)
					break;

				instruction = m_info.Instructions[index - 3];
				code = instruction.Untyped.OpCode.Code;
				if (code != Code.Ldfld)
					break;

				instruction = m_info.Instructions[index - 4];
				code = instruction.Untyped.OpCode.Code;
				if (code != Code.Ldarg_0)
					break;

				match = true;
			}
			while (false);

			return match;
		}
				
		private class Entry
		{
			public readonly Dictionary<MethodDefinition, List<MethodReference>> Calls;	// methods called while the lock is held
			public readonly Dictionary<MethodReference, string> ExternalCalls;			// methods that call a delegate or fire an event
			
			public Entry()
			{
				Calls = new Dictionary<MethodDefinition, List<MethodReference>>();
				ExternalCalls = new Dictionary<MethodReference, string>();
			}
		}
		
		private MethodInfo m_info;
		private Entry m_current;
		private Dictionary<TypeDefinition, Entry> m_table = new Dictionary<TypeDefinition, Entry>();
	}
}

