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
	internal sealed class PropertyReturnsCollectionRule : Rule
	{				
		public PropertyReturnsCollectionRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "P1022")
		{
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitBegin");
			dispatcher.Register(this, "VisitProp");
		}
				
		public void VisitBegin(BeginType begin)
		{
			Log.DebugLine(this, "-----------------------------------"); 
			Log.DebugLine(this, "{0}", begin.Type);				

			m_needsCheck = begin.Type.IsPublic || begin.Type.IsNestedPublic;
		}
		
		public void VisitProp(PropertyDefinition prop)
		{
			if (m_needsCheck)
			{
				foreach (string name in m_bad)
				{
					if (prop.PropertyType.FullName.Contains(name))
					{
						Log.DebugLine(this, "{0} type = {1}", prop.Name, prop.PropertyType);				
					
						if (prop.GetMethod != null)
							Reporter.MethodFailed(prop.GetMethod, CheckID, 0, string.Empty);
						else if (prop.SetMethod != null)
							Reporter.MethodFailed(prop.SetMethod, CheckID, 0, string.Empty);
					}
				}
			}
		}

		private bool m_needsCheck;
		private string[] m_bad = new string[]{
			"[]",
			"System.Collections.Generic.Dictionary",
			"System.Collections.Generic.LinkedList",
			"System.Collections.Generic.List",
			"System.Collections.Generic.Queue",
			"System.Collections.Generic.SortedDictionary",
			"System.Collections.Generic.SortedList",
			"System.Collections.Generic.Stack",
			};
	}
}

