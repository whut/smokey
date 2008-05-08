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
	/// <summary>Represents Conv_I1, Conv_I2, Conv_I4, Conv_I8, Conv_R4, Conv_R8, 
	/// Conv_U4, Conv_U8, Conv_R_Un, Conv_Ovf_I1_Un, Conv_Ovf_I2_Un, Conv_Ovf_I4_Un, 
	/// Conv_Ovf_I8_Un, Conv_Ovf_U1_Un, Conv_Ovf_U2_Un, Conv_Ovf_U4_Un, 
	/// Conv_Ovf_U8_Un, Conv_Ovf_I_Un, Conv_Ovf_U_Un, Conv_Ovf_I1, Conv_Ovf_U1, 
	/// Conv_Ovf_I2, Conv_Ovf_U2, Conv_Ovf_I4, Conv_Ovf_U4, Conv_Ovf_I8, Conv_Ovf_U8, 
	/// Conv_U2, Conv_U1, Conv_I, Conv_Ovf_I, Conv_Ovf_U, and Conv_U.</summary>
	/// 
	/// <remarks>Pops a value off the stack, converts it into a new value, and
	/// pushes the result. Note that the only exception thrown is OverflowException
	/// and that only by the Ovf versions. </remarks>.
	public class Conv : TypedInstruction
	{		
		internal Conv(Instruction untyped, int index) : base(untyped, index)
		{
		}
	}
}
