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
using Smokey.Framework;
using Smokey.Framework.Instructions;
using Smokey.Framework.Support;
using Smokey.Framework.Support.Advanced;

namespace Smokey.Internal.Rules
{	
	internal class DestroyStackTraceRule : Rule
	{				
		public DestroyStackTraceRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "MS1003")
		{
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitBegin");
		}
				
		public void VisitBegin(BeginMethod method)
		{
			Log.DebugLine(this, "-----------------------------------"); 
			Log.DebugLine(this, "{0:F}", method.Info.Instructions);				

			// Loop through each try block,
			int offset = -1;
			for (int i = 0; i < method.Info.Instructions.TryCatchCollection.Length && offset < 0; ++i)
			{
				TryCatch tc = method.Info.Instructions.TryCatchCollection[i];

				// and the associated catch blocks,
				for (int j = 0; j < tc.Catchers.Count && offset < 0; ++j)
				{
					CatchBlock cb = tc.Catchers[j];
					
					// if the first instruction in a catch is a store local then the code
					// is storing a reference to the exception object and we need to check
					// to see if it's being rethrown.				
					StoreLocal store = method.Info.Instructions[cb.Index] as StoreLocal;
					if (store != null)
						offset = DoCheckCatch(method.Info.Instructions, cb, store.Variable);
				}
			}			
			
			if (offset >= 0)
				Reporter.MethodFailed(method.Info.Method, CheckID, offset, string.Empty);
		}

		private int DoCheckCatch(TypedInstructionCollection instructions, CatchBlock cb, int varIndex)
		{
			int offset = -1;
			
			// Loop through all of the instructions in the catch block,
			for (int index = cb.Index; index < cb.Index + cb.Length && offset < 0; ++index)
			{
				TypedInstruction instruction = instructions[index];
				
				// if we find a throw instruction,
				if (instruction.Untyped.OpCode.Code == Code.Throw)
				{
					// and it's preceded by a load from varIndex then we have a problem.
					LoadLocal load = instructions[index - 1] as LoadLocal;
					if (load != null && load.Variable == varIndex)
					{
						offset = instruction.Untyped.Offset;
						Log.DebugLine(this, "bad throw at {0:X2}", offset); 
					}
				}
			}
			
			return offset;
		}
	}
}

