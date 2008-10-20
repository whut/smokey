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
using Smokey.Framework;
using Smokey.Framework.Instructions;
using Smokey.Framework.Support;
using Smokey.Framework.Support.Advanced;
using Smokey.Framework.Support.Advanced.Values;

namespace Smokey.Internal.Rules
{		
	internal static class NullCheck
	{		
		public static int Check<T>(TypedInstruction instruction, Tracker tracker, out string details) where T: class
		{
			int offset = -1;
			details = string.Empty;
							
			do
			{
				Call call = instruction as Call;
				if (call != null && call.Target.HasThis)
				{
					int count = call.Target.Parameters.Count + (call.Target.HasThis ? 1 : 0);
					long? value = tracker.GetStack(call.Index, count - 1);
					if (value.HasValue && value.Value == 0)
					{
						offset = instruction.Untyped.Offset;
						details = string.Format("calling {0}", call.Target.Name);
						Log.DebugLine((T) null, "null method at {0:X2}", offset); 
					}
					break;	
				}

				if (instruction.Untyped.OpCode.Code == Code.Ldfld || instruction.Untyped.OpCode.Code == Code.Ldflda)
				{
					long? value = tracker.GetStack(instruction.Index, 0);
					if (value.HasValue && value.Value == 0)
					{
						offset = instruction.Untyped.Offset;
						
						FieldReference field = (FieldReference) instruction.Untyped.Operand;
						details = string.Format("loading field {0}", field.Name);
						Log.DebugLine((T) null, "null field load at {0:X2}", offset); 
					}
					break;
				}

				if (instruction.Untyped.OpCode.Code == Code.Stfld)
				{
					long? value = tracker.GetStack(instruction.Index, 1);
					if (value.HasValue && value.Value == 0)
					{
						offset = instruction.Untyped.Offset;

						FieldReference field = (FieldReference) instruction.Untyped.Operand;
						details = string.Format("loading field {0}", field.Name);
						Log.DebugLine((T) null, "null field store at {0:X2}", offset); 
					}
					break;
				}

				if (instruction.Untyped.OpCode.Code == Code.Unbox)
				{
					long? value = tracker.GetStack(instruction.Index, 0);
					if (value.HasValue && value.Value == 0)
					{
						offset = instruction.Untyped.Offset;
						details = "unboxing";
						Log.DebugLine((T) null, "null unbox at {0:X2}", offset); 
					}
					break;
				}
			}
			while (false);	
			
			return offset;
		}
	}
}
