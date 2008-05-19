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
using Smokey.Framework.Instructions;
using Smokey.Framework.Support;

namespace Smokey.Internal.Rules
{	
	internal sealed class InternalExceptionRule : Rule
	{				
		public InternalExceptionRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "D1012")
		{
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitType");
		}
		
		public void VisitType(TypeDefinition type)
		{
			if (Cache.HasPublicTypes)
			{
				Log.DebugLine(this, "-----------------------------------"); 
				Log.DebugLine(this, "{0}", type);	
				
				TypeAttributes visibility = type.Attributes & TypeAttributes.VisibilityMask;
				if (visibility != TypeAttributes.Public && visibility != TypeAttributes.NestedPublic)
				{
					Log.DebugLine(this, "not public: {0}", visibility);	
				
					if (type.BaseType != null)
					{
						string name = type.BaseType.ToString();
						if (name == "System.Exception" || name == "System.SystemException" || name == "System.ApplicationException")
						{
							Log.DebugLine(this, "exception base: {0}", name);	
							Reporter.TypeFailed(type, CheckID, string.Empty);
						}
					}
				}
			}
		}
	}
}

