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
	internal sealed class DisposeableRule : Rule
	{				
		public DisposeableRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "D1062")
		{
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitBegin");
			dispatcher.Register(this, "VisitMethodBegin");
			dispatcher.Register(this, "VisitCall");	
			dispatcher.Register(this, "VisitNewObj");	
			dispatcher.Register(this, "VisitMethodEnd");
			dispatcher.Register(this, "VisitEnd");
		}
		
		public void VisitBegin(BeginMethods begin)
		{
			m_type = begin.Type;
			m_disposable = IsDisposable.Type(Cache, begin.Type) && !m_type.IsCompilerGenerated();
			m_details = string.Empty;
		
			m_hasFinalizer = false;
			m_supressWasCalled = false;
			m_checkForSupress = false;

			m_disposableFields.Clear();

			m_noThrowMethods = string.Empty;
			
			if (m_disposable)
			{
				Log.DebugLine(this, "-----------------------------------"); 
				Log.DebugLine(this, "{0}", m_type);				
			}
			
			// We can't visit fields using the dispatcher because we're kicking 
			// the visit off with BeginMethods instead of BeginType so we have 
			// to manually visit them.
			foreach (FieldDefinition field in m_type.Fields)
				VisitField(field);
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

		public void VisitMethodBegin(BeginMethod begin)
		{			
			if (m_disposable)
			{
				m_minfo = begin.Info;

				if (begin.Info.Method.Name == "Finalize")
				{
					Log.DebugLine(this, "has finalizer");
					m_hasFinalizer = true;
				}
				
				// The IDisposable pattern says that SuppressFinalize should only be called from Dispose().
				m_checkForSupress = begin.Info.Method.Name == "Dispose" && begin.Info.Method.Parameters.Count == 0;
				
				m_needsThrowCheck = false;
				m_doesThrow = false;
				if (!begin.Info.Method.IsStatic && !begin.Info.Method.IsConstructor && begin.Info.Method.HasBody)
				{
					MethodAttributes attrs = begin.Info.Method.Attributes;
					if ((attrs & MethodAttributes.MemberAccessMask) == MethodAttributes.Public)
					{
						if (begin.Info.Method.Name != "Dispose" && begin.Info.Method.Name != "Close")
						{
							m_needsThrowCheck = true;
						}
					}
				}
			}		
		}

		public void VisitCall(Call call)
		{
			if (m_checkForSupress)
			{
				if (call.Target.ToString().Contains("System.GC::SuppressFinalize(System.Object)"))
				{
					Log.DebugLine(this, "has suppress call");
					m_supressWasCalled = true;
				}
			}

			if (m_disposableFields.Count > 0)
			{
				// ldfld    class System.IO.TextWriter Smokey.DisposableFieldsTest/GoodCase::m_writer
				// callvirt instance void class [mscorlib]System.IO.TextWriter::Dispose()
				if (call.Target.Name == "Dispose" || call.Target.Name == "Close")
				{
					if (m_minfo.Instructions[call.Index - 1].Untyped.OpCode.Code == Code.Ldfld)
					{
						FieldReference field = (FieldReference) m_minfo.Instructions[call.Index - 1].Untyped.Operand;
						Log.DebugLine(this, "found Dispose call for {0}", field.Name);
						Ignore.Value = m_disposableFields.Remove(field.ToString());
					}
				}
			}
		}

		// newobj   System.Void System.ObjectDisposedException::.ctor(System.String)
		public void VisitNewObj(NewObj call)
		{
			if (call.Ctor.ToString().Contains("ObjectDisposedException"))
				m_doesThrow = true;
		}

		public void VisitMethodEnd(EndMethod end)
		{			
			if (m_needsThrowCheck && !m_doesThrow)
			{
				m_noThrowMethods += end.Info.Method + Environment.NewLine;
			}		
		}

		public void VisitEnd(EndMethods end)
		{
			if (m_noThrowMethods.Length > 0)
			{
				m_details += "Methods which don't throw ObjectDisposedException: " + Environment.NewLine;
				m_details += m_noThrowMethods;
			}

			if (m_disposableFields.Count > 0)
			{
				StringBuilder builder = new StringBuilder();
				builder.Append("Fields which are not disposed: ");
				foreach (string field in m_disposableFields)
				{
					int i = field.LastIndexOf(':');
					if (i >= 0)
						builder.Append(field.Substring(i + 1));
					else
						builder.Append(field);
					builder.Append(' ');
				}
				
				m_details += builder.ToString() + Environment.NewLine;
			}
			
			if (m_hasFinalizer && !m_supressWasCalled)
				m_details += "GC.SuppressFinalize was not called" + Environment.NewLine;

			m_details = m_details.Trim();
			if (m_details.Length > 0)
			{
				Log.DebugLine(this, "Details: {0}", m_details);
				Reporter.TypeFailed(end.Type, CheckID, m_details);
			}
		}
						
		private TypeDefinition m_type;
		private bool m_disposable;
		private string m_details;
		private MethodInfo m_minfo;

		private bool m_hasFinalizer;
		private bool m_supressWasCalled;
		private bool m_checkForSupress;

		private List<string> m_disposableFields = new List<string>();

		private bool m_needsThrowCheck;
		private bool m_doesThrow;
		private string m_noThrowMethods;
	}
}
