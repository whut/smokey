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
			m_used.Clear();
		}
		
		/// <summary>Good and bad names should be of the form "TestClass".</summary>
		public AssemblyTest(string[] good, string[] bad) : this(good, bad, new string[0])
		{
		}
																
		/// <summary>Good, badm and used names should be of the form "TestClass". Used is a list
		/// of type names that aren't checked, but are used during testing.</summary>
		public AssemblyTest(string[] good, string[] bad, string[] used)
		{
			try
			{			
				DoGetTypes(m_good, good);	
				DoGetTypes(m_bad, bad);
				DoGetTypes(m_used, used);
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
			List<TypeDefinition> types = new List<TypeDefinition>(m_good);
			types.AddRange(m_used);
			
			AssemblyCache cache = new AssemblyCache(Assembly, types);
			Rule rule = OnCreate(cache, this);		

			foreach (TypeDefinition type in m_good)
			{
				RuleDispatcher dispatcher = new RuleDispatcher();
				rule.Register(dispatcher);
				m_failed = false;

				dispatcher.Dispatch(Assembly);
				dispatcher.Dispatch(type);
				
				foreach (MethodInfo info in cache.Methods)
				{
					if (info.Type.MetadataToken == type.MetadataToken)	
						dispatcher.Dispatch(info);
						
					else if (m_used.Exists(e => e.MetadataToken == info.Type.MetadataToken))
						dispatcher.Dispatch(info);
				}
			
				dispatcher.DispatchCallGraph();
				if (m_failed)
					Assert.Fail("good cases {0} should have passed", type.Name);
			}
			
			// Test the bad cases.
			types = new List<TypeDefinition>(m_bad);
			types.AddRange(m_used);
			
			cache = new AssemblyCache(Assembly, types);
			rule = OnCreate(cache, this);		

			foreach (TypeDefinition type in m_bad)
			{
				RuleDispatcher dispatcher = new RuleDispatcher();
				rule.Register(dispatcher);
				m_failed = false;

				dispatcher.Dispatch(Assembly);
				dispatcher.Dispatch(type);

				foreach (MethodInfo info in cache.Methods)
				{
					if (info.Type.MetadataToken == type.MetadataToken)	
						dispatcher.Dispatch(info);
						
					else if (m_used.Exists(e => e.MetadataToken == info.Type.MetadataToken))
						dispatcher.Dispatch(info);
				}
	
				dispatcher.DispatchCallGraph();
				if (!m_failed)
					Assert.Fail("bad cases {0} should have failed", type.Name);
			}			
		}
		
		public void AssemblyFailed(AssemblyDefinition assembly, string checkID, string details)
		{
			m_failed = true;
		}

		public void TypeFailed(TypeDefinition type, string checkID, string details)
		{
			DBC.Fail("shouldn't get type failed for an assembly rule!");
		}

		public void MethodFailed(MethodDefinition method, string checkID, int offset, string details)	
		{
			DBC.Fail("shouldn't get method failed for an assembly rule!");
		}

		#region Protected Methods ---------------------------------------------
		protected abstract Rule OnCreate(AssemblyCache cache, IReportViolations reporter);
		#endregion
		
		#region Private Methods -----------------------------------------------
		private void DoGetTypes(List<TypeDefinition> types, string[] names)
		{
			DBC.Assert(names.Distinct().Count() == names.Length, "duplicate name in " + string.Join(", ", names));

			string testName = GetType().FullName;
			
			foreach (string name in names)
			{				
				if (name.IndexOf('.') < 0)
				{
					string fullName = string.Format("{0}/{1}", testName, name);
					TypeDefinition type = Assembly.MainModule.Types[fullName];
					DBC.Assert(type != null, "Couldn't find {0}", fullName);

					types.Add(type);
				}
			}
		}
		#endregion 
		
		#region Fields --------------------------------------------------------
		private List<TypeDefinition> m_good = new List<TypeDefinition>();
		private List<TypeDefinition> m_bad = new List<TypeDefinition>();
		private List<TypeDefinition> m_used = new List<TypeDefinition>();
		private bool m_failed;
		#endregion
	} 
}