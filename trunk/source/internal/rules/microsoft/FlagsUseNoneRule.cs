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
	internal class FlagsUseNoneRule : Rule
	{				
		public FlagsUseNoneRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "MS1022")
		{
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitBegin");
			dispatcher.Register(this, "VisitField");
			dispatcher.Register(this, "VisitEnd");
		}
				
		public void VisitBegin(BeginType type)
		{
			Log.DebugLine(this, "-----------------------------------"); 
			Log.DebugLine(this, "{0}", type.Type);				

			m_needsCheck = type.Type.IsFlagsEnum();
			m_foundZero = false;
			m_foundNone = false;
		}
		
		public void VisitField(FieldDefinition field)
		{			
			if (m_needsCheck && !m_foundNone && field.IsStatic)
			{
				if (field.HasConstant && field.Constant != null)
				{
					IConvertible convertible = (IConvertible) field.Constant;
					if (convertible.ToInt64(null) == 0)
					{
						Log.DebugLine(this, "found zero field named {0}", field.Name);				
						m_foundZero = true;
						m_foundNone = field.Name == "None";
					}
				}
			}
		}

		public void VisitEnd(EndType type)
		{
			if (m_needsCheck && m_foundZero && !m_foundNone)
			{
				Reporter.TypeFailed(type.Type, CheckID, string.Empty);
			}
		}
				
		private bool m_needsCheck;
		private bool m_foundZero;
		private bool m_foundNone;
	}
}

