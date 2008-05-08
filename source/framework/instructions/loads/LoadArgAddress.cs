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
	/// <summary>Represents Ldarga and Ldarga_S.</summary>
	/// 
	/// <remarks>Loads a method argument onto the stack.. </remarks>.
	public class LoadArgAddress : LoadPointer
	{		
		internal LoadArgAddress(MethodDefinition method, Instruction untyped, int index) : 
			base(untyped, index)
		{						
			switch (untyped.OpCode.Code)
			{
				case Code.Ldarga:
				case Code.Ldarga_S:
					ParameterDefinition param = untyped.Operand as ParameterDefinition;
					if (param != null)
						Arg = param.Sequence;
					else
						Arg = (int) untyped.Operand;
					break;
					
				default:
					DBC.Fail(untyped.OpCode.Code + " is not a valid LoadArg");
					break;
			}
			
			if (Arg == 0)
				Name = "this";
			else
				Name = method.Parameters[Arg - 1].Name;
		}

		/// <summary>This will be zero if the argument is the 'this' pointer
		/// or in [1..n] if the argument is a normal method argument.</summary>
		/// <remarks>Note that MethodReference::Parameters can be used to get
		/// information about normal arguments, but the index will have to be
		/// decremented first.</remarks>
		public readonly int Arg;
		
		/// <summary>Either the real name of the argument or something lame like A_1.</summary>
		public readonly string Name;

		protected override string OnOperandToString()
		{
			return Name;
		}
	}
}
