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
using System.Collections.Generic;
using System.Diagnostics;
using Smokey.Framework;
using Smokey.Framework.Instructions;
using Smokey.Framework.Support;

#if OLD
namespace Smokey.Internal.Rules
{	
	internal static class IntegerHelpers
	{						
		public static bool IsIntOperand(MethodInfo info, int index, int nth)
		{
			bool isInt = false;
			
			int i = info.Tracker.GetStackIndex(index, nth);
			if (i >= 0)
			{
				do
				{
					LoadArg arg = info.Instructions[i] as LoadArg;
					if (arg != null && arg.Arg >= 1)
					{
						ParameterDefinition p = info.Method.Parameters[arg.Arg - 1];
						if (p.ParameterType.FullName == "System.Int32")
							isInt = true;
						break;
					}

					LoadConstantInt constant = info.Instructions[i] as LoadConstantInt;
					if (constant != null)
					{
						Code code = constant.Untyped.OpCode.Code;
						switch (code)
						{
							case Code.Ldc_I4_M1:
							case Code.Ldc_I4_0:
							case Code.Ldc_I4_1:
							case Code.Ldc_I4_2:
							case Code.Ldc_I4_3:
							case Code.Ldc_I4_4:
							case Code.Ldc_I4_5:
							case Code.Ldc_I4_6:
							case Code.Ldc_I4_7:
							case Code.Ldc_I4_8:
							case Code.Ldc_I4_S:
							case Code.Ldc_I4:
								isInt = true;
								break;
						}
						break;
					}

					LoadField field = info.Instructions[i] as LoadField;
					if (field != null)
					{
						if (field.Field.FieldType.FullName == "System.Int32")
							isInt = true;
						break;
					}

					LoadLocal local = info.Instructions[i] as LoadLocal;
					if (local != null)
					{
						VariableDefinition v = info.Method.Body.Variables[local.Variable];
						if (v.VariableType.FullName == "System.Int32")
							isInt = true;
						break;
					}

					LoadStaticField sfield = info.Instructions[i] as LoadStaticField;
					if (sfield != null)
					{
						if (sfield.Field.FieldType.FullName == "System.Int32")
							isInt = true;
						break;
					}
				} 
				while (false);	
			}
			
			return isInt;
		}
	}
}
#endif