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

#if OLD
namespace Smokey.Internal.Rules
{	
	internal sealed class BadExplicitImplementationRule : Rule
	{				
		public BadExplicitImplementationRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "D1054")
		{
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitType");
		}
				
		public void VisitType(TypeDefinition type)
		{		
			if (!type.IsSealed && type.ExternallyVisible(Cache))
			{
				Log.DebugLine(this, "-----------------------------------"); 
				Log.DebugLine(this, "{0}", type);
				
				foreach (MethodDefinition method in type.Methods)
				{
					if (method.IsPrivate && !method.IsReuseSlot)
					{
						Log.DebugLine(this, "{0} is an explicit implementation", method);
						
						string name = method.Name.Substring(method.Name.LastIndexOf('.') + 1);
						
						bool found = false;
						MethodDefinition[] methods = type.Methods.GetMethod(name);
						foreach (MethodDefinition candidate in methods)
						{
							Log.DebugLine(this, "   checking {0}", candidate);
							
							if (!candidate.IsPrivate)
							{
								Log.DebugLine(this, "   it's an alternative");
								found = true;
								break;
							}
						}
						
						if (!found)
						{
							string details = "Method: " + name;
							Reporter.TypeFailed(type, CheckID, details);
							break;
						}
					}
				}
			}
		}
	}
}
#endif
