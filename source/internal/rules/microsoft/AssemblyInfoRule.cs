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
	internal sealed class AssemblyDescriptionRule : Rule
	{				
		public AssemblyDescriptionRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "MS1000")
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

			if (assembly.Name.Version.Major == 0 && assembly.Name.Version.MajorRevision == 0 && 
				assembly.Name.Version.Minor == 0 && assembly.Name.Version.MinorRevision == 0 &&
				assembly.Name.Version.Revision == 0)
			{
				Log.DebugLine(this, "version is zero"); 
				Reporter.AssemblyFailed(assembly, CheckID, string.Empty);
			}
			else
			{
				bool foundDesc = assembly.CustomAttributes.Has("AssemblyDescriptionAttribute");
				if (!foundDesc)
				{
					Log.DebugLine(this, "no AssemblyDescriptionAttribute"); 
					Reporter.AssemblyFailed(assembly, CheckID, string.Empty);
				}
			}
		}
	}
}

