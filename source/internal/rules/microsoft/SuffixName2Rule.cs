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
	internal sealed class SuffixName2Rule : Rule
	{				
		public SuffixName2Rule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "MS1024")
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
		
			if (DoTypeFailed(type) || DoEventHandlerFailed(type))
			{
				Reporter.TypeFailed(type, CheckID, string.Empty);
			}
		}
		
		private bool DoTypeFailed(TypeDefinition type)
		{
			bool failed = false;
			
			if (DoCheckSuffix(type, "Attribute"))
				failed = !type.IsSubclassOf("System.Attribute", Cache);
				
			else if (DoCheckSuffix(type, "Queue"))
				failed = !type.IsSubclassOf("System.Collections.Queue", Cache);

			else if (DoCheckSuffix(type, "Dictionary"))
				failed = !type.ClassOrBaseImplements("System.Collections.IDictionary", Cache);

			else if (DoCheckSuffix(type, "Stack"))
				failed = !type.IsSubclassOf("System.Collections.Stack", Cache);				

			else if (DoCheckSuffix(type, "Collection"))
				failed = !type.IsSubclassOf("System.Collections.Queue", Cache) &&
					!type.IsSubclassOf("System.Collections.Stack", Cache) &&
					!type.IsSubclassOf("System.Data.DataSet", Cache) &&
					!type.IsSubclassOf("System.Data.DataTable", Cache) &&
					!type.ClassOrBaseImplements("System.Collections.ICollection", Cache) &&
					!type.ClassOrBaseImplements("System.Collections.IEnumerable", Cache) &&
					!type.ClassOrBaseImplements("System.Collections.Generic.ICollection", Cache);
				
			else if (DoCheckSuffix(type, "EventArgs"))
				failed = !type.IsSubclassOf("System.EventArgs", Cache);
				
			else if (DoCheckSuffix(type, "Exception"))
				failed = !type.IsSubclassOf("System.Exception", Cache);
				
			else if (DoCheckSuffix(type, "Stream"))
				failed = !type.IsSubclassOf("System.IO.Stream", Cache);				
													
			else if (DoCheckSuffix(type, "Permission"))
				failed = !type.ClassOrBaseImplements("System.Security.IPermission", Cache);

			return failed;
		}
				
		private bool DoEventHandlerFailed(TypeDefinition type)
		{
			bool failed = false;
			
			if (DoCheckSuffix(type, "EventHandler"))
			{
				MethodDefinition[] methods = type.Methods.GetMethod("Invoke");
				if (methods.Length == 1)
				{
					MethodDefinition method = methods[0];						
					if (method.Parameters.Count == 2)
					{
						ParameterDefinition arg2 = method.Parameters[1];
						if (!arg2.ParameterType.IsSameOrSubclassOf("System.EventArgs", Cache))
							failed = true;
					}
					else
						failed = true;
				}
				else
					failed = true;
			}

			return failed;
		}
				
		private static bool DoCheckSuffix(TypeDefinition type, string extension)
		{
			return type.FullName.EndsWith(extension);
		}
	}
}

