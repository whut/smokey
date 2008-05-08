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
	internal class SerializableMethodsRule : Rule
	{				
		public SerializableMethodsRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "D1030")
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
			
			foreach (CustomAttribute attr in method.CustomAttributes)
			{	
				if (attr.Constructor.ToString().Contains("System.Runtime.Serialization.OnSerializingAttribute") ||
					attr.Constructor.ToString().Contains("System.Runtime.Serialization.OnSerializedAttribute") ||
					attr.Constructor.ToString().Contains("System.Runtime.Serialization.OnDeserializingAttribute") ||
					attr.Constructor.ToString().Contains("System.Runtime.Serialization.OnDeserializedAttribute"))
				{
					do
					{
						MethodAttributes attrs = method.Attributes & MethodAttributes.MemberAccessMask;
						if (attrs != MethodAttributes.Private)
						{
							Log.DebugLine(this, "access is {0}", attrs); 
							Reporter.MethodFailed(method, CheckID, 0, string.Empty);
							break;
						}

						if (method.ReturnType.ReturnType.ToString() != "System.Void")
						{
							Log.DebugLine(this, "return type is {0}", method.ReturnType.ReturnType); 
							Reporter.MethodFailed(method, CheckID, 0, string.Empty);
							break;
						}

						if (method.Parameters.Count != 1)
						{
							Log.DebugLine(this, "has {0} arguments", method.Parameters.Count); 
							Reporter.MethodFailed(method, CheckID, 0, string.Empty);
							break;
						}

						if (method.Parameters[0].ParameterType.FullName != "System.Runtime.Serialization.StreamingContext")
						{
							Log.DebugLine(this, "arg type is {0}", method.Parameters[0].ParameterType.FullName); 
							Reporter.MethodFailed(method, CheckID, 0, string.Empty);
							break;
						}
					}
					while (false);
				}
			}
		}
	}
}

