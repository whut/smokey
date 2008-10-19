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
using System.Security;
using System.Collections.Generic;
using Smokey.Framework;
using Smokey.Framework.Support;

#if OLD
namespace Smokey.Internal.Rules
{		
	internal abstract class BaseAptca : Rule
	{				
		protected BaseAptca(AssemblyCache cache, IReportViolations reporter, string id) 
			: base(cache, reporter, id)
		{
		}
				
		protected bool IsFullyTrusted(AssemblyDefinition assembly)
		{
			bool fullTrust = false;
			
			if (!m_fullTrust.TryGetValue(assembly, out fullTrust))
			{
				// If the assembly is strongly named,
				if (DoIsStronglyNamed(assembly))
				{
					// does not have AllowPartiallyTrustedCallersAttribute,
					if (!assembly.CustomAttributes.Has("AllowPartiallyTrustedCallersAttribute"))
					{
						// and does not explicitly refuse full trust,
						bool refused = false;
						foreach (SecurityDeclaration sec in assembly.SecurityDeclarations)
						{	
							// TODO: need to check to see if Name == "FullTrust" but afaict
							// mono 1.2.5 doesn't generate the right metadata so we'll just
							// check for a refusal and skip the rule if we find any refusal.
							if (sec.Action == SecurityAction.RequestRefuse)
							{	
								Log.DebugLine(this, "{0} refuses full trust", assembly.Name.Name);
								refused = true;
							}
						}						
						
						// then it has full trust.
						if (!refused)
						{
							fullTrust = true;
							Log.DebugLine(this, "{0} is fully trusted", assembly.Name.Name);
						}
					}
					else
					{
						Log.DebugLine(this, "{0} is aptca", assembly.Name.Name);
					}
				}
				else
				{
					Log.DebugLine(this, "{0} is weakly named", assembly.Name.Name);
				}
				
				m_fullTrust.Add(assembly, fullTrust);
			}
			
			return fullTrust;
		}
									
		private static bool DoIsStronglyNamed(AssemblyDefinition assembly)
		{
			bool hasKey = assembly.Name.PublicKey != null && assembly.Name.PublicKey.Length > 0;
			
			bool nonZero = false;
			for (int i = 0; i < assembly.Name.PublicKey.Length && !nonZero; ++i)
			{
				nonZero = assembly.Name.PublicKey[i] != 0;
			}

			return hasKey && nonZero;			
		}
		
		private Dictionary<AssemblyDefinition, bool> m_fullTrust = new Dictionary<AssemblyDefinition, bool>();
	}
}
#endif
