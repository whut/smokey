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
using Smokey.Framework.Instructions;
using Smokey.Framework.Support;
using Smokey.Framework.Support.Advanced;

namespace Smokey.Internal.Rules
{	
	internal sealed class PublicTypeRule : Rule
	{				
		public PublicTypeRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "D1031")
		{
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitType");
			dispatcher.Register(this, "VisitFini");
		}
		
		public void VisitType(TypeDefinition type)
		{
			TypeAttributes vis = type.Attributes & TypeAttributes.VisibilityMask;
			if (vis == TypeAttributes.Public)
			{
				if (m_details.Length == 0)
					m_details = type.FullName;
				else
					m_details += ", " + type.FullName;					
			}
		}
		
		public void VisitFini(EndTesting end)
		{
			if (Cache.Assembly.EntryPoint != null)
			{
				Log.DebugLine(this, "entry point: {0}", Cache.Assembly.EntryPoint);

				if (m_details.Length != 0)
				{
					m_details = "Public types: " + m_details;
					
					Log.DebugLine(this, m_details);
					Reporter.AssemblyFailed(Cache.Assembly, CheckID, m_details);
				}
			}
		}
		
		private string m_details = string.Empty;
	}
}

