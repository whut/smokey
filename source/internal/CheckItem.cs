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
using Smokey.App;
using Smokey.Framework;
using Smokey.Framework.Support;
using System;
using System.Collections.Generic;

namespace Smokey.Internal
{	
	// Uses introspection to find and store the type rules.
	internal class CheckItem : IReportViolations
	{				
#if UNUSED
		public CheckItem()
		{
			m_dispatcher = new RuleDispatcher(this);
		}
#endif

		public CheckItem(Rule.KeepAliveCallback callback)
		{
			m_callback = callback;
			m_dispatcher = new RuleDispatcher(this);
		}
		
		public RuleDispatcher Dispatcher
		{
			get {return m_dispatcher;}
		}
		
		public string[] ExcludedChecks
		{
			set {m_excludedChecks = value;}
		}
		
		internal string[] ExcludedNames	// of types, methods, etc
		{
			set 	
			{
				List<string> names = new List<string>();
				Dictionary<string, List<string>> table = new Dictionary<string, List<string>>();
				
				foreach (string exclude in value)
				{
					int index = exclude.IndexOf('@');
					if (exclude.IndexOf('@') >= 0)
					{
						string checkID = exclude.Substring(0, index);
						string name = exclude.Substring(index + 1);
						
						List<string> checkIDs;
						if (!table.TryGetValue(name, out checkIDs))
						{
							checkIDs = new List<string>();
							table.Add(name, checkIDs);
						}
						
						checkIDs.Add(checkID);
					}
					else
						names.Add(exclude);
				}

				m_dispatcher.ExcludedNames = table;
				m_excludedNames = names.ToArray();
			}
		}
		
		public void LoadRules(System.Reflection.Assembly assembly, AssemblyCache cache, Severity severity, bool ignoreBreaks)
		{
			DBC.Assert(cache != null, "cache is null");

			Profile.Start("LoadRules");
			DoLoadRules(assembly, cache, severity, ignoreBreaks);
			DoLoadCustomRules(cache, severity, ignoreBreaks);
			Profile.Stop("LoadRules");
		}
		
		public void Init(AssemblyCache cache, List<Error> errors)
		{
			DBC.Assert(cache != null, "cache is null");
			DBC.Assert(errors != null, "errors is null");

			m_cache = cache;
			m_errors = errors;			
		}
		
		public void Check(AssemblyDefinition assembly)
		{
			m_dispatcher.Dispatch(assembly);
		}
		
		public void Check(TypeDefinition type)
		{			
			if (!type.Name.Contains("CompilerGenerated"))
				DoCheckType(type);
		}
		
		public void Check(MethodInfo method)
		{			
			if (method.Method.Body != null)
				DoCheckMethod(method);
		}
		
		public void CheckCallGraph()
		{
			m_dispatcher.DispatchCallGraph();
		}
		
		public int NumRules
		{
			get {return m_rules.Count;}
		}
		
		public void AssemblyFailed(AssemblyDefinition assembly, string checkID, string details)
		{
			if (assembly == null || !assembly.CustomAttributes.HasDisableRule(checkID))
			{
				Location loc = new Location();
				loc.Name = assembly != null ? string.Format("Assembly: {0}", assembly.Name.Name) : string.Empty;
				loc.Line = -1;
				loc.Details = details;
				
				Violation violation = ViolationDatabase.Get(checkID);
				m_errors.Add(new Error(loc, violation));
			}
		}

		public void TypeFailed(TypeDefinition type, string checkID, string details)
		{
			if (!type.CustomAttributes.HasDisableRule(checkID))
			{
			DBC.Assert(m_cache != null, "m_cache is null");
			DBC.Assert(m_cache.Symbols != null, "m_cache.Symbols is null");
			DBC.Assert(type != null, "type is null");
			DBC.Assert(details != null, "details is null");

				Location location = m_cache.Symbols.Location(type, details);
				Violation violation = ViolationDatabase.Get(checkID);
			DBC.Assert(violation != null, "violation is null");
			DBC.Assert(m_errors != null, "m_errors is null");
						
				m_errors.Add(new Error(location, violation));
			}
		}

		public void MethodFailed(MethodDefinition method, string checkID, int offset, string details)
		{
			if (!method.CustomAttributes.HasDisableRule(checkID))
			{
				Location location = m_cache.Symbols.Location(method, offset, details);
				Violation violation = ViolationDatabase.Get(checkID);
						
				m_errors.Add(new Error(location, violation));
			}
		}

		#region Private methods				
		private void DoCheckType(TypeDefinition type)
		{			
			if (DoObjectRequiresCheck(type.FullName))
			{
				m_callback(type.FullName);
				m_dispatcher.Dispatch(type);
			}
		}

		private void DoCheckMethod(MethodInfo info)
		{			
			if (DoObjectRequiresCheck(info.Method.ToString()))
			{
				m_callback(info.Method.ToString());
				m_dispatcher.Dispatch(info);
			}
		}

		// Don't bother checking a type/method if we're excluding all rules for it.
		private bool DoObjectRequiresCheck(string fullName)
		{			
			// This is slow, but there shouldn't be too many of these...
			foreach (string name in m_excludedNames)
			{
				if (fullName.Contains(name))
					return false;
			}

			return true;
		}

		private void DoLoadCustomRules(AssemblyCache cache, Severity severity, bool ignoreBreaks)
		{
			string paths = Settings.Get("custom", string.Empty);
			foreach (string path in paths.Split(':'))
			{
				try
				{
					if (path.Length > 0)
					{
						System.Reflection.Assembly assembly = System.Reflection.Assembly.LoadFrom(path);
						ViolationDatabase.LoadCustom(assembly);
						DoLoadRules(assembly, cache, severity, ignoreBreaks);
					}
				}
				catch (Exception e)
				{
					Console.Error.WriteLine("Couldn't load {0}", path);
					Console.Error.WriteLine(e.Message);
					Console.Error.WriteLine(e.StackTrace);
				}
			}
		}
	
		private void DoLoadRules(System.Reflection.Assembly assembly, AssemblyCache cache, Severity severity, bool ignoreBreaks)
		{			
			TargetRuntime runtime = cache.Assembly.Runtime;
			
			Type[] types = assembly.GetTypes();
			foreach (Type t in types)
			{
				if (t.IsClass && !t.IsAbstract && typeof(Rule).IsAssignableFrom(t))
				{
					try
					{
						object o = Activator.CreateInstance(t, new object[]{cache, this});
						Rule rule = (Rule) o;
						
						if (rule.Runtime <= runtime)
						{
							if (DoRuleRequiresCheck(rule.CheckID, severity, ignoreBreaks))
							{
								rule.Register(m_dispatcher);
								m_rules.Add(rule);
							}
						}
					}
					catch (Exception e)
					{
						Console.Error.WriteLine("Couldn't instantiate {0}", t);
						Console.Error.WriteLine(e.Message);
						Log.ErrorLine(this, e.StackTrace);
					}
				}
			}
		}
		
		// Don't bother with a rule if the user doesn't care about rules of that
		// severity or he's excluded the rule.
		private bool DoRuleRequiresCheck(string checkID, Severity severity, bool ignoreBreaks)
		{			
			Violation violation = ViolationDatabase.Get(checkID);
			
			if (violation.Severity > severity)
				return false;
				
			if (ignoreBreaks && violation.Breaking)
				return false;
		
			if (Array.IndexOf(m_excludedChecks, checkID) >= 0)
				return false;
		
			return true;
		}
		#endregion

		#region Fields
		private RuleDispatcher m_dispatcher;
		private List<Rule> m_rules = new List<Rule>();
		private Rule.KeepAliveCallback m_callback;
		private string[] m_excludedChecks = new string[0];
		private string[] m_excludedNames = new string[0];

		private List<Error> m_errors;
		private AssemblyCache m_cache;
		#endregion
	}
}