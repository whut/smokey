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
	internal sealed class VisibleFieldsRule : Rule
	{				
		public VisibleFieldsRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "MS1018")
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

			m_needsCheck = !begin.Type.IsEnum && DoIsVisible(begin.Type);
			m_visible.Clear();
		}
		
		public void VisitField(FieldDefinition field)
		{			
			if (m_needsCheck)
			{
				if (DoIsVisible(field) && !DoIsConst(field))
				{
					m_visible.Add(field.Name);
					Log.DebugLine(this, "{0} is visible", field.Name);
				}
			}
		}

		public void VisitEnd(EndType end)
		{
			if (m_visible.Count > 0)
			{
				string details;
				if (m_visible.Count == 1)
					details = m_visible[0] + " is visible";
				else
					details = string.Join(", ", m_visible.ToArray()) + " are visible";
				
				Reporter.TypeFailed(end.Type, CheckID, details);
			}
		}

		#region Private methods
		private bool DoIsVisible(TypeDefinition type)
		{
			bool visible = type.ExternallyVisible(Cache);
			Log.DebugLine(this, "{0}.visible = {1}", type.Name, visible);
			
			return visible;
		}

		private bool DoIsVisible(FieldDefinition field)
		{
			bool visible = false;
			
			switch (field.Attributes & FieldAttributes.FieldAccessMask)
			{
				case FieldAttributes.Public:
				case FieldAttributes.Family:
				case FieldAttributes.FamORAssem:
					visible = true;
					break;
			}
			Log.DebugLine(this, "{0}.visible = {1}, attribute = {2}", field.Name, visible, field.Attributes & FieldAttributes.FieldAccessMask);
			
			return visible;
		}

		private bool DoIsConst(FieldDefinition field)
		{
			bool cnst = false;
			
			if ((field.Attributes & FieldAttributes.Literal) == FieldAttributes.Literal)
				cnst = true;
			
			else if ((field.Attributes & FieldAttributes.InitOnly) == FieldAttributes.InitOnly)
				cnst = true;
			
			Log.DebugLine(this, "{0}.constant = {1}", field.Name, cnst);
			
			return cnst;
		}
		#endregion
		
		private bool m_needsCheck;
		private List<string> m_visible = new List<string>();
	}
}
#endif
