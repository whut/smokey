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
	/// <summary>Represents Ldloca_S and Ldloca.</summary>
	/// <remarks>Loads the address of a local variable onto the stack.</remarks>.
	public class LoadLocalAddress : LoadPointer
	{		
		internal LoadLocalAddress(SymbolTable symbols, MethodDefinition method, Instruction untyped, int index) : base(untyped, index)
		{			
			switch (untyped.OpCode.Code)
			{
				case Code.Ldloca_S:
				case Code.Ldloca:
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
					Name = symbols.LocalName(method, untyped, Variable);
					break;
										
				default:
					DBC.Fail(untyped.OpCode.Code + " is not a valid LoadLocalAddress");
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

