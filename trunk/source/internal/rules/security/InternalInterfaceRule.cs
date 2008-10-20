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
	internal sealed class InternalInterfaceRule : Rule
	{				
		public InternalInterfaceRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "S1009")
		{
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitBegin");
		}
						
		public void VisitBegin(BeginMethod begin)
		{
			Log.DebugLine(this, "checking {0}", begin.Info.Method);

			TypeReference type = begin.Info.Method.GetDeclaredIn(Cache);
			if (type != null)
			{
				TypeDefinition td = Cache.FindType(type);
				if (td != null && td.IsInterface && !td.IsPublic && !td.IsNestedPublic)
				{
					Log.DebugLine(this, "   implements {0}", type);
					
					if (begin.Info.Type.ExternallyVisible(Cache))
					{
						Log.DebugLine(this, "   declaring type is externally visible");
						
						if (begin.Info.Method.IsVirtual && !begin.Info.Method.IsFinal)
						{
							Log.DebugLine(this, "   is virtual");
							
							if (DoHasPublicCtor(begin.Info.Type))
							{
								Log.DebugLine(this, "   has public ctor");
								
								string details = "Interface: " + type.FullName;
								Reporter.MethodFailed(begin.Info.Method, CheckID, 0, details);
							}
						}
					}
				}
			}
		}
		
		private static bool DoHasPublicCtor(TypeDefinition type)
		{
			foreach (MethodDefinition method in type.Constructors)
			{
				MethodAttributes attrs = method.Attributes & MethodAttributes.MemberAccessMask;
				if (attrs == MethodAttributes.Public)	// can't make an implementation protected so we only need to check for public
					return true;
			}
			
			return false;
		}
	}
}

