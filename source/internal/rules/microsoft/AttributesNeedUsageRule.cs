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
using Smokey.Framework.Support;

namespace Smokey.Internal.Rules
{	
	internal class AttributesNeedUsageRule : Rule
	{				
		public AttributesNeedUsageRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "MS1001")
		{
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitType");
		}
				
		public void VisitType(TypeDefinition type)
		{						
			Log.DebugLine(this, "-----------------------------------"); 
			Log.DebugLine(this, "checking {0}", type);				

			if (type.BaseType != null)
			{			
				bool passed = !type.IsSubclassOf("System.Attribute", Cache);

				for (int i = 0; i < type.CustomAttributes.Count && !passed; ++i)
				{
					string name = type.CustomAttributes[i].Constructor.ToString();
					if (name.Contains("System.AttributeUsageAttribute"))
					{
						passed = true;
					}
				}
				
				if (!passed)
				{
					Log.DebugLine(this, "{0} has no usage attribute", type.Name); 
					Reporter.TypeFailed(type, CheckID, string.Empty);
				}
			}
		}
	}
}

