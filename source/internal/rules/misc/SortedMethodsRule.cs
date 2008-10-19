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
using Smokey.Framework.Support;
using System;
using System.Collections.Generic;

#if OLD
namespace Smokey.Internal.Rules
{		
	internal sealed class SortedMethodsRule : Rule
	{				
		public SortedMethodsRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "M1001")
		{
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitType");
		}
				
		public void VisitType(TypeDefinition type)
		{
			Log.DebugLine(this, "-----------------------------------"); 
			Log.DebugLine(this, "{0}", type);				

			m_details = string.Empty;
			DoCheck(type.Methods, MethodAttributes.Public);
			DoCheck(type.Methods, MethodAttributes.FamORAssem);
			DoCheck(type.Methods, MethodAttributes.Assem);
			DoCheck(type.Methods, MethodAttributes.Family);
			DoCheck(type.Methods, MethodAttributes.FamANDAssem);
			DoCheck(type.Methods, MethodAttributes.Private);
			
			if (m_details.Length > 0)
			{
				m_details = "Unsorted: " + m_details;
				Log.DebugLine(this, m_details);
				Reporter.TypeFailed(type, CheckID, m_details);
			}
		}		
		
		private void DoCheck(MethodDefinitionCollection methods, MethodAttributes access)
		{			
			List<string> unsorted = new List<string>();
			int numMethods = 0;
			
			string previous = null;
			foreach (MethodDefinition method in methods)
			{
				if (DoIsValidMethod(method))
				{
					if ((method.Attributes & MethodAttributes.MemberAccessMask) == access)
					{
						string name = DoGetName(method);
						if (previous != null && string.Compare(previous, name) > 0)
						{
							Log.DebugLine(this, "{0} isn't sorted ({1})", method.Name, method);
							unsorted.Add(name);
						}
						else
						{
							Log.DebugLine(this, "{0} is sorted", method.Name);
							++numMethods;
							previous = name;
						}
					}
				}
			}
			
			if (numMethods > 0)
			{
				float percent = (float) (numMethods - unsorted.Count)/numMethods;
				Log.DebugLine(this, "{0}% of the {1} methods are sorted", percent, access);
				if (percent >= 0.8 && unsorted.Count > 0)
				{
					m_details += string.Join(" ", unsorted.ToArray()) + " ";
				}
			}
		}
		
		private static bool DoIsValidMethod(MethodDefinition method)
		{
			MethodSemanticsAttributes attrs = method.SemanticsAttributes;
		
			if (attrs != 0)
			{
//				Log.DebugLine(this, "{0} is {1}", method.Name, attrs);

				// Allow properties but disallow indexers and events.
				if ((attrs & MethodSemanticsAttributes.Getter) == MethodSemanticsAttributes.Getter)
					return method.Parameters.Count == 0;		// note that indexers cannot be static

				else if ((attrs & MethodSemanticsAttributes.Setter) == MethodSemanticsAttributes.Setter)
					return method.Parameters.Count == 1;
					
				else
					return false;
			}
			
			if (method.Name.StartsWith("op_"))
				return false;
			
			return true;
		}
				
		private static string DoGetName(MethodDefinition method)
		{
			MethodSemanticsAttributes attrs = method.SemanticsAttributes;

			if (method.Name.Length > 4)		// this shouldn't be neccesary but Cecil can return ? for method names (eg for Reflector)
			{
				if ((attrs & MethodSemanticsAttributes.Setter) == MethodSemanticsAttributes.Setter)
					return method.Name.Substring(4);
	
				else if ((attrs & MethodSemanticsAttributes.Getter) == MethodSemanticsAttributes.Getter)
					return method.Name.Substring(4);
			}
			
			return method.Name;
		}
				
		private string m_details;
	}
}
#endif
