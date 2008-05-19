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
using System.Diagnostics;
using Smokey.Framework;
using Smokey.Framework.Support;

namespace Smokey.Internal.Rules
{	
	internal sealed class AssemblyAttributeSpellingRule : Rule
	{				
		public AssemblyAttributeSpellingRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "C1020")
		{
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitAssembly");
		}
				
		public void VisitAssembly(AssemblyDefinition assembly)
		{						
			Log.DebugLine(this, "-----------------------------------"); 
			Log.DebugLine(this, "checking {0}", assembly.Name);				

			if (Aspell.Instance != null)
			{				
				string details = string.Empty;
				foreach (CustomAttribute attr in assembly.CustomAttributes)
				{
					foreach (object o in attr.ConstructorParameters)
					{
						string text = o.ToString();
						Ignore.Value = CheckSpelling.Text(text, ref details);
					}
				}
				
				if (details.Length > 0)
				{
					details = "Words: " + details;
					Log.DebugLine(this, details);
					Reporter.AssemblyFailed(assembly, CheckID, details);
				}
			}
		}
	}
}
