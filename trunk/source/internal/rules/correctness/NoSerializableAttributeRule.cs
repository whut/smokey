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
using System.Diagnostics;
using Smokey.Framework;
using Smokey.Framework.Instructions;
using Smokey.Framework.Support;

namespace Smokey.Internal.Rules
{	
	internal sealed class NoSerializableAttributeRule : Rule
	{				
		public NoSerializableAttributeRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "C1006")
		{
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitType");
		}
				
		public void VisitType(TypeDefinition type)
		{
			TypeAttributes vis = type.Attributes & TypeAttributes.VisibilityMask;
			if (vis == TypeAttributes.Public || vis == TypeAttributes.NestedPublic || vis == TypeAttributes.NestedFamily || vis == TypeAttributes.NestedFamORAssem)
			{
				if (type.Implements("System.Runtime.Serialization.ISerializable"))
				{
					Log.DebugLine(this, "-----------------------------------"); 
					Log.DebugLine(this, "{0}", type);
					
					if ((type.Attributes & TypeAttributes.Serializable) == 0)
					{
						Log.DebugLine(this, "no SerializeAttribute"); 
						Reporter.TypeFailed(type, CheckID, string.Empty);
					}
				}
			}
		}


		public void VisitBegin(BeginMethod method)
		{
			m_needsCheck = Aspell.Instance != null;
			m_offset = -1;
			m_details = string.Empty;
			
			if (m_needsCheck)
			{
				Log.DebugLine(this, "-----------------------------------"); 
				Log.DebugLine(this, "{0:F}", method.Info.Instructions);				
			}
		}
		
		public void VisitLoad(LoadString load)
		{
			if (m_needsCheck)
			{
				if (CheckSpelling.Text(load.Value, ref m_details))
				{
					if (m_offset < 0)
						m_offset = load.Untyped.Offset;						
				}
			}
		}

		public void VisitEnd(EndMethod method)
		{
			if (m_needsCheck && m_offset >= 0)
			{
				m_details = "Words: " + m_details;
				Log.DebugLine(this, m_details); 
				Reporter.MethodFailed(method.Info.Method, CheckID, m_offset, m_details);
			}
		}
				
		private int m_offset;
		private bool m_needsCheck;
		private string m_details;
	}
}
