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
using Smokey.Framework.Support;
using Smokey.Framework.Instructions;
using Smokey.Framework.Support.Advanced;
using System;
using System.Collections.Generic;

namespace Smokey.Internal.Rules
{	
	internal sealed class PublicImplementationRule : Rule
	{				
		public PublicImplementationRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "D1051")
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
			DBC.Assert(!m_checkingCalls, "VisitBegin was called after we started visiting calls");
			
			// If the class is internal,
			if (begin.Type.IsNotPublic || begin.Type.IsNestedAssembly || begin.Type.IsNestedFamilyAndAssembly)
			{
				Log.DebugLine(this, "-----------------------------------"); 
				Log.DebugLine(this, "{0}", begin.Type);				
				
				// and it directly implements one or more public interfaces,
				int count = DoCountInterfaces(begin.Type);
				if (count > 0)
				{	
					// then build a list of all the non-private/protected methods 
					// in the class that were not declared in an interface.					
					foreach (MethodDefinition method in begin.Type.Methods)
					{
						if (!method.IsPrivate && !method.IsFamily)
						{
							if (!method.IsConstructor)
							{
								TypeReference t = method.GetDeclaredIn(Cache);
								if (t == begin.Type)
								{
									Log.DebugLine(this, "   adding {0}", method.Name);				
									m_candidates.Add(method, new Info(begin.Type));
								}
							}
						}
					}
				}
			}
		}		
				
		public void VisitMethod(BeginMethod begin)
		{
			Log.DebugLine(this, "++++++++++++++++++++++++"); 
			Log.DebugLine(this, "{0:F}", begin.Info.Instructions);				

			m_type = begin.Info.Type;
		}		
				
		public void VisitCall(Call call)
		{
			m_checkingCalls = true;
			
			Info info;
			if (m_candidates.TryGetValue(call.Target, out info))
			{
				if (info.CalledBy == null || info.CalledBy == info.Defined)
				{
					Log.DebugLine(this, "   {0} was called by {1}", call.Target, m_type.FullName);	
					info.CalledBy = m_type;
				}
			}
		}		
				
		public void VisitFini(EndTesting end)
		{
			Dictionary<TypeDefinition, List<string>> bad = new Dictionary<TypeDefinition, List<string>>();
			List<TypeDefinition> hasCalls = new List<TypeDefinition>();
		
			foreach (KeyValuePair<MethodReference, Info> entry in m_candidates)
			{
				// We have a violation if a candidate method is only
				// called by its declaring type or a subclass (if it's not called at
				// all we'll let the UnusedMethod rule handle it).
				if (entry.Value.CalledBy != null)
				{
					if (entry.Value.CalledBy == entry.Value.Defined || entry.Value.CalledBy.IsSubclassOf(entry.Value.Defined, Cache))
					{
						List<string> methods;
						
						if (!bad.TryGetValue(entry.Value.Defined, out methods))
						{
							methods = new List<string>();
							bad.Add(entry.Value.Defined, methods);
						}
						
						methods.Add(entry.Key.ToString());
					}
					else
						hasCalls.Add(entry.Value.Defined);
				}
			}
			
			foreach (KeyValuePair<TypeDefinition, List<string>> entry2 in bad)
			{
				if (hasCalls.IndexOf(entry2.Key) < 0)
				{
					string details = "Methods: " + string.Join(Environment.NewLine, entry2.Value.ToArray());
					Log.DebugLine(this, details);	
					Reporter.TypeFailed(entry2.Key, CheckID, details);
				}
			}			
		}

		private int DoCountInterfaces(TypeDefinition type)
		{
			int count = 0;
			
			foreach (TypeReference i in type.Interfaces)
			{
				TypeDefinition id = Cache.FindType(i);
				if (id != null && (id.IsPublic || id.IsNestedPublic))
					++count;
			}
			
			return count;
		}
		
		private class Info
		{
			public Info(TypeDefinition defined)
			{
				m_definedIn = defined;
			}
			
			public TypeDefinition Defined
			{
				get {return m_definedIn;}
			}
			
			public TypeReference CalledBy
			{
				get {return m_calledBy;}
				set {m_calledBy = value;}
			}
			
			private TypeDefinition m_definedIn;
			private TypeReference m_calledBy;
		}
		
		private TypeDefinition m_type;
		private bool m_checkingCalls;
		private Dictionary<MethodReference, Info> m_candidates = new Dictionary<MethodReference, Info>();
	}
}

