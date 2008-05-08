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
using Smokey.Framework.Support;
using Smokey.Framework.Support.Advanced;

namespace Smokey.Internal.Rules
{	
	internal class SwallowExceptionRule : Rule
	{				
		public SwallowExceptionRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "MS1011")
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
					
					// if the catch block consists of pop and leave then it's empty.
					if (cb.Length == 2)
					{
						if (method.Info.Instructions[cb.Index].Untyped.OpCode.Code == Code.Pop)
						{
							if (method.Info.Instructions[cb.Index + 1].Untyped.OpCode.Code == Code.Leave || method.Info.Instructions[cb.Index + 1].Untyped.OpCode.Code == Code.Leave_S)
							{
								string name = cb.CatchType.FullName;
								if (name.Contains("System.Exception") || name.Contains("System.SystemException"))
								{
									offset = method.Info.Instructions[cb.Index].Untyped.Offset;
									Log.DebugLine(this, "empty catch at {0:X2}", offset); 
									Reporter.MethodFailed(method.Info.Method, CheckID, offset, string.Empty);
								}
							}
						}
					}
				}
			}			
		}
	}
}

