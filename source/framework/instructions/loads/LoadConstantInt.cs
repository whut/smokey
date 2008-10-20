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
	/// <summary>Represents Ldc_I4_M1, Ldc_I4_0, 
	/// Ldc_I4_1, Ldc_I4_2, Ldc_I4_3, Ldc_I4_4, Ldc_I4_5, Ldc_I4_6, Ldc_I4_7, 
	/// Ldc_I4_8, Ldc_I4_S, Ldc_I4, and Ldc_I8.</summary>
	/// 
	/// <remarks>Pushes an integer constant onto the stack. </remarks>.
	public class LoadConstantInt : Load
	{		
		internal LoadConstantInt(Instruction untyped, int index) : base(untyped, index)
		{			
			switch (untyped.OpCode.Code)
			{
				case Code.Ldc_I4_M1:
					Value = -1;
					break;
																				
				case Code.Ldc_I4_0:
					Value = 0;
					break;
										
				case Code.Ldc_I4_1:
					Value = 1;
					break;
										
				case Code.Ldc_I4_2:
					Value = 2;
					break;
										
				case Code.Ldc_I4_3:
					Value = 3;
					break;
										
				case Code.Ldc_I4_4:
					Value = 4;
					break;
										
				case Code.Ldc_I4_5:
					Value = 5;
					break;
										
				case Code.Ldc_I4_6:
					Value = 6;
					break;
										
				case Code.Ldc_I4_7:
					Value = 7;
					break;
										
				case Code.Ldc_I4_8:
					Value = 8;
					break;
													
				case Code.Ldc_I4_S:
					Value = (SByte) untyped.Operand;
					break;
										
				case Code.Ldc_I4:
					Value = Convert.ToInt32(untyped.Operand);
					break;
										
				case Code.Ldc_I8:
					Value = Convert.ToInt64(untyped.Operand);
					break;
																														
				default:
					DBC.Fail(untyped.OpCode.Code + " is not a valid LoadConstantInt");
					break;
			}
		}

		/// <summary>The value we're pushing onto the stack.</summary>
		public readonly long Value;
	}
}

