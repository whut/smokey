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
using Smokey.Framework;
using Smokey.Framework.Instructions;
using Smokey.Framework.Support;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

#if OLD
namespace Smokey.Internal.Rules
{	
	internal sealed class ConstantResultRule : Rule
	{				
		public ConstantResultRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "C1038")
		{
		}
		
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitMethod");
		}
		
		public void VisitMethod(BeginMethod begin)
		{
			Log.DebugLine(this, "-----------------------------------"); 
			Log.DebugLine(this, "{0:F}", begin.Info.Instructions);	
			
			switch (begin.Info.Method.Name)
			{	                       
				case "Equals":
				case "GetHashCode":
				case "CompareTo":
				case "op_Equality":
				case "op_Inequality":
				case "op_GreaterThanOrEqual":
				case "op_LessThan":
				case "op_LessThanOrEqual":
					DoCheck(begin.Info);
					break;
			}
		}
		
		private void DoCheck(MethodInfo info)
		{
			if (info.Instructions.Length == 2)
			{
				LoadConstantInt load = info.Instructions[0] as LoadConstantInt;
				End end = info.Instructions[1] as End;
				
				if (load != null && end != null && end.Untyped.OpCode.Code == Code.Ret)
				{
					Log.DebugLine(this, "returns constant");
					Reporter.MethodFailed(info.Method, CheckID, 0, string.Empty);
				}
			}
		}
	}
}
#endif