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
using System.Collections.Generic;
using System.Text;

namespace Smokey.Internal.Rules
{	
	internal class UnusedMethodRule : Rule
	{				
		public UnusedMethodRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "D1032")
		{
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitType");
			dispatcher.Register(this, "VisitMethod");
			dispatcher.Register(this, "VisitEnd");
			dispatcher.Register(this, "VisitCall");
			dispatcher.Register(this, "VisitFunPtr");
			dispatcher.Register(this, "VisitNew");
			dispatcher.Register(this, "VisitFini");
		}
		
		public void VisitType(TypeDefinition type)
		{
			m_type = type;
			m_needsCheck = true;
			m_checkCtors = true;
			
			if (type.IsSubclassOf("System.Delegate", Cache))
				m_needsCheck = false;				
			else if (type.IsSubclassOf("System.MulticastDelegate", Cache))
				m_needsCheck = false;
			
			// If UnusedClassRule is disabled don't complain about
			// unused ctors.
			if (type.HasDisableRule("P1012", Cache))
				m_checkCtors = false;
		}
		
		public void VisitMethod(MethodDefinition method)
		{
			DBC.Assert(m_type.MetadataToken == method.DeclaringType.MetadataToken, "type mismatch");
			
			do
			{
				if (!m_needsCheck)
					break;
										
				if (method.IsConstructor && !m_checkCtors)
					break;
					
				if (method.ToString().Contains("PrivateImplementationDetails"))
					break;
					
				// Method cannot be externally visible.
				TypeAttributes vis = m_type.Attributes & TypeAttributes.VisibilityMask;
				MethodAttributes access = method.Attributes & MethodAttributes.MemberAccessMask;
				if (vis == TypeAttributes.Public || vis == TypeAttributes.NestedPublic || vis == TypeAttributes.NestedFamily || vis == TypeAttributes.NestedFamORAssem)
				{
					if (access == MethodAttributes.Family || access == MethodAttributes.FamORAssem || access == MethodAttributes.Public)
						break;
				}
				
				// Type and method cannot be generic (because a simple MetadataToken
				// comparison doesn't work with them).
				if (m_type.GenericParameters.Count > 0)	
					break;

				if ((method.CallingConvention & MethodCallingConvention.Generic) == MethodCallingConvention.Generic)
					break;

				// The method cannot be virtual (because virtual methods are
				// called by the runtime),
				if (method.IsVirtual)
					break;

				// The type cannot be an attribute (most attribute members are called by the
				// runtime),
				if (m_type.IsSubclassOf("System.Attribute", Cache))
					break;

				// The method cannot be type ctor (because they are called by the runtime),
				if (method.Name == ".cctor")
					break;

				// The method can't be an operator or Equals.
				if (method.IsSpecialName && method.Name.StartsWith("op_"))
					break;

				if (method.Name == "Equals")
					break;
					
				// The method can't be the assemblies main entry point.
				if (method == Cache.Assembly.EntryPoint)
					break;

				// The rule can't be disabled for the type or method.
				if (m_type.HasDisableRule("D1032", Cache))
					break;

				if (method.CustomAttributes.HasDisableRule("D1032"))
					break;

				m_methods.Add(method);
			}
			while (false);
		}
		
		public void VisitEnd(EndTypes end)
		{
			if (m_needsCheck)
				m_methods.Sort(m_comparer);
		}
		
		public void VisitFunPtr(LoadFunctionAddress load)
		{
			if (m_needsCheck)
			{
				int i = m_methods.BinarySearch(load.Method, m_comparer);
				if (i >= 0)
					m_methods.RemoveAt(i);
			}
		}
		
		public void VisitCall(Call call)
		{
			if (m_needsCheck)
			{
				int i = m_methods.BinarySearch(call.Target, m_comparer);
				if (i >= 0)
					m_methods.RemoveAt(i);
				}
		}
		
		public void VisitNew(NewObj newer)
		{
			if (m_needsCheck)
			{
				int i = m_methods.BinarySearch(newer.Ctor, m_comparer);
				if (i >= 0)
					m_methods.RemoveAt(i);
			}
		}
		
		public void VisitFini(EndTesting end)
		{
			if (m_needsCheck && m_methods.Count > 0)
			{
				CompareNames comparer = new CompareNames();
				m_methods.Sort(comparer);
				
				StringBuilder builder = new StringBuilder();
				builder.AppendLine("Methods: ");
				for (int i = 0; i < m_methods.Count; ++i)
				{
					builder.Append(m_methods[i].ToString());
					if (i + 1 < m_methods.Count)
						builder.AppendLine();
				}
			
				string details = builder.ToString();
				
				Log.DebugLine(this, details);
				Reporter.AssemblyFailed(Cache.Assembly, CheckID, details);
			}
		}
		
		private class CompareTokens : IComparer<MethodReference>
		{
			public int Compare(MethodReference lhs, MethodReference rhs)
			{
				if (lhs.DeclaringType == rhs.DeclaringType)
					return lhs.MetadataToken.RID.CompareTo(rhs.MetadataToken.RID);
				else
					return lhs.DeclaringType.FullName.CompareTo(rhs.DeclaringType.FullName);
			}
		}
		
		private class CompareNames : IComparer<MethodReference>
		{
			public int Compare(MethodReference lhs, MethodReference rhs)
			{
				return lhs.ToString().CompareTo(rhs.ToString());
			}
		}
		
		private List<MethodReference> m_methods = new List<MethodReference>();
		private CompareTokens m_comparer = new CompareTokens();
		private TypeDefinition m_type;
		private bool m_needsCheck;
		private bool m_checkCtors;
	}
}

