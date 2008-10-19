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
	/// <summary>Represents Add, Add_Ovf, Add_Ovf_Un, Div, Div_Un, Mul, Mul_Ovf, Mul_Ovf_Un
	/// Rem, Rem_Un, Sub, Sub_Ovf, Sub_Ovf_Un, And, Or, Xor, Shl, Shr, and Shr_Un.</summary>
	/// 
	/// <remarks>An instruction that behaves like the binary operators in C#.
	/// </remarks>.
	public class BinaryOp : TypedInstruction
	{		
		internal BinaryOp(Instruction untyped, int index) : base(untyped, index)
		{			
		}
	}
}
#endif
