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
using Mono.Cecil.Metadata;
using System;
using System.Security;
using System.Collections.Generic;
using Smokey.Framework;
using Smokey.Framework.Instructions;
using Smokey.Framework.Support;
using Smokey.Framework.Support.Advanced;

namespace Smokey.Internal.Rules
{		
	internal sealed class IndirectLinkDemandRule : Rule
	{				
		public IndirectLinkDemandRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "S1004")
		{
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitGraph");
		}
		
		public void VisitGraph(CallGraph graph)
		{
			string bad = string.Empty;
			
			// Iterate over each method in the assembly,
			foreach (KeyValuePair<MethodReference, List<MethodReference>> entry in graph.Entries())
			{
				// if it does not have a security demand then,
				MethodInfo info = Cache.FindMethod(entry.Key);
				if (info != null && info.Method.PubliclyVisible(Cache) && !DoHasSecurityDemand(info.Method))
				{
					foreach (MethodReference callee in entry.Value)
					{
						// if it's calling a method with a link demand then
						// we have a problem. Note that we won't find the
						// method info if the callee is in a different assembly.
						info = Cache.FindMethod(callee);
						if (info != null && DoHasLinkDemand(info.Method))
						{
							Log.DebugLine(this, "bad: {0}", info.Method);
							bad = string.Format("{0} {1}", bad, info.Method);
						}
					}
				}
			}
			
			if (bad.Length > 0)
			{
				string details = "Methods: " + bad;
				Reporter.AssemblyFailed(Cache.Assembly, CheckID, details);
			}
		}
		
		// TODO: might want to use IsSubsetOf instead of a simple existence test
		private bool DoHasSecurityDemand(MethodDefinition method)
		{
			if (method.SecurityDeclarations.Count > 0)
			{
				Log.DebugLine(this, "{0} has a security attr", method);
				return true;
			}
			
			Log.DebugLine(this, "{0} has no security attr", method);
			return false;
		}

		private bool DoHasLinkDemand(MethodDefinition method)
		{
			foreach (SecurityDeclaration dec in method.SecurityDeclarations)
			{
				if (dec.Action == SecurityAction.LinkDemand)
				{
					Log.DebugLine(this, "   {0} has a link demand attr", method);
					return true;
				}
			}
			
			return false;
		}
	}
}

