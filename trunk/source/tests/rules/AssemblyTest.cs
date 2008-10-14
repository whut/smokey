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
using NUnit.Framework;

using Smokey.Framework;
using Smokey.Framework.Support;
using Smokey.Framework.Support.Advanced;
using Smokey.Internal;

using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace Smokey.Tests
{
	/// <summary>Base class for tests for assembly rules.</summary>
	public abstract class AssemblyTest : CecilTest, IReportViolations
	{	
		[TestFixtureTearDown]
		public void TearDown()
		{
			m_good.Clear();
			m_bad.Clear();
			m_usedTypes.Clear();
		}
		
		/// <summary>Good and bad names should be of the form "TestClass". If multiple types 
		/// should be visited during a single test concatenate the type names with "+".</summary>
		public AssemblyTest(string[] good, string[] bad) : this(good, bad, new string[0])
		{
		}
																
		/// <summary>Good, bad, and used names should be of the form "TestClass". If multiple 
		/// types should be visited during a single test concatenate the type names with "+". 
		/// Used is a list of type names that aren't checked, but are used during testing.</summary>
		public AssemblyTest(string[] good, string[] bad, string[] used) : this(good, bad, used, System.Reflection.Assembly.GetExecutingAssembly().Location)
		{
		}
				
		/// <summary>This is used for custom test dlls.</summary>
		public AssemblyTest(string[] good, string[] bad, string[] used, string loc) : base(loc)
		{
			try
			{			
				DoGetTypes(m_good, good);			
				DoGetTypes(m_bad, bad);

				var temp = new List<List<TypeDefinition>>();
				DoGetTypes(temp, used);
				
				foreach (var l in temp)
					foreach (var t in l)
						m_usedTypes.Add(t);
			}
			catch (Exception e)
			{
				Console.Error.WriteLine(e.Message);	// nunit doesn't print exceptions from TextFixtures...
				Console.Error.WriteLine(e.StackTrace);
				throw;
			}
		}
				
		[Test]
		public void Test()
		{
			// Test the good cases.
			List<List<TypeDefinition>> types = new List<List<TypeDefinition>>(m_good);
			
			foreach (List<TypeDefinition> cases in types)
			{
				cases.AddRange(m_usedTypes);
				AssemblyCache cache = new AssemblyCache(Assembly, cases);
				Rule rule = OnCreate(cache, this);		

				DoRunTest(rule, cases, cache);
				if (m_failed)
					Assert.Fail("good cases {0} should have passed", string.Join("+", cases.Select(c => c.Name).ToArray()));
			}
			
			// Test the bad cases.
			types = new List<List<TypeDefinition>>(m_bad);
			
			foreach (List<TypeDefinition> cases in types)
			{
				cases.AddRange(m_usedTypes);
				AssemblyCache cache = new AssemblyCache(Assembly, cases);
				Rule rule = OnCreate(cache, this);		

				DoRunTest(rule, cases, cache);
				if (!m_failed)
					Assert.Fail("bad cases {0} should have failed", string.Join("+", cases.Select(c => c.Name).ToArray()));
			}			
		}
		
		public void AssemblyFailed(AssemblyDefinition assembly, string checkID, string details)
		{
			m_failed = true;
		}

		public void TypeFailed(TypeDefinition type, string checkID, string details)
		{
			m_failed = true;
		}

		public void MethodFailed(MethodDefinition method, string checkID, int offset, string details)	
		{
			m_failed = true;
		}

		#region Protected Methods ---------------------------------------------
		protected abstract Rule OnCreate(AssemblyCache cache, IReportViolations reporter);
		#endregion
		
		#region Private Methods -----------------------------------------------
		private void DoRunTest(Rule rule, List<TypeDefinition> types, AssemblyCache cache)
		{
			RuleDispatcher dispatcher = new RuleDispatcher();
			rule.Register(dispatcher);
			m_failed = false;

			dispatcher.Dispatch(new BeginTesting());
			dispatcher.Dispatch(Assembly);
			
			foreach (TypeDefinition t in types)
				dispatcher.Dispatch(t);

			foreach (MethodInfo info in cache.Methods)
			{
				dispatcher.Dispatch(info);
			}

			dispatcher.DispatchCallGraph();
			dispatcher.Dispatch(new EndTesting());
		}
		
		private void DoGetTypes(List<List<TypeDefinition>> types, string[] names)
		{
			DBC.Assert(names.Distinct().Count() == names.Length, "duplicate name in " + string.Join(", ", names));

			string testName = GetType().FullName;
			
			foreach (string composed in names)
			{
				string[] compose = composed.Split('+');
				
				List<TypeDefinition> inner = new List<TypeDefinition>();
				foreach (string name in compose)
				{				
					DBC.Assert(name.IndexOf('.') < 0, "type name has a period");
					
					string fullName = string.Format("{0}/{1}", testName, name);
					TypeDefinition type = Assembly.MainModule.Types[fullName];
					DBC.Assert(type != null, "Couldn't find {0}", fullName);
	
					inner.Add(type);
				}
				
				types.Add(inner);
			}
		}
		#endregion 
		
		#region Fields --------------------------------------------------------
		private List<List<TypeDefinition>> m_good = new List<List<TypeDefinition>>();
		private List<List<TypeDefinition>> m_bad = new List<List<TypeDefinition>>();
		private List<TypeDefinition> m_usedTypes = new List<TypeDefinition>();
		private bool m_failed;
		#endregion
	} 
}