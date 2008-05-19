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
using Smokey.Framework;
using Smokey.Framework.Instructions;
using Smokey.Framework.Support;

namespace Smokey.Internal.Rules
{	
	internal sealed class DisposeNativeResourcesRule : Rule
	{				
		public DisposeNativeResourcesRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "R1001")
		{
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitBegin");
			dispatcher.Register(this, "VisitMethod");
			dispatcher.Register(this, "VisitEnd");
		}
				
		public void VisitBegin(BeginType type)
		{
			Log.DebugLine(this, "-----------------------------------"); 
			Log.DebugLine(this, "{0}", type.Type);				

			m_type = type.Type;
			m_hasFinalizer = DoHasFinalizer(type.Type);
			m_newsNative = false;
			m_details = string.Empty;
			Log.DebugLine(this, "hasFinalizer: {0}", m_hasFinalizer);
		}
		
		public void VisitMethod(MethodDefinition method)
		{			
			if (method.Body != null && method.IsConstructor && !m_hasFinalizer)
			{
				InstructionCollection instructions = method.Body.Instructions;
//				Log.DebugLine(this, "{0:F}", new TypedInstructionCollection(new SymbolTable(), method));				
				
				for (int i = 1; i < instructions.Count && !m_newsNative; ++i)
				{
					Instruction instruction = instructions[i];
					
					// call  native int class Smokey.DisposeNativeResourcesTest/GoodCase2::CreateHandle()
					// stfld native int Smokey.DisposeNativeResourcesTest/GoodCase2::m_resource
					if (instruction.OpCode.Code == Code.Stfld)
					{
						FieldReference fieldRef = (FieldReference) instruction.Operand;
						FieldDefinition field = m_type.Fields.GetField(fieldRef.Name);
						if (field != null && field.MetadataToken == fieldRef.MetadataToken)
						{
							if (!field.IsStatic && (field.FieldType.FullName == "System.IntPtr" || field.FieldType.FullName == "System.Runtime.InteropServices.HandleRef"))
							{
								if (instructions[i - 1].OpCode.Code == Code.Call || instructions[i - 1].OpCode.Code == Code.Callvirt)
								{
									MethodReference target = (MethodReference) instructions[i - 1].Operand;
									if (!target.Name.StartsWith("get_") && target.Name != "op_Explicit")	// ignore property and (IntPtr) 0
									{
										m_details = string.Format("Field: {0}", field.Name);
										Log.DebugLine(this, "found {0}", m_details);
										m_newsNative = true; 
									}
								}
							}
						}
					}
					
					// ldarg.0  this
					// ldflda   System.Runtime.InteropServices.HandleRef Smokey.Tests.DisposeNativeResourcesTest/BadCase2::m_resource
					else if (instruction.OpCode.Code == Code.Ldflda)
					{
						if (instructions[i - 1].OpCode.Code == Code.Ldarg_0)
						{
							FieldReference fieldRef = (FieldReference) instruction.Operand;
							FieldDefinition field = m_type.Fields.GetField(fieldRef.Name);
							if (field != null && field.MetadataToken == fieldRef.MetadataToken)
							{
								if (!field.IsStatic && (field.FieldType.FullName == "System.IntPtr" || field.FieldType.FullName == "System.Runtime.InteropServices.HandleRef"))
								{
									m_details = string.Format("Field: {0}", field.Name);
									Log.DebugLine(this, "found {0}", m_details);
									m_newsNative = true; 
								}
							}
						}
					}
				}
			}
		}

		public void VisitEnd(EndType type)
		{
			if (!m_hasFinalizer && m_newsNative)
				Reporter.TypeFailed(type.Type, CheckID, m_details);
		}
		
		private static bool DoHasFinalizer(TypeDefinition type)
		{
			bool has = type.Methods.GetMethod("Finalize", Type.EmptyTypes) != null;
			
			if (!has)
				has = DoHasOverridenDispose(type, "Dispose") || DoHasOverridenDispose(type, "DoDispose") || DoHasOverridenDispose(type, "OnDispose");
		
			return has;
		}
		
		private static bool DoHasOverridenDispose(TypeDefinition type, string name)
		{
			MethodDefinition method = type.Methods.GetMethod(name, new Type[]{typeof(bool)});
			if (method != null)
			{
				MethodAttributes vtable = method.Attributes & MethodAttributes.VtableLayoutMask;
				if ((vtable & MethodAttributes.ReuseSlot) == MethodAttributes.ReuseSlot)
					return true;
			}
		
			return false;
		}
		
		private TypeDefinition m_type;
		private bool m_hasFinalizer;
		private bool m_newsNative;
		private string m_details;
	}
}

