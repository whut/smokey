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
using Smokey.Framework;
using Smokey.Framework.Support;

namespace Smokey.Internal.Rules
{		
	// Based on <http://www.mono-project.com/Coding_Guidelines>.
	internal sealed class MonoNamingRule : Rule
	{				
		public MonoNamingRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "MO1000")
		{
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
#if !TEST
			string convention = Settings.Get("naming", "mono");
			if (convention == "mono")
#endif
			{
				dispatcher.Register(this, "VisitBegin");
				dispatcher.Register(this, "VisitType");
				dispatcher.Register(this, "VisitMethod");
				dispatcher.Register(this, "VisitField");
//				dispatcher.Register(this, "VisitEvent");
				dispatcher.Register(this, "VisitProperty");
				dispatcher.Register(this, "VisitEnd");
			}
		}
				
		public void VisitBegin(BeginType arg)
		{			
			Log.DebugLine(this, "-----------------------------------"); 
			Log.DebugLine(this, "checking {0}", arg.Type);				

			m_details = string.Empty;
			m_type = arg.Type;
			m_needsCheck = false;
			
//			if (!m_type.FullName.Contains("PrivateImplementationDetails"))
				if (!m_type.Name.StartsWith("yy") && !m_type.IsCompilerGenerated())
					m_needsCheck = true;
		}
		
		public void VisitType(TypeDefinition type)
		{
			if (m_needsCheck)
			{
				// Type name must be PascalCase.
				if (!NamingUtils.IsPascalCase(type.Name))
				{
					m_details += "Type: " + type.Name + ". ";
				}
				
				if (type.IsInterface)
				{
					if (type.Name[0] != 'I')
					{
						m_details += "Interface: " + type.Name + ". ";
					}
				}
			}
		}

		public void VisitMethod(MethodDefinition method)	
		{
			if (!m_needsCheck || method.Name.StartsWith("yy"))
//			if (!m_needsCheck || method.Name.StartsWith("yy") || method.IsCompilerGenerated() || method.ToString().Contains("AnonymousMethod"))
				return;
			
			// Method name must be PascalCase.
			if (!method.IsSpecialName && !NamingUtils.IsPascalCase(method.Name))
			{
				m_details += "Method: " + method.Name + ". ";
			}
			
			// Parameters must be camelCase.
			foreach (ParameterDefinition param in method.Parameters)
			{
				if (!NamingUtils.IsCamelCase(param.Name))
				{
					m_details = string.Format("{0}Argument: {1}. ", m_details, param.Name);
				}
			}
		}

		public void VisitField(FieldDefinition field)
		{
			if (!m_needsCheck)
				return;
			
			// Protected and private fields must be divided_by_underscores. 
			if ((field.Attributes & FieldAttributes.FieldAccessMask) != FieldAttributes.Public)
			{
				if ((m_type.Attributes & TypeAttributes.Serializable) != TypeAttributes.Serializable)
				{
					if ((field.Attributes & FieldAttributes.Literal) != FieldAttributes.Literal)
					{
						if (field.FieldType.FullName.Contains("System.EventHandler"))	// events are special
						{
							if (!NamingUtils.IsPascalCase(field.Name))
							{
								m_details = string.Format("{0}Event: {1}. ", m_details, field.Name);
								Log.DebugLine(this, "event field {0} isn't a pascal name", field.Name); 
							}
						}
						else if (!NamingUtils.IsUnderScoreName(field.Name))
						{
							m_details = string.Format("{0}Field: {1}. ", m_details, field.Name);
							Log.DebugLine(this, "field {0} isn't an underscore name", field.Name); 
						}
						
						// m_my_name and the like are not legit
						if (m_details.Length == 0 && field.Name.Length > 2)
						{
							if (field.Name[0] == 'm' && field.Name[1] == '_')
							{
								m_details = string.Format("{0}Field: {1}. ", m_details, field.Name);
								Log.DebugLine(this, "field {0} uses m_", field.Name); 
							}
							else if (field.Name[0] == 's' && field.Name[1] == '_')
							{
								m_details = string.Format("{0}Field: {1}. ", m_details, field.Name);
								Log.DebugLine(this, "field {0} uses s_", field.Name); 
							}
							else if (field.Name[0] == 'm' && field.Name[1] == 's' && field.Name[2] == '_')
							{
								m_details = string.Format("{0}Field: {1}. ", m_details, field.Name);
								Log.DebugLine(this, "field {0} uses ms_", field.Name); 
							}
						}
					}
					else
					{
						// except for constants
						if (!NamingUtils.IsPascalCase(field.Name))
						{
							m_details = string.Format("{0}Constant: {1}. ", m_details, field.Name);
							Log.DebugLine(this, "constant field {0} isn't pascal case", field.Name); 
						}
					}
				}
			}
		}

		public static void VisitEvent(EventDefinition evt)	
		{
			// mono has no standards for these
		}

		public void VisitProperty(PropertyDefinition prop)	
		{
			if (!m_needsCheck)
				return;
			
			if (!NamingUtils.IsPascalCase(prop.Name))
				m_details = string.Format("{0}Property: {1}. ", m_details, prop.Name);
		}

		public void VisitEnd(EndType type)
		{
			if (m_needsCheck && m_details.Length > 0)
			{
				Log.DebugLine(this, m_details);
				Reporter.TypeFailed(type.Type, CheckID, m_details);
			}
		}
		
		private string m_details;
		private TypeDefinition m_type;
		private bool m_needsCheck;
	}
}

