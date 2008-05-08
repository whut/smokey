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

namespace Smokey.Internal.Rules
{	
	internal class EventHandlerRule : Rule
	{				
		public EventHandlerRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "D1023")
		{
			Runtime = TargetRuntime.NET_2_0;
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitType");
		}
				
		public void VisitType(TypeDefinition outerType)
		{
			Log.DebugLine(this, "-----------------------------------"); 
			Log.DebugLine(this, "{0}", outerType);	
			
			foreach (TypeDefinition type in outerType.NestedTypes)
			{
				Log.DebugLine(this, "+{0}", type.Name);	
				
				if (type.BaseType != null)
				{
					Log.DebugLine(this, "type.BaseType.Name: {0}", type.BaseType.FullName);	
					if (type.IsSubclassOf("System.Delegate", Cache) || type.IsSubclassOf("System.MulticastDelegate", Cache))
					{
						MethodDefinition[] methods = type.Methods.GetMethod("Invoke");
						if (methods.Length == 1)
						{
							MethodDefinition method = methods[0];
							Log.DebugLine(this, "method: {0}", method);	
							if (method.ReturnType.ReturnType.ToString() == "System.Void")
							{
								if (method.Parameters.Count == 2)
								{
									ParameterDefinition arg1 = method.Parameters[0];
									if (arg1.ParameterType.FullName == "System.Object")
									{
										ParameterDefinition arg2 = method.Parameters[1];
										if (arg2.ParameterType.Name.Contains("EventArgs"))	// TODO: we can do this better now, see VisibleEventHandlerRule
										{													// TODO: might want to search for Contains("
											string details = string.Format("Event: {0}.", type.Name);
											Reporter.TypeFailed(outerType, CheckID, details);
											return;
										}
									}
								}
							}
						}
					}
				}
			}
		}				
	}
}

