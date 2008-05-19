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
using Smokey.Framework.Support;

namespace Smokey.Internal.Rules
{	
	internal sealed class LargeNamespaceRule : Rule
	{				
		public LargeNamespaceRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "D1000")
		{
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitType");
			dispatcher.Register(this, "VisitEnd");
		}
						
		public void VisitType(TypeDefinition type)
		{
			// If it's public,
			TypeAttributes attrs = type.Attributes & TypeAttributes.VisibilityMask;
			if (attrs == TypeAttributes.Public)
			{
				// and it's not in the global namespace,
				if (type.Namespace != null)
				{
					// then increment our counter.
					Log.DebugLine(this, "found {0}.{1}", type.Namespace, type.Name);	
					
					if (m_counts.ContainsKey(type.Namespace))
						m_counts[type.Namespace] += 1;
					else
						m_counts.Add(type.Namespace, 1);
				}
			}
		}

		public void VisitEnd(EndTypes method)
		{
			int maxNames = Settings.Get("maxNamespace", 40);
			
			string details = string.Empty;
			foreach (KeyValuePair<string, int> entry in m_counts)
			{
				if (entry.Value > maxNames)
					details = string.Format("{0} has {1} public types. {2}", entry.Key, entry.Value, details);
			}
			
			if (details.Length > 0)
			{
				Log.DebugLine(this, details);	
				Reporter.AssemblyFailed(Cache.Assembly, CheckID, details);
			}
		}
				
		private Dictionary<string, int> m_counts = new Dictionary<string, int>();		
	}
}

