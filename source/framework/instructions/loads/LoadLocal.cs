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
using System.Text;

namespace Smokey.Framework.Instructions
{		
	/// <summary>Represents Ldloc_0, Ldloc_1,
	/// Ldloc_2, and Ldloc_3, Ldloc_S, and Ldloc.</summary>
	/// 
	/// <remarks>Loads a local variable onto the stack. Note that these are initialized
	/// to 0 if the method's initialize flag is set. </remarks>.
	public class LoadLocal : Load
	{		
		internal LoadLocal(SymbolTable symbols, MethodDefinition method, Instruction untyped, int index) : base(untyped, index)
		{			
			switch (untyped.OpCode.Code)
			{
				case Code.Ldloc_0:
					Name = symbols.LocalName(method, 0);
					Type = method.Body.Variables[Variable].VariableType;
					break;
					
				case Code.Ldloc_1:
					Variable = 1;
					Name = symbols.LocalName(method, 1);
					Type = method.Body.Variables[Variable].VariableType;
					break;
					
				case Code.Ldloc_2:
					Variable = 2;
					Name = symbols.LocalName(method, 2);
					Type = method.Body.Variables[Variable].VariableType;
					break;
					
				case Code.Ldloc_3:
					Variable = 3;
					Name = symbols.LocalName(method, 3);
					Type = method.Body.Variables[Variable].VariableType;
					break;
					
				case Code.Ldloc:
				case Code.Ldloc_S:
					VariableDefinition param = untyped.Operand as VariableDefinition;
					if (param != null)
					{
						Variable = param.Index;
						Type = param.VariableType;
					}
					else
					{
						Variable = (int) untyped.Operand;
						Type = method.Body.Variables[Variable].VariableType;
					}
					Name = symbols.LocalName(method, Variable);
					break;
					
				default:
					DBC.Fail(untyped.OpCode.Code + " is not a valid LoadLocal");
					break;
			}		
			
			RealName = symbols.HaveLocalNames(method);
		}
		
		/// <summary>The local variable that was loaded onto the stack. Note
		/// that scopes do not affect this index.</summary>
		/// <remarks>MethodDefinition::Body.Variables can be used to get details
		/// about the variable.</remarks>
		public readonly int Variable;
		
		/// <summary>Either the name of the local or something lame like V_1.</summary>
		public readonly string Name;

		/// <summary>True if Name is the real name of the local (although unnamed temporaries
		/// will still ne named V_n).</summary>
		public readonly bool RealName;

		/// <summary>The local variable's type.</summary>
		public readonly TypeReference Type;

		protected override string OnOperandToString()
		{
			return Name;
		}
	}
}
