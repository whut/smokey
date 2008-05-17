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
using Smokey.Framework;
using Smokey.Framework.Support;
using Smokey.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Smokey.App 
{
	/// <summary>This is the class used by Smokey.Internal.Program and external programs to
	/// find problems with assemblies.</summary>
	public class AnalyzeAssembly
	{				
		public AnalyzeAssembly() : this(null)
		{
		}
				
		/// <remarks>Callback will be called periodically as types and methods are checked. Smokey
		/// uses this with a watchdog class to ensure no rule falls into an infinite loop.</remarks>
		public AnalyzeAssembly(Rule.KeepAliveCallback callback)
		{
			m_callback = callback;
			m_checker = new CheckItem(callback);
		}
				
		/// <summary>List of checkIDs to skip when checking the assembly.</summary>
		/// <exception cref="System.ArgumentException">Thrown if a checkID is not in the ViolationDatabase.</exception>
		public IEnumerable<string> ExcludedChecks
		{
			set 
			{
				foreach (string checkID in value)
					if (!ViolationDatabase.IsValid(checkID))
						throw new ArgumentException(string.Format("{0} is not a valid checkID", checkID));
						
				m_checker.ExcludedChecks = value;
			}
		}
				
		/// <summary>List of type/method names to skip when checking the assembly.</summary>
		/// <remarks>The name may be followed with an ampersand and a checkID in order to skip
		/// the type/method for only that checkID. Note that the compare need not be exact 
		/// ("AnalyzeAssembly::set_" will match all of our property setters).</remarks>
		/// <exception cref="System.ArgumentException">Thrown if a checkID is not in the ViolationDatabase.</exception>
		public IEnumerable<string> ExcludeNames
		{
			set 
			{						
				foreach (string exclude in value)
				{
					int index = exclude.IndexOf('@', 1);
					if (index > 0)
					{
						string checkID = exclude.Substring(0, index);
						if (!ViolationDatabase.IsValid(checkID))
							throw new ArgumentException(string.Format("{0} is not a valid checkID", checkID));
					}
				}
				
				m_checker.ExcludedNames = value;
			}
		}
		
		/// <summary>Check an assembly and return an array of violations.</summary>
		/// <remarks>OnlyType is used to check only the types that contain a string in the array which may be null.
		/// Severity is the minimum rule severity to use. IgnoreBreaks is true if we want to
		/// ignore rules that break binary compatibility. Note that this should only be called once:
		/// if you want to smoke multiple assemblies create a new AnalyzeAssembly object.</remarks>
		public Error[] Analyze(string imagePath, string[] onlyType, Severity severity, bool ignoreBreaks)
		{
			DBC.Assert(!m_analyzed, "Analyze can only be called once");	// TODO: would be nice to relax this, maybe add an overload to allow another assembly to be checked with the same settings
			m_analyzed = true;
			
			Profile.Start("AssemblyFactory");
			AssemblyDefinition assemblyDef = AssemblyFactory.GetAssembly(imagePath);
			Profile.Stop("AssemblyFactory");

			string path = Path.GetFullPath(imagePath);	// need to add a cecil search directory so that it finds assemblies in the same directory as the assembly we're checking
			string dir = Path.GetDirectoryName(path);
			BaseAssemblyResolver resolver = assemblyDef.Resolver as BaseAssemblyResolver;
			DBC.Assert(resolver != null, "assemblyDef.Resolver isn't a BaseAssemblyResolver");
			resolver.AddSearchDirectory(dir);
				
			m_symbols = new SymbolTable(imagePath);
			AssemblyCache cache = new AssemblyCache(m_symbols, assemblyDef, m_callback);

			var assembly = System.Reflection.Assembly.GetExecutingAssembly();					
			m_checker.LoadRules(assembly, cache, severity, ignoreBreaks);
											
			Error[] errors = DoCheckAssembly(assemblyDef, cache, onlyType, severity);

			return errors;
		}
		
		/// <summary>The number of rules we're using.</summary>
		/// <remarks>This will vary if types/methods/rules are filtered.</remarks>
		public int NumRules
		{
			get {return m_checker.NumRules;}
		}
		
		#region Private Methods
		private Error[] DoCheckAssembly(AssemblyDefinition assembly, AssemblyCache cache, string[] onlyType, Severity severity)
		{
			List<Error> errors = new List<Error>();
			Log.DebugLine(this, "checking assembly {0}", assembly.Name.Name);
			
			m_checker.Init(cache, errors);
						
			m_checker.Dispatcher.Dispatch(new BeginTesting());
			if (onlyType == null || onlyType.Length == 0)
				DoCheckAssembly(assembly, severity);	// note that we don't want to interleave these because our locality would suck
			
			DoCheckTypes(cache, onlyType, severity);
			DoCheckMethods(cache, onlyType, severity);
	
			if (onlyType == null || onlyType.Length == 0)
				m_checker.CheckCallGraph();
			m_checker.Dispatcher.Dispatch(new EndTesting());

			return errors.ToArray();
		}

		private void DoCheckAssembly(AssemblyDefinition assembly, Severity severity)
		{
			Profile.Start("CheckAssembly");
			m_checker.Check(assembly);
			Profile.Stop("CheckAssembly");
		}

		private void DoCheckMethods(AssemblyCache cache, string[] onlyType, Severity severity)
		{
			Profile.Start("CheckMethods");
			
			foreach (KeyValuePair<TypeDefinition, List<MethodInfo>> entry in cache.TypeMethods)
			{
				if (DoNameNeedsCheck(entry.Key.FullName, onlyType))
				{
					BeginMethods begin = new BeginMethods(entry.Key);
					m_checker.Dispatcher.Dispatch(begin);
				
					foreach (MethodInfo method in entry.Value)
					{
						m_checker.Check(method);
					}
	
					EndMethods end = new EndMethods(entry.Key);
					m_checker.Dispatcher.Dispatch(end);
				}
			}

			Profile.Stop("CheckMethods");
		}
		
		private void DoCheckTypes(AssemblyCache cache, string[] onlyType, Severity severity)
		{
			Profile.Start("CheckTypes");
			BeginTypes begin = new BeginTypes();
			m_checker.Dispatcher.Dispatch(begin);
			
			foreach (TypeDefinition type in cache.Types)
			{	
				if (DoNameNeedsCheck(type.FullName, onlyType))
				{
					m_checker.Check(type);
				}
			}
			
			EndTypes end = new EndTypes();
			m_checker.Dispatcher.Dispatch(end);
			Profile.Stop("CheckTypes");
		}

		private static bool DoNameNeedsCheck(string name, string[] only)
		{
			if (only == null || only.Length == 0)
			{
				return true;
			}
			else
			{
				foreach (string partialName in only)	// should be able to use partial, but mono 1.2.4 seems buggy
				{
					if (partialName.Length > 0 && name.Contains(partialName))
						return true;
				}
			}
			
			return false;
		}
		#endregion
		
		#region Fields
		private SymbolTable m_symbols;
		private CheckItem m_checker;
		private Rule.KeepAliveCallback m_callback;
		private bool m_analyzed;
		#endregion
	}
}