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
using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Configuration;
using System.Reflection;
using Smokey.Framework;
using Smokey.Framework.Support;
using Smokey.Internal;

namespace Smokey.Tests
{
	/// <summary>Base class for tests for type rules.</summary>
	public abstract class TypeTest : CecilTest, IReportViolations
	{	
		[TestFixtureTearDown]
		public void TearDown()
		{
			m_good.Clear();
			m_bad.Clear();
			m_used.Clear();
		}
		
		/// <summary>Good and bad names should be a list of type names.</summary>
		public TypeTest(string[] good, string[] bad) : this(good, bad, new string[0])
		{
		}
								
		/// <summary>Good and bad names should be a list of type names. Used is a list
		/// of type names that aren't checked, but are used during testing.</summary>
		public TypeTest(string[] good, string[] bad, string[] used) : this(good, bad, used, System.Reflection.Assembly.GetExecutingAssembly().Location)
		{
		}
				
		/// <summary>This is used for custom test dlls.</summary>
		public TypeTest(string[] good, string[] bad, string[] used, string loc) : base(loc)
		{
			try
			{					
				DoGetTypes(Assembly, m_good, good);	
				DoGetTypes(Assembly, m_bad, bad);
				DoGetTypes(Assembly, m_used, used);
			}
			catch (Exception e)
			{
				Console.Error.WriteLine(e.Message);	// nunit doesn't print exceptions from TextFixtures...
				throw;
			}
		}
				
		[Test]
		public void Test()
		{
			// Create the cache and the dispatcher.
			List<TypeDefinition> types = new List<TypeDefinition>();
			types.AddRange(m_good);	
			types.AddRange(m_bad);
			types.AddRange(m_used);
			types.Add(Assembly.MainModule.Types[GetType().FullName]);		// some of the tests require an externally visible type

			AssemblyCache cache = new AssemblyCache(Assembly, types);
			RuleDispatcher dispatcher = new RuleDispatcher();

			// Create the rule.
			Rule rule = OnCreate(cache, this);		// note that dispatcher will keep the rule from being GCed
			rule.Register(dispatcher);
			
			// Test the good types.
			foreach (TypeDefinition type in m_good)
			{
				m_failed = false;
				DoVisit(dispatcher, type);
			
				if (m_failed)
					Assert.Fail(string.Format("{0} should have passed", type));
			}
			
			// Test the bad types.
			foreach (TypeDefinition type in m_bad)
			{
				m_failed = false;
				DoVisit(dispatcher, type);
			
				if (!m_failed)
					Assert.Fail(string.Format("{0} should have failed", type));
			}
		}
		
		private void DoVisit(RuleDispatcher dispatcher, TypeDefinition type)
		{
			dispatcher.Dispatch(type);
			
			// Some rules consider all the methods in a type. These rules cannot be tested
			// with a MethodTest so we'll visit methods here in order to be able to unit
			// test those rules.
			dispatcher.Dispatch(new BeginMethods(type));
			foreach (MethodDefinition method in type.Methods)
			{
				var minfo = new Smokey.Framework.Support.MethodInfo(type, method);
				dispatcher.Dispatch(minfo);
			}
			dispatcher.Dispatch(new EndMethods(type));
		}
		
		public void AssemblyFailed(AssemblyDefinition assembly, string checkID, string details)
		{
			DBC.Fail("shouldn't get type failed for a type rule!");
		}

		public void TypeFailed(TypeDefinition type, string checkID, string details)
		{
			m_failed = true;
		}

		public void MethodFailed(MethodDefinition type, string checkID, int offset, string details)	
		{
			DBC.Fail("shouldn't get method failed for a type rule!");
		}

		#region Protected methods
		protected abstract Rule OnCreate(AssemblyCache cache, IReportViolations reporter);
		#endregion
		
		#region Private methods
		private void DoGetTypes(AssemblyDefinition assembly, List<TypeDefinition> types, string[] names)
		{
			string testName = GetType().FullName;
			
			foreach (string name in names)
			{				
				DBC.Assert(name.IndexOf('.') < 0, "{0} has a method", name);
				
				string fullName = string.Format("{0}/{1}", testName, name);
				TypeDefinition type = assembly.MainModule.Types[fullName];
				DBC.Assert(type != null, "Couldn't find {0}", fullName);

				types.Add(type);
			}
		}
		#endregion 
		
		#region Fields
		private List<TypeDefinition> m_good = new List<TypeDefinition>();
		private List<TypeDefinition> m_bad = new List<TypeDefinition>();
		private List<TypeDefinition> m_used = new List<TypeDefinition>();
		private bool m_failed;
		#endregion
	} 
}