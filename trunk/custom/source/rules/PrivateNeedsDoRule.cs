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

namespace Custom
{	
	internal class PrivateNeedsDoRule : Rule
	{				
		// The checkID must match the id in the xml. Note that multiple classes can
		// share the same checkID.
		public PrivateNeedsDoRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "CU1000 - example custom rules")
		{
		}
		
		// This is where you register the types that you want to visit. You can
		// visit AssemblyDefinition, TypeDefinition, MethodDefinition, the types
		// within the method, etc. See RuleDispatcher for the complete list.
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitMethod");
		}
		
		// All we care about are the method properties and the name so we can get
		// by with just visiting MethodDefinition. 
		public void VisitMethod(MethodDefinition method)
		{
			// If the method is private and not p/invoke,
			MethodAttributes attrs = method.Attributes;
			if ((attrs & MethodAttributes.MemberAccessMask) == MethodAttributes.Private && 
				(attrs & MethodAttributes.PInvokeImpl) != MethodAttributes.PInvokeImpl)
			{
				// not a ctor, not virtual (it'll be virtual if it's an explicit interface
				// member implementation), and not a property or event,
				if (!method.IsConstructor && !method.IsVirtual && method.SemanticsAttributes == 0)
				{
					// then it's a candidate for our rule so we'll go ahead and
					// do some logging. Note that DebugLine is compiled out in
					// release builds.
					Log.DebugLine(this, "-----------------------------------"); 
					Log.DebugLine(this, "{0}", method);				
			
					// If the name does not start with Do and isn't compiler generated,
					if (!method.Name.StartsWith("Do") && !method.ToString().Contains("CompilerGenerated"))
					{
						// then we have a failure. For rules which fail within a
						// method, offset should be the first offset to the instruction
						// which triggered the violation. Details can be used to
						// communicate additional information to the user about the
						// cause of the violation.
						int offset = 0;
						string details = string.Empty;
						Log.DebugLine(this, "doesn't start with Do");
						
						// Note that there are also AssemblyFailed and TypeFailed methods.
						Reporter.MethodFailed(method, CheckID, offset, details);
					}
				}
			}
		}
	}
}
