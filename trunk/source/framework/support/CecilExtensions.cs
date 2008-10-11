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
using Smokey.Framework.Instructions;
using System;
using System.Collections.Generic;
using SR = System.Reflection;

namespace Smokey.Framework.Support
{
	/// <summary>Some helpful Cecil related methods.</summary>
	public static class CecilExtensions 
	{ 
		#region AssemblyDefinition --------------------------------------------
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
		#endregion 

		#region CustomAttributeCollection -------------------------------------
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
		#endregion
		
		#region FieldDefinition -----------------------------------------------
		/// <summary>Returns true if field is created by type's constructor(s).</summary>
		public static bool IsOwnedBy(this FieldDefinition field, TypeDefinition type)
		{
			DBC.Pre(field != null, "field is null");
			DBC.Pre(type != null, "type is null");
				
			List<string> tested = new List<string>();
			for (int i = 0; i < type.Constructors.Count; ++i)
			{
				if (DoOwnsField(type, type.Constructors[i], field, tested))
					return true;
			}
			
			return false;
		}
		#endregion
		
		#region Instruction ---------------------------------------------------
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
		#endregion
		
		#region MethodDefinition ----------------------------------------------
		/// <summary>Returns true if the method is public and its declaring type(s) are
		/// public.</summary>
		public static bool ExternallyVisible(this MethodDefinition method, AssemblyCache cache)	
		{
			DBC.Pre(method != null, "method is null");				
			DBC.Pre(cache != null, "cache is null");
				
			bool visible = false;
			
			if ((method.Attributes & MethodAttributes.MemberAccessMask) == MethodAttributes.Public)
			{
				TypeDefinition type = cache.FindType(method.DeclaringType);
				if (type != null)
					visible = type.ExternallyVisible(cache);
			}
							
			return visible;
		}
		
		/// <summary>Returns the first base class method which declares the specified method. 
		/// Note that this may return an abstract method or the original method.</summary>
		public static MethodDefinition GetPreviousMethod(this MethodDefinition method, AssemblyCache cache)
		{			
			DBC.Pre(method != null, "method is null");
			
			MethodDefinition result = method;
			
			if (method.IsVirtual)
			{
				TypeDefinition type = cache.FindType(method.DeclaringType);
				if (type != null)
					type = cache.FindType(type.BaseType);
				
				if (type != null)
				{
					MethodMatcher matcher = new MethodMatcher(method);

					MethodDefinition[] candidates = type.Methods.GetMethod(method.Name);
					foreach (MethodDefinition candidate in candidates)
						if (candidate.IsVirtual && !candidate.IsFinal && matcher == candidate)
							result = candidate;
				}
			}
			
			return result;
		}

		/// <summary>Returns true if the method is private or its declaring type is private.</summary>
		public static bool PrivatelyVisible(this MethodDefinition method, AssemblyCache cache)
		{
			DBC.Pre(method != null, "method is null");				
			DBC.Pre(cache != null, "cache is null");
			
			bool invisible = false;
			
			if ((method.Attributes & MethodAttributes.MemberAccessMask) == MethodAttributes.Private)
			{
				invisible = true;
			}
			else
			{
				TypeDefinition type = cache.FindType(method.DeclaringType);
				if (type != null)
					invisible = (type.Attributes & TypeAttributes.VisibilityMask) == TypeAttributes.NestedPrivate;
			}
			
			return invisible;
		}
		#endregion
		
		#region MethodReference -----------------------------------------------
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

		public static bool IsCompilerGenerated(this MethodReference method)
		{
			DBC.Pre(method != null, "method is null");

			if (IsCompilerGenerated(method.DeclaringType))
				return true;
				
//			if (method.ToString().Contains("CompilerGenerated"))
//				return true;
				
			return false;
		}		

		/// <summary>Returns true if the method has the specified result type, name, and argument types.</summary>
		public static bool Matches(this MethodReference method, string rtype, string mname, params string[] atypes)
		{			
			DBC.Pre(method != null, "method is null");
			DBC.Pre(!string.IsNullOrEmpty(rtype), "rtype is null or empty");
			DBC.Pre(!string.IsNullOrEmpty(mname), "mname is null or empty");
			DBC.Pre(atypes != null, "atypes is null");
			
			bool match = false;
			if (method.ReturnType.ReturnType.FullName == rtype)
			{
				if (method.Name == mname)
				{
					if (method.Parameters.Count == atypes.Length)
					{
						match = true;
						
						for (int i = 0; i < atypes.Length && match; ++i)
							match = atypes[i] == method.Parameters[i].ParameterType.FullName;
					}
				}
			}
			
			return match;
		}		
		#endregion
		
		#region TypeDefinition ------------------------------------------------
		/// <summary>Returns true if a base class implements the interface (e.g. "System.IDisposable").
		/// Also see TypeImplements and TypeOrBaseImplements.</summary>
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
					if (TypeImplements(type, name))
						return true;
						
					type = cache.FindType(type.BaseType);
				}
			}
			
			return false;
		}
		
		/// <summary>Returns true if the type is public and its declaring type(s) are
		/// public.</summary>
		public static bool ExternallyVisible(this TypeDefinition type, AssemblyCache cache)	
		{
			DBC.Pre(type != null, "type is null");				
			DBC.Pre(cache != null, "cache is null");
			
			bool visible = false;
				
			TypeAttributes attrs = type.Attributes & TypeAttributes.VisibilityMask;
			if (attrs == TypeAttributes.Public)
			{
				visible = true;
			}
			else if (attrs == TypeAttributes.NestedPublic)	// this just means that the class is public and nested
			{
				type = cache.FindType(type.DeclaringType);
				if (type != null)
					visible = type.ExternallyVisible(cache);
			}
			
			return visible;
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
		
		/// <summary>Returns true if type directly implements the interface (e.g. "System.ICloneable").</summary>
		public static bool TypeImplements(this TypeDefinition type, string name)
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
		public static bool TypeOrBaseImplements(this TypeDefinition type, string name, AssemblyCache cache)
		{				
			if (TypeImplements(type, name))
				return true;
				
			else if (BaseImplements(type, name, cache))
				return true;
			
			return false;
		}
		#endregion
	
		#region TypeReference -------------------------------------------------
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
		#endregion
				
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

		private static bool DoOwnsField(TypeDefinition type, MethodDefinition method, FieldDefinition field, List<string> tested)
		{
			bool owns = false;
			tested.Add(method.Name);
			
			if (method.Body != null)
			{
				for (int i = 1; i < method.Body.Instructions.Count && !owns; ++i)
				{
					FieldReference fref = DoGetOwnedField(type, method.Body.Instructions, i);
					if (fref != null)
					{
						owns = field.Name == fref.Name;
					}
					else
					{
						Instruction instruction = method.Body.Instructions[i];
						if (instruction.OpCode.Code == Code.Call || instruction.OpCode.Code == Code.Callvirt)
						{	
							MethodReference target = (MethodReference) instruction.Operand;
							MethodDefinition md = type.Methods.GetMethod(target.Name, target.Parameters);
							if (md != null && md.MetadataToken == target.MetadataToken && tested.IndexOf(target.Name) < 0) 
								owns = DoOwnsField(type, md, field, tested);
						}
					}
				}
			}
			
			return owns;
		}
		
		private static FieldReference DoGetOwnedField(TypeDefinition type, InstructionCollection instructions, int index)
		{
			FieldReference owned = null;
			
			Instruction instruction = instructions[index];
			if (instruction.OpCode.Code == Code.Stfld)	
			{
				FieldReference fr = (FieldReference) instruction.Operand;
				FieldDefinition field = type.Fields.GetField(fr.Name);
				if (field != null && field.MetadataToken == fr.MetadataToken)
				{
					if (!field.IsStatic)					// we should be checking the store field's target to see if it is 'this', but hopefully ctors won't be assigning to other class instances
					{
						do
						{
							// newobj instance void class [mscorlib]System.IO.StringWriter::.ctor()
							// stfld  class [mscorlib]System.IO.StringWriter Smokey.DisposableFieldsTest/GoodCase::m_writer
							if (instructions[index - 1].OpCode.Code == Code.Newobj || instructions[index - 1].OpCode.Code == Code.Newarr)
							{
								owned = field;
								break;
							}	

							// call  native int class Smokey.DisposeNativeResourcesTest/GoodCase2::CreateHandle()
							// stfld native int Smokey.DisposeNativeResourcesTest/GoodCase2::m_resource
							if (instructions[index - 1].OpCode.Code == Code.Call || instructions[index - 1].OpCode.Code == Code.Callvirt)
							{
								MethodReference target = (MethodReference) instructions[index - 1].Operand;
								if (!target.Name.StartsWith("get_") && target.Name != "op_Explicit")	// ignore property and (IntPtr) 0
								{
									if (target.ToString().IndexOf("Create") >= 0 || target.ToString().IndexOf("Make") >= 0)
									{
										owned = field;
										break;
									}
								}		
							}
						}
						while (false);
					}
				}
			}
			// ldarg.0  this
			// ldflda   System.Runtime.InteropServices.HandleRef Smokey.Tests.DisposeNativeResourcesTest/BadCase2::m_resource
			// ldc.i4.s 0x64
			// call instance void native int::'.ctor'(int32)
			//
        	// ldarg.0 
    	    // ldflda valuetype [mscorlib]System.Runtime.InteropServices.HandleRef Smokey.Tests.DisposeNativeResourcesTest/BadCase3::m_resource
	        // initobj [mscorlib]System.Runtime.InteropServices.HandleRef
			else if (instruction.OpCode.Code == Code.Ldflda)
			{
				if (instructions[index - 1].OpCode.Code == Code.Ldarg_0)
				{
					FieldReference fr = (FieldReference) instruction.Operand;
					FieldDefinition field = type.Fields.GetField(fr.Name);
					if (field != null && field.MetadataToken == fr.MetadataToken)
					{
						if (!field.IsStatic)
						{
							int i = index + 1;
							while (i < instructions.Count)
							{
								if (DoIsLoad(instructions[i].OpCode.Code))
									++i;
								else
									break;
							}
							
							if (i < instructions.Count)
							{
								if (instructions[i].OpCode.Code == Code.Call || instructions[i].OpCode.Code == Code.Callvirt)
								{
									MethodReference target = (MethodReference) instructions[i].Operand;
									if (target.ToString().IndexOf("ctor") >= 0 || target.ToString().IndexOf("Create") >= 0 || target.ToString().IndexOf("Make") >= 0)
									{
										owned = field;
									}
								}
								else if (instructions[i].OpCode.Code == Code.Initobj)
								{
									owned = field;
								}
							}
						}
					}
				}
			}
			
			return owned;
		}		
		
		private static bool DoIsLoad(Code code)
		{
			switch (code)
			{
				case Code.Ldelem_I1:
				case Code.Ldelem_U1:
				case Code.Ldelem_I2:
				case Code.Ldelem_U2:
				case Code.Ldelem_I4:
				case Code.Ldelem_U4:
				case Code.Ldelem_I8:
				case Code.Ldelem_I:
				case Code.Ldelem_R4:
				case Code.Ldelem_R8:
				case Code.Ldelem_Ref:
				case Code.Ldelem_Any:
				case Code.Ldind_I1:
				case Code.Ldind_U1:
				case Code.Ldind_I2:
				case Code.Ldind_U2:
				case Code.Ldind_I4:
				case Code.Ldind_U4:
				case Code.Ldind_I8:
				case Code.Ldind_I:
				case Code.Ldind_R4:
				case Code.Ldind_R8:
				case Code.Ldind_Ref:
				case Code.Ldlen:
				case Code.Ldloca_S:
				case Code.Ldloca:
				case Code.Ldstr:
				case Code.Ldsflda:
				case Code.Ldsfld:
				case Code.Ldelema:
				case Code.Ldtoken:
				case Code.Ldnull:
				case Code.Ldloc_0:
				case Code.Ldloc_1:
				case Code.Ldloc_2:
				case Code.Ldloc_3:
				case Code.Ldloc_S:
				case Code.Ldloc:
				case Code.Ldftn:
				case Code.Ldvirtftn:
				case Code.Ldflda:
				case Code.Ldfld:
				case Code.Ldc_I4_M1:
				case Code.Ldc_I4_0:
				case Code.Ldc_I4_1:
				case Code.Ldc_I4_2:
				case Code.Ldc_I4_3:
				case Code.Ldc_I4_4:
				case Code.Ldc_I4_5:
				case Code.Ldc_I4_6:
				case Code.Ldc_I4_7:
				case Code.Ldc_I4_8:
				case Code.Ldc_I4_S:
				case Code.Ldc_I4:
				case Code.Ldc_I8:
				case Code.Ldc_R4:
				case Code.Ldc_R8:
				case Code.Ldarga:
				case Code.Ldarga_S:
				case Code.Ldarg_0:
				case Code.Ldarg_1:
				case Code.Ldarg_2:
				case Code.Ldarg_3:
				case Code.Ldarg:
				case Code.Ldarg_S:
					return true;
			}
			
			return false;
		}
		#endregion
	}
} 
