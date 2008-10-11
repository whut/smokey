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
	internal sealed class DisposeNativeResourcesRule : Rule
	{				
		public DisposeNativeResourcesRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "R1001")
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
			m_field = null;
		}
		
		public void VisitField(FieldDefinition field)
		{
			if (m_field == null)
			{
				if (field.FieldType.IsNative() && field.IsOwnedBy(m_type))
				{
					Log.DebugLine(this, "{0} is owned by the type", field.Name);				
					m_field = field;
				}
			}
		}
		
		public void VisitEnd(EndType begin)
		{
			if (m_field != null)
			{
				TypeDefinition type = DoFindFinalizer();
				
				if (type == null || type.FullName == "System.Object")
				{
					Log.DebugLine(this, "no finalizer");	
					Reporter.TypeFailed(begin.Type, CheckID, string.Empty);
				}
				else if (!m_type.TypeOrBaseImplements("System.IDisposable", Cache))
				{
					Log.DebugLine(this, "not IDisposable");	
					Reporter.TypeFailed(begin.Type, CheckID, string.Empty);
				}

			}
		}
				
		private TypeDefinition DoFindFinalizer()
		{
			TypeDefinition result = null;
			
			TypeDefinition type = m_type;
			while (type != null && result == null)
			{
				if (type.Methods.GetMethod("Finalize", Type.EmptyTypes) != null)
					result = type;

				type = Cache.FindType(type.BaseType);
			}
			
			return result;
		}

		private TypeDefinition m_type;
		private FieldDefinition m_field;
	}
}

