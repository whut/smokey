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
using Mono.Cecil.Metadata;
using Smokey.Framework;
using Smokey.Framework.Instructions;
using Smokey.Framework.Support;
using Smokey.Framework.Support.Advanced;
using Smokey.Internal;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Smokey.Internal.Rules
{	
	internal sealed class DisposeDoesStaticRemoveRule : Rule
	{				
		public DisposeDoesStaticRemoveRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "C1027")
		{
			// System.Collections.Generic
			m_removers.Add("System.Collections.Generic.Dictionary`", new List<string>{"Clear", "Remove", "set_Item"});
			m_removers.Add("System.Collections.Generic.HashSet`", new List<string>{"Clear", "ExceptWith", "IntersectWith", "Remove", "RemoveWhere", "SymmetricExceptWith"});
			m_removers.Add("System.Collections.Generic.List`", new List<string>{"Clear", "Remove", "RemoveAll", "RemoveAt", "RemoveRange"});
			m_removers.Add("System.Collections.Generic.Queue`", new List<string>{"Clear", "Dequeue"});
			m_removers.Add("System.Collections.Generic.SortedDictionary`", new List<string>{"Clear", "Remove", "set_Item"});
			m_removers.Add("System.Collections.Generic.Stack`", new List<string>{"Clear", "Pop"});
			
			// System.Collections
			m_removers.Add("System.Collections.ArrayList", new List<string>{"Clear", "Remove", "RemoveAt", "RemoveRange"});
			m_removers.Add("System.Collections.Hashtable", new List<string>{"Clear", "Remove", "set_Item"});
			m_removers.Add("System.Collections.Queue", new List<string>{"Clear", "Dequeue"});
			m_removers.Add("System.Collections.SortedList", new List<string>{"Clear", "Remove", "RemoveAt", "set_Item"});
			
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitType");
			dispatcher.Register(this, "VisitBegin");
			dispatcher.Register(this, "VisitMethod");
			dispatcher.Register(this, "VisitCall");
			dispatcher.Register(this, "VisitGraph");
		}
				
		public void VisitType(TypeDefinition type)
		{			
			if (IsDisposable.Type(Cache, type))
			{				
				List<MethodDefinition> candidates = new List<MethodDefinition>();
				candidates.AddRange(type.Methods.GetMethod("Dispose"));
				candidates.AddRange(type.Methods.GetMethod("DoDispose"));
				
				foreach (MethodDefinition candidate in candidates)
				{
					if (!candidate.CustomAttributes.HasDisableRule("C1027"))
						m_disposes.Add(candidate);
				}
			}
		}
				
		public void VisitBegin(BeginMethods begin)
		{
			m_fields.Clear();
			m_keys.Clear();
			
			foreach (FieldDefinition field in begin.Type.Fields)
			{
				if (field.IsStatic)
				{
					string key = DoIsCollection(field.FieldType);
					if (key != null)
					{
						Log.DebugLine(this, "-----------------------------------"); 
						Log.DebugLine(this, "{0:F} has a static collection named {1}", begin.Type, field.Name);				
				
						if (DoHasReferenceElements(field.FieldType))
						{
							Log.DebugLine(this, "and the element types are reference types");	
							m_fields.Add(field);
							m_keys.Add(field, key);
						}
					}
				}
			}
		}
		
		public void VisitMethod(BeginMethod begin)
		{
			if (m_fields.Count > 0)
			{
				Log.DebugLine(this, "----------"); 
				Log.DebugLine(this, "{0:F}", begin.Info.Instructions);	
				
				m_info = begin.Info;
			}
		}
		
		public void VisitCall(Call call)
		{
			if (m_fields.Count > 0 && !m_info.Method.IsConstructor)
			{
				int numArgs = call.Target.Parameters.Count;
				int index = m_info.Tracker.GetStackIndex(call.Index, numArgs);
				
				if (index >= 0)
				{
					LoadStaticField load = m_info.Instructions[index] as LoadStaticField;

					if (load != null && m_fields.IndexOf(load.Field) >= 0)
					{
						string key = m_keys[load.Field];
						
						if (m_removers[key].IndexOf(call.Target.Name) >= 0)
						{
							if (key != "set_Item" || DoIsNullSet(call.Index, call.Target.Name))
							{
								Log.DebugLine(this, "found a remove at {0:X2}", call.Untyped.Offset); 
								m_removes[m_info.Method] = load.Field;	// note that we don't really care which field we're calling remove on
							}
						}
					}
				}
			}
		}
		
		public void VisitGraph(CallGraph graph)
		{
			if (m_disposes.Count > 0 && m_removes.Count > 0)
			{				
				foreach (MethodDefinition method in m_disposes)
				{
					Log.DebugLine(this, "checking {0}", method); 

					List<MethodReference> route = new List<MethodReference>();
					if (DoFindRoute(graph, method, route, 0))
					{
//						route.Reverse();
						
						string details = "Calls: ";
						for (int i = 0; i < route.Count; ++i)
						{
							details += route[i].ToString();
							if (i + 1 < route.Count)
								details += " =>" + Environment.NewLine + "       ";
						}
						
						FieldReference field = m_removes[route[route.Count - 1]];
						details += Environment.NewLine + "Field: " + field;

						TypeDefinition type = Cache.FindType(method.DeclaringType);
						if (type != null)
						{
							Log.DebugLine(this, "{0}", details);				
							Reporter.TypeFailed(type, CheckID, details);
						}
						else
							Log.ErrorLine(this, "Couldn't find type definition for {0}", method.DeclaringType.FullName);
					}
				}
			}
		}
		
		private bool DoFindRoute(CallGraph graph, MethodReference method, List<MethodReference> route, int depth)
		{
//			if (depth > 20)
//				return false;
				
			route.Add(method);
			if (m_removes.ContainsKey(method))
			{
				return true;
			}
			
			List<MethodReference> calls = graph.GetCalls(method);
			if (calls != null)
			{
				foreach (MethodReference next in calls)
				{
					if (route.IndexOf(next) < 0)		// don't recurse
					{
						if (DoFindRoute(graph, next, route, depth + 1))
						{
							return true;
						}
					}
				}
			}
			
			route.RemoveAt(route.Count - 1);
			return false;
		}
				
		private string DoIsCollection(TypeReference type)
		{
			string name = type.FullName;
			
			foreach (string candidate in m_removers.Keys)
			{
				if (name.StartsWith(candidate))
					return candidate;
			}
			
			return null;
		}
		
		// Check for table[key] = null
		private bool DoIsNullSet(int index, string name)
		{
			bool isNull = false;
			
			if (name == "set_Item")
			{
				long? value = m_info.Tracker.GetStack(index, 0);
				if (value.HasValue && value.Value == 0)
				{
					isNull = true;
				}
			}
			
			return isNull;
		}

		[DisableRule("D1042", "IdenticalMethods")]	// TODO: shared with NoStaticRemoveRule
		private bool DoHasReferenceElements(TypeReference tr)
		{
			GenericInstanceType gt = tr as GenericInstanceType;
			if (gt != null)
			{
				foreach (TypeReference er in gt.GenericArguments)
				{
					TypeDefinition et = Cache.FindType(er);
					if (et != null && et.FullName != "System.WeakReference" && !et.IsValueType)
					{
						return true;
					}
				}
				return false;
			}
			
			return true;		// non-generic collection
		}
				
		private List<MethodDefinition> m_disposes = new List<MethodDefinition>();
		private Dictionary<string, List<string>> m_removers = new Dictionary<string, List<string>>();

		private List<FieldReference> m_fields = new List<FieldReference>();
		private Dictionary<FieldReference, string> m_keys = new Dictionary<FieldReference, string>();		
		private Dictionary<MethodReference, FieldReference> m_removes = new Dictionary<MethodReference, FieldReference>();
		private MethodInfo m_info;
	}
}
