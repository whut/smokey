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

#if OLD
namespace Smokey.Internal.Rules
{	
	internal sealed class SuffixNameRule : Rule
	{				
		public SuffixNameRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "MS1023")
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
				if (DoBaseFailed(type) || DoInterfaceFailed(type))
				{
					Reporter.TypeFailed(type, CheckID, string.Empty);
				}
			}
		}
		
		private bool DoBaseFailed(TypeDefinition type)
		{
			bool failed = false;
			
			if (type.BaseType != null)
				Log.DebugLine(this, "base name: {0}", type.BaseType.FullName);				

			if (type.IsSubclassOf("System.Attribute", Cache))
				failed = DoCheckSuffix(type, "Attribute");
			
			else if (type.IsSubclassOf("System.EventArgs", Cache))
				failed = DoCheckSuffix(type, "EventArgs");
			
			else if (type.IsSubclassOf("System.Exception", Cache))
				failed = DoCheckSuffix(type, "Exception");
			
			else if (type.IsSubclassOf("System.Collections.Queue", Cache))
				failed = DoCheckSuffix(type, "Collection", "Queue");
			
			else if (type.IsSubclassOf("System.Collections.Stack", Cache))
				failed = DoCheckSuffix(type, "Collection", "Stack");
			
			else if (type.IsSubclassOf("System.Data.DataSet", Cache))
				failed = DoCheckSuffix(type, "DataSet");
			
			else if (type.IsSubclassOf("System.Data.DataTable", Cache))
				failed = DoCheckSuffix(type, "Collection", "DataTable");
			
			else if (type.IsSubclassOf("System.IO.Stream", Cache))
				failed = DoCheckSuffix(type, "Stream");
									
			return failed;
		}
		
		private bool DoInterfaceFailed(TypeDefinition type)
		{
			bool failed = false;
			
			if (type.TypeOrBaseImplements("System.Collections.IDictionary", Cache))	
				failed = DoCheckSuffix(type, "Dictionary");
				
			else if (type.TypeOrBaseImplements("System.Collections.Generic.IDictionary", Cache))	
				failed = DoCheckSuffix(type, "Dictionary");
				
			else if (type.TypeOrBaseImplements("System.Collections.ICollection", Cache))	
				failed = DoCheckSuffix(type, "Collection");
				
			else if (type.TypeOrBaseImplements("System.Collections.Generic.ICollection", Cache))	
				failed = DoCheckSuffix(type, "Collection");
				
			else if (type.TypeOrBaseImplements("System.Collections.IEnumerable", Cache))	
				failed = DoCheckSuffix(type, "Collection");
				
			else if (type.TypeOrBaseImplements("System.Security.IPermission", Cache))	
				failed = DoCheckSuffix(type, "Permission");
				
			else if (type.TypeOrBaseImplements("System.Security.Policy.IMembershipCondition", Cache))	
				failed = DoCheckSuffix(type, "Condition");
							
			return failed;
		}
		
		private static bool DoCheckSuffix(TypeDefinition type, string extension)
		{
			return !type.FullName.EndsWith(extension);
		}
		
		private static bool DoCheckSuffix(TypeDefinition type, string extension1, string extension2)
		{
			return !type.FullName.EndsWith(extension1) && !type.FullName.EndsWith(extension2);
		}
	}
}
#endif
