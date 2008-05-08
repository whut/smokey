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
using Mono.Cecil.Metadata;
using Smokey.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Smokey.Framework.Support
{		
	/// <summary>Caches all of the types and methods in an assembly.</summary>
	public class AssemblyCache
	{		
		internal AssemblyCache(SymbolTable symbols, AssemblyDefinition assembly, Rule.KeepAliveCallback callback)
		{
			DBC.Pre(symbols != null, "symbols is null");
			DBC.Pre(assembly != null, "assembly is null");

			Profile.Start("AssemblyCache ctor");
			m_symbols = symbols;
			m_assembly = assembly;
						
			foreach (ModuleDefinition module in assembly.Modules) 
			{
				foreach (TypeDefinition type in module.Types)
				{	
					if (!m_hasPublics)
						DoCheckForPublics(type);
					
					m_types.Add(type.MetadataToken, type);
					if (callback != null)
						callback(string.Format("adding {0}", type.Name));
		
					List<MethodInfo> ml = new List<MethodInfo>();
					DoAddMethods(symbols, type, type.Constructors, ml);
					DoAddMethods(symbols, type, type.Methods, ml);
					m_typeMethods.Add(type, ml);
				}
			}
			
			DoLoadDependentAssemblies(callback);
						
			Profile.Stop("AssemblyCache ctor");
		}	
		
#if TEST
		public AssemblyCache(AssemblyDefinition assembly, List<MethodInfo> methods)
		{			
			foreach (MethodInfo method in methods)
			{
				if (!m_types.ContainsKey(method.Type.MetadataToken))
				{
					if (!m_hasPublics)
						DoCheckForPublics(method.Type);
					
					m_types.Add(method.Type.MetadataToken, method.Type);
				}
				
				if (!m_methods.ContainsKey(method.Method.MetadataToken))
					m_methods.Add(method.Method.MetadataToken, method);
			}

			m_assembly = assembly;
			
			if (ms_externalTypes != null)
				m_externalTypes = ms_externalTypes;	
			else
			{
				DoLoadDependentAssemblies(null);
				ms_externalTypes = m_externalTypes;	
			}
		}	

		public AssemblyCache(AssemblyDefinition assembly, List<TypeDefinition> types)
		{			
			foreach (TypeDefinition type in types)
			{
				if (!m_hasPublics)
					DoCheckForPublics(type);
					
				m_types.Add(type.MetadataToken, type);

				List<MethodInfo> ml = new List<MethodInfo>();			
				DoAddMethods(type, type.Constructors, ml);
				DoAddMethods(type, type.Methods, ml);
				m_typeMethods.Add(type, ml);
			}

			m_assembly = assembly;
			
			if (ms_externalTypes != null)
				m_externalTypes = ms_externalTypes;	
			else
			{
				DoLoadDependentAssemblies(null);
				ms_externalTypes = m_externalTypes;	
			}
		}	
#endif

		public AssemblyDefinition Assembly
		{
			get {return m_assembly;}
		}

		/// <summary>Returns all of the types defined in the assembly.</summary>
		public IEnumerable<TypeDefinition> Types
		{
			get
			{
				foreach (TypeDefinition type in m_types.Values)
					yield return type;
			}
		}

		/// <summary>Returns detailed information about every method in the assembly.</summary>
		public IEnumerable<MethodInfo> Methods
		{
			get
			{
				foreach (MethodInfo method in m_methods.Values)
					yield return method;
			}
		}

		/// <summary>Note that this may return a type defined in a dependent assembly.</summary>
		/// <summary>Returns null if the type cannot be found.</summary>
		public TypeDefinition FindType(TypeReference tr)
		{
			TypeDefinition type = tr as TypeDefinition;
			
			if (tr != null && type == null)
			{
				if (!m_types.TryGetValue(tr.MetadataToken, out type) || !DoMatchType(type, tr))
				{						
					if (!m_externalTypes.TryGetValue(tr.FullName, out type))
					{
						string name = tr.FullName;	// System.Collections.Generic.List`1<System.Int32>
						int i = name.IndexOf('<');	// extern names won't be instantiated and won't have angle brackets
						if (i >= 0)
						{
							name = name.Substring(0, i);
							Ignore.Value = m_externalTypes.TryGetValue(name, out type);
						}
					}
				}
			}
			
			if (type != null)
				if (!DoMatchType(tr, type))
					type = null;
			
			return type;
		}
		
		/// <summary>Returns the base type of a type.</summary>
		/// <summary>Returns null if the base cannot be found.</summary>
		public TypeReference FindBase(TypeReference tr)
		{
			TypeReference result = null;
			
			TypeDefinition type = FindType(tr);
			if (type != null)
				result = type.BaseType;
			
			return result;
		}
		
		/// <summary>Returns the assembly the type is defined in.</summary>
		/// <summary>Returns null if the assembly cannot be found.</summary>
		public AssemblyDefinition FindAssembly(TypeReference type)
		{
			AssemblyDefinition assembly = null;
			string name = type.FullName;

			if (!m_assemblies.TryGetValue(name, out assembly))
			{
				foreach (AssemblyDefinition candidate in m_depedent)
				{
					foreach (ModuleDefinition module in candidate.Modules) 
					{
						if (module.Types.Contains(name))
						{
							assembly = candidate;
							m_assemblies.Add(name, assembly);
							return assembly;	// it's possible for internal types to have the exact same name in different assemblies so we'll bail as soon as we find one
						}
					}
				}
			}
			
			return assembly;
		}
		
		/// <summary>Returns null if the method is defined in another assembly. Note also
		/// that generic methods (or nested classes inside a generic) have one
		/// token when they are defined and another when they are called with
		/// a type.</summary>
		public MethodInfo FindMethod(MethodReference m)
		{
			MethodInfo info;
			if (m_methods.TryGetValue(m.MetadataToken, out info))
			{
				if (m.ToString() == info.Method.ToString())
					return info;
				else
					return null;
			}
			else
				return null;
		}
		
		/// <summary>Returns all the methods in type whose names are equal to name.</summary>
		public List<MethodInfo> FindMethods(TypeDefinition type, string name)
		{
			DBC.Pre(type != null, "type is null");
			DBC.Pre(!string.IsNullOrEmpty(name), "name is null or empty");

			var methods = new List<MethodInfo>();
			
			foreach (MethodDefinition method in type.Methods)
			{
				if (method.Name == name)
				{
					MethodInfo info = FindMethod(method);
					if (info != null)
						methods.Add(info);
				}
			}
		
			return methods;
		}
		
		public Dictionary<TypeDefinition, List<MethodInfo>> TypeMethods
		{
			get {return m_typeMethods;}
		}
		
		// Returns null if we're running a unit test.
		internal SymbolTable Symbols
		{
			get {return m_symbols;}
		}
		
		/// <summary>Returns true if the assembly has externally visible types.</summary>
		public bool HasPublicTypes
		{
			get {return m_hasPublics;}
		}
		
		#region Private Methods -----------------------------------------------
		// This is just a quick and dirty check to ensure that the type we got
		// from a MetadataToken isn't completely whacked. It'd be simpler to
		// just compare full names, but that doesn't always work with generic
		// types.
		private bool DoMatchType(TypeReference lhs, TypeReference rhs)
		{
			if (lhs.Namespace == rhs.Namespace)
				if (lhs.Name == rhs.Name)
					return true;
					
//			Log.InfoLine(true, "   {0} != {1}", lhs.FullName, rhs.FullName);

			return false;
		}
		
		private void DoLoadDependentAssemblies(Rule.KeepAliveCallback callback)
		{
			foreach (ModuleDefinition module in m_assembly.Modules)
			{
				foreach (AssemblyNameReference assemblyName in module.AssemblyReferences)
				{
					Log.InfoLine(this, "loading dependent assembly '{0}'", assemblyName);
					
					try
					{
						AssemblyDefinition subAssembly = m_assembly.Resolver.Resolve(assemblyName);	
						m_depedent.Add(subAssembly);
						
						foreach (ModuleDefinition subModule in subAssembly.Modules) 
						{
							foreach (TypeDefinition type in subModule.Types)
							{				
								if (!m_externalTypes.ContainsKey(type.FullName))
								{
//									Log.InfoLine(true, "   {0} {1}", type.FullName, type.MetadataToken);
									m_externalTypes.Add(type.FullName, type);
									
									if (callback != null)
										callback(string.Format("adding {0}", type.Name));
								}
							}
						}
					}
					catch (Exception e)
					{
						Log.ErrorLine(this, "failed to load dependent assembly '{0}'", assemblyName);
						Log.ErrorLine(this, e);
					}
				}
			}
		}

#if TEST
		private void DoAddMethods(TypeDefinition type, ICollection methods, List<MethodInfo> ml)
		{				
			foreach (MethodDefinition method in methods)
			{
				MethodInfo info = new MethodInfo(type, method);
				m_methods.Add(method.MetadataToken, info);
				ml.Add(info);
			}			
		}
#endif

		private void DoAddMethods(SymbolTable symbols, TypeDefinition type, ICollection methods, List<MethodInfo> ml)
		{							
			foreach (MethodDefinition method in methods)
			{
				MethodInfo info = new MethodInfo(symbols, type, method);
				m_methods.Add(method.MetadataToken, info);
				ml.Add(info);
			}			
		}
		
		private void DoCheckForPublics(TypeDefinition type)
		{
			TypeAttributes visibility = type.Attributes & TypeAttributes.VisibilityMask;
			if (visibility == TypeAttributes.Public || visibility == TypeAttributes.NestedPublic)	// need nested for the unit test
				m_hasPublics = true;
		}
		#endregion
		
		#region Fields --------------------------------------------------------
		private AssemblyDefinition m_assembly;	
		private Dictionary<MetadataToken, TypeDefinition> m_types = new Dictionary<MetadataToken, TypeDefinition>();
		private Dictionary<MetadataToken, MethodInfo> m_methods = new Dictionary<MetadataToken, MethodInfo>();
		private Dictionary<TypeDefinition, List<MethodInfo>> m_typeMethods = new Dictionary<TypeDefinition, List<MethodInfo>>();
		private Dictionary<string, TypeDefinition> m_externalTypes = new Dictionary<string, TypeDefinition>();
		private Dictionary<string, AssemblyDefinition> m_assemblies = new Dictionary<string, AssemblyDefinition>();
		private List<AssemblyDefinition> m_depedent = new List<AssemblyDefinition>();

		private SymbolTable m_symbols;
		private bool m_hasPublics;

#if TEST
		private static Dictionary<string, TypeDefinition> ms_externalTypes;
#endif
		#endregion
	}
}
