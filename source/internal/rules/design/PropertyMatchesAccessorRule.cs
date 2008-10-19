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
	internal sealed class PropertyMatchesAccessorRule : Rule
	{				
		public PropertyMatchesAccessorRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "D1060")
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
			
			string names = string.Empty;
			foreach (PropertyDefinition prop in type.Properties)
			{
				string name = prop.Name;
				
				MethodDefinition[] methods1 = type.Methods.GetMethod("Get" + name);
				MethodDefinition[] methods2 = type.Methods.GetMethod("Set" + name);
				
				if (methods1.Length > 0 && methods1[0].IsPublic)
					names += name + " ";
				else if (methods2.Length > 0 && methods2[0].IsPublic)
					names += name + " ";
			}
			
			if (names.Length > 0)
			{
				Log.DebugLine(this, "names: {0}", names);
				Reporter.TypeFailed(type, CheckID, "Properties: " + names);
			}
		}
	}
}
#endif
