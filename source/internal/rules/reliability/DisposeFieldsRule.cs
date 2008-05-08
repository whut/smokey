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
using System.Text;
using Smokey.Framework;
using Smokey.Framework.Instructions;
using Smokey.Framework.Support;

namespace Smokey.Internal.Rules
{	
	internal class DisposeFieldsRule : Rule
	{				
		public DisposeFieldsRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "R1002")
		{
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitBegin");
			dispatcher.Register(this, "VisitField");
			dispatcher.Register(this, "VisitMethod");
			dispatcher.Register(this, "VisitEnd");
		}
				
		public void VisitBegin(BeginType type)
		{
			Log.DebugLine(this, "-----------------------------------"); 
			Log.DebugLine(this, "{0}", type.Type);				

			m_type = type.Type;
			m_disposable = IsDisposable.Type(Cache, type.Type);		// if it's not IDisposable DisposableFieldsRule will catch it
			m_disposableFields.Clear();
		}
		
		public void VisitField(FieldDefinition field)
		{			
			if (m_disposable && !field.IsStatic)
			{
				if (IsDisposable.Type(Cache, field.FieldType) && OwnsFields.One(m_type, field))
				{
					Log.DebugLine(this, "{0} is disposable", field.Name);
					m_disposableFields.Add(field.ToString());
				}
			}
		}

		public void VisitMethod(MethodDefinition method)
		{			
			if (method.Body != null && m_disposableFields.Count > 0)
			{
				InstructionCollection instructions = method.Body.Instructions;
				
				for (int i = 1; i < instructions.Count; ++i)
				{
					// ldfld    class System.IO.TextWriter Smokey.DisposableFieldsTest/GoodCase::m_writer
					// callvirt instance void class [mscorlib]System.IO.TextWriter::Dispose()
					if (instructions[i].OpCode.Code == Code.Call || instructions[i].OpCode.Code == Code.Callvirt)
					{
						MethodReference target = (MethodReference) instructions[i].Operand;							
						if (target.ToString().EndsWith("::Dispose()") || target.ToString().EndsWith("::Close()"))
						{
							if (instructions[i - 1].OpCode.Code == Code.Ldfld)
							{
								FieldReference field = (FieldReference) instructions[i - 1].Operand;
								Log.DebugLine(this, "found Dispose call for {0}", field.Name);
								Ignore.Value = m_disposableFields.Remove(field.ToString());
							}
						}
					}
				}
			}
		}

		public void VisitEnd(EndType type)
		{
			if (m_disposable && m_disposableFields.Count > 0)
			{
				StringBuilder builder = new StringBuilder();
				builder.Append("Fields: ");
				foreach (string field in m_disposableFields)
				{
					int i = field.LastIndexOf(':');
					if (i >= 0)
						builder.Append(field.Substring(i + 1));
					else
						builder.Append(field);
					builder.Append(' ');
				}
					
				Reporter.TypeFailed(type.Type, CheckID, builder.ToString());
			}
		}
		
		private TypeDefinition m_type;
		private bool m_disposable;
		private List<string> m_disposableFields = new List<string>();
	}
}

