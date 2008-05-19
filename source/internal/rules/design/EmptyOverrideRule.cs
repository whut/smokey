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
using System;
using Smokey.Framework;
using Smokey.Framework.Instructions;
using Smokey.Framework.Support;

namespace Smokey.Internal.Rules
{	
	internal sealed class EmptyOverrideRule : Rule
	{				
		public EmptyOverrideRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "D1044")
		{
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitMethod");
		}
				
		public void VisitMethod(MethodDefinition method)
		{
			Log.DebugLine(this, "-----------------------------------"); 
			Log.DebugLine(this, "{0}", method);				

			if (method.IsVirtual && method.IsReuseSlot)
			{
				if (method.Body != null && method.Body.Instructions.Count == 1)
				{
					MethodDefinition parent = DoGetBase(method);
					if (parent.Body != null && parent.Body.Instructions.Count > 1)
					{
						Reporter.MethodFailed(method, CheckID, 0, string.Empty);
					}
				}
			}
		}
		
		private MethodDefinition DoGetBase(MethodDefinition derived)
		{
			MethodDefinition parent = null;
			
			TypeReference tr = derived.GetDeclaredIn(Cache);
			if (tr != null)
			{
				Log.DebugLine(this, "declared in {0}", tr);				
				TypeDefinition type = Cache.FindType(tr);
				if (type != null)
				{
					parent = type.Methods.GetMethod(derived.Name, derived.Parameters);
				}
			}
			
			return parent;
		}
	}
}

