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
	internal sealed class ClassCanBeMadeStaticRule : Rule
	{				
		public ClassCanBeMadeStaticRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "D1001")
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

			if (!type.IsCompilerGenerated())
			{
				if (!type.IsAbstract)
//				if (!type.IsAbstract && !type.FullName.Contains("PrivateImplementationDetails"))
				{
					if (type.BaseType != null && type.BaseType.FullName == "System.Object")
					{
						if (DoHasNoVirtuals(type) && DoAllFieldsAreStatic(type))
						{
							Log.DebugLine(this, "cab be made static"); 
							Reporter.TypeFailed(type, CheckID, string.Empty);
						}
					}
				}
			}
		}
		
		private static bool DoHasNoVirtuals(TypeDefinition type)
		{
			if (type.Methods.Count - type.Constructors.Count < 0)	// ignore classes with no methods
				return false;
				
			foreach (MethodDefinition method in type.Methods)
			{
				if (method.IsVirtual && method.IsNewSlot)
					return false;
					
				if (method.Name == "Finalize")
					return false;
			}
			
			return true;
		}
		
		private static bool DoAllFieldsAreStatic(TypeDefinition type)
		{			
			foreach (FieldDefinition field in type.Fields)
			{
				if (!field.IsStatic)
					return false;
			}
						
			return true;
		}
	}
}

