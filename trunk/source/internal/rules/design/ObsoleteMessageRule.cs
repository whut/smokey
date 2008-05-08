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

namespace Smokey.Internal.Rules
{	
	internal class ObsoleteMessageRule : Rule
	{				
		public ObsoleteMessageRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "D1022")
		{
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitType");
			dispatcher.Register(this, "VisitProp");
			dispatcher.Register(this, "VisitMethod");
		}
				
		public void VisitType(TypeDefinition type)
		{			
			if (type.CustomAttributes.Count > 0)
			{
				Log.DebugLine(this, "-----------------------------------"); 
				Log.DebugLine(this, "{0}", type);				
		
				CustomAttribute attr = DoFindObsolete(type.CustomAttributes);
				if (attr != null)
				{
					if (attr.Constructor.Parameters.Count == 0)
					{
						Reporter.TypeFailed(type, CheckID, string.Empty);
					}
				}
			}
		}
		
		public void VisitProp(PropertyDefinition prop)
		{			
			if (prop.CustomAttributes.Count > 0)
			{
				Log.DebugLine(this, "-----------------------------------"); 
				Log.DebugLine(this, "{0}", prop);				
		
				CustomAttribute attr = DoFindObsolete(prop.CustomAttributes);
				if (attr != null)
				{
					Log.DebugLine(this, "attr.Params.Count: {0}", attr.Constructor.Parameters.Count);
					if (attr.Constructor.Parameters.Count == 0)
					{
						Reporter.MethodFailed(prop.GetMethod, CheckID, 0, string.Empty);
					}
				}
			}
		}
		
		public void VisitMethod(MethodDefinition method)
		{			
			if (method.CustomAttributes.Count > 0)
			{
				Log.DebugLine(this, "-----------------------------------"); 
				Log.DebugLine(this, "{0}", method);				
		
				CustomAttribute attr = DoFindObsolete(method.CustomAttributes);
				if (attr != null)
				{
					Log.DebugLine(this, "attr.Params.Count: {0}", attr.Constructor.Parameters.Count);
					if (attr.Constructor.Parameters.Count == 0)
					{
						Reporter.MethodFailed(method, CheckID, 0, string.Empty);
					}
				}
			}
		}
		
		private CustomAttribute DoFindObsolete(CustomAttributeCollection attrs)
		{
			foreach (CustomAttribute attr in attrs)
			{
				Log.DebugLine(this, "attr.Name: {0}", attr.Constructor.DeclaringType.Name);
				if (attr.Constructor.DeclaringType.Name ==  "ObsoleteAttribute")
					return attr;
			}
			
			return null;
		}
	}
}

