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
	internal class JurassicNamingRule : NetNamingRule
	{				
		public JurassicNamingRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "M1000")
		{
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
#if !TEST
			string convention = Settings.Get("naming", "mono");
			if (convention == "jurassic")
#endif
			{
				dispatcher.Register(this, "VisitBegin");
				dispatcher.Register(this, "VisitType");
				dispatcher.Register(this, "VisitMethod");
				dispatcher.Register(this, "VisitJurassicFields");
				dispatcher.Register(this, "VisitEvent");
				dispatcher.Register(this, "VisitProperty");
				dispatcher.Register(this, "VisitEnd");
			}
		}
				
		public void VisitJurassicFields(FieldDefinition field)
		{
			if (!NeedsCheck)
				return;
				
			if ((Type.Attributes & TypeAttributes.Serializable) != TypeAttributes.Serializable)
			{
				// Public are pascal cased.
				if ((field.Attributes & FieldAttributes.FieldAccessMask) == FieldAttributes.Public)
				{
					if (!NamingUtils.IsPascalCase(field.Name))
					{
						Details = string.Format("{0}Public Field: {1}. ", Details, field.Name);
					}
				}
				
				// Internal are pascal cased.
				else if ((field.Attributes & FieldAttributes.FieldAccessMask) == FieldAttributes.Assembly)
				{
					if (!NamingUtils.IsPascalCase(field.Name))
					{
						Details = string.Format("{0}Internal Field: {1}. ", Details, field.Name);
					}
				}
				
				// Literals are pascal cased.
				else if ((field.Attributes & FieldAttributes.Literal) == FieldAttributes.Literal)
				{
					if (!NamingUtils.IsPascalCase(field.Name))
					{
						Details = string.Format("{0}Constant Field: {1}. ", Details, field.Name);
					}
				}
				
				// Protected and private static are ms_fooBar, s_fooBar, or sFooBar, or msFooBar.
				else if (field.IsStatic)
				{
					if (field.Name.StartsWith("ms_"))
					{
						if (!NamingUtils.IsCamelCase(field.Name.Substring(3)))
						{
							Details = string.Format("{0}Static Field: {1}. ", Details, field.Name);
							}
					}
					else if (field.Name.StartsWith("s_"))
					{
						if (!NamingUtils.IsCamelCase(field.Name.Substring(2)))
						{
							Details = string.Format("{0}Static Field: {1}. ", Details, field.Name);
							}
					}
					else if (field.Name.StartsWith("s"))
					{
						if (!NamingUtils.IsPascalCase(field.Name.Substring(1)))
						{
							Details = string.Format("{0}Static Field: {1}. ", Details, field.Name);
							}
					}
					else if (field.Name.StartsWith("ms"))
					{
						if (!NamingUtils.IsPascalCase(field.Name.Substring(2)))
						{
							Details = string.Format("{0}Static Field: {1}. ", Details, field.Name);
							}
					}
					else
					{
						Details = string.Format("{0}Static Field: {1}. ", Details, field.Name);
					}
				}

				// Protected and private fields must be m_fooBar or mFooBar. 
				else
				{
					if (field.FieldType.FullName.Contains("System.EventHandler"))	// events are special
					{
						if (!NamingUtils.IsPascalCase(field.Name))
						{
							Details = string.Format("{0}Field: {1}. ", Details, field.Name);
							}
					}
					else if (field.Name.StartsWith("m_"))
					{
						if (!NamingUtils.IsCamelCase(field.Name.Substring(2)))
						{
							Details = string.Format("{0}Field: {1}. ", Details, field.Name);
							}
					}
					else if (field.Name.StartsWith("m"))
					{
						if (!NamingUtils.IsPascalCase(field.Name.Substring(2)))
						{
							Details = string.Format("{0}Field: {1}. ", Details, field.Name);
							}
					}
					else
					{
						Details = string.Format("{0}Field: {1}. ", Details, field.Name);
					}	
				}
			}
		}
	}
}

