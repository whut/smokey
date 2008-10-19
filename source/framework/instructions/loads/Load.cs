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
	/// <summary>Represents Ldelem_I1, Ldelem_U1, Ldelem_I2, Ldelem_U2, Ldelem_I4, Ldelem_U4, 
	/// Ldelem_I8, Ldelem_I, Ldelem_R4, Ldelem_R8, Ldelem_Ref, Ldelem_Any, 
	/// Ldind_I1, Ldind_U1, Ldind_I2, Ldind_U2, Ldind_I4, Ldind_U4, Ldind_I8, 
	/// Ldind_I, Ldind_R4, Ldind_R8, Ldind_Ref, Ldlen, LoadArg, LoadConstant, 
	/// LoadField, LoadLocal, LoadNull, LoadPointer, LoadStaticField, and
	/// LoadString.</summary>
	/// 
	/// <remarks>An instruction that loads a value onto the stack. </remarks>
	public class Load : TypedInstruction
	{		
		internal Load(Instruction untyped, int index) : base(untyped, index)
		{
		}
	}
}
#endif
