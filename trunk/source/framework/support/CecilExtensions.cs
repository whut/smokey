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
using System.Collections.Generic;
using SR = System.Reflection;

namespace Smokey.Framework.Support
{
	/// <summary>Some helpful Cecil related methods.</summary>
	public static class CecilExtensions 
	{ 
		public static bool Has(this CustomAttributeCollection attrs, string name)
		{
			DBC.Pre(attrs != null, "attrs is null");
			DBC.Pre(name != null, "name is null");
				
			foreach (CustomAttribute attr in attrs)
			{
//				Log.InfoLine(true, "   {0}", attr.Constructor.DeclaringType.Name);
				if (attr.Constructor.DeclaringType.Name == name)
					return true;
			}
			
			return false;
		}
		
		public static bool HasDisableRule(this CustomAttributeCollection attrs, string checkID)
		{
			DBC.Pre(attrs != null, "attrs is null");
			DBC.Pre(checkID != null, "checkID is null");
								
			foreach (CustomAttribute attr in attrs)
			{	
				if (attr.Constructor.ToString().Contains("DisableRuleAttribute"))
				{
					if (attr.ConstructorParameters.Count > 0)
					{
						string id = attr.ConstructorParameters[0] as string;
						if (id != null && id == checkID)
							return true;
					}
				}
			}
			
			return false;
		}
		
		public static bool HasDisableRule(this TypeDefinition derived, string checkID, AssemblyCache cache)
		{				
			DBC.Pre(derived != null, "derived is null");
			DBC.Pre(checkID != null, "checkID is null");
			DBC.Pre(cache != null, "cache is null");

			TypeDefinition type = derived;
			
			while (type != null)
			{
				if (HasDisableRule(type.CustomAttributes, checkID))
					return true;
					
				foreach (TypeReference t in type.Interfaces)
				{					
					TypeDefinition td = cache.FindType(t);
					if (td != null && HasDisableRule(td.CustomAttributes, checkID))
						return true;
				}
				
				type = cache.FindType(type.BaseType);
			}
			
			return false;
		}
		
		/// <summary>Returns true if type directly implements the interface (e.g. "System.ICloneable").</summary>
		public static bool Implements(this TypeDefinition type, string name)
		{
			DBC.Pre(type != null, "type is null");
			DBC.Pre(name != null, "name is null");
				
			bool implements = false;
			
			for (int i = 0; i < type.Interfaces.Count && !implements; ++i)
			{
				TypeReference t = type.Interfaces[i];
				string fname = t.FullName;
				int j = fname.IndexOf('`');		// System.Collections.Generic.ICollection`1<System.Int32>
				if (j >= 0)
					fname = fname.Substring(0, j);
					
				implements = fname == name;
			}
			
			return implements;
		}
		
		/// <summary>Returns true if a base class implements the interface (e.g. "System.IDisposable").</summary>
		public static bool BaseImplements(this TypeDefinition derived, string name, AssemblyCache cache)
		{				
			DBC.Pre(derived != null, "derived is null");
			DBC.Pre(name != null, "name is null");
			DBC.Pre(cache != null, "cache is null");
				
			if (derived.BaseType != null)
			{
				TypeDefinition type = cache.FindType(derived.BaseType);
				while (type != null)
				{
					if (Implements(type, name))
						return true;
						
					type = cache.FindType(type.BaseType);
				}
			}
			
			return false;
		}
		
		/// <summary>Returns true if a base class implements the interface (e.g. "System.IDisposable").</summary>
		public static bool ClassOrBaseImplements(this TypeDefinition type, string name, AssemblyCache cache)
		{				
			if (Implements(type, name))
				return true;
				
			else if (BaseImplements(type, name, cache))
				return true;
			
			return false;
		}
		
		/// <summary>Returns true if type is an enum with the Flags attribute.</summary>
		public static bool IsFlagsEnum(this TypeDefinition type)
		{
			DBC.Pre(type != null, "type is null");
				
			if (type.IsEnum)
			{
				foreach (CustomAttribute attr in type.CustomAttributes)
				{
					if (attr.Constructor.ToString().Contains("System.FlagsAttribute::.ctor"))
						return true;
				}
			}
			
			return false;
		}
		
		/// <summary>Returns true if the assembly makes use of System.Windows.Forms or gtk.</summary>
		public static bool IsGui(this AssemblyDefinition assembly)
		{
			DBC.Pre(assembly != null, "assembly is null");
				
			foreach (AssemblyNameReference r in assembly.MainModule.AssemblyReferences) 
			{
				if (r.Name == "gtk-sharp")
				{
					byte[] token = r.PublicKeyToken;
					if (token != null) 
					{
						if (token[0] == 0x35 && token[1] == 0xe1 &&
						    token[2] == 0x01 && token[3] == 0x95 &&
						    token[4] == 0xda && token[5] == 0xb3 &&
						    token[6] == 0xc9 && token[7] == 0x9f)
							return true;
					}
				}
				else if (r.Name == "System.Windows.Forms")
				{
					byte[] token = r.PublicKeyToken;
					if (token != null) 
					{
						if (token[0] == 0xb7 && token[1] == 0x7a &&
						    token[2] == 0x5c && token[3] == 0x56 &&
						    token[4] == 0x19 && token[5] == 0x34 &&
						    token[6] == 0xe0 && token[7] == 0x89)
							return true;
					}
				}
			}
			
			return false;
		}
		
		/// <summary>Returns true if type is IntPtr, IntPtr[], List&lt;IntPtr&gt;, etc.</summary>
		public static bool IsNative(this TypeReference type)
		{
			DBC.Pre(type != null, "type is null");
								
			if (type.FullName.Contains("System.IntPtr") || 
				type.FullName.Contains("System.UIntPtr") ||
				type.FullName.Contains("System.Runtime.InteropServices.HandleRef"))
				return true;
			
			return false;
		}
		
		/// <summary>Returns true if the two instructions are equal.</summary>
		public static bool Matches(this Instruction lhs, Instruction rhs)
		{
			DBC.Pre(lhs != null, "lhs is null");				
			DBC.Pre(rhs != null, "rhs is null");
				
//			if (lhs.Operand != null)
//				Log.InfoLine(true, "lhs operand: {0} ({1})", lhs.Operand, lhs.Operand.GetType()); 
//			else
//				Log.InfoLine(true, "lhs operand: null"); 

//			if (rhs.Operand != null)
//				Log.InfoLine(true, "rhs operand: {0} ({1})", rhs.Operand, rhs.Operand.GetType()); 
//			else
//				Log.InfoLine(true, "rhs operand: null"); 
				
			if (lhs.OpCode != rhs.OpCode)
				return false;
							
			if (lhs.Operand != null || rhs.Operand != null)
			{
				if (lhs.Operand == null || rhs.Operand == null)
					return false;
				
				if (lhs.OpCode.OperandType != rhs.OpCode.OperandType)
					return false;
					
				ParameterReference p1 = lhs.Operand as ParameterReference;
				ParameterReference p2 = rhs.Operand as ParameterReference;
				VariableReference v1 = lhs.Operand as VariableReference;
				VariableReference v2 = rhs.Operand as VariableReference;
								
				if (p1 != null)
				{
					return p1.Sequence == p2.Sequence;
				}
				else if (v1 != null)
				{
					return v1.Index == v2.Index;
				}
				else if (lhs.OpCode.OperandType == OperandType.InlineBrTarget || lhs.OpCode.OperandType == OperandType.ShortInlineBrTarget)
				{
					int n1 = ((Instruction) lhs.Operand).Offset - lhs.Offset;
					int n2 = ((Instruction) rhs.Operand).Offset - rhs.Offset;
					if (n1 != n2)
						return false;
				}
				else if (!lhs.Operand.Equals(rhs.Operand))
					return false;
			}
		
			return true;
		}
		
		/// <summary>Returns true if the method is public and its declaring type(s) are
		/// public.</summary>
		public static bool PubliclyVisible(this MethodDefinition method, AssemblyCache cache)	// TODO: shouldn't this be externally visible?
		{
			DBC.Pre(method != null, "method is null");				
			DBC.Pre(cache != null, "cache is null");
				
			bool pblc = (method.Attributes & MethodAttributes.MemberAccessMask) == MethodAttributes.Public;
			
			TypeReference tr = method.DeclaringType;
			while (tr != null && pblc)
			{
				TypeDefinition type = cache.FindType(tr);
				if (type != null)
				{
					TypeAttributes attrs = type.Attributes & TypeAttributes.VisibilityMask;
					pblc = (attrs == TypeAttributes.Public) || (attrs == TypeAttributes.NestedPublic);
				}
				
				tr = tr.DeclaringType;
			}
			
			return pblc;
		}
		
		/// <summary>Returns true if the method is private or its declaring type(s) are
		/// private.</summary>
		public static bool PrivatelyVisible(this MethodDefinition method, AssemblyCache cache)
		{
			DBC.Pre(method != null, "method is null");				
			DBC.Pre(cache != null, "cache is null");
				
			bool prvt = (method.Attributes & MethodAttributes.MemberAccessMask) == MethodAttributes.Private;
			
			TypeReference tr = method.DeclaringType;
			while (tr != null && !prvt)
			{
				TypeDefinition type = cache.FindType(tr);
				if (type != null)
					prvt = (type.Attributes & TypeAttributes.VisibilityMask) == TypeAttributes.NestedPrivate;
				
				tr = tr.DeclaringType;
			}
			
			return prvt;
		}
		
		/// <summary>Returns the interface or type the method was first declared in.</summary>
		public static TypeReference GetDeclaredIn(this MethodReference method, AssemblyCache cache)
		{
			DBC.Pre(method != null, "method is null");
			DBC.Pre(cache != null, "cache is null");
								
			TypeReference declared = method.DeclaringType;
							
			TypeReference candidate = DoGetDeclaringInterface(declared, new MethodMatcher(method), cache);
			if (candidate != null)
			{
				declared = candidate;
			}	
			else
			{
				TypeDefinition baseType = cache.FindType(declared);
				if (baseType != null)
				{
					Log.DebugLine(true, "   type: {0}", baseType);
					baseType = cache.FindType(baseType.BaseType);
					while (baseType != null && declared.FullName == method.DeclaringType.FullName)
					{
						Log.DebugLine(true, "   checking {0} for {1}", baseType, method.Name);
						if (DoDeclares(baseType, new MethodMatcher(method)))
							declared = baseType;
						
						baseType = cache.FindType(baseType.BaseType);
					}
				}
			}
			
			return declared;
		}
		
		/// <summary>Returns either null or the most derived class that is both a lhs and a rhs.</summary>
		public static TypeReference CommonClass(this TypeReference lhs, TypeReference rhs, AssemblyCache cache)
		{
			DBC.Pre(lhs != null, "lhs is null");				
			DBC.Pre(rhs != null, "rhs is null");
			DBC.Pre(cache != null, "cache is null");
				
			TypeReference common = null;
			
			if (lhs.IsSubclassOf(rhs, cache))
				common = lhs;
			else if (rhs.IsSubclassOf(lhs, cache))
				common = rhs;
			
			return common;
		}
		
		public static bool IsCompilerGenerated(this TypeReference type)
		{
			DBC.Pre(type != null, "type is null");

			if (type.Name.Contains("CompilerGenerated"))
				return true;
				
			else if (type.FullName.Contains("<PrivateImplementationDetails>"))
				return true;
				
			return false;
		}
		
		public static bool IsCompilerGenerated(this MethodReference method)
		{
			DBC.Pre(method != null, "method is null");

			if (IsCompilerGenerated(method.DeclaringType))
				return true;
				
//			if (method.ToString().Contains("CompilerGenerated"))
//				return true;
				
			return false;
		}
		
		/// <summary>Returns true if lhs is a subclass of rhs. Returns false if they are the same class.
		/// Interfaces are ignored.</summary>
		public static bool IsSubclassOf(this TypeReference lhs, TypeReference rhs, AssemblyCache cache)
		{			
			DBC.Pre(lhs != null, "lhs is null");				
			DBC.Pre(rhs != null, "rhs is null");
			DBC.Pre(cache != null, "cache is null");
				
			while (lhs != null)
			{
				if (lhs.FullName != "System.Object" && rhs.FullName == "System.Object")	// everything is a subclass of object
					return true;
					
				TypeDefinition type = cache.FindType(lhs);
				lhs = type != null && !type.IsInterface ? type.BaseType : null;
				if (lhs != null && lhs.FullName == rhs.FullName)
					return true;
			}
			
			return false;
		}
		
		/// <summary>Returns true if lhs is a subclass of rhs. Returns false if they are the same class.
		/// Interfaces are ignored.</summary>
		public static bool IsSubclassOf(this TypeReference lhs, string rhs, AssemblyCache cache)
		{			
			DBC.Pre(lhs != null, "lhs is null");				
			DBC.Pre(rhs != null, "rhs is null");
			DBC.Pre(cache != null, "cache is null");
				
			TypeDefinition type = cache.FindType(lhs);

			if (type != null && type.BaseType != null)
			{
				type = cache.FindType(type.BaseType);
				
				while (type != null)
				{
					if (type.FullName == rhs)
						return true;
						
					type = cache.FindType(type.BaseType);
				}
			}
			
			return false;
		}
		
		/// <summary>Returns true if lhs is a subclass of rhs. Returns false if they are the same class.
		/// Interfaces are ignored.</summary>
		public static bool IsSameOrSubclassOf(this TypeReference lhs, string rhs, AssemblyCache cache)
		{			
			DBC.Pre(lhs != null, "lhs is null");
			DBC.Pre(rhs != null, "rhs is null");
			DBC.Pre(cache != null, "cache is null");
				
			if (lhs.FullName == rhs)
				return true;
				
			else if (IsSubclassOf(lhs, rhs, cache))
				return true;
			
			return false;
		}
		
		#region Private Methods -----------------------------------------------
		private static TypeReference DoGetDeclaringInterface(TypeReference declared, MethodMatcher matcher, AssemblyCache cache)
		{			
			Log.DebugLine(true, "   finding {0}", declared.FullName);
			TypeDefinition type = cache.FindType(declared);
			if (type != null)
			{
				Log.DebugLine(true, "   found {0}, {1} interfaces", type.FullName, type.Interfaces.Count);
				foreach (TypeReference inRef in type.Interfaces)
				{
					Log.DebugLine(true, "   references {0}", inRef.FullName);
					TypeDefinition inType = cache.FindType(inRef);
					if (inType != null)
					{
						Log.DebugLine(true, "   checking {0}", inType);
						if (DoDeclares(inType, matcher))
						{
							return inType;
						}
					}
				}
			}
			
			return null;
		}
		
		private static bool DoDeclares(TypeDefinition type, MethodMatcher matcher)
		{			
			foreach (MethodDefinition method in type.Methods)
			{
				if (matcher == method)
				{
					return true;
				}
			}
			
			return false;
		}
		#endregion
	}
} 
