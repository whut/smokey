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
using Smokey.Framework;
using Smokey.Framework.Support;

namespace Smokey.Internal.Rules
{	
	internal class NoObjectDisposedExceptionRule : Rule
	{				
		public NoObjectDisposedExceptionRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "MS1009")
		{
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitType");
		}
				
		// Another lame inter-procedual test.
		public void VisitType(TypeDefinition type)
		{						
			if (type.Implements("System.IDisposable"))
			{
				Log.DebugLine(this, "-----------------------------------"); 
				Log.DebugLine(this, "checking {0}", type);				
		
				int numPublicMethods = 0, numMethodsThatThrow = 0;
				m_details = string.Empty;
				
				foreach (MethodDefinition method in type.Methods)
				{
					if (DoNeedCheck(method))
					{						
						++numPublicMethods;
						if (DoHasThrow(method))
							++numMethodsThatThrow;
					}
				}
				
				if (numPublicMethods > 0 && numMethodsThatThrow == 0)
				{
					m_details = "Methods: " + m_details.Trim();
					Reporter.TypeFailed(type, CheckID, m_details);
				}
			}
		}
		
		
		// newobj   System.Void System.ObjectDisposedException::.ctor(System.String)
		private bool DoHasThrow(MethodDefinition method)
		{
			bool hasThrow = false;
			
			for (int i = 0; i < method.Body.Instructions.Count && !hasThrow; ++i)
			{
				Instruction instruction = method.Body.Instructions[i];
				
				if (instruction.OpCode.Code == Code.Newobj)
				{
					MethodReference target = (MethodReference) instruction.Operand;
					if (target.ToString().Contains("ObjectDisposedException"))
						hasThrow = true;
				}
			}
			
			if (!hasThrow)
				m_details += method + Environment.NewLine;
			
			return hasThrow;
		}
				
		private static bool DoNeedCheck(MethodDefinition method)
		{
			bool needs = false;
			
			if (!method.IsStatic && !method.IsConstructor && method.HasBody)
			{
				MethodAttributes attrs = method.Attributes;
				if ((attrs & MethodAttributes.MemberAccessMask) == MethodAttributes.Public)
				{
					if (!method.Name.Contains("Dispose") && !method.Name.Contains("Close"))
					{
						needs = true;
					}
				}
			}

			return needs;
		}

		private string m_details;
	}
}

