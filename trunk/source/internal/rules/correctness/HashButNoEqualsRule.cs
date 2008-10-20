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

using Smokey.Framework;
using Smokey.Framework.Instructions;
using Smokey.Framework.Support;
using Smokey.Framework.Support.Advanced.Values;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Smokey.Internal.Rules
{	
	internal sealed class HashButNoEqualsRule : Rule
	{				
		public HashButNoEqualsRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "C1035")
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

			// Try to find Equals and GetHashCode.
			var methods = new List<MethodInfo>();
			methods.AddRange(Cache.FindMethods(type, "Equals"));
			methods.AddRange(Cache.FindMethods(type, "GetHashCode"));
			
			// Make sure we found the correct ones.
			bool foundEquals = false;
			for (int i = 0; i < methods.Count && !foundEquals; ++i)
				if (methods[i].Method.Reuses("System.Boolean", "Equals", "System.Object"))
					foundEquals = true;

			bool foundHash = false;
			for (int i = 0; i < methods.Count && !foundHash; ++i)
			{
				if (methods[i].Method.Reuses("System.Int32", "GetHashCode"))
					foundHash = true;
			}
			
			// Check for hash but no equals.
			if (foundHash && !foundEquals)
			{
				Reporter.TypeFailed(type, CheckID, string.Empty);
			}
		}
	}
}
