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
	internal sealed class RecursiveLock1Rule : Rule
	{				
		public RecursiveLock1Rule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "R1037")
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
			FieldReference field = DoSaveLocks(call);
			if (field != null)
				DoSaveCalls(call, field);
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
				foreach (KeyValuePair<Source, List<MethodReference>> entry2 in entry1.Value.Calls)
				{
					foreach (MethodReference method in entry2.Value)
					{
						List<string> chain = new List<string>();
						chain.Add(entry2.Key.Method.ToString());
						
						if (DoCallsLock(method, entry2.Key.Field, entry1.Value, graph, chain, 0))
						{
							string details = "Field: " + entry2.Key.Field.Name + Environment.NewLine;
							details += "Calls: " + string.Join(" =>" + Environment.NewLine + "       ", chain.ToArray());
							Log.DebugLine(this, details);
							
							Reporter.TypeFailed(entry1.Key, CheckID, details);
						}
					}
				}
			}
		}
		
		[DisableRule("D1047", "TooManyArgs")]
		private bool DoCallsLock(MethodReference method, FieldReference field, Entry entry, CallGraph graph, List<string> chain, int depth)
		{
			if (depth > 8)			// this can't be too large or the rule takes a very long time to run
				return false;
			
			chain.Add(method.ToString());
			if (entry.Locked.ContainsKey(method) && entry.Locked[method].IndexOf(field) >= 0)
			{
				return true;
			}
			
			foreach (MethodReference candidate in graph.Calls(method))
			{
				if (DoCallsLock(candidate, field, entry, graph, chain, depth + 1))
					return true;
			}
			chain.RemoveAt(chain.Count - 1);
			
			return false;
		}
		
		private FieldReference DoSaveLocks(Call call)
		{
			FieldReference field = null;
			
			// Look for a lock using one of our fields.
			if (DoMatchLock1(call.Index))
			{
				LoadStaticField sload = (LoadStaticField) m_info.Instructions[call.Index - 3];
				if (sload.Field.DeclaringType == m_info.Type)
					field = sload.Field;
			}
			else if (DoMatchLock2(call.Index))
			{
				LoadField load = (LoadField) m_info.Instructions[call.Index - 3];
				field = load.Field;
			}
			
			// If we found one then record that the method locked the field.
			if (field != null)
			{
				List<FieldReference> fields;
				
				if (!m_current.Locked.TryGetValue(m_info.Method, out fields))
				{
					fields = new List<FieldReference>();
					m_current.Locked.Add(m_info.Method, fields);
				}
				
				fields.Add(field);
				Log.DebugLine(this, "{0} locks {1}", m_info.Method.Name, field.Name);
			}
			
			return field;
		}
				
		private void DoSaveCalls(Call call, FieldReference field)
		{
			// If the lock is immediately followed by a try/finally block then,
			TryCatch tc = m_info.Instructions.TryCatchCollection.HandlerStartsAt(call.Index + 1);
			if (tc != null && tc.Finally.Length > 0)
			{
				// iterate over each instruction in the try block,
				for (int index = call.Index + 1; index < call.Index + tc.Try.Length + 1; ++index)
				{
					Call call2 = m_info.Instructions[index] as Call;
					if (call2 != null)
					{
						List<MethodReference> methods;
						
						// and record each method we call while holding the lock.
						Source key = new Source(m_info.Method, field);
						if (!m_current.Calls.TryGetValue(key, out methods))
						{
							methods = new List<MethodReference>();
							m_current.Calls.Add(key, methods);
						}
						
						methods.Add(call2.Target);
						Log.DebugLine(this, "{0} is called while {1} is locked", call2.Target.Name, field.Name);
					}
				}
			}
		}
				
		// 00: ldsfld     System.Object EvilDoer.BadLocker2::lock1
		// 05: stloc.N    V_0
		// 06: ldloc.N    V_0
		// 07: call       System.Void System.Threading.Monitor::Enter(System.Object)
		[DisableRule("D1042", "IdenticalMethods")]		// TODO: matches RecursiveLock2Rule
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
		[DisableRule("D1042", "IdenticalMethods")]		// TODO: matches RecursiveLock2Rule
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
		
		private struct Source
		{
			public MethodDefinition Method;
			public FieldReference Field;
			
			public Source(MethodDefinition m, FieldReference f)
			{
				Method = m;
				Field = f;
			}

			public override bool Equals(object rhsObj)
			{
				if (rhsObj == null)                  
					return false;
				
				if (GetType() != rhsObj.GetType()) 
					return false;
			
				Source rhs = (Source) rhsObj;                    
				return Method == rhs.Method && Field == rhs.Field;
			}
				
			public override int GetHashCode()
			{
				int hash;
				
				unchecked
				{
					hash = Method.GetHashCode() + Field.GetHashCode();
				}
				
				return hash;
			}
 		}
		
		private sealed class Entry
		{
			public readonly Dictionary<MethodReference, List<FieldReference>> Locked;	// fields the method locks
			public readonly Dictionary<Source, List<MethodReference>> Calls;			// methods called while the lock is held
			
			public Entry()
			{
				Locked = new Dictionary<MethodReference, List<FieldReference>>();
				Calls = new Dictionary<Source, List<MethodReference>>();
			}
		}
		
		private MethodInfo m_info;
		private Entry m_current;
		private Dictionary<TypeDefinition, Entry> m_table = new Dictionary<TypeDefinition, Entry>();
	}
}

