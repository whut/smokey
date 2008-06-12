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
using System.Collections.Generic;
using Smokey.Framework;
using Smokey.Framework.Support;

namespace Smokey.Tests
{
	/// <summary>Base class for tests for events.</summary>
	public abstract class EventTest : CecilTest, IReportViolations
	{	
		[TestFixtureTearDown]
		public void TearDown()
		{
			m_good.Clear();
			m_bad.Clear();
			m_used.Clear();
		}
		
		/// <summary>Good and bad names should be of the form "TestClass.EventName".</summary>
		public EventTest(string[] good, string[] bad) : this(good, bad, new string[0])
		{
		}
																				
		/// <summary>Good and bad names should be of the form "TestClass.EventName". 
		/// Used should be type names.</summary>
		public EventTest(string[] good, string[] bad, string[] used) : this(good, bad, used, System.Reflection.Assembly.GetExecutingAssembly().Location)
		{
		}
				
		/// <summary>This is used for custom test dlls.</summary>
		public EventTest(string[] good, string[] bad, string[] used, string loc) : base(loc)
		{
			try
			{			
				DoGetEvents(Assembly, m_good, good);	
				DoGetEvents(Assembly, m_bad, bad);
				DoGetUsed(Assembly, m_used, used);
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
			List<EventDefinition> events = new List<EventDefinition>();
			events.AddRange(m_good);	
			events.AddRange(m_bad);
			
			List<TypeDefinition> types = new List<TypeDefinition>();
			types.AddRange(m_used);
			
			AssemblyCache cache = new AssemblyCache(Assembly, types);
			RuleDispatcher dispatcher = new RuleDispatcher();

			// Create the rule.
			Rule rule = OnCreate(cache, this);		// note that dispatcher will keep the rule from being GCed
			rule.Register(dispatcher);
			
			// Test the good events.
			foreach (EventDefinition evt in m_good)
			{
				m_failed = false;
				dispatcher.Dispatch(evt);
			
				if (m_failed)
					Assert.Fail(string.Format("good {0} should have passed", evt.Name));
			}
			
			// Test the bad events.
			foreach (EventDefinition evt in m_bad)
			{
				m_failed = false;
				dispatcher.Dispatch(evt);
			
				if (!m_failed)
					Assert.Fail(string.Format("bad {0} should have failed", evt.Name));
			}
		}
		
		public void AssemblyFailed(AssemblyDefinition assembly, string checkID, string details)
		{
			DBC.Fail("shouldn't get type failed for an event rule!");
		}

		public void TypeFailed(TypeDefinition type, string checkID, string details)
		{
			m_failed = true;
		}

		public void MethodFailed(MethodDefinition method, string checkID, int offset, string details)	
		{
			DBC.Fail("shouldn't get method failed for an event rule!");
		}

		#region Protected methods
		protected abstract Rule OnCreate(AssemblyCache cache, IReportViolations reporter);
		#endregion
		
		#region Private methods
		private void DoGetEvents(AssemblyDefinition assembly, List<EventDefinition> events, string[] names)
		{
			string testName = GetType().FullName;
			
			foreach (string name in names)
			{
				string[] parts = name.Split('.');
				DBC.Assert(parts.Length == 2, "{0} should be of the form 'test.event'", name);
				
				string caseType = parts[0];
				string fullName = string.Format("{0}/{1}", testName, caseType);
				TypeDefinition type = assembly.MainModule.Types[fullName];
				DBC.Assert(type != null, "null type from {0}", fullName);
				
				string caseEvent = parts[1];
				DBC.Assert(caseEvent.Length > 0, "{0} has no event part", name);
				
				EventDefinition evt = type.Events.GetEvent(caseEvent);
				DBC.Assert(evt != null, "couldn't find an event for {0}", caseType + "::" + caseEvent);
				
				events.Add(evt);
			}
		}

		private void DoGetUsed(AssemblyDefinition assembly, List<TypeDefinition> types, string[] names)
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
		private List<EventDefinition> m_good = new List<EventDefinition>();
		private List<EventDefinition> m_bad = new List<EventDefinition>();
		private List<TypeDefinition> m_used = new List<TypeDefinition>();
		private bool m_failed;
		#endregion
	} 
}