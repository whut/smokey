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

namespace Smokey.Internal.Rules
{	
	internal class HiddenMethodRule : Rule
	{				
		public HiddenMethodRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "D1010")
		{
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitBegin");
		}
				
		public void VisitBegin(BeginMethod begin)
		{	
			if (!begin.Info.Method.IsVirtual)
			{
				Log.DebugLine(this, "-----------------------------------"); 
				Log.DebugLine(this, "{0}", begin.Info.Instructions);
				
				string details = string.Empty;
				List<MethodDefinition> methods = new List<MethodDefinition>();
				if (begin.Info.Type.BaseType != null)
					DoGetBaseMethods(begin.Info.Type.BaseType, begin.Info.Method, methods);
					
				foreach (MethodDefinition method in methods)
				{
					if (DoLeftHidesRight(begin.Info.Method, method))
						details = string.Format("{0} hides {1}. {2}", begin.Info.Method, method, details);
				}
				
				if (details.Length > 0)
				{
					Log.DebugLine(this, details); 
					Reporter.MethodFailed(begin.Info.Method, CheckID, 0, details);
				}
			}
		}
		
		private void DoGetBaseMethods(TypeReference tr, MethodDefinition method, List<MethodDefinition> methods)
		{			
			TypeDefinition type = Cache.FindType(tr);
			if (type != null)
			{
				MethodDefinition[] m = type.Methods.GetMethod(method.Name);
				methods.AddRange(m);
				
				if (type.BaseType != null)
					DoGetBaseMethods(type.BaseType, method, methods);
			}
		}

		private bool DoLeftHidesRight(MethodReference left, MethodReference right)
		{
			DBC.Assert(left.Name == right.Name, "names don't match");
			
			bool hides = false;
			
			if (left.Parameters.Count == right.Parameters.Count)
			{
				int count = 0;
				for (int i = 0; i < left.Parameters.Count && count >= 0; ++i)
				{
					TypeReference leftType = left.Parameters[i].ParameterType;
					TypeReference rightType = right.Parameters[i].ParameterType;
					
					if (leftType.MetadataToken != rightType.MetadataToken)
					{				
						if (rightType.IsSubclassOf(leftType, Cache))
							++count;
						else
							count = -1;					// left types have to all be equal or a base class of right
					}
				}
				
				hides = count > 0;
			}
			
			return hides;
		}
	}
}

