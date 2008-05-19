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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Smokey.Internal.Rules
{
	// Mapping from a set of field names to the set of methods using those fields.
	// When the partition table is first built the fields are all of length one
	// and each field name appears once and only once. After the partitions are
	// coalesced both field names and methods appear once and only once.
	internal sealed class Partition
	{
		public Partition(string field, MethodDefinition method)
		{
			m_fields.Add(field);
			m_methods.Add(method);
		}
		
		public List<string> Fields			// TODO: in mono 1.9 we can replace this with HashSet
		{
			get {return m_fields;}
		}
		
		public List<MethodDefinition> Methods
		{
			get {return m_methods;}
		}
		
		public bool HasVirtual()
		{
			foreach (MethodDefinition m in Methods)
			{
				if (m.IsVirtual)
					return true;
			}
			
			return false;
		}

		public override string ToString()
		{
//			return 	// this doesnt seem to work in mono 1.2.6
//				Fields.Aggregate(string.Empty, (sum, x, z) => sum + " " + x) +
//				Methods.Aggregate(string.Empty, (sum, x, z) => sum + " " + x.Name);

			StringBuilder b = new StringBuilder();
			
			foreach (string s in Fields)
			{
				b.Append(s);
				b.Append(" ");
			}

			b.Append("<= ");

			foreach (MethodDefinition m in Methods)
			{
				b.Append(m.Name);
				b.Append(" ");
			}

			return b.ToString();			
		}
				
		private List<string> m_fields = new List<string>();
		private List<MethodDefinition> m_methods = new List<MethodDefinition>();
	}
	
	internal sealed class SchizoidTypeRule : Rule
	{				
		public SchizoidTypeRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "D1046")
		{
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitBegin");
			dispatcher.Register(this, "VisitMethod");
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
			
			m_needsCheck = begin.Type.Methods.Count >= 6 && begin.Type.Fields.Count >= 6;
			m_type = begin.Type;
			m_method = null;
			m_partitions.Clear();
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
				
//				else if (begin.Info.Instructions.Length <= 10)
//					m_method = null;			
				
				else
					m_method = method;

//				if (m_method != null)
//					Log.DebugLine(this, "{0}", method);				
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
			if (m_needsCheck)
			{				
				Log.DebugLine(this, "Initial partition:");
				foreach (Partition p in m_partitions)
					Log.DebugLine(this, "{0}", p);				
				Log.DebugLine(this, " ");
				
				DoCoalesce();
				DoClean();
				Log.DebugLine(this, "Final partition:");
				foreach (Partition p in m_partitions)
					Log.DebugLine(this, "{0}", p);				
				
				List<Partition> bad = new List<Partition>();
				foreach (Partition p in m_partitions)
				{
					if (p.Fields.Count >= 3 && p.Methods.Count >= 3 && !p.HasVirtual())
						bad.Add(p);
				}
				
				if (bad.Count >= 2)
				{
					StringBuilder b = new StringBuilder();
					
					foreach (Partition p in bad)
					{
						if (p == bad[bad.Count - 1])
							b.Append(p.ToString());
						else
							b.AppendLine(p.ToString());
					}	

					Reporter.TypeFailed(end.Type, CheckID, b.ToString());
				}
			}
		}
		
		private void DoCoalesce()
		{
			for (int i = 0; i < m_partitions.Count - 1; ++i)
			{
				Partition p = m_partitions[i];
				
				int count = p.Methods.Count;
				for (int k = 0; k < count; ++k)
				{
					MethodDefinition m = p.Methods[k];
					
					for (int j = i + 1; j < m_partitions.Count; ++j)
					{
						Partition q = m_partitions[j];

						if (q.Methods.IndexOf(m) >= 0)
							DoMerge(p, q);
					}
				}
			}

			for (int i = 0; i < m_partitions.Count;)
			{
				if (m_partitions[i].Fields.Count == 0)
				{
					DBC.Assert(m_partitions[i].Methods.Count == 0, "fields is empty but methods isn't");
					m_partitions.RemoveAt(i);
				}
				else
					++i;
			}
		}
		
		private void DoMerge(Partition p, Partition q)
		{
			foreach (string field in q.Fields)
			{
				if (p.Fields.IndexOf(field) < 0)
					p.Fields.Add(field);
			}
			q.Fields.Clear();

			foreach (MethodDefinition method in q.Methods)
			{
				if (p.Methods.IndexOf(method) < 0)
					p.Methods.Add(method);
			}
			q.Methods.Clear();
		}
						
		public void DoAddField(FieldReference field)
		{
			if (m_needsCheck && m_method != null)
			{
				if (field.DeclaringType.FullName == m_type.FullName)
				{
					Partition p = m_partitions.Find(x => x.Fields.IndexOf(field.Name) >= 0);
					if (p != null)
					{
						if (p.Methods.IndexOf(m_method) < 0)
							p.Methods.Add(m_method);
					}
					else
					{
						p = new Partition(field.Name, m_method);					
						m_partitions.Add(p);
					}
				}
			}
		}
		
		private void DoClean()
		{
			foreach (Partition p in m_partitions)
			{
				for (int i = 0; i < p.Methods.Count;)
				{
					MethodDefinition method = p.Methods[i];
					
					if (method.IsPrivate || method.Name.StartsWith("get_") || method.Name.StartsWith("set_"))
						p.Methods.RemoveAt(i);
					else
						++i;
				}
			}
		}

		private List<Partition> m_partitions = new List<Partition>();
		private bool m_needsCheck;
		private TypeDefinition m_type;
		private MethodDefinition m_method;
	}
}

