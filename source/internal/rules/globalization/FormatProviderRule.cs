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

using Smokey.Framework;
using Smokey.Framework.Instructions;
using Smokey.Framework.Support;
using Smokey.Framework.Support.Advanced;

using System;
using System.IO;
using System.Collections.Generic;

namespace Smokey.Internal.Rules
{	
	internal class FormatProviderRule : Rule
	{				
		public FormatProviderRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "G1003")
		{
			m_enabled = Settings.Get("*localized*", "true") == "true";
			Log.TraceLine(this, "enabled: {0}", m_enabled);
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			if (m_enabled)
			{
				dispatcher.Register(this, "VisitBegin");
				dispatcher.Register(this, "VisitCall");
				dispatcher.Register(this, "VisitEnd");
			}
		}
		
		public void VisitBegin(BeginMethod begin)
		{
			Log.DebugLine(this, "-----------------------------------"); 
			Log.DebugLine(this, "{0:F}", begin.Info.Instructions);				

			m_offset = -1;
			m_bad = null;
			m_info = begin.Info;
			m_needsCheck = !begin.Info.Type.FullName.Contains("CompilerGenerated");	// TODO: why are we even called for these?
		}

		public void VisitCall(Call call)
		{
			if (m_offset < 0 && m_needsCheck)
			{			
				if (DoMatchMethod(call.Target))
				{
					Log.DebugLine(this, "candidate at {0:X2}", call.Untyped.Offset);
					
					TypeReference type = DoGetThisType(call);
					if (type == null)
						type = call.Target.DeclaringType;
					
					if (DoHasOverload(call.Target.Name, type))
					{
						m_offset = call.Untyped.Offset;
						m_bad = call.Target.ToString();
						Log.DebugLine(this, "bad call at {0:X2}", m_offset);
					}
				}
			}
		}
				
		public void VisitEnd(EndMethod end)
		{
			if (m_offset >= 0)
			{
				string details = "Method: " + m_bad;
				Reporter.MethodFailed(end.Info.Method, CheckID, m_offset, details);
			}
		}
		
		private TypeReference DoGetThisType(Call call)	// TODO: may want to make this a method on Call
		{
			TypeReference self = null;
			
			if (call.Target.HasThis)
			{
				int numArgs = call.Target.Parameters.Count;
				int index = m_info.Tracker.GetStackIndex(call.Index, numArgs);
			
				if (index >= 0)
				{
					do
					{
						LoadArg arg = m_info.Instructions[index] as LoadArg;
						if (arg != null)
						{
							self = arg.Type;
							break;
						}

						LoadField field = m_info.Instructions[index] as LoadField;
						if (field != null)
						{
							self = field.Field.FieldType;
							break;
						}

						LoadStaticField sfield = m_info.Instructions[index] as LoadStaticField;
						if (sfield != null)
						{
							self = sfield.Field.FieldType;
							break;
						}

						LoadLocal local = m_info.Instructions[index] as LoadLocal;
						if (local != null)
						{
							self = local.Type;
							break;
						}
					}
					while (false);
				}
			}
			
			return self;
		}

		// Method name has to match one of the methods we're looking for
		// and can't already use IFormatProvider.
		private bool DoMatchMethod(MethodReference method)
		{
			if (DoHasProvider(method))
				return false;
			
			string name = method.ToString();
			foreach (string pattern in m_methods)
			{
				if (name.Contains(pattern))
					return true;
			}
			
			return false;
		}
		
		// The method's declaring type, or a base class, must have an overload
		// which takes an IFormatProvider.
		private bool DoHasOverload(string name, TypeReference tr)
		{
			do
			{
				TypeDefinition type = Cache.FindType(tr);
				if (type != null)
				{
					MethodDefinition[] methods = type.Methods.GetMethod(name);
					foreach (MethodDefinition method in methods)
					{
						if (DoHasProvider(method))
							return true;
					}
					
					tr = type.BaseType;
				}
				else
					break;
			}
			while (tr != null);
			
			return false;
		}
		
		private bool DoHasProvider(MethodReference method)
		{
			foreach (ParameterDefinition p in method.Parameters)
			{
				if (p.ParameterType.FullName == "System.IFormatProvider")
					return true;
			}
			
			return false;
		}
												
		private bool m_enabled;
		private int m_offset;
		private string m_bad;
		private bool m_needsCheck;
		private MethodInfo m_info;
		private string[] m_methods = new string[]
		{
			" System.Convert::ChangeType(",
			" System.Convert::ToByte(",
			" System.Convert::ToDateTime(",	
			" System.Convert::ToDecimal(",
			" System.Convert::ToDouble(",
			" System.Convert::ToInt16(",
			" System.Convert::ToInt32(",
			" System.Convert::ToInt64(",
			" System.Convert::ToSByte(",
			" System.Convert::ToSingle(",
			" System.Convert::ToUInt16(",
			" System.Convert::ToUInt32(",
			" System.Convert::ToUInt64(",
			" System.String::Format(",
			" System.Text.StringBuilder::AppendFormat(",
			"::Parse(",
			"::TryParse(",
			"::ToString(",
		};
	}
}
