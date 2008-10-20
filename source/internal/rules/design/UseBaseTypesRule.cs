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

namespace Smokey.Internal.Rules
{	
	internal sealed class UseBaseTypesRule : Rule
	{				
		public UseBaseTypesRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "D1007")
		{
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitBegin");
			dispatcher.Register(this, "VisitCall");
			dispatcher.Register(this, "VisitLoadField");
			dispatcher.Register(this, "VisitStoreField");
			dispatcher.Register(this, "VisitStoreStaticField");
			dispatcher.Register(this, "VisitStoreLocal");
			dispatcher.Register(this, "VisitEnd");
		}
				
		public void VisitBegin(BeginMethod begin)
		{
			m_types.Clear();
			m_formalTypes.Clear();
			m_info = begin.Info;
			m_needsCheck = false;
			
			if (begin.Info.Method.ExternallyVisible(Cache))
			{
				if (begin.Info.Method.SemanticsAttributes == 0)
				{
					bool isExtensionMethod = begin.Info.Method.CustomAttributes.Has("ExtensionAttribute");					
					if (!isExtensionMethod)
					{
						Log.DebugLine(this, "-----------------------------------"); 
						Log.DebugLine(this, "{0:F}", begin.Info.Instructions);	
						
						m_formalTypes.Add(null);
						m_types.Add(null);
						
						for (int i = 0; i < m_info.Method.Parameters.Count; ++i)
						{
							TypeReference type = begin.Info.Method.Parameters[i].ParameterType;
							if (!type.IsValueType && !type.FullName.Contains("<"))	// TODO: code doesn't handle generics all that well (eg List<T>)
							{
								m_formalTypes.Add(type);
								m_types.Add(new List<TypeReference>());
								m_needsCheck = true;
							}
							else
							{
								m_formalTypes.Add(null);
								m_types.Add(null);
							}
						}
					}
				}
			}
		}
		
		public void VisitCall(Call call)
		{
			if (m_needsCheck)
			{
				Log.Indent();

				// See if an arg is being used as a this pointer,
				if (call.Target.HasThis)
				{
					int index = call.GetThisIndex(m_info);
					if (index >= 0)
					{					
						LoadArg load = m_info.Instructions[index] as LoadArg;
						if (load != null && m_types[load.Arg] != null)
						{
							TypeReference tr = call.Target.GetDeclaredIn(Cache);
							TypeDefinition type = Cache.FindType(tr);
							if (type != null && type.ExternallyVisible(Cache))
							{
								Log.DebugLine(this, "found type {0}", type.FullName);
							
								if (!type.Name.StartsWith("_"))		// can get weird stuff like System.Runtime.InteropServices._Type otherwise
								{
									DoUseArg(load.Arg, call.Untyped.Offset, type);
								}
								else if ("_" + load.Type.Name != type.Name)
								{
									DoUseArg(load.Arg, call.Untyped.Offset, type);
								}
							}
						}
					}
				}
				
				// See if an arg is being used as an arg.
				for (int nth = 0; nth < call.Target.Parameters.Count; ++nth)
				{
					int index = m_info.Tracker.GetStackIndex(call.Index, nth);
					if (index >= 0)
					{					
						LoadArg load = m_info.Instructions[index] as LoadArg;
						if (load != null && m_types[load.Arg] != null)
						{
							int argIndex = call.Target.Parameters.Count - nth - 1;
							TypeReference type = call.Target.Parameters[argIndex].ParameterType;
							DoUseArg(load.Arg, call.Untyped.Offset, type);
						}
					}
				}
				
				Log.Unindent();
			}
		}
		
		public void VisitLoadField(LoadField field)
		{
			if (m_needsCheck)
			{
				Log.Indent();
				LoadArg load = m_info.Instructions[field.Index - 1] as LoadArg;
				if (load != null && m_types[load.Arg] != null)
				{
					TypeReference type = field.Field.DeclaringType;
					DoUseArg(load.Arg, field.Untyped.Offset, type);
				}
				Log.Unindent();
			}
		}
		
		public void VisitStoreField(StoreField store)
		{
			if (m_needsCheck)
			{
				Log.Indent();
				LoadArg load = m_info.Instructions[store.Index - 1] as LoadArg;
				if (load != null && m_types[load.Arg] != null)
				{
					TypeReference type = store.Field.FieldType;
					DoUseArg(load.Arg, store.Untyped.Offset, type);
				}
				Log.Unindent();
			}
		}
		
		public void VisitStoreStaticField(StoreStaticField store)
		{
			if (m_needsCheck)
			{
				Log.Indent();
				LoadArg load = m_info.Instructions[store.Index - 1] as LoadArg;
				if (load != null && m_types[load.Arg] != null)
				{
					TypeReference type = store.Field.FieldType;
					DoUseArg(load.Arg, store.Untyped.Offset, type);
				}
				Log.Unindent();
			}
		}
		
		public void VisitStoreLocal(StoreLocal store)
		{
			if (m_needsCheck)
			{
				Log.Indent();
				LoadArg load = m_info.Instructions[store.Index - 1] as LoadArg;
				if (load != null && m_types[load.Arg] != null)
				{
					Log.DebugLine(this, "arg{0}: was saved to a local (at {1:X2})", load.Arg, store.Untyped.Offset); 
					m_types[load.Arg] = null;
				}
				Log.Unindent();
			}
		}
		
		public void VisitEnd(EndMethod end)
		{			
			if (m_needsCheck)
			{
				string details = string.Empty;
			
				for (int i =  m_types.Count - 1; i >= 1; --i)
				{
					if (m_types[i] != null)
					{
						if (m_types[i].Count == 1)
						{
							details = string.Format("Argument '{0}' can be declared as {1}. {2}", m_info.Method.Parameters[i - 1].Name, m_types[i][0].FullName, details);
						}
						else if (m_types[i].Count > 0)
						{
							TypeReference type = DoConsolidateTypes(m_types[i]);
							if (type != null && (type.FullName != m_formalTypes[i].FullName))
								details = string.Format("Argument '{0}' can be declared as {1}. {2}", m_info.Method.Parameters[i - 1].Name, type.FullName, details);
						}
					}
				}
					
				if (details.Length > 0)
				{
					Log.DebugLine(this, details); 
					Reporter.MethodFailed(end.Info.Method, CheckID, 0, details);
				}			
			}
		}
		
		private void DoUseArg(int arg, int offset, TypeReference usedType)
		{
			if (usedType.FullName[0] != '!')	// TODO: there doesn't appear to be a good way to get the type name of a GenericParameter
			{
				if (usedType.FullName != m_formalTypes[arg].FullName)	// note that we can't use the metadata token because of generics...
				{
					Log.DebugLine(this, "arg{0} used type {1} (at {2:X2})", arg, usedType, offset); 
					if (!m_types[arg].Exists(e => e.FullName == usedType.FullName))
					{
						m_types[arg].Add(usedType);
					}
				}
				else
				{
					Log.DebugLine(this, "arg{0}: used the formal type (at {1:X2})", arg, offset); 
					m_types[arg] = null;
				}
			}
		}
		
		private TypeReference DoConsolidateTypes(List<TypeReference> types)
		{
			DBC.Assert(types.Count > 1, "can only consolidate multiple types");

			TypeReference current = types[0];
			return DoConsolidateTypes(types, current, 1);
		}
		
		private TypeReference DoConsolidateTypes(List<TypeReference> types, TypeReference current, int i)
		{
			DBC.Assert(i > 0, "can't have current with zero index");
			DBC.Assert(i < types.Count, "i is out of range");
			
			TypeReference result = current.CommonClass(types[i], Cache);
			
			if (i + 1 < types.Count && result != null)
				result = DoConsolidateTypes(types, result, i + 1);
				
			return result;
		}
		
		private bool m_needsCheck;
		private MethodInfo m_info;
		private List<TypeReference> m_formalTypes = new List<TypeReference>();
		private List<List<TypeReference>> m_types = new List<List<TypeReference>>();	// null => arg is ok
	}
}

