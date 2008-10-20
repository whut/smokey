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
using Smokey.Framework.Instructions;
using Smokey.Framework.Support;

namespace Smokey.Internal.Rules
{		
	internal class NetNamingRule : Rule
	{				
		public NetNamingRule(AssemblyCache cache, IReportViolations reporter, string checkID) 
			: base(cache, reporter, checkID)
		{
		}
				
		public NetNamingRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "MS1017")
		{
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
#if !TEST
			string convention = Settings.Get("naming", "mono");
			if (convention == "net")
#endif
			{
				dispatcher.Register(this, "VisitBegin");
				dispatcher.Register(this, "VisitType");
				dispatcher.Register(this, "VisitMethod");
				dispatcher.Register(this, "VisitField");
				dispatcher.Register(this, "VisitEvent");
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
		
		[DisableRule("D1042", "IdenticalMethods")]		// TODO: matches MonoNaming
		public void VisitType(TypeDefinition type)
		{
			if (type.Name.Length >= 4)				// weird generated types like P1`1
				if (char.IsLetter(type.Name[0]) && char.IsDigit(type.Name[1]) && type.Name[2] == '`' && char.IsDigit(type.Name[3]))
					return;
			
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
			
			if ((method.Attributes & MethodAttributes.PInvokeImpl) == MethodAttributes.PInvokeImpl)
				return;
			
			if (method.Name.StartsWith("<"))
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
				
			if (field.Name.IndexOf("<") >= 0)	// auto-property
				return;
			
			if (field.Name[0] == '$')
				return;
			
			// Protected and private fields must be camelCase. Public and internal fields must 
			// be PascalCase.
			if ((m_type.Attributes & TypeAttributes.Serializable) != TypeAttributes.Serializable)
			{
				if ((field.Attributes & FieldAttributes.FieldAccessMask) == FieldAttributes.Public)
				{
					if (!NamingUtils.IsPascalCase(field.Name))
					{
						m_details = string.Format("{0}Public Field: {1}. ", m_details, field.Name);
					}
				}
				else if ((field.Attributes & FieldAttributes.FieldAccessMask) == FieldAttributes.Assembly)
				{
					if (!NamingUtils.IsPascalCase(field.Name))
					{
						m_details = string.Format("{0}Internal Field: {1}. ", m_details, field.Name);
					}
				}
				else if ((field.Attributes & FieldAttributes.Literal) == FieldAttributes.Literal)
				{
					if (!NamingUtils.IsPascalCase(field.Name))
					{
						m_details = string.Format("{0}Constant Field: {1}. ", m_details, field.Name);
					}
				}
				else
				{
					if (!NamingUtils.IsCamelCase(field.Name))
					{
						m_details = string.Format("{0}Field: {1}. ", m_details, field.Name);
					}
					
					// mUpperCase is not legit
					if (field.Name.Length > 1)
					{
						if (field.Name[0] == 'm' && char.IsUpper(field.Name[1]))
						{
							m_details = string.Format("{0}Field: {1}. ", m_details, field.Name);
						}
					}
				}
			}
		}

		public void VisitEvent(EventDefinition evt)
		{
			if (!m_needsCheck)
				return;
			
			if (!NamingUtils.IsPascalCase(evt.Name))
				m_details = string.Format("{0}Event: {1}. ", m_details, evt.Name);
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
		
		protected string Details		{get {return m_details;} set {m_details = value;}}
		protected TypeDefinition Type	{get {return m_type;}}
		protected bool NeedsCheck 		{get {return m_needsCheck;}}
		
		private string m_details;
		private TypeDefinition m_type;
		private bool m_needsCheck;
	}
}

