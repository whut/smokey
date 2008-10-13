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
			dispatcher.Register(this, "VisitConditional");	
			dispatcher.Register(this, "VisitCall");	
			dispatcher.Register(this, "VisitNewObj");	
			dispatcher.Register(this, "VisitThrow");	
			dispatcher.Register(this, "VisitMethodEnd");
			dispatcher.Register(this, "VisitEnd");
		}
		
		public void VisitBegin(BeginMethods begin)
		{
			m_type = begin.Type;
			m_disposable = begin.Type.TypeOrBaseImplements("System.IDisposable", Cache) && !m_type.IsCompilerGenerated();
		
			m_hasFinalizer = false;
			m_supressWasCalled = false;
			m_hasVirtualDispose = false;
			m_hasPublicDispose = false;
			m_hasNonProtectedDispose = false;
			m_hasNonVirtualDispose = false;
			m_disposeThrows = false;
			m_hasNativeFields = false;
			
			m_badDisposeNames.Clear();
			m_illegalDisposeNames.Clear();

			m_disposableFields.Clear();

			m_noThrowMethods = string.Empty;
			
			m_hasNullCall = false;
			m_ownedFields.Clear();
			
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
				if (field.IsOwnedBy(m_type))
				{
					m_ownedFields.Add(field);
					
					TypeDefinition type = Cache.FindType(field.FieldType);
					if (type != null && type.TypeOrBaseImplements("System.IDisposable", Cache))
					{
						Log.DebugLine(this, "{0} is disposable", field.Name);
						m_disposableFields.Add(field.ToString());
					}
				}
				
				if (!m_hasNativeFields && field.FieldType.IsNative())
				{
					m_hasNativeFields = true;
				}
			}
		}

		public void VisitMethodBegin(BeginMethod begin)
		{			
			if (m_disposable)
			{
				m_minfo = begin.Info;

				MethodDefinition method = begin.Info.Method;
				if (method.Name == "Finalize")
				{
					Log.DebugLine(this, "has finalizer");	
					m_hasFinalizer = true;
				}
				
				if (method.Name == "Dispose")	
				{
					if (method.ReturnType.ReturnType.FullName != "System.Void")
						m_badDisposeNames.Add(method.ToString());
						
					else if (method.Parameters.Count > 1)
						m_badDisposeNames.Add(method.ToString());
						
					else if (method.Parameters.Count == 1 && method.Parameters[0].ParameterType.FullName != "System.Boolean")
						m_badDisposeNames.Add(method.ToString());
				}
				
				if (method.Name == "OnDispose" || method.Name == "DoDispose")
					if (!method.IsPrivate)
						m_illegalDisposeNames.Add(method.ToString());

				// The IDisposable pattern says that SuppressFinalize should only be called from Dispose().
				m_isNullaryDispose = method.Matches("System.Void", "Dispose");
		
				if (m_isNullaryDispose && method.IsVirtual && !method.IsFinal)
					m_hasVirtualDispose = true;
				
				m_isUnaryDispose = method.Matches("System.Void", "Dispose", "System.Boolean");
				if (m_isUnaryDispose && method.IsPublic)
					m_hasPublicDispose = true;
					
				if (m_isUnaryDispose && !m_type.IsSealed && !method.IsFamily && !method.IsFamilyOrAssembly && !method.IsFamilyAndAssembly)
					m_hasNonProtectedDispose = true;
					
				if (m_isUnaryDispose && !m_type.IsSealed && (!method.IsVirtual || method.IsFinal))
					m_hasNonVirtualDispose = true;
					
				m_checkForThrows = (m_isNullaryDispose || m_isUnaryDispose) && begin.Info.Instructions.TryCatchCollection.Length == 0;

				m_needsThrowCheck = false;
				m_doesThrow = false;
				if (!method.IsStatic && !method.IsConstructor && method.HasBody)
				{
					MethodAttributes attrs = method.Attributes;
					if ((attrs & MethodAttributes.MemberAccessMask) == MethodAttributes.Public)
					{
						if (method.Name != "Dispose" && method.Name != "Close" && !method.Name.StartsWith("get_"))
						{
							m_needsThrowCheck = true;
						}
					}
				}
				
				m_hasNoBranches = true;
				m_callsNullableField = false;
			}		
		}
		
        // ldfld bool Smokey.Tests.DisposeableTest/NoNullCheck2::m_disposed
        // brtrue IL_001d
		public void VisitConditional(ConditionalBranch branch)
		{
			if (m_disposable)
			{
				if (m_isNullaryDispose || m_isUnaryDispose)
				{
					if (branch.Index > 0)
					{
						LoadField load = m_minfo.Instructions[branch.Index - 1] as LoadField;
						if (load == null || load.Field.Name.IndexOf("m_disposed") < 0)		// testing m_disposed does not count as a branch for null checking
							m_hasNoBranches = false;
					}
				}
			}
		}

		public void VisitCall(Call call)
		{
			if (m_disposable)
			{
				if (m_isNullaryDispose)
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
							Unused.Value = m_disposableFields.Remove(field.ToString());
						}
					}
				}
	
				// ldarg.0 
				// ldfld class [mscorlib]System.IO.StringWriter Smokey.Tests.DisposeableTest/NoNullCheck1::m_writer
				// callvirt instance void class [mscorlib]System.IO.TextWriter::Dispose()
				if ((m_isNullaryDispose || m_isUnaryDispose) && call.Index >= 2 && !m_callsNullableField && !m_hasNullCall)
				{
					LoadArg load1 = m_minfo.Instructions[call.Index - 2] as LoadArg;
					LoadField load2 = m_minfo.Instructions[call.Index - 1] as LoadField;
					if (load1 != null && load2 != null)
					{
						if (load1.Arg == 0 && !load2.Field.FieldType.IsValueType)		
						{
							if (m_ownedFields.IndexOf(load2.Field) >= 0)
								m_callsNullableField = true;
						}
					}
				}
			}
		}

		public void VisitThrow(Throw t)
		{
			Unused.Value = t;

			if (m_disposable)
			{
				if (m_checkForThrows)
				{
					m_disposeThrows = true;
				}
			}
		}

		// newobj   System.Void System.ObjectDisposedException::.ctor(System.String)
		public void VisitNewObj(NewObj call)
		{
			if (m_disposable)
			{
				if (call.Ctor.ToString().Contains("ObjectDisposedException"))
					m_doesThrow = true;
			}
		}

		public void VisitMethodEnd(EndMethod end)
		{			
			if (m_disposable)
			{
				if (m_needsThrowCheck && !m_doesThrow)
				{
					m_noThrowMethods += end.Info.Method + Environment.NewLine;
				}	
			
				// This is a tricky case because people may use arbitrary state to figure
				// out if a field is null or not. So, we'll be conservative and say there
				// is a problem if a nullable field is called and either there are no
				// branches or the only branch uses m_disposed.
				if (m_hasNoBranches && m_callsNullableField)
				{
					m_hasNullCall = true;
				}
			}
		}

		public void VisitEnd(EndMethods end)
		{
			if (m_disposable)
			{
				string details = string.Empty;
				
				if (m_noThrowMethods.Length > 0)
				{
					details += "Methods which don't throw ObjectDisposedException: " + Environment.NewLine;
					details += m_noThrowMethods;
				}
	
				if (m_badDisposeNames.Count > 0)
				{
					details += "Unusual Dispose methods: ";
					details += string.Join(" ", m_badDisposeNames.ToArray());
					details += Environment.NewLine;
				}
				
				if (m_illegalDisposeNames.Count > 0)
				{
					details += "Bad method names: ";
					details += string.Join(" ", m_illegalDisposeNames.ToArray());
					details += Environment.NewLine;	
				}
				
				if (m_hasVirtualDispose)
					details += "Dispose() is virtual" + Environment.NewLine;
				
				if (m_hasPublicDispose)
					details += "Dispose(bool) is public." + Environment.NewLine;
					
				if (m_hasNonProtectedDispose)	
					details += "The type is unsealed, but Dispose(bool) is not protected." + Environment.NewLine;
					
				if (m_hasNonVirtualDispose)
					details += "The type is unsealed, but Dispose(bool) is not virtual." + Environment.NewLine;
				
				if (m_disposeThrows)
					details += "A Dispose methods throws, but does not catch." + Environment.NewLine;
				
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
					
					details += builder.ToString() + Environment.NewLine;
				}
				
				if (m_hasFinalizer && !m_supressWasCalled)
					details += "GC.SuppressFinalize was not called" + Environment.NewLine;
					
				if (m_hasNativeFields && m_type.IsValueType)
					details += "The type is a value type, but has native fields." + Environment.NewLine;
					
				if (m_hasNullCall)
					details += "Dispose calls a method on a field which may be null (because the constructor may have thrown)." + Environment.NewLine;
	
				details = details.Trim();
				if (details.Length > 0)
				{
					Log.DebugLine(this, "Details: {0}", details);
					Reporter.TypeFailed(end.Type, CheckID, details);
				}
			}
		}
						
		private TypeDefinition m_type;
		private bool m_disposable;
		private MethodInfo m_minfo;

		private bool m_hasFinalizer;
		private bool m_supressWasCalled;
		private bool m_isNullaryDispose;
		private bool m_isUnaryDispose;
		private bool m_hasNativeFields;
		
		private List<string> m_badDisposeNames = new List<string>();
		private List<string> m_illegalDisposeNames = new List<string>();
		
		private bool m_hasVirtualDispose;
		private bool m_hasPublicDispose;
		private bool m_hasNonProtectedDispose;
		private bool m_hasNonVirtualDispose;
		private bool m_disposeThrows;
		private bool m_checkForThrows;

		private List<string> m_disposableFields = new List<string>();

		private bool m_needsThrowCheck;
		private bool m_doesThrow;
		private string m_noThrowMethods;
		
		private bool m_hasNullCall;
		private bool m_hasNoBranches;
		private bool m_callsNullableField;
		private List<FieldReference> m_ownedFields = new List<FieldReference>();
	}
}
