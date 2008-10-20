// Copyright (C) 2008 Jesse Jones
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

using Smokey.Framework;
using Smokey.Framework.Instructions;
using Smokey.Framework.Support;
using Smokey.Framework.Support.Advanced;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Smokey.Internal.Rules
{	
	internal sealed class StringUri3Rule : Rule
	{				
		public StringUri3Rule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "R1033")
		{
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitMethod");
		}
				
		public void VisitMethod(BeginMethod begin)
		{
			Log.DebugLine(this, "-----------------------------------"); 
			Log.DebugLine(this, "{0}", begin.Info.Method);				
			
			if (begin.Info.Method.ReturnType.ReturnType.FullName == "System.String")
			{
				string[] parts = begin.Info.Method.Name.CapsSplit();
				Log.DebugLine(this, "parts: {0}", string.Join("-", parts));				

				if (parts.Intersect(m_bad).Count() > 0)
				{
					Log.DebugLine(this, "failed for {0}", begin.Info.Method.Name);					
					Reporter.MethodFailed(begin.Info.Method, CheckID, 0, string.Empty);
				}
			}
		}
						
		private string[] m_bad = new[] {"uri", "Uri", "urn", "Urn", "url", "Url"};
	}
}

