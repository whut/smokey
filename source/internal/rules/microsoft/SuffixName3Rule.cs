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
	internal sealed class SuffixName3Rule : Rule
	{				
		public SuffixName3Rule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "MS1025")
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
		
			if (DoSuffixFailed(type))
			{
				Reporter.TypeFailed(type, CheckID, string.Empty);
			}
		}
		
		private bool DoSuffixFailed(TypeDefinition type)
		{
			bool failed = false;
			
			if (DoCheckSuffix(type, "Delegate"))
				failed = true;
				
			else if (DoCheckSuffix(type, "Enum"))
				failed = true;
				
			else if (DoCheckSuffix(type, "Flags"))
				failed = type.IsSubclassOf("System.Enum", Cache);
				
			else if (DoCheckSuffix(type, "Impl"))
				failed = true;

			return failed;
		}
				
		private static bool DoCheckSuffix(TypeDefinition type, string extension)
		{
			return type.FullName.EndsWith(extension);
		}
	}
}

