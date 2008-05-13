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
	internal class StringUri1Rule : Rule
	{				
		public StringUri1Rule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "R1031")
		{
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitBegin");
		}
				
		public void VisitBegin(BeginType begin)
		{
			Log.DebugLine(this, "-----------------------------------"); 
			Log.DebugLine(this, "{0}", begin.Type);				
			
			foreach (MethodDefinition method in begin.Type.Methods)
			{
				int index = DoGetStringArg(method);
				if (index >= 0)
				{
					Log.DebugLine(this, "{0} has a string uri arg at {1}", method.Name, index);

					MethodDefinition uri = DoFindUri(begin.Type, method, index);
					Log.DebugLine(this, "has matching Uri: {0}", uri != null);
					if (uri == null)
					{						
						string details = "Method: " + method.Name;
						Reporter.TypeFailed(begin.Type, CheckID, details);
						break;
					}
				}
			}
		}
				
		private int DoGetStringArg(MethodDefinition method)
		{
			for (int index = 0; index < method.Parameters.Count; ++index)
			{
				ParameterDefinition p = method.Parameters[index];
				
				if (p.ParameterType.FullName == "System.String")
				{
					string[] parts = p.Name.CapsSplit();
					Log.DebugLine(this, "   parts: {0}", string.Join("-", parts));				

					if (parts.Intersect(m_bad).Count() > 0)
					{
						return index;
					}
				}
			}
			
			return -1;
		}
		
		private MethodDefinition DoFindUri(TypeDefinition type, MethodDefinition str, int index)
		{
			foreach (MethodDefinition candidate in type.Methods)
			{
				if (candidate != str)
				{
					if (candidate.Name == str.Name && candidate.Parameters.Count == str.Parameters.Count)
					{
						bool match = true;
	
						for (int i = 0; i < candidate.Parameters.Count && match; ++i)
						{
							ParameterDefinition ps = candidate.Parameters[i];
							
							if (i == index)
							{
								match = ps.ParameterType.FullName == "System.Uri";
							}
							else
							{
								ParameterDefinition pu = str.Parameters[i];
								match = ps.ParameterType.FullName == pu.ParameterType.FullName;
							}
						}
						
						if (match)
							return candidate;
					}
				}
			}
			
			return null;
		}
		
		private string[] m_bad = new[] {"uri", "Uri", "urn", "Urn", "url", "Url"};
	}
}

