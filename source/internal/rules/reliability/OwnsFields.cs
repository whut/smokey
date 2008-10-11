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
using Smokey.Framework.Support;

#if OLD
namespace Smokey.Internal.Rules
{	
	internal static class OwnsFields
	{				
		// Returns true if the type news any of the specified fields.
//		public static bool Any(TypeDefinition type, List<FieldDefinition> fields)
//		{			
//			List<string> tested = new List<string>();
//
//			for (int i = 0; i < type.Constructors.Count; ++i)
//			{
//				if (DoNewsField(type, type.Constructors[i], fields, tested))
//					return true;
//			}
//			
//			return false;
//		}
				
		// Returns true if the type news the specified field.
		public static bool One(TypeDefinition type, FieldDefinition field)
		{			
			List<string> tested = new List<string>();
			
			List<FieldDefinition> fields = new List<FieldDefinition>();
			fields.Add(field);

			for (int i = 0; i < type.Constructors.Count; ++i)
			{
				if (DoNewsField(type, type.Constructors[i], fields, tested))
					return true;
			}
			
			return false;
		}
				
		#region Private methods
		private static bool DoNewsField(TypeDefinition type, MethodDefinition method, List<FieldDefinition> fields, List<string> tested)
		{
			bool news = false;
			tested.Add(method.Name);
			
			if (method.Body != null)
			{
				for (int i = 1; i < method.Body.Instructions.Count && !news; ++i)
				{
					FieldReference fref = DoGetNewedField(method.Body.Instructions, i);
					if (fref != null)
					{
						news = fields.Exists(delegate (FieldDefinition field)
						{
							return field.Name == fref.Name;
						});
					}
					else
					{
						Instruction instruction = method.Body.Instructions[i];
						if (instruction.OpCode.Code == Code.Call || instruction.OpCode.Code == Code.Callvirt)
						{	
							MethodReference target = (MethodReference) instruction.Operand;
							MethodDefinition md = type.Methods.GetMethod(target.Name, target.Parameters);
							if (md != null && md.MetadataToken == target.MetadataToken && tested.IndexOf(target.Name) < 0) 
								news = DoNewsField(type, md, fields, tested);
						}
					}
				}
			}
			
			return news;
		}
		
		// newobj instance void class [mscorlib]System.IO.StringWriter::.ctor()
        // stfld  class [mscorlib]System.IO.StringWriter Smokey.DisposableFieldsTest/GoodCase::m_writer
		private static FieldReference DoGetNewedField(InstructionCollection instructions, int index)
		{
			FieldReference fref = null;
			
			if (instructions[index].OpCode.Code == Code.Stfld)
			{
				if (instructions[index - 1].OpCode.Code == Code.Newobj || instructions[index - 1].OpCode.Code == Code.Newarr)
					fref = (FieldReference) instructions[index].Operand;
			}
			
			return fref;
		}				
		#endregion
	}
}
#endif