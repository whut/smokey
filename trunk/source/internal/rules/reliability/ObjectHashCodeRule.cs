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
using System.Text;
using Smokey.Framework;
using Smokey.Framework.Instructions;
using Smokey.Framework.Support;

namespace Smokey.Internal.Rules
{	
	internal class ObjectHashCodeRule : Rule
	{				
		public ObjectHashCodeRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "R1012")
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

			if (!type.IsValueType)
			{				
				var methods = new List<MethodInfo>();
				methods.AddRange(Cache.FindMethods(type, "Equals"));
				methods.AddRange(Cache.FindMethods(type, "op_Equality"));
				
				if (methods.Count > 0 && DoUsesObjectHash(type))
				{					
					for (int i = 0; i < methods.Count && passed; ++i)
					{						
						if (DoChecksState(methods[i]))
						{
							Log.DebugLine(this, "and {0} checks state", methods[i].Method.Name);	
							Reporter.TypeFailed(type, CheckID, string.Empty);
						}
					}
				}
			}
		}

		private bool DoChecksState(MethodInfo info)	
		{
			bool usesStates = false;
			Log.DebugLine(this, "checking:", info.Instructions);	
			Log.DebugLine(this, "{0:F}", info.Instructions);	
			
			for (int i = 1; i < info.Instructions.Length && !usesStates; ++i)
			{
				Code code = info.Instructions[i].Untyped.OpCode.Code;
				if (code == Code.Ldfld || code == Code.Ldflda)
				{	
					if (info.Instructions[i - 1].Untyped.OpCode.Code == Code.Ldarg_0)
					{
						usesStates = true;
					}
				}
				else if (code == Code.Call || code == Code.Callvirt)
				{
					string name = ((MethodReference) info.Instructions[i].Untyped.Operand).Name;
					if (name.StartsWith("get_"))
					{	
						if (info.Instructions[i - 1].Untyped.OpCode.Code == Code.Ldarg_0)
						{
							usesStates = true;
						}
					}
				}
			}

			return usesStates;
		}
		
		private bool DoUsesObjectHash(TypeDefinition type)
		{
			List<MethodInfo> methods = Cache.FindMethods(type, "GetHashCode");
			bool usesBase = methods.Count == 0;
			
			if (!usesBase)
			{
				Log.DebugLine(this, "checking:");	
				Log.DebugLine(this, "{0:F}", methods[0].Instructions);	
				
				for (int i = 1; i < methods[0].Instructions.Length; ++i)
				{
					Call call = methods[0].Instructions[i] as Call;
					if (call != null && call.Target.ToString().Contains("System.Object::GetHashCode()"))
					{	
						if (methods[0].Instructions[i - 1].Untyped.OpCode.Code == Code.Ldarg_0)
						{
							Log.DebugLine(this, "calls Object.GetHashCode");	
							usesBase = true;
						}
					}
				}
			}
			else
				Log.DebugLine(this, "{0} doesn't have GetHashCode", type.Name);	
		
			return usesBase;
		}
	}
}

