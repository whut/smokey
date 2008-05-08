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
using System;
using System.Collections.Generic;
using Smokey.Framework;
using Smokey.Framework.Instructions;
using Smokey.Framework.Support;

// This seems to work as intended, but it fires a lot for classes which are a
// bit unfocused (like AssemblyCache) but not really poorly written.

	<Violation checkID = "D1046" severity = "Nitpick">
		<Translation lang = "en" typeName = "MaximizeCohesion" category = "Design">			
			<Cause>
			A class has low cohesion.
			</Cause>
		
			<Description>
			This rule measures the relatedness of the non-trivial  methods in a class relative to each other.
			It does this by checking every method pair to see if they share a common field. If
			every method pair shares a common field the class is perfectly cohesive and the cohesion
			will be 1.0. If no methods share a common field the class is not at all cohesive and
			cohesion will be 0.0.
			
			Cohesion is one of the most important metrics in object oriented design: classes are
			much easier to develop with and maintain if they are narrowly tailored to support one
			and only one reponsibility. See 
			&lt;http://en.wikipedia.org/wiki/Cohesion_%28computer_science%29&gt; for more details
			on cohesion.
			</Description>
	
			<Fix>
			The class probably has multiple responsibilities or does not have a clear purpose.
			Break the class into two or more separate classes and make each class responsible
			for one thing.
			</Fix>
		</Translation>
	</Violation>	

namespace Smokey.Internal.Rules
{	
	internal class MaximizeCohesionRule : Rule
	{				
		public MaximizeCohesionRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "D1046")
		{
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitBegin");
			dispatcher.Register(this, "VisitMethod");
			dispatcher.Register(this, "VisitCall");
			dispatcher.Register(this, "VisitLoadField");
			dispatcher.Register(this, "VisitLoadFieldAdr");
			dispatcher.Register(this, "VisitStoreField");
			dispatcher.Register(this, "VisitSLoadField");
			dispatcher.Register(this, "VisitSStoreField");
			dispatcher.Register(this, "VisitEnd");
		}
		
		public void VisitBegin(BeginMethods begin)
		{
			Log.DebugLine(this, "-----------------------------------"); 
			Log.DebugLine(this, "{0}", begin.Type);				
			
			m_needsCheck = begin.Type.Methods.Count >= 4 && begin.Type.Fields.Count >= 4;
			m_type = begin.Type;
			m_method = null;
			m_valid.Clear();
			m_calls.Clear();
			m_table.Clear();
		}
		
		public void VisitMethod(BeginMethod begin)
		{
			if (m_needsCheck)
			{
				MethodDefinition method = begin.Info.Method;				
				if (method.IsConstructor)
					m_method = null;
					
				else if (method.Name == "Finalize")
					m_method = null;			
				
				else if (begin.Info.Instructions.Length <= 10)
					m_method = null;			
				
				else
					m_method = method;

				Log.DebugLine(this, "{0} has {1} instructions", method.Name, begin.Info.Instructions.Length);				
				m_calls.Add(method, new List<MethodReference>());
				m_table.Add(method, new List<string>());
				
				if (m_method != null)
					m_valid.Add(m_method);
			}
		}
		
		public void VisitCall(Call call)
		{
			if (m_needsCheck && m_method != null)
			{
				if (call.Target.DeclaringType.FullName == m_type.FullName)
				{
					if (m_calls[m_method].IndexOf(call.Target) < 0)			
						m_calls[m_method].Add(call.Target);					
				}
			}
		}
		
		public void VisitLoadField(LoadField load)
		{
			DoAddField(load.Field);
		}
		
		public void VisitLoadFieldAdr(LoadFieldAddress load)
		{
			DoAddField(load.Field);
		}
		
		public void VisitStoreField(StoreField store)
		{
			DoAddField(store.Field);
		}
		
		public void VisitSLoadField(LoadStaticField load)
		{
			DoAddField(load.Field);
		}
		
		public void VisitSStoreField(StoreStaticField store)
		{
			DoAddField(store.Field);
		}
				
		public void VisitEnd(EndMethods end)
		{
			if (m_needsCheck && m_table.Count >= 4)
			{				
				float cohesion = DoGetCohesion();
				string details = string.Format("Cohesion: {0:0.0}", cohesion);
				Log.DebugLine(this, details);	

				if (cohesion < 0.5)
				{
					Reporter.TypeFailed(end.Type, CheckID, details);
				}
			}
		}
				
		public void DoAddField(FieldReference field)
		{
			if (m_needsCheck && m_method != null)
			{
				if (field.DeclaringType.FullName == m_type.FullName)
					if (m_table[m_method].IndexOf(field.Name) < 0)
						m_table[m_method].Add(field.Name);
			}
		}
		
		private float DoGetCohesion()
		{
			Dictionary<int, List<string>> table = DoMergeMethods();
			if (table.Count < 4)
				return 1.0f;
			
			float share = 0.0f;		// number of method pairs that share a common field
			float unshare = 0.0f;	// number of method pairs that do not share a field
			
			List<int> ids = new List<int>(table.Keys);
			for (int i = 0; i < ids.Count - 1; ++i)
			{
				for (int j = i + 1; j < ids.Count; ++j)
				{
					if (DoShareFields(table[ids[i]], table[ids[j]]))
						share += 1.0f;
					else
						unshare += 1.0f;
				}
			}

			Log.DebugLine(this, "share: {0}, unshare: {1}", share, unshare);				
			
			return share/(share + unshare);

#if NOT_USED		
			List<MethodReference> methods = new List<MethodReference>(m_table.Keys);

			float share = 0.0f;		// number of method pairs that share a common field
			float unshare = 0.0f;	// number of method pairs that do not share a field
			
			share += methods.Count;	// for symmetry a method is cohesive with itself (this way a half cohesive method has a value of 0.5)
			for (int i = 0; i < methods.Count - 1; ++i)
			{
				for (int j = i + 1; j < methods.Count; ++j)
				{
					if (DoShareFields(methods[i], methods[j]))
						share += 1.0f;
					else
						unshare += 1.0f;
				}
			}
			
			return share/(share + unshare);
#endif
		}
		
		private bool DoShareFields(List<string> l, List<string> r)
		{			
			if (l.Count == 0 || r.Count == 0)	// if one or both methods use no fields then they don't affect cohesion
				return true;
				
			foreach (string s in l)
			{
				if (r.IndexOf(s) >= 0)
					return true;
			}
			
			return false;
		}
		
#if NOT_USED		
		private bool DoShareFields(MethodReference lhs, MethodReference rhs)
		{
			List<string> l = m_table[lhs];
			List<string> r = m_table[rhs];
			
			if (l.Count == 0 || r.Count == 0)	// if one or both methods use no fields then they don't affect cohesion
				return true;
				
			foreach (string s in l)
			{
				if (r.IndexOf(s) >= 0)
					return true;
			}
			
			return false;
		}
#endif
		
		// For cohesion we don't want to consider the methods in isolation. Instead
		// we want to treat the transitive closure of internal methods calls as a
		// single unit so that we get an accurate representation of which fields
		// the method uses directly or indirectly.
		private Dictionary<int, List<string>> DoMergeMethods()
		{
			Dictionary<int, List<string>> table = new Dictionary<int, List<string>>();
			Dictionary<int, List<string>> names = new Dictionary<int, List<string>>();

			int id = 0;
			Dictionary<MethodReference, int> merged = new Dictionary<MethodReference, int>();
			foreach (MethodReference method in m_calls.Keys)
			{
				DoAddMethod(method, merged, id);
				names.Add(id, new List<string>());
				++id;
			}
			
			foreach (KeyValuePair<MethodReference, int> entry in merged)
			{
				List<string> lhs;
				List<string> rhs = m_table[entry.Key];
				if (rhs.Count > 0 && m_valid.IndexOf(entry.Key) >= 0)
				{
					if (!table.TryGetValue(entry.Value, out lhs))
					{
						lhs = new List<string>();
						table.Add(entry.Value, lhs);
					}
									
					foreach (string field in rhs)
					{
						if (lhs.IndexOf(field) < 0)
							lhs.Add(field);
					}
				}
					
				names[entry.Value].Add(entry.Key.Name);
			}
			
#if DEBUG
			Log.DebugLine(this, " ");			
			foreach (KeyValuePair<int, List<string>> entry in names)
			{
				if (table.ContainsKey(entry.Key) && table[entry.Key].Count > 0)
				{
					string[] a = entry.Value.ToArray();
					Array.Sort(a);
					Log.DebugLine(this, "({0}) {1}:", entry.Key, string.Join(" ", a));	
					
					a = table[entry.Key].ToArray();
					Array.Sort(a);
					Log.DebugLine(this, "   {0}", string.Join(" ", a));			
				}
			}
#endif

			List<int> ids = new List<int>(table.Keys);
			foreach (int i in ids)
			{
				if (table[i].Count == 0)
					table.Remove(i);
			}
			
#if DEBUG
			Log.DebugLine(this, " ");			
			foreach (KeyValuePair<int, List<string>> entry in table)
			{
				Log.DebugLine(this, "{0} => {1}", entry.Key, string.Join(", ", entry.Value.ToArray()));			
			}
			Log.DebugLine(this, " ");			
#endif
			
			return table;
		}
		
		private void DoAddMethod(MethodReference method, Dictionary<MethodReference, int> merged, int id)
		{
			if (!merged.ContainsKey(method))
			{
				merged.Add(method, id);

				foreach (MethodReference m in m_calls[method])
					DoAddMethod(m, merged, id);
			}
		}
				
		private bool m_needsCheck;
		private TypeDefinition m_type;
		private MethodDefinition m_method;
		private List<MethodReference> m_valid = new List<MethodReference>();
		private Dictionary<MethodReference, List<MethodReference>> m_calls = new Dictionary<MethodReference, List<MethodReference>>();
		private Dictionary<MethodReference, List<string>> m_table = new Dictionary<MethodReference, List<string>>();
	}
}

