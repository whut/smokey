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
using Smokey.Framework;
using Smokey.Framework.Support;

#if OLD
namespace Smokey.Internal.Rules
{	
	internal sealed class PreferMonitor1Rule : Rule
	{				
		public PreferMonitor1Rule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "D1052")
		{
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitType");
		}
				
		public void VisitType(TypeDefinition type)
		{		
			FieldDefinition field = DoGetBadField(type);
			
			if (field != null)
			{
				string details = "Field: " + field.Name;
				Reporter.TypeFailed(type, CheckID, details);
			}
		}

		private FieldDefinition DoGetBadField(TypeDefinition type)
		{
			foreach (FieldDefinition field in type.Fields)
			{
				Log.DebugLine(this, "{0} is of type {1}", field.Name, field.FieldType);
			
				switch (field.FieldType.FullName)
				{
					case "System.Threading.AutoResetEvent":
					case "System.Threading.ManualResetEvent":
					case "System.Threading.Semaphore":
						return field;
				}
			}
			
			return null;
		}
	}
}
#endif
