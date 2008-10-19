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
using Smokey.Framework;
using Smokey.Framework.Instructions;
using Smokey.Framework.Support;
using Smokey.Internal;
using System;
using System.Collections.Generic;
using System.Diagnostics;

#if OLD
namespace Smokey.Internal.Rules
{	
	internal sealed class NullFieldRule : Rule
	{				
		public NullFieldRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "C1017")
		{
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitBegin");
			dispatcher.Register(this, "VisitStore1");
			dispatcher.Register(this, "VisitStore2");
			dispatcher.Register(this, "VisitLoad1");
			dispatcher.Register(this, "VisitLoad2");
			dispatcher.Register(this, "VisitEnd");
		}
				
		public void VisitBegin(BeginMethods begin)
		{
			Log.DebugLine(this, "-----------------------------------"); 
			Log.DebugLine(this, "{0:F}", begin.Type);				

			m_fields.Clear();

			if (begin.Type.Constructors.Count > 0)	// some of the types in the 1.9 system dll's have no ctors...
			{
				foreach (FieldDefinition field in begin.Type.Fields)
					if (field.IsPrivate && !field.IsLiteral)
						if (!field.FieldType.IsValueType && !field.IsCompilerControlled)
							m_fields.Add(field.Name);
			}
		}
		
		public void VisitStore1(StoreField store)
		{
			if (store.Untyped.Previous.OpCode.Code != Code.Ldnull)
			{
				Unused.Value = m_fields.Remove(store.Field.Name);
			}
		}

		public void VisitStore2(StoreStaticField store)
		{
			if (store.Untyped.Previous.OpCode.Code != Code.Ldnull)
			{
				Unused.Value = m_fields.Remove(store.Field.Name);
			}
		}

		public void VisitLoad1(LoadFieldAddress load)
		{
			Unused.Value = m_fields.Remove(load.Field.Name);
		}

		public void VisitLoad2(LoadStaticFieldAddress load)
		{
			Unused.Value = m_fields.Remove(load.Field.Name);
		}

		public void VisitEnd(EndMethods end)
		{
			if (m_fields.Count > 0)
			{
				string details = "Fields: " + string.Join(" ", m_fields.ToArray());
				Log.DebugLine(this, "{0}", details);				
				
				Reporter.TypeFailed(end.Type, CheckID, details);
			}
		}
				
		private List<string> m_fields = new List<string>();
	}
}
#endif