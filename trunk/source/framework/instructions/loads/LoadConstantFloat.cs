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
	/// <summary>Represents Ldc_R4 and
	/// Ldc_R8.</summary>
	/// 
	/// <remarks>Pushes a floating point constant onto the stack. </remarks>.
	public class LoadConstantFloat : Load
	{		
		internal LoadConstantFloat(Instruction untyped, int index) : base(untyped, index)
		{			
			switch (untyped.OpCode.Code)
			{
				case Code.Ldc_R4:
					Value = (double) Convert.ToSingle(untyped.Operand);
					break;
										
				case Code.Ldc_R8:
					Value = Convert.ToDouble(untyped.Operand);
					break;
																				
				default:
					DBC.Fail(untyped.OpCode.Code + " is not a valid LoadConstantFloat");
					break;
			}
		}

		/// <summary>The value we're pushing onto the stack.</summary>
		public readonly double Value;
	}
}
#endif
