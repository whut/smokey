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
using Smokey.Framework;
using Smokey.Framework.Instructions;
using Smokey.Framework.Support;
using Smokey.Framework.Support.Advanced;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#if OLD
namespace Smokey.Internal.Rules
{	
	internal sealed class NotInstantiatedRule : Rule
	{				
		public NotInstantiatedRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "P1012")
		{
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitBegin");
			dispatcher.Register(this, "VisitType");
			dispatcher.Register(this, "VisitEndTypes");
			dispatcher.Register(this, "VisitMethod");
			dispatcher.Register(this, "VisitCall");
			dispatcher.Register(this, "VisitNewObj");
			dispatcher.Register(this, "VisitInitObj");
			dispatcher.Register(this, "VisitFini");
		}
				
		public void VisitBegin(BeginTesting begin)
		{						
			Unused.Value = begin;
			
			Log.DebugLine(this, "++++++++++++++++++++++++++++++++++"); 
			m_keys.Clear();		// need this for unit tests
			m_types.Clear();
			m_state = 0;
		}
		
		public void VisitType(TypeDefinition type)
		{						
			DBC.Assert(m_state == State.Types, "state is {0}", m_state);
			Log.DebugLine(this, "{0}", type.FullName);

			if (!type.ExternallyVisible(Cache) && DoInstantiable(type) && DoValidType(type))
			{	
				if (!type.IsCompilerGenerated())
				{
					var key = new AssemblyCache.TypeKey(type);
					DBC.Assert(m_keys.IndexOf(key) < 0, "{0} is already in types", type.FullName);
					Log.DebugLine(this, "adding {0}", type.FullName);
					
					m_keys.Add(key);
				}
			}
		}
		
		public void VisitEndTypes(EndTypes end)
		{
			Unused.Value = end;
			
			DBC.Assert(m_state == State.Types, "state is {0}", m_state);

			Log.DebugLine(this, "-------------"); 
			Log.DebugLine(this, "VisitEndTypes");
			
			m_types.AddRange(m_keys.Select(k => k.Type.FullName));
			m_types.Sort();
			m_state = State.Calls;
		}
		
		// These are visited after types.
		public void VisitMethod(BeginMethod begin)
		{
//			Log.DebugLine(this, "{0}", begin.Info.Method);
		}
		
		// call  System.Void Smokey.Tests.NotInstantiatedTest/Tuple2`2<T0,T1>::.ctor(T0,T1)
		public void VisitCall(Call call)
		{
			DBC.Assert(m_state == State.Calls, "state is {0}", m_state);

			if (m_types.Count > 0)
			{
				MethodInfo info = Cache.FindMethod(call.Target);
				if (info != null && info.Method.IsConstructor)
				{
					DoRemoveTypes(info.Method.DeclaringType);
				}
			}
		}
				
		public void VisitNewObj(NewObj newobj)
		{
			DBC.Assert(m_state == State.Calls, "state is {0}", m_state);

			if (m_types.Count > 0)
			{
				DoRemoveTypes(newobj.Ctor.DeclaringType);
			}
		}
				
		public void VisitInitObj(InitObj init)
		{
			DBC.Assert(m_state == State.Calls, "state is {0}", m_state);

			if (m_types.Count > 0)
			{
				DoRemoveTypes(init.Type);
			}
		}
				
		// This is visited after methods.
		public void VisitFini(EndTesting end)
		{
			Unused.Value = end;
			
			DBC.Assert(m_state == State.Calls, "state is {0}", m_state);
			m_state = State.End;

			if (m_types.Count > 0)
			{
				string details = "Unused: " + string.Join(Environment.NewLine, m_types.ToArray());
				Log.DebugLine(this, details);

				Reporter.AssemblyFailed(Cache.Assembly, CheckID, details);
			}
		}
		
		private bool DoInstantiable(TypeDefinition type)
		{
//			Log.DebugLine(this, "   {0} ctors", type.Constructors.Count);
			if (type.IsValueType)
			{
				return true;
			}
			else
			{
				foreach (MethodDefinition ctor in type.Constructors)
				{
					MethodAttributes access = ctor.Attributes & MethodAttributes.MemberAccessMask;
					if (access != MethodAttributes.Compilercontrolled && access != MethodAttributes.Private)
					{
						Log.DebugLine(this, "   is instantiable");
						return true;
					}
				}
			}
			
			return false;
		}
		
		private bool DoValidType(TypeDefinition type)
		{
			if (type.IsEnum)
			{
				Log.DebugLine(this, "   enum");
				return false;
			}
			else if (type.IsAbstract)
			{
				Log.DebugLine(this, "   abstract");
				return false;
			}
			else if (type.IsSubclassOf("System.Delegate", Cache) || type.IsSubclassOf("System.MulticastDelegate", Cache))
			{
				Log.DebugLine(this, "   delegate");
				return false;
			}	
			else if (type.IsSubclassOf("System.Attribute", Cache))
			{
				Log.DebugLine(this, "   attribute");
				return false;
			}	
			else if (DoAnyDisablesRule(type))
			{
				Log.DebugLine(this, "   disabled");
				return false;
			}	
			else if (type.CustomAttributes.Has("CompilerGeneratedAttribute"))
			{
				Log.DebugLine(this, "   compiler generated");
				return false;
			}
				
//			foreach (MethodDefinition method in type.Methods)
//			{
//				if (!method.IsStatic)
//				{
					Log.DebugLine(this, "   valid type");
//					return true;
//				}
//			}

			return true;
		}
		
		private bool DoAnyDisablesRule(TypeDefinition type)
		{
			if (type.CustomAttributes.HasDisableRule("P1012"))
				return true;

			TypeDefinition t = Cache.FindType(type.BaseType);
			if (t != null && DoAnyDisablesRule(t))
				return true;

			foreach (TypeReference tr in type.Interfaces)
			{
				t = Cache.FindType(tr);
				if (t != null && DoAnyDisablesRule(t))
					return true;
			}
			
			return false;
		}
		
		private void DoRemoveTypes(TypeReference tr)
		{
			while (tr != null)
			{
				DoRemove(tr);
				TypeDefinition type = Cache.FindType(tr);
				if (type != null)
					tr = Cache.FindType(type.BaseType);
				else
					tr = null;
			}			
		}
		
		private void DoRemove(TypeReference type)
		{
			string name = type.FullName;
			
			int i = m_types.BinarySearch(name);
			if (i >= 0)
			{
				Log.DebugLine(this, "found reference to {0}", name);
				m_types.RemoveAt(i);
			}
		}
				
		private enum State {Types, Calls, End};
				
		private List<AssemblyCache.TypeKey> m_keys = new List<AssemblyCache.TypeKey>();
		private List<string> m_types = new List<string>();
		private State m_state;
	}
}
#endif
