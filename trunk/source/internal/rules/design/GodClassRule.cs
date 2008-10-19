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

#if OLD
namespace Smokey.Internal.Rules
{	
	internal sealed class GodClassRule : Rule
	{				
		public GodClassRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "D1045")
		{
			m_maxTypes = 60;
		}

#if TEST	
		public GodClassRule(AssemblyCache cache, IReportViolations reporter, int maxTypes) 
			: base(cache, reporter, "D1045")
		{
			m_maxTypes = maxTypes;
		}
#endif

		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitBegin");
			dispatcher.Register(this, "VisitMethod");
			dispatcher.Register(this, "VisitCast");
			dispatcher.Register(this, "VisitCall");
			dispatcher.Register(this, "VisitInit");
			dispatcher.Register(this, "VisitLoadField");
			dispatcher.Register(this, "VisitLoadStaticField");
			dispatcher.Register(this, "VisitNew");
			dispatcher.Register(this, "VisitStoreField");
			dispatcher.Register(this, "VisitStoreStaticField");
			dispatcher.Register(this, "VisitEnd");
		}
		
		public void VisitBegin(BeginMethods begin)
		{
			Log.DebugLine(this, "-----------------------------------"); 
			Log.DebugLine(this, "{0}", begin.Type.FullName);

			m_type = begin.Type;
			m_types.Clear();
		}
				
		public void VisitMethod(BeginMethod begin)
		{	
//			Log.DebugLine(this, "{0:F}", begin.Info.Instructions);
		}
		
		public void VisitCast(CastClass cast)
		{	
			DoAdd(cast.ToType);
		}
		
		public void VisitCall(Call call)
		{	
			DoAdd(call.Target.DeclaringType);
		}
		
		public void VisitInit(InitObj init)
		{	
			DoAdd(init.Type);
		}
		
		public void VisitLoadField(LoadField field)
		{	
			DoAdd(field.Field.FieldType);
		}
		
		public void VisitLoadStaticField(LoadStaticField field)
		{	
			DoAdd(field.Field.FieldType);
		}
		
		public void VisitNew(NewObj obj)
		{	
			DoAdd(obj.Ctor.DeclaringType);
		}
		
		public void VisitStoreField(StoreField field)
		{	
			DoAdd(field.Field.FieldType);
		}
		
		public void VisitStoreStaticField(StoreStaticField field)
		{	
			DoAdd(field.Field.FieldType);
		}
		
		public void VisitEnd(EndMethods end)
		{
			Log.DebugLine(this, "   {0} types", m_types.Count);
			if (m_types.Count >= m_maxTypes)
			{
				Log.TraceLine(this, "{0} references {1} types:", m_type.FullName, m_types.Count);
				foreach (string type in m_types)
					Log.TraceLine(this, "   {0}", type);
				
				string details = string.Format("references {0} types", m_types.Count);
				Reporter.TypeFailed(end.Type, CheckID, details);
			}
		}
		
		private void DoAdd(TypeReference type)
		{
			if (type != m_type)
			{
				GenericInstanceType generic = type as GenericInstanceType;
				if (generic != null)
				{
					DoAdd(generic.GetOriginalType());
					for (int i = 0; i < generic.GenericArguments.Count; ++i)
						DoAdd(generic.GenericArguments[i]);
				}
				else
				{
					string name = type.FullName;
				
					if (!type.IsCompilerGenerated() && m_types.IndexOf(name) < 0)
					{
						Log.DebugLine(this, "   {0}", name);
						m_types.Add(name);
					}
				}
			}
		}
		
		private TypeReference m_type;
		private List<string> m_types = new List<string>();
		private int m_maxTypes;
	}
}
#endif
