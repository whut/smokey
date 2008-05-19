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
	internal sealed class Aptca1Rule : BaseAptca
	{				
		public Aptca1Rule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "S1000")
		{
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitAssembly");
			dispatcher.Register(this, "VisitBegin");
			dispatcher.Register(this, "VisitCall");
			dispatcher.Register(this, "VisitEnd");
			dispatcher.Register(this, "VisitFini");
		}
		
		public void VisitAssembly(AssemblyDefinition assembly)
		{
			m_needsCheck = assembly.CustomAttributes.Has("AllowPartiallyTrustedCallersAttribute");
			Log.DebugLine(this, "needs check = {0}", m_needsCheck);
		}
		
		public void VisitBegin(BeginMethod begin)
		{
			if (m_needsCheck)
			{
				Log.DebugLine(this, "checking {0}", begin.Info.Method);
	
				m_checkMethod = true;
				m_type = begin.Info.Type;
				m_candidates.Clear();
				
				foreach (SecurityDeclaration sec in begin.Info.Method.SecurityDeclarations)
				{	
					if (sec.Action == SecurityAction.Demand || sec.Action == SecurityAction.NonCasDemand)
					{	
						if (sec.PermissionSet.IsUnrestricted())
						{
							Log.DebugLine(this, "   demands full trust");
							m_checkMethod = false;
						}
					}
				}
			}
		}
		
		public void VisitCall(Call call)
		{
			// If we allow partially trusted callers,
			if (m_checkMethod)
			{
				// and we're calling into a different assembly,
				ModuleDefinition module = call.Target.DeclaringType.Module;	// note that this is the current assemblies module
				if (module.MemberReferences.Contains(call.Target))
				{
					// and the target is not a base method (if it is APTCA2 will handle it),
					if (!m_type.IsSubclassOf(call.Target.DeclaringType, Cache))
					{
						// and that assembly requires full trust,
						AssemblyDefinition assembly = Cache.FindAssembly(call.Target.DeclaringType);
						if (assembly != null)
						{
							if (IsFullyTrusted(assembly))
							{
								// then we have a potential problem.
								string name = call.Target.ToString();
								if (!m_candidates.Contains(name))
									m_candidates.Add(name);
							}
						}
						else
						{
							Log.DebugLine(this, "{0} calls into an unknown assembly", call.Target.Name);
						}
					}
				}
				
				// If we're programmatically demanding permissions then assume the code
				// is OK (technically we should also check to see which permission is
				// being demanded).
				if (call.Target.ToString() == "System.Void System.Security.PermissionSet::Demand()")
				{
					Log.DebugLine(this, "   found permission demand");
					m_checkMethod = false;
				}
			}
		}
		
		public void VisitEnd(EndMethod end)
		{
			if (m_checkMethod)
			{
				foreach (string name in m_candidates)
				{
					if (!m_bad.Contains(name))
					{
						Log.DebugLine(this, "   bad call: {0}", name);
						m_bad.Add(name);
					}
				}
			}
		}
		
		public void VisitFini(EndTesting end)
		{
			if (m_bad.Count > 0)
			{
				string details = "Bad Calls: " + string.Join(Environment.NewLine, m_bad.ToArray());
				Log.DebugLine(this, details);
				
				Reporter.AssemblyFailed(Cache.Assembly, CheckID, details);
			}
		}
											
		private bool m_needsCheck;
		private bool m_checkMethod;
		private TypeDefinition m_type;
		private List<string> m_bad = new List<string>();
		private List<string> m_candidates = new List<string>();
	}
}

