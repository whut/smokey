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
	internal sealed class DisposableFieldsRule : Rule
	{				
		public DisposableFieldsRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "R1000")
		{
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitBegin");
			dispatcher.Register(this, "VisitField");
			dispatcher.Register(this, "VisitEnd");
		}
				
		public void VisitBegin(BeginType begin)
		{
			Log.DebugLine(this, "-----------------------------------"); 
			Log.DebugLine(this, "{0}", begin.Type);				

			m_type = begin.Type;
			m_ownsField = false;
			m_disposable = IsDisposable.Type(Cache, begin.Type);
			m_details = string.Empty;
		}
		
		public void VisitField(FieldDefinition field)
		{			
			if (!m_disposable && !field.IsStatic)
			{
				if (IsDisposable.Type(Cache, field.FieldType))
				{
					Log.DebugLine(this, "found a disposable field: {0}", field.Name);				
					if (!m_ownsField)
					{
						if (field.IsOwnedBy(m_type))
						{
							m_ownsField = true;
							Log.DebugLine(this, "{0} is disposable and owns {1}", m_type, field.Name);
				
							m_details = string.Format("{0}{1} ", m_details, field.Name);
						}
					}
				}
			}
		}

		public void VisitEnd(EndType end)
		{
			if (!m_disposable && m_ownsField && m_details.Length > 0)
				Reporter.TypeFailed(end.Type, CheckID, "Field: " + m_details);
		}
		
		private TypeDefinition m_type;
		private bool m_disposable;
		private bool m_ownsField;
		private string m_details;
	}
}

