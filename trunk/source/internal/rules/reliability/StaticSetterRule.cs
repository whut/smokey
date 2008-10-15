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
using System.Collections.Generic;
using Smokey.Framework;
using Smokey.Framework.Instructions;
using Smokey.Framework.Support;
using Smokey.Framework.Support.Advanced;

namespace Smokey.Internal.Rules
{	
	internal sealed class StaticSetterRule : Rule
	{				
		public StaticSetterRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "R1014")
		{
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			m_dispatcher = dispatcher;
			
			dispatcher.Register(this, "VisitAssembly");
			dispatcher.Register(this, "VisitBegin");
			dispatcher.Register(this, "VisitCall");
			dispatcher.Register(this, "VisitStoreStatic");
			dispatcher.Register(this, "VisitEnd");
			dispatcher.Register(this, "VisitCallGraph");
		}
						
		public void VisitAssembly(AssemblyDefinition assembly)
		{
			Unused.Value = assembly;
			
			Log.DebugLine(this, "++++++++++++++++++++++++++++++++++"); 
			m_setters.Clear();		// need to do this here for unit tests
		}
				
		public void VisitBegin(BeginMethod begin)
		{
			Log.DebugLine(this, "{0}", begin.Info.Method); 
			m_wasStatic = false;	
		
			if (!begin.Info.Method.PrivatelyVisible(Cache))
			{
				m_wasStatic = begin.Info.Method.IsStatic;
				m_info = begin.Info;	
		
				if (m_wasStatic)
				{
					m_hasLock = false;
					m_setsState = false;
				}
			}
		}

		public void VisitCall(Call call)
		{
			if (m_wasStatic && !m_hasLock)
			{
				string name = call.Target.ToString();
				if (name.Contains("System.Threading.Monitor::Enter(System.Object)"))
				{
					Log.DebugLine(this, "found lock at {0:X2}", call.Untyped.Offset);
					m_hasLock = true;
				}
			}
		}

		public void VisitStoreStatic(StoreStaticField store)
		{		
			if (m_wasStatic && !m_setsState)
			{
				if (store.Field.DeclaringType.MetadataToken == m_info.Type.MetadataToken)
				{
					if (!DoAtomicField(store.Field) || !DoVolatileStore(store))
					{
						Log.DebugLine(this, "found store at {0:X2}", store.Untyped.Offset);	
						m_setsState = true;
					}
				}
			}
		}

		public void VisitEnd(EndMethod method)
		{
			if (m_wasStatic)
			{
				if (!m_hasLock)
				{
					Log.DebugLine(this, "has no lock");	
					m_unlocked.Add(method.Info.Method);
				}
				if (m_setsState)
				{
					Log.DebugLine(this, "it's a static setter");	
					m_setters.Add(method.Info.Method);
				}
			}
		}
		
		public void VisitCallGraph(CallGraph graph)
		{
			string details = string.Empty;
			List<string> chain = new List<string>();
			
			foreach (MethodReference method in m_dispatcher.ClassifyMethod.ThreadRoots())
			{
				chain.Clear();
				m_visited.Clear();
				if (DoFoundBadSetter(graph, method, chain))
				{					
					details = string.Format("{0}{1}{1}{2}", ListExtensions.Accumulate(
						chain, string.Empty,
						(s, e) => s.Length > 0 ? s + " -> " + Environment.NewLine + e : e),
					Environment.NewLine, details);
				}
			}
			
			details = details.Trim();
			if (details.Length > 0)
			{
				Log.DebugLine(this, details);	
				Reporter.AssemblyFailed(Cache.Assembly, CheckID, details);
			}
		}
		
		private bool DoAtomicField(FieldReference field)
		{
			bool atomic = false;
			
			// atomic if the field is the size of a a native int or smaller (and the write is aligned)
			switch (field.FieldType.FullName)
			{
				case "System.Boolean":
				case "System.Byte":
				case "System.Char":
				case "System.Int16":
				case "System.Int32":
				case "System.IntPtr":
				case "System.SByte":
				case "System.Single":
				case "System.UInt16":
				case "System.UInt32":
				case "System.UIntPtr":
					atomic = true;
					break;
					
				default:
					if (!field.FieldType.IsValueType)
						atomic = true;					// reference type writes are atomic
					break;
			}
						
			return atomic;
		}

		// ldarg.0 
        // volatile. 
        // stsfld bool Smokey.Tests.StaticSetterTest/Static::ms_vatomic
		private bool DoVolatileStore(StoreStaticField store)
		{
			TypedInstruction prior = m_info.Instructions[store.Index - 1];
			return prior.Untyped.OpCode.Code == Code.Volatile;
		}
		
		private bool DoFoundBadSetter(CallGraph graph, MethodReference method, List<string> chain)
		{
			if (m_visited.IndexOf(method) >= 0)
				return false;
			m_visited.Add(method);
			
			bool found = false;
			Log.DebugLine(this, "checking {0}", method);	

			if (m_unlocked.IndexOf(method) >= 0 && m_setters.IndexOf(method) >= 0) 
			{
				Log.DebugLine(this, "it's a setter");	
				found = true;
			}
			else
			{
				foreach (MethodReference callee in graph.Calls(method))
				{
					Log.Indent();
					if (DoFoundBadSetter(graph, callee, chain))
					{
						found = true;
						Log.Unindent();
						break;
					}
					Log.Unindent();
				}
			}
			
			if (found)
				chain.Insert(0, method.ToString());
			
			return found;
		}
		
		private bool m_wasStatic;
		private bool m_hasLock;
		private bool m_setsState;
		private RuleDispatcher m_dispatcher;		
		private MethodInfo m_info;

		private List<MethodReference> m_setters = new List<MethodReference>();
		private List<MethodReference> m_unlocked = new List<MethodReference>();
		private List<MethodReference> m_visited = new List<MethodReference>();
	}
}

