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

namespace Smokey.Internal.Rules
{	
	internal class Const1Rule : Rule
	{				
		public Const1Rule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "P1016")
		{
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitBegin");
			dispatcher.Register(this, "VisitField");
			dispatcher.Register(this, "VisitMethod");
		}
				
		public void VisitBegin(BeginType begin)
		{
			Log.DebugLine(this, "-----------------------------------"); 
			Log.DebugLine(this, "checking {0}", begin.Type);				

			m_type = begin.Type;
			m_candidates.Clear();
		}
		
		public void VisitField(FieldDefinition field)
		{						
			Log.DebugLine(this, "{0}", field.Name);				

			// Must be internal,
			FieldAttributes access = field.Attributes & FieldAttributes.FieldAccessMask;
			if (access != FieldAttributes.Public && access != FieldAttributes.Family && access != FieldAttributes.FamORAssem)
			{
				// static,
				if ((field.Attributes & FieldAttributes.Static) == FieldAttributes.Static)
				{
					// readonly,
					if ((field.Attributes & FieldAttributes.InitOnly) == FieldAttributes.InitOnly)
					{
						// const type and value.
						if (DoConstType(field.FieldType))
						{
							Log.DebugLine(this, "{0} is a candidate", field.Name);
							m_candidates.Add(field.Name);
						}
					}
				}
			}
		}

		public void VisitMethod(MethodDefinition method)
		{
			if (method.Body != null && method.IsConstructor && method.IsStatic)
			{
				InstructionCollection instructions = method.Body.Instructions;
//				Log.DebugLine(this, "{0:F}", new TypedInstructionCollection(new SymbolTable(), method));				
				
				string details = string.Empty;
				for (int i = 1; i < instructions.Count; ++i)
				{
					Instruction instruction = instructions[i];
					
					// ldc.i4.1 
        			// stsfld int32 Alpha::Value
					if (instruction.OpCode.Code == Code.Stsfld)
					{
						Instruction prev = instructions[i - 1];

						// TODO: could do some more analysis here
						switch (prev.OpCode.Code)
						{
							case Code.Ldc_I4_M1:
							case Code.Ldc_I4_0:
							case Code.Ldc_I4_1:
							case Code.Ldc_I4_2:
							case Code.Ldc_I4_3:
							case Code.Ldc_I4_4:
							case Code.Ldc_I4_5:
							case Code.Ldc_I4_6:
							case Code.Ldc_I4_7:
							case Code.Ldc_I4_8:
							case Code.Ldc_I4_S:
							case Code.Ldc_I4:
							case Code.Ldc_I8:
							case Code.Ldc_R4:
							case Code.Ldc_R8:
							case Code.Ldnull:
							case Code.Ldstr:
								Log.DebugLine(this, "found literal store at {0:X2}", instruction.Offset);
	
								FieldReference field = (FieldReference) instruction.Operand;
								if (m_candidates.IndexOf(field.Name) >= 0)
								{
									details = string.Format("{0} {1}", details, field.Name);
								}
								break;
						}
					}
				}

				if (details.Length > 0)
				{
					details = string.Format("Can be made const:{0}", details);
					Reporter.TypeFailed(m_type, CheckID, details);
				}
			}
		}
				
		private bool DoConstType(TypeReference type)
		{			
			bool isConst = false;
			switch (type.FullName)
			{
				case "System.Boolean":
				case "System.Byte":
				case "System.Char":
				case "System.Decimal":
				case "System.Float":
				case "System.Double":
				case "System.Int16":
				case "System.Int32":
				case "System.Int64":
				case "System.SByte":
				case "System.String":
				case "System.UInt16":
				case "System.UInt32":
				case "System.UInt64":
					isConst = true;
					break;
			}
			
			if (!isConst)
			{
				TypeDefinition t = Cache.FindType(type);
				isConst = t != null && t.IsEnum;
			}
			
			Log.DebugLine(this, "constType({0}) == {1}", type.FullName, isConst);

			return isConst;
		}
		
		private TypeDefinition m_type;
		private List<string> m_candidates = new List<string>();
	}
}

