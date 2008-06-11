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
//using System.Reflection;

namespace Smokey.Tests
{
	/// <summary>Base class for tests for method rules.</summary>
	public abstract class MethodTest : CecilTest, IReportViolations
	{	
		[TestFixtureTearDown]
		public void TearDown()
		{
			m_good.Clear();
			m_bad.Clear();
			m_used.Clear();
		}
		
		/// <summary>Good and bad names should be of the form "TestClass.TestMethod".</summary>
		public MethodTest(string[] good, string[] bad) : this(good, bad, new string[0])
		{
		}
								
		/// <summary>Good and bad names should be of the form "TestClass.TestMethod". Used may
		/// have no method in which case all of the methods that haven't already been
		/// added are added.</summary>
		public MethodTest(string[] good, string[] bad, string[] used)
		{
			try
			{			
				DoGetMethods(Assembly, m_good, good, false);	
				DoGetMethods(Assembly, m_bad, bad, false);
				DoGetMethods(Assembly, m_used, used, true);
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
			// Create the cache and the dispatcher.
			List<MethodInfo> methods = new List<MethodInfo>();
			methods.AddRange(m_good);	
			methods.AddRange(m_bad);
			methods.AddRange(m_used);
			
			AssemblyCache cache = new AssemblyCache(Assembly, methods);
			RuleDispatcher dispatcher = new RuleDispatcher();

			// Create the rule.
			Rule rule = OnCreate(cache, this);		// note that dispatcher will keep the rule from being GCed
			rule.Register(dispatcher);
			
			// Test the good methods.
			foreach (MethodInfo method in m_good)
			{
				m_failed = false;
				dispatcher.Dispatch(method.Method);
				dispatcher.Dispatch(method);
			
				if (m_failed)
					Assert.Fail(string.Format("{0} should have passed", method.Method));
			}
			
			// Test the bad methods.
			foreach (MethodInfo method in m_bad)
			{
				m_failed = false;
				dispatcher.Dispatch(method.Method);
				dispatcher.Dispatch(method);
			
				if (!m_failed)
					Assert.Fail(string.Format("{0} should have failed", method.Method));
			}
		}
		
		public void AssemblyFailed(AssemblyDefinition assembly, string checkID, string details)
		{
			DBC.Fail("shouldn't get type failed for a method rule!");
		}

		public void TypeFailed(TypeDefinition type, string checkID, string details)
		{
			DBC.Fail("shouldn't get type failed for a method rule!");
		}

		public void MethodFailed(MethodDefinition method, string checkID, int offset, string details)	
		{
			DBC.Pre(offset >= 0, "offset is negative");
			m_failed = true;
		}

		#region Protected Methods ---------------------------------------------
		protected abstract Rule OnCreate(AssemblyCache cache, IReportViolations reporter);
		#endregion
		
		#region Private Methods -----------------------------------------------
		private void DoGetMethods(AssemblyDefinition assembly, List<MethodInfo> methods, string[] names, bool allowEmpty)
		{
			string testName = GetType().FullName;
			
			DBC.Assert(names.Distinct().Count() == names.Length, "duplicate name in " + string.Join(", ", names));
			
			foreach (string name in names)
			{
				string[] parts = name.Split('.');
				DBC.Assert(parts.Length == 2, "{0} should be of the form 'test.method'", name);
				
				string caseType = parts[0];
				string fullName = string.Format("{0}/{1}", testName, caseType);
				TypeDefinition type = assembly.MainModule.Types[fullName];
				DBC.Assert(type != null, "null type from {0}", fullName);
				
				string caseMethod = parts[1];
				if (caseMethod.Length > 0)
				{
					if (caseMethod == caseType)
					{
						DBC.Assert(type.Constructors.Count > 0, "expected a ctor, but {0} has no ctors", caseType);

						foreach (MethodDefinition method in type.Constructors)
							methods.Add(new MethodInfo(type, method));
					}
					else
					{
						MethodDefinition[] defs = type.Methods.GetMethod(caseMethod);
						DBC.Assert(defs.Length == 1, "expected 1 method, but {0} has {1} methods", caseType + "::" + caseMethod, defs.Length);
						methods.Add(new MethodInfo(type, defs[0]));
					}					
				}
				else
				{
					DBC.Assert(allowEmpty, "{0} needs a method name", name);
					
					foreach (MethodDefinition method in type.Constructors)
						methods.Add(new MethodInfo(type, method));

					foreach (MethodDefinition method in type.Methods)
						methods.Add(new MethodInfo(type, method));
				}
			}
		}
		#endregion 
		
		#region Fields --------------------------------------------------------
		private List<MethodInfo> m_good = new List<MethodInfo>();
		private List<MethodInfo> m_bad = new List<MethodInfo>();
		private List<MethodInfo> m_used = new List<MethodInfo>();
		private bool m_failed;
		#endregion
	} 
}