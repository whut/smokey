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
using System;
using Smokey.Framework;
using Smokey.Framework.Instructions;
using Smokey.Framework.Support;
using Smokey.Framework.Support.Advanced;

namespace Smokey.Internal.Rules
{	
	internal sealed class CollectionToStringRule : Rule
	{				
		public CollectionToStringRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "D1040")
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

			m_offset = -1;
			m_info = begin.Info;
		}
		
		public void VisitCall(Call call)
		{				
			if (m_offset < 0)
			{
				if (call.Target.ToString() == "System.String System.Object::ToString()")
				{
					if (DoIsBad(call.Index - 1))
						m_offset = call.Untyped.Offset;
				}
				else if (call.Target.ToString().StartsWith("System.Void System.Console::WriteLine("))
				{
					for (int nth = 0; nth < call.Target.Parameters.Count && m_offset < 0; ++nth)
					{
						int index = m_info.Tracker.GetStackIndex(call.Index, nth);
						
						if (index >= 0)
						{
							if (DoIsBad(index))
								m_offset = call.Untyped.Offset;
						}
					}
				}
			}
		}
				
		public void VisitEnd(EndMethod end)
		{			
			if (m_offset >= 0)
			{
				Reporter.MethodFailed(end.Info.Method, CheckID, m_offset, string.Empty);
			}
		}
		
		private bool DoIsBad(int index)
		{
			bool bad = false;
			
			do
			{
				LoadArg arg = m_info.Instructions[index] as LoadArg;
				if (arg != null && arg.Arg >= 1)
				{	
					Log.DebugLine(this, "arg: {0}", arg.Arg);				

					ParameterDefinition p = m_info.Method.Parameters[arg.Arg - 1];
					Log.DebugLine(this, "param {0} is of type {1}", arg.Arg, p.ParameterType.FullName);				
					if (DoIsBad(p.ParameterType))
						bad = true;
					break;
				}

				LoadLocal local = m_info.Instructions[index] as LoadLocal;
				if (local != null)
				{	
					Log.DebugLine(this, "local: {0}", local.Variable);				

					TypeReference type = m_info.Method.Body.Variables[local.Variable].VariableType;
					Log.DebugLine(this, "local {0} is of type {1}", local.Variable, type.FullName);				
					if (DoIsBad(type))
						bad = true;
					break;
				}

				LoadField field = m_info.Instructions[index] as LoadField;
				if (field != null)
				{	
					Log.DebugLine(this, "field: {0}", field.Field.Name);				

					TypeReference type = field.Field.FieldType;
					Log.DebugLine(this, "field {0} is of type {1}", field.Field.Name, type.FullName);				
					if (DoIsBad(type))
						bad = true;
					break;
				}

				LoadStaticField sfield = m_info.Instructions[index] as LoadStaticField;
				if (sfield != null)
				{	
					Log.DebugLine(this, "static field: {0}", sfield.Field.Name);				

					TypeReference type = sfield.Field.FieldType;
					Log.DebugLine(this, "static field {0} is of type {1}", sfield.Field.Name, type.FullName);				
					if (DoIsBad(type))
						bad = true;
					break;
				}
			}
			while (false);
			
			return bad;
		}

		private bool DoIsBad(TypeReference type)
		{
			if (type.FullName.EndsWith("[]"))
				return true;
				
			else if (type.FullName.Contains("System.Collections.Generic.Dictionary"))
				return true;
				
			else if (type.FullName.Contains("System.Collections.Generic.HashSet"))
				return true;
				
			else if (type.FullName.Contains("System.Collections.Generic.KeyedByTypeCollection"))
				return true;
				
			else if (type.FullName.Contains("System.Collections.Generic.LinkedList"))
				return true;
				
			else if (type.FullName.Contains("System.Collections.Generic.List"))
				return true;
				
			else if (type.FullName.Contains("System.Collections.Generic.Queue"))
				return true;
				
			else if (type.FullName.Contains("System.Collections.Generic.SortedDictionary"))
				return true;
				
			else if (type.FullName.Contains("System.Collections.Generic.SortedList"))
				return true;
				
			else if (type.FullName.Contains("System.Collections.Generic.Stack"))
				return true;
				
			else if (type.FullName.Contains("System.Collections.Generic.SynchronizedCollection"))
				return true;
				
			else if (type.FullName.Contains("System.Collections.Generic.SynchronizedKeyedCollection"))
				return true;
				
			else if (type.FullName.Contains("System.Collections.Generic.SynchronizedReadOnlyCollection"))
				return true;
								
			return false;
		}
		
		private int m_offset;
		private MethodInfo m_info;
	}
}

