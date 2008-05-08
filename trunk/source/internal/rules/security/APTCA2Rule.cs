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
	internal class Aptca2Rule : BaseAptca
	{				
		public Aptca2Rule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "S1001")
		{
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitAssembly");
			dispatcher.Register(this, "VisitType");
		}
		
		public void VisitAssembly(AssemblyDefinition assembly)
		{
			m_needsCheck = assembly.CustomAttributes.Has("AllowPartiallyTrustedCallersAttribute");
			Log.DebugLine(this, "needs check = {0}", m_needsCheck);
		}
		
		public void VisitType(TypeDefinition type)
		{
			// If we allow partially trusted callers,
			if (m_needsCheck)
			{
				foreach (SecurityDeclaration sec in type.SecurityDeclarations)
				{	
					if (sec.Action == SecurityAction.Demand || sec.Action == SecurityAction.NonCasDemand)
					{	
						if (sec.PermissionSet.IsUnrestricted())
						{
							Log.DebugLine(this, "   demands full trust");
							return;
						}
					}
				}

				Log.DebugLine(this, "checking {0}", type);

				// and the type derives from a type in another assembly,
				TypeReference super = Cache.FindBase(type);
				while (super != null)
				{
					Log.DebugLine(this, "checking {0}/{1}", type, super);

					ModuleDefinition module = super.Module;			// note that this is the current assemblies module
					if (module.TypeReferences.Contains(super))
					{
						Log.DebugLine(this, "   {0} is in another assembly", super);
						
						// and that assembly requires full trust,
						AssemblyDefinition assembly = Cache.FindAssembly(super);
						if (assembly != null)
						{
							if (IsFullyTrusted(assembly))
							{
								// then we have a potential problem.
								string details = string.Format("{0} is defined in a fully trusted assembly", super.FullName);
								Log.DebugLine(this, details);
								Reporter.TypeFailed(type, CheckID, details);
								return;
							}
						}
						else
						{
							Log.DebugLine(this, "{0} is defined in an unknown assembly", super.FullName);
						}
					}
					
					super = Cache.FindBase(super);
				}				
			}	
		}
		
		private bool m_needsCheck;
	}
}

