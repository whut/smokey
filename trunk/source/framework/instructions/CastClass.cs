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
	/// <summary>Represents Castclass and Isinst.</summary>
	/// 
	/// <remarks>Pops a pointer off the stack and attempts to cast it. If the pointer is null
	/// or the cast succeeds the pointer is pushed back onto the stack. If the cast
	/// fails Castclass throws and Isinst pushes null. </remarks>.
	public class CastClass : TypedInstruction
	{		
		internal CastClass(Instruction untyped, int index) : base(untyped, index)
		{
			ToType = (TypeReference) untyped.Operand;
		}
		
		/// <summary>The type we're casting to.</summary>
		public readonly TypeReference ToType;
	}
}

