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
using System.Diagnostics;
using System.Linq;

#if OLD
namespace Smokey.Internal.Rules
{	
	internal sealed class EqualsMissesStateRule : Rule
	{				
		public EqualsMissesStateRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "C1036")
		{
		}
		
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitBegin");
			dispatcher.Register(this, "VisitMethodBegin");
			dispatcher.Register(this, "VisitCall");
			dispatcher.Register(this, "VisitLoadField");
			dispatcher.Register(this, "VisitLoadFieldAddress");
			dispatcher.Register(this, "VisitMethodEnd");
			dispatcher.Register(this, "VisitEnd");
		}
		
		public void VisitBegin(BeginMethods begin)
		{
			Log.DebugLine(this, "-----------------------------------"); 
			Log.DebugLine(this, "{0}", begin.Type);				
			
			m_equalities.Clear();
			m_properties.Clear();
		}

		public void VisitMethodBegin(BeginMethod begin)
		{
			Log.DebugLine(this, "{0:F}", begin.Info.Instructions);		
			
			m_equality = null;
			m_minfo = begin.Info;

			MethodDefinition method = begin.Info.Method;
			if (method.IsPublic )
			{
				if (method.Name .StartsWith("get_") && !method.IsStatic)
					DoTrivialGetter();
				else if (method.Name .StartsWith("set_") && !method.IsStatic)
					DoTrivialSetter();
				else if (method.Name == "Equals" || method.Name == "op_Equality")
					// TODO: need to check for instance method calls as well
					m_equality = new EqualityMethod(method);
			}
		}
		
		public void VisitCall(Call call)		
		{
			if (m_equality != null)
			{
				if (call.Target.Name.StartsWith("get_"))
				{
					if (m_minfo.Instructions.LoadsThisArg(call.Index - 1))
					{
						Log.DebugLine(this, "checks {0}", call.Target);		
						m_equality.AddProperty(call.Target);
					}
				}
				else if (call.Target.DeclaringType == m_minfo.Type)
				{
					Log.DebugLine(this, "makes an instance call to {0}", call.Target);		
					m_equality = null;
				}
			}
		}
		
		public void VisitLoadField(LoadField load)
		{
			if (m_equality != null)
				DoFieldRef(load.Field, load.Index);
		}
		
		public void VisitLoadFieldAddress(LoadFieldAddress load)
		{
			if (m_equality != null)
				DoFieldRef(load.Field, load.Index);
		}
				
		public void VisitMethodEnd(EndMethod end)
		{
			Unused.Value = end;
			
			if (m_equality != null)
				m_equalities.Add(m_equality);
		}
		
		public void VisitEnd(EndMethods end)
		{
			string details = string.Empty;
			
			foreach (Property prop in m_properties)
			{
				if (prop.HasTrivialGetter && prop.HasTrivialSetter)
				{
					foreach (EqualityMethod method in m_equalities)
					{
						if (method.ChecksSomething)
							if (!method.ChecksField(prop.Field) && !method.ChecksProp(prop.Name))
								details += string.Format("{0} does not check {1}", method.Name, prop.Name) + Environment.NewLine;
					}
				}
			}
	
			details = details.Trim();
			if (details.Length > 0)
			{
				Log.DebugLine(this, "Details: {0}", details);
				Reporter.TypeFailed(end.Type, CheckID, details);
			}
		}
		
		private void DoFieldRef(FieldReference field, int index)
		{
			if (m_minfo.Instructions.LoadsThisArg(index - 1))
			{
				Log.DebugLine(this, "checks {0}", field.Name);		
				m_equality.AddField(field);
			}
		}
				
        // ldarg.0 
        // ldfld string Smokey.Tests.EqualsMissesStateTest/Good1::m_name
        // ret 
		private void DoTrivialGetter()
		{
			do
			{
				if (m_minfo.Instructions.Length != 3)
					break;

				LoadArg larg = m_minfo.Instructions[0] as LoadArg;
				if (larg == null || larg.Arg != 0)
					break;

				LoadField load = m_minfo.Instructions[1] as LoadField;
				if (load == null)
					break;
					
				if (m_minfo.Type.Fields.GetField(load.Field.Name) == null)	// only count fields defined int this class
					break;

				if (m_minfo.Instructions[2].Untyped.OpCode.Code != Code.Ret)
					break;
				
				Log.DebugLine(this, "found trivial getter for {0}", load.Field.Name);				
				string name = m_minfo.Method.Name.Substring(4);
				Property prop = m_properties.SingleOrDefault(p => p.Name == name && p.Field == load.Field.Name);
				if (prop == null)
				{
					prop = new Property(name, load.Field.Name);
					m_properties.Add(prop);
				}
				prop.HasTrivialGetter = true;
			}
			while (false);
		}
		
        // ldarg.0 
        // ldarg.1 
        // stfld string Smokey.Tests.EqualsMissesStateTest/Good1::m_name
        // ret 
		private void DoTrivialSetter()
		{
			do
			{
				if (m_minfo.Instructions.Length != 4)
					break;

				LoadArg larg = m_minfo.Instructions[0] as LoadArg;
				if (larg == null || larg.Arg != 0)
					break;

				larg = m_minfo.Instructions[1] as LoadArg;
				if (larg == null || larg.Arg != 1)
					break;

				StoreField store = m_minfo.Instructions[2] as StoreField;
				if (store == null)
					break;

				if (m_minfo.Type.Fields.GetField(store.Field.Name) == null)	// only count fields defined int this class
					break;

				if (m_minfo.Instructions[3].Untyped.OpCode.Code != Code.Ret)
					break;
				
				Log.DebugLine(this, "found trivial setter for {0}", store.Field.Name);				
				string name = m_minfo.Method.Name.Substring(4);
				Property prop = m_properties.SingleOrDefault(p => p.Name == name && p.Field == store.Field.Name);
				if (prop == null)
				{
					prop = new Property(name, store.Field.Name);
					m_properties.Add(prop);
				}
				prop.HasTrivialSetter = true;
			}
			while (false);
		}
		
		#region Private Types -------------------------------------------------
		private sealed class EqualityMethod
		{
			public EqualityMethod(MethodDefinition method)
			{
				Name = method.ToString();
			}
			
			public string Name {get; private set;}	
			
			public void AddField(FieldReference field)
			{
				if (m_fieldsChecked.IndexOf(field.Name) < 0)
					m_fieldsChecked.Add(field.Name);
			}
			
			public void AddProperty(MethodReference method)
			{
				string name = method.Name.Substring(4);
				
				if (m_propsChecked.IndexOf(name) < 0)
					m_propsChecked.Add(name);
			}
			
			public bool ChecksSomething
			{
				get {return m_fieldsChecked.Count > 0 || m_propsChecked.Count > 0;}
			}
		
			public bool ChecksField(string field)
			{
				return m_fieldsChecked.IndexOf(field) >= 0;
			}

			public bool ChecksProp(string prop)
			{
				return m_propsChecked.IndexOf(prop) >= 0;
			}

			private List<string> m_fieldsChecked = new List<string>();
			private List<string> m_propsChecked = new List<string>();
		}

		private sealed class Property
		{
			public Property(string name, string field)
			{
				Name = name;
				Field = field;
			}
			
			public string Name {get; private set;}				// name minus the "get_" or "set_"
			public bool HasTrivialGetter {get; set;}
			public bool HasTrivialSetter {get; set;}
			public string Field {get; private set;}
		}
		#endregion
		
		#region Fields --------------------------------------------------------
		private List<EqualityMethod> m_equalities = new List<EqualityMethod>();
		private List<Property> m_properties = new List<Property>();

		private EqualityMethod m_equality;
		private MethodInfo m_minfo;
		#endregion
	}
}
#endif