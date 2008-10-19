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

#if OLD
namespace Smokey.Framework.Instructions
{	
	/// <summary>Base class for typed Cecil instructions.</summary> 
	///
	/// <remarks>Note that not all instructions
	/// are explicitly typed: just the ones that rules tend to want to visit.</remarks>
	public abstract class TypedInstruction
	{								
		/// <summary>The Cecil instruction.</summary>
		public readonly Instruction Untyped;
		
		/// <summary>The index into the TypedInstructionCollection at which this instruction appears.</summary>
		public readonly int Index;
		
		#region Overrides
		public override string ToString()
		{
			return string.Format("{0,-2:X2}: {1,-10} {2}", Untyped.Offset, Untyped.OpCode, OnOperandToString());
		}
		#endregion
		
		#region Internal methods
		protected TypedInstruction(Instruction untyped, int index)
		{
			Untyped = untyped;
			Index = index;
		}

		protected virtual string OnOperandToString()
		{
			if (Untyped.Operand != null)
				return Untyped.Operand.ToString();
			else
				return string.Empty;
		}
		#endregion
	}
}
#endif
