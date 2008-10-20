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

namespace Smokey.Framework.Instructions
{		
	/// <summary>Represents Starg_S and Starg.</summary>
	/// 
	/// <remarks>Pops a value from the stack and stores it into an argument. 
	/// </remarks>.
	public class StoreArg : Store
	{		
		internal StoreArg(MethodDefinition method, Instruction untyped, int index) : base(untyped, index)
		{			
			int delta = method.HasThis ? 1 : 0;

			switch (untyped.OpCode.Code)
			{
				case Code.Starg:
				case Code.Starg_S:
					ParameterDefinition param = untyped.Operand as ParameterDefinition;
					if (param != null)
					{
						if (method.HasThis)
							Argument = param.Sequence;
						else
							Argument = param.Sequence - 1;
						Name = param.Name;
					}
					else
					{
						Argument = (int) untyped.Operand;
						Name = method.Parameters[Argument - delta].Name;
					}
					break;
					
				default:
					DBC.Fail(untyped.OpCode.Code + " is not a valid StoreArg");
					break;
			}
		}

		/// <summary>The argument we're storing into.</summary>
		public readonly int Argument;
		
		/// <summary>Either the real name of the argument or something lame like A_1.</summary>
		public readonly string Name;

		protected override string OnOperandToString()
		{
			return Name;
		}
	}
}

