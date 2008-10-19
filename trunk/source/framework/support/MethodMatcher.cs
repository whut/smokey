// Copyright (C) 2008 Jesse Jones
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

#if OLD
namespace Smokey.Framework.Support
{	
	/// <summary>Used to compare two MethodReferences for equality.</summary>
	public struct MethodMatcher : IEquatable<MethodMatcher>
	{
		public MethodMatcher(MethodReference method)
		{
			DBC.Pre(method != null, "method is null");
			
			m_method = method;
		}
		
		public override bool Equals(object rhsObj)
		{
			if (rhsObj == null)					
				return false;
			
			if (GetType() != rhsObj.GetType()) 
				return false;
		
			MethodMatcher rhs = (MethodMatcher) rhsObj;					
			return this == rhs;
		}
			
		public bool Equals(MethodMatcher rhs)	
		{
			return this == rhs;
		}

		public static bool operator==(MethodMatcher lhs, MethodMatcher rhs)
		{
			return Match(lhs.m_method, rhs.m_method);
		}
		
		public static bool operator!=(MethodMatcher lhs, MethodMatcher rhs)
		{
			return !(lhs == rhs);
		}
		
		public static bool operator==(MethodMatcher lhs, MethodReference rhs)
		{
			return Match(lhs.m_method, rhs);
		}
		
		public static bool operator!=(MethodMatcher lhs, MethodReference rhs)
		{
			return !(lhs == rhs);
		}
		
		public static bool Match(MethodReference lhs, MethodReference rhs)
		{
			DBC.Pre(lhs != null, "lhs is null");				
			DBC.Pre(rhs != null, "rhs is null");
				
			if (lhs.Name != rhs.Name)
			{
				return false;
			}
			
			if (lhs.Parameters.Count != rhs.Parameters.Count)
			{
				return false;
			}
			
			for (int i = 0; i < lhs.Parameters.Count; ++i)
			{
				if (!DoMatchTypes(lhs, rhs, lhs.Parameters[i].ParameterType, rhs.Parameters[i].ParameterType))
				{
					return false;
				}
			}
				
			if (!DoMatchTypes(lhs, rhs, lhs.ReturnType.ReturnType, rhs.ReturnType.ReturnType))
			{
				return false;
			}
			
			return true;
		}
		
		public override int GetHashCode()
		{
			int hash = 0;
			
			unchecked
			{
				hash += m_method.Name.GetHashCode();
				
				for (int i = 0; i < m_method.Parameters.Count; ++i)
					hash += m_method.Parameters[i].ParameterType.FullName.GetHashCode();
				
				hash += m_method.ReturnType.ReturnType.FullName.GetHashCode();
			}
			
			return hash;
		}
		
		private static bool DoMatchTypes(MethodReference lhsMethod, MethodReference rhsMethod, TypeReference lhsType, TypeReference rhsType)
		{
			TypeReference lhsDeclared = lhsMethod.DeclaringType;
			TypeReference rhsDeclared = rhsMethod.DeclaringType;
			
			// If one of the types is generic then we'll assume that it matches anything.
			// TODO: to do this right we'd have to do the type inference ourselves but it should rarely matter...
			if (DoIsGeneric(lhsDeclared.GenericParameters, lhsType.Name) || DoIsGeneric(lhsMethod.GenericParameters, lhsType.Name))
				return true;

			if (DoIsGeneric(rhsDeclared.GenericParameters, rhsType.Name) || DoIsGeneric(rhsMethod.GenericParameters, rhsType.Name))
				return true;
			
			return lhsType.FullName == rhsType.FullName;
		}
		
		private static bool DoIsGeneric(GenericParameterCollection ps, string name)
		{
			foreach (GenericParameter p in ps)
			{
				if (p.Name == name)
					return true;
			}
			
			return false;
		}
		
		private MethodReference m_method;
	}
}
#endif
