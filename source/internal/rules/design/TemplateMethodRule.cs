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
using Mono.Cecil.Metadata;
using Smokey.Framework;
using Smokey.Framework.Instructions;
using Smokey.Framework.Support;
using System;
using System.Collections.Generic;

namespace Smokey.Internal.Rules
{	
	internal sealed class TemplateMethodRule : Rule
	{				
		public TemplateMethodRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "D1043")
		{
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitBegin");
			dispatcher.Register(this, "VisitCall");
			dispatcher.Register(this, "VisitEnd");
		}
		
		public void VisitBegin(BeginMethod begin)
		{
			Log.DebugLine(this, "-----------------------------------"); 
			Log.DebugLine(this, "{0:F}", begin.Info.Instructions);				

			m_numVirtuals = 0.0f;
			m_needsCheck = false;
			m_type = begin.Info.Type;
			m_virtuals.Clear();

			MethodDefinition method = begin.Info.Method;	
			if (method.IsVirtual && !method.IsFinal && method.IsNewSlot)
				if (!m_type.IsSealed)
					if (method.Body != null)
						if (method.Body.Instructions.Count >= m_limit/4)
							m_needsCheck = true;
			
			if (m_needsCheck)
				foreach (MethodDefinition m in m_type.Methods)
					if (m.IsVirtual && !m.IsFinal)
						m_virtuals.Add(m.MetadataToken);

			if (method.Body != null)
				Log.DebugLine(this, "numInstructions: {0}", method.Body.Instructions.Count);				
		}
		
		public void VisitCall(Call call)
		{				
			if (m_needsCheck)
			{
				if (call.Target.DeclaringType.MetadataToken == m_type.MetadataToken)
				{
					if (m_virtuals.IndexOf(call.Target.MetadataToken) >= 0)
					{
						Log.DebugLine(this, "found a self virtual at {0:X2}", call.Untyped.Offset);	
						m_numVirtuals += 1.0f;
					}
				}
			}
		}
				
		public void VisitEnd(EndMethod end)
		{			
			if (m_needsCheck)
			{
				float ratio = end.Info.Method.Body.Instructions.Count;
				if (m_numVirtuals > 0)
					ratio /= m_numVirtuals;
					
				if (ratio >= m_limit)		
				{
					Log.DebugLine(this, "numVirtuals: {0}", (int) m_numVirtuals);				
					Log.DebugLine(this, "ratio: {0}", ratio);				
					Reporter.MethodFailed(end.Info.Method, CheckID, 0, string.Empty);
				}
			}
		}
		
		private bool m_needsCheck;
		private TypeDefinition m_type;
		private float m_numVirtuals;
		private List<MetadataToken> m_virtuals = new List<MetadataToken>();
		private int m_limit = 120;	// increase to make fewer methods fail
	}
}

