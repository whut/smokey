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
	internal sealed class BeforeEventRule : Rule
	{				
		public BeforeEventRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "D1027")
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
				
				string name = evt.Name.ToLower();
				foreach (string bad in m_bad)
				{
					if (name.Length > bad.Length && name.StartsWith(bad))
					{
						TypeDefinition type = Cache.FindType(evt.DeclaringType);
						if (type != null)
						{
							string details = evt.Name;
							Reporter.TypeFailed(type, CheckID, details);
						}
						else
							Log.ErrorLine(this, "couldn't find type for {0}", evt.Name);
					}
				}
			}
		}
		
		private string[] m_bad = new string[]{"before", "after"};
	}
}
#endif
