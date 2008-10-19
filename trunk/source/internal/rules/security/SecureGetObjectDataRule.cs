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
using System.Security;
using System.Collections.Generic;
using Smokey.Framework;
using Smokey.Framework.Instructions;
using Smokey.Framework.Support;
using Smokey.Framework.Support.Advanced;

#if OLD
namespace Smokey.Internal.Rules
{		
	internal sealed class SecureGetObjectDataRule : Rule
	{				
		public SecureGetObjectDataRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "S1011")
		{
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitMethod");
		}
		
		public void VisitMethod(MethodDefinition method)
		{
			Log.DebugLine(this, "checking {0}", method);		
			
			if (method.Name == "GetObjectData")
			{
				TypeReference type = method.GetDeclaredIn(Cache);
				if (type != null && type.FullName == "System.Runtime.Serialization.ISerializable")
				{
					Log.DebugLine(this, "   it's declared in ISerializable");		

					if (!DoIsSecured(method))
					{
						Reporter.MethodFailed(method, CheckID, 0, string.Empty);
					}
				}
			}
		}
		
		// TODO: would be nice to check for SerializationFormatter
		private bool DoIsSecured(MethodDefinition method)
		{
			foreach (SecurityDeclaration dec in method.SecurityDeclarations)
			{
				if (dec.Action == SecurityAction.NonCasDemand || dec.Action == SecurityAction.NonCasLinkDemand || dec.Action == SecurityAction.Demand || dec.Action == SecurityAction.LinkDemand)	// TODO: may be a mono or cecil bug here, doesn't seem like we should have to check NonCasDemand
				{
					Log.DebugLine(this, "   has a demand attr");
					return true;
				}
			}
			
			return false;
		}
	}
}
#endif
