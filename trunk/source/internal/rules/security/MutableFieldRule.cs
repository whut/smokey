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
using System.Security;
using System.Collections.Generic;
using Smokey.Framework;
using Smokey.Framework.Instructions;
using Smokey.Framework.Support;
using Smokey.Framework.Support.Advanced;

namespace Smokey.Internal.Rules
{		
	internal sealed class MutableFieldRule : Rule
	{				
		public MutableFieldRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "S1021")
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
			m_needsCheck = begin.Type.ExternallyVisible(Cache);
			m_badFields.Clear();
			
			if (m_needsCheck)
			{
				Log.DebugLine(this, "-----------------------------------"); 
				Log.DebugLine(this, "{0}", begin.Type);		
			}
		}
		
		public void VisitField(FieldDefinition field)
		{
			if (m_needsCheck)
			{
				if ((field.IsPublic || field.IsFamily || field.IsFamilyOrAssembly) && field.IsInitOnly)
				{
					TypeDefinition type = Cache.FindType(field.FieldType);
					Log.DebugLine(this, "field: {0}, type: {1}", field.Name, field.FieldType.FullName);		
					if (type != null && !type.IsValueType)
					{
						if (DoIsMutable(type))
						{
							m_badFields.Add(field.Name);
						}
					}
				}
			}
		}

		public void VisitEnd(EndType end)
		{
			if (m_badFields.Count > 0)
			{
				string details = "Fields: " + string.Join(" ", m_badFields.ToArray());
				Log.DebugLine(this, details);
				Reporter.TypeFailed(end.Type, CheckID, details);
			}
		}
				
		private bool DoIsMutable(TypeDefinition type)
		{
			while (type != null)
			{
				if (DoTypeIsMutable(type)) 
					return true;
					
				foreach (TypeReference tr in type.Interfaces)
					if (DoTypeIsMutable(tr)) 
						return true;
				
				int i = type.FullName.IndexOf('`');
				string name = i >= 0 ? type.FullName.Substring(0, i) : type.FullName;
				switch (name)
				{
					case "System.Collections.Queue":		// check some selected mutable types which don't have setters
					case "System.Collections.Stack":
					case "System.Collections.Generic.HashSet":
					case "System.Collections.Generic.ICollection":
					case "System.Collections.Generic.Queue":
					case "System.Collections.Generic.Stack":
						return true;
				}
				
				type = Cache.FindType(type.BaseType);
			}
			
			return false;
		}
		
		private bool DoTypeIsMutable(TypeReference tr)
		{
			TypeDefinition type = Cache.FindType(tr);

			if (type != null)
			{
				foreach (PropertyDefinition prop in type.Properties)
				{
					if (prop.SetMethod != null && prop.SetMethod.IsPublic) 
						return true;
				}
			}
			
			return false;
		}
		
		private bool m_needsCheck;
		private List<string> m_badFields = new List<string>();
	}
}

