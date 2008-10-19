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

#if OLD
namespace Smokey.Internal.Rules
{	
	internal sealed class NonSerializableFieldRule : Rule
	{				
		public NonSerializableFieldRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "R1020")
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
			m_needsCheck = false;
			m_hasBadField = false;
			m_details = string.Empty;
			
			if ((begin.Type.Attributes & TypeAttributes.Serializable) == TypeAttributes.Serializable)
			{
				if (!begin.Type.TypeOrBaseImplements("System.Runtime.Serialization.ISerializable", Cache))
				{
					m_needsCheck = true;

					Log.DebugLine(this, "-----------------------------------"); 
					Log.DebugLine(this, "checking {0}", begin.Type);	
				}
			}
		}
		
		public void VisitField(FieldDefinition field)
		{						
			if (m_needsCheck && !m_hasBadField)
			{
				TypeDefinition type = Cache.FindType(field.FieldType);
				if (type != null && type.BaseType != null && type.BaseType.FullName != "System.Enum")		// enums are serializable tho they are not marked as such
				{
					if ((type.Attributes & TypeAttributes.Serializable) == 0)
					{
						Log.DebugLine(this, "{0} field type isn't serializable", field.Name); 
						if ((field.Attributes & FieldAttributes.NotSerialized) == 0)
						{
							Log.DebugLine(this, "and the field isn't marked as nonserializable"); 
							m_details = "Field: " + field.Name;
							m_hasBadField = true;
						}
					}
				}
			}
		}

		public void VisitEnd(EndType end)
		{			
			if (m_needsCheck && m_hasBadField)
			{
				Reporter.TypeFailed(end.Type, CheckID, m_details);
			}
		}
		
		private bool m_needsCheck;
		private bool m_hasBadField;
		private string m_details;
	}
}
#endif
