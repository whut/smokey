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
	[DisableRule("M1000", "UseJurassicNaming")]	// this should be enabled, but i dont want to try svn moving a file with only case changes
	internal sealed class CLSCompliantRule : Rule
	{				
		public CLSCompliantRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "D1018")
		{
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitAssembly");
		}
				
		public void VisitAssembly(AssemblyDefinition assembly)
		{				
			if (Cache.HasPublicTypes)
			{
				bool foundCLS = assembly.CustomAttributes.Has("CLSCompliantAttribute");
				if (!foundCLS)
				{
					Log.DebugLine(this, "no CLSCompliantAttribute"); 
					Reporter.AssemblyFailed(assembly, CheckID, string.Empty);
				}
			}
		}
	}
}

