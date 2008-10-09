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
using Smokey.Framework.Support;

namespace Smokey.Internal.Rules
{	
	internal sealed class EventSignatureRule : Rule
	{				
		public EventSignatureRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "D1008")
		{
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitEvent");
		}
				
		public void VisitEvent(EventDefinition evt)
		{							
			if (evt.AddMethod.ExternallyVisible(Cache))
			{
				Log.DebugLine(this, "-----------------------------------"); 
				Log.DebugLine(this, "event: {0}", evt.Name);	
				Log.DebugLine(this, "type: {0}", evt.EventType);	
				
				TypeDefinition type = evt.EventType as TypeDefinition;
				if (type != null)
				{
					MethodDefinition[] methods = type.Methods.GetMethod("Invoke");
					if (methods.Length == 1)
					{
						MethodDefinition method = methods[0];
						
						string details = string.Empty;
						if (method.ReturnType.ReturnType.ToString() != "System.Void")
							details += "Return type should be void. ";
							
						if (method.Parameters.Count == 2)
						{
							ParameterDefinition arg1 = method.Parameters[0];
							if (arg1.Name != "sender")
								details += "First argument should be named 'sender'. ";
							if (arg1.ParameterType.FullName != "System.Object")
								details += "First argument should be a System.Object. ";

							ParameterDefinition arg2 = method.Parameters[1];
							if (arg2.Name != "e")
								details += "Second argument should be named 'e'. ";
							if (!arg2.ParameterType.IsSameOrSubclassOf("System.EventArgs", Cache))
								details += "Second argument should be a System.EventArgs or a subclass. ";
						}
						else
							details += "The event should have two arguments. ";
							
						if (details.Length > 0)
						{
							Log.DebugLine(this, details);
							Reporter.TypeFailed(type, CheckID, details);
						}
					}
				}
			}
		}
	}
}

