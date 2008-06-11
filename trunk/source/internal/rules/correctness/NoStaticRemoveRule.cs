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
using Smokey.Internal;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Smokey.Internal.Rules
{	
	internal sealed class NoStaticRemoveRule : Rule
	{				
		public NoStaticRemoveRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "C1026")
		{
			// Collections.Generic
			m_adders.Add  ("System.Collections.Generic.Dictionary`", new List<string>{"Add", "set_Item"});
			m_removers.Add("System.Collections.Generic.Dictionary`", new List<string>{"Clear", "Remove"});
			
			m_adders.Add  ("System.Collections.Generic.HashSet`", new List<string>{"Add", "UnionWith"});
			m_removers.Add("System.Collections.Generic.HashSet`", new List<string>{"Clear", "ExceptWith", "IntersectWith", "Remove", "RemoveWhere", "SymmetricExceptWith"});
			
			m_adders.Add  ("System.Collections.Generic.List`", new List<string>{"Add", "AddRange", "Insert", "InsertRange"});
			m_removers.Add("System.Collections.Generic.List`", new List<string>{"Clear", "Remove", "RemoveAll", "RemoveAt", "RemoveRange"});
			
			m_adders.Add  ("System.Collections.Generic.Queue`", new List<string>{"Enqueue"});
			m_removers.Add("System.Collections.Generic.Queue`", new List<string>{"Clear", "Dequeue"});
			
			m_adders.Add  ("System.Collections.Generic.SortedDictionary`", new List<string>{"Add", "set_Item"});
			m_removers.Add("System.Collections.Generic.SortedDictionary`", new List<string>{"Clear", "Remove"});
			
			m_adders.Add  ("System.Collections.Generic.Stack`", new List<string>{"Push"});
			m_removers.Add("System.Collections.Generic.Stack`", new List<string>{"Clear", "Pop"});
			
			// Collections
			m_adders.Add  ("System.Collections.ArrayList", new List<string>{"Add", "AddRange", "Insert", "InsertRange"});
			m_removers.Add("System.Collections.ArrayList", new List<string>{"Clear", "Remove", "RemoveAt", "RemoveRange"});
			
			m_adders.Add  ("System.Collections.Hashtable", new List<string>{"Add", "set_Item"});
			m_removers.Add("System.Collections.Hashtable", new List<string>{"Clear", "Remove"});
			
			m_adders.Add  ("System.Collections.Queue", new List<string>{"Enqueue"});
			m_removers.Add("System.Collections.Queue", new List<string>{"Clear", "Dequeue"});
			
			m_adders.Add  ("System.Collections.SortedList", new List<string>{"Add", "set_Item"});
			m_removers.Add("System.Collections.SortedList", new List<string>{"Clear", "Remove", "RemoveAt"});
			
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitBegin");
			dispatcher.Register(this, "VisitMethod");
			dispatcher.Register(this, "VisitCall");
			dispatcher.Register(this, "VisitEnd");
		}
		
		public void VisitBegin(BeginMethods begin)
		{
			m_fields.Clear();
			m_table.Clear();
			
			foreach (FieldDefinition field in begin.Type.Fields)
			{
				if (field.IsPrivate && field.IsStatic)
				{
					string key = DoIsCollection(field.FieldType);
					if (key != null)
					{
						Log.DebugLine(this, "-----------------------------------"); 
						Log.DebugLine(this, "{0:F} has a private static collection named {1}", begin.Type, field.Name);				
				
						if (DoHasReferenceElements(field.FieldType))
						{
							Log.DebugLine(this, "and the element types are reference types");	
							m_fields.Add(field);
							m_table.Add(field, new Entry(key));
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
						Entry entry = m_table[load.Field];

						if (m_adders[entry.Key].IndexOf(call.Target.Name) >= 0)
						{
							if (!DoIsNullSet(call.Index, call.Target.Name))
							{
								if (!entry.FoundAdd)
								{
									Log.DebugLine(this, "found an add at {0:X2}", call.Untyped.Offset); 
									entry.FoundAdd = true;
								}
							}
							else
							{
								if (!entry.FoundRemove)
								{
									Log.DebugLine(this, "found a null set at {0:X2}", call.Untyped.Offset); 
									entry.FoundRemove = true;
								}
							}
						}
						
						if (!entry.FoundRemove)
						{
							if (m_removers[entry.Key].IndexOf(call.Target.Name) >= 0)
							{
								Log.DebugLine(this, "found a remove at {0:X2}", call.Untyped.Offset); 
								entry.FoundRemove = true;
							}
						}
					}
				}
			}
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

		public void VisitEnd(EndMethods end)
		{
			string details = string.Empty;
			foreach (FieldDefinition field in m_fields)
			{
				Entry entry = m_table[field];
				if (entry.FoundAdd && !entry.FoundRemove)
					details = details + field.Name + " ";
			}
			
			if (details.Length > 0)
			{
				details = "Fields: " + details;
				Log.DebugLine(this, "{0}", details);				
				
				Reporter.TypeFailed(end.Type, CheckID, details);
			}
		}
				
		private string DoIsCollection(TypeReference type)
		{
			string name = type.FullName;
			
			foreach (string candidate in m_adders.Keys)
			{
				if (name.StartsWith(candidate))
					return candidate;
			}
			
			return null;
		}
		
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
		
		private class Entry
		{
			public Entry(string key)
			{
				m_key = key;
			}
			
			public string Key
			{
				get {return m_key;}
			}
			
			public bool FoundAdd
			{
				get {return m_foundAdd;}
				set {m_foundAdd = value;}
			}
			
			public bool FoundRemove
			{
				get {return m_foundRemove;}
				set {m_foundRemove = value;}
			}
			
			private string m_key;			// into m_adders and m_removers
			private bool m_foundAdd;
			private bool m_foundRemove;
		}
		
		private List<FieldReference> m_fields = new List<FieldReference>();
		private MethodInfo m_info;
		private Dictionary<FieldReference, Entry> m_table = new Dictionary<FieldReference, Entry>();
		
		private Dictionary<string, List<string>> m_adders = new Dictionary<string, List<string>>();
		private Dictionary<string, List<string>> m_removers = new Dictionary<string, List<string>>();
	}
}
