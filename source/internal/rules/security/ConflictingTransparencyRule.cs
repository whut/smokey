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
using System.Security;
using System.Collections.Generic;
using Smokey.Framework;
using Smokey.Framework.Instructions;
using Smokey.Framework.Support;
using Smokey.Framework.Support.Advanced;

namespace Smokey.Internal.Rules
{		
	internal sealed class ConflictingTransparencyRule : Rule
	{				
		public ConflictingTransparencyRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "S1012")
		{
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitAssembly");
			dispatcher.Register(this, "VisitMethod");
			dispatcher.Register(this, "VisitType");
			dispatcher.Register(this, "VisitFini");
		}
		
		public void VisitAssembly(AssemblyDefinition assembly)
		{
			m_failed = false;
			m_needsCheck = assembly.CustomAttributes.Has("SecurityTransparentAttribute");
		}

		public void VisitMethod(MethodDefinition method)
		{
			if (m_needsCheck && !m_failed)
			{
				if (method.CustomAttributes.Has("SecurityCriticalAttribute"))
				{
					m_failed = true;
				}
			}
		}
		
		public void VisitType(TypeDefinition type)
		{
			if (m_needsCheck && !m_failed)
			{
				if (type.CustomAttributes.Has("SecurityCriticalAttribute"))
				{
					m_failed = true;
				}
			}
		}
		
		public void VisitFini(EndTesting end)
		{
			Unused.Value = end;
			
			if (m_failed)
			{
				Reporter.AssemblyFailed(Cache.Assembly, CheckID, string.Empty);
			}
		}
	
		private bool m_failed;
		private bool m_needsCheck;
	}
}

