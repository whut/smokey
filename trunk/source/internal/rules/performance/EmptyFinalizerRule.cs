// Copyright (C) 2007 Jesse Jones
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//	Jesse Jones <jesjones@mindspring.com>
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

namespace Smokey.Internal.Rules
{	
	internal sealed class EmptyFinalizerRule : Rule
	{				
		public EmptyFinalizerRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "P1000")
		{
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitType");
		}
				
		public void VisitType(TypeDefinition type)
		{			
			bool passed = true;
			
			Log.DebugLine(this, "-----------------------------------"); 
			Log.DebugLine(this, "checking {0}", type);				

			// If the type has a finalizer then,
			MethodDefinition finalizer = type.Methods.GetMethod("Finalize", Type.EmptyTypes);
			if (finalizer != null)
			{
				// if it's empty we have a problem.
				passed = false;
				for (int i = 0; i < finalizer.Body.Instructions.Count && !passed; ++i)
				{
					Instruction instruction = finalizer.Body.Instructions[i]; 
					switch (instruction.OpCode.Code) 
					{
						case Code.Call:		// if it's calling something besides a base Finalize method it's doing something useful
							MethodReference mr = instruction.Operand as MethodReference;
							if (mr == null || mr.Name != "Finalize")
								passed = true;
							break;
						
						case Code.Nop:			// these are used to call the base Finalize method for empty methods
						case Code.Leave:		
						case Code.Leave_S:
						case Code.Ldarg_0:
						case Code.Endfinally:
						case Code.Ret:
							break;
						
						default:			// anything else means we're doing real work
							passed = true;
							break;
					}
				}

				if (!passed)
				{
					Log.DebugLine(this, "found empty finalizer"); 
					Reporter.TypeFailed(type, CheckID, string.Empty);
				}
			}
		}
	}
}
