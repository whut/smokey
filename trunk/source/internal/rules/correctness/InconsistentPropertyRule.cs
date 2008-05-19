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
using Smokey.Framework.Support.Advanced.Values;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Smokey.Internal.Rules
{	
	internal sealed class InconsistentPropertyRule : Rule
	{				
		public InconsistentPropertyRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "C1028")
		{
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitType");
		}
				
		public void VisitType(TypeDefinition type)
		{
			Log.DebugLine(this, "-----------------------------------"); 
			Log.DebugLine(this, "{0}", type);		
			
			foreach (PropertyDefinition prop in type.Properties)
			{
				FieldReference getter = null;
				FieldReference setter = null;
				
				if (prop.GetMethod != null && prop.GetMethod.Parameters.Count == 0)
				{
					if (!prop.GetMethod.CustomAttributes.HasDisableRule("C1028"))
					{
						if (prop.GetMethod.Body != null && prop.GetMethod.Body.Instructions.Count == 3)
						{
							Instruction i = prop.GetMethod.Body.Instructions[1];
							if (i.OpCode.Code == Code.Ldfld || i.OpCode.Code == Code.Ldsfld)
							{
								getter = (FieldReference) i.Operand;
							}
						}
					}
				}

				if (prop.SetMethod != null && prop.SetMethod.Parameters.Count == 1)
				{
					if (prop.SetMethod.Body != null && prop.SetMethod.Body.Instructions.Count == 4)
					{
						Instruction i = prop.SetMethod.Body.Instructions[2];
						if (i.OpCode.Code == Code.Stfld || i.OpCode.Code == Code.Stsfld)
						{
							setter = (FieldReference) i.Operand;
						}
					}
				}
				
				if (getter != null && setter != null)
				{
					if (getter.ToString() != setter.ToString())
					{
						Log.DebugLine(this, "getter uses {0}, but setter uses {1}", getter.Name, setter.Name);		
						Reporter.MethodFailed(prop.GetMethod, CheckID, 0, string.Empty);
					}
				}
			}
		}
	}
}
