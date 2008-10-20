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
	/// <summary>Represents Stelem_I, Stelem_I1, Stelem_I2, Stelem_I4, 
	/// Stelem_I8, Stelem_R4, Stelem_R8, Stelem_Ref, Stelem_Any, 
	/// Stind_I, Stind_Ref, Stind_I1, Stind_I2, Stind_I4, Stind_I8, 
	/// Stind_R4, Stind_R8, Stobj, StoreArg, StoreField StoreLocal, and StoreStaticField.</summary>
	/// 
	/// <remarks>An instruction that pops a value off the stack and stores it into
	/// memory. </remarks>.
	public class Store : TypedInstruction
	{		
		internal Store(Instruction untyped, int index) : base(untyped, index)
		{
		}
	}
}

