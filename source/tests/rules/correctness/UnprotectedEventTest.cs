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
using System.Reflection;
using Smokey.Framework.Support;
using Smokey.Internal;
using Smokey.Internal.Rules;

namespace Smokey.Tests
{
	[TestFixture]
	public class UnprotectedEventTest : MethodTest
	{	
		#region Test classes
		public class NameEventArgs : EventArgs
		{
			public readonly string Name;
			
			public NameEventArgs(string n)
			{
				Name = n;
			}
		}
		
		public class Good1
		{
			public event EventHandler Event1;
			public static event EventHandler Event3;
			public event EventHandler<NameEventArgs> Event4;

			private Dictionary<object, EventHandler> m_eventTable = new Dictionary<object, EventHandler>();
			private static object ms_nameKey = new object();

			public event EventHandler Event6
			{
				add {m_eventTable.Add(ms_nameKey, value);}
				remove {m_eventTable.Remove(ms_nameKey);}
			}

			public void SetName1(string value) 
			{
				if (value != m_name)
				{
					m_name = value;

					if (Event1 != null)
						Event1(this, EventArgs.Empty);
				}
			}
			
			public void SetName3(string value) 
			{
				if (value != m_name)
				{
					m_name = value;

					if (Event3 != null)
						Event3(this, EventArgs.Empty);
				}
			}
			
			public void SetName4(string value) 
			{
				if (value != m_name)
				{
					m_name = value;

					if (Event4 != null)
						Event4(this, new NameEventArgs(value));
				}
			}
			
			public void SetName5(string value) 
			{
				if (value != m_name)
				{
					m_name = value;

					if (null != Event4)
						Event4(this, new NameEventArgs(value));
				}
			}
			
			public void SetName6(string value) 
			{
				if (value != m_name)
				{
					m_name = value;

					EventHandler handler = m_eventTable[ms_nameKey];
					if (handler != null)
						handler(this, EventArgs.Empty);
				}
			}
			
			private string m_name;
		}
				
		public class Bad1
		{
			public event EventHandler Event1;
			public event EventHandler Event2;
			public static event EventHandler Event3;
			public event EventHandler<NameEventArgs> Event4;

			private Dictionary<object, EventHandler> m_eventTable = new Dictionary<object, EventHandler>();
			private static object ms_nameKey = new object();

			public event EventHandler Event6
			{
				add {m_eventTable.Add(ms_nameKey, value);}
				remove {m_eventTable.Remove(ms_nameKey);}
			}

			public void SetName1(string value) 
			{
				if (value != m_name)
				{
					m_name = value;

					Event1(this, EventArgs.Empty);
				}
			}

			public void SetName2(string value) 
			{
				if (value != m_name)
				{
					m_name = value;

					if (Event2 != null)
						Event1(this, EventArgs.Empty);
				}
			}
			
			public void SetName3(string value) 
			{
				if (value != m_name)
				{
					m_name = value;

					Event3(this, EventArgs.Empty);
				}
			}
			
			public void SetName4(string value) 
			{
				if (value != m_name)
				{
					m_name = value;

					Event4(this, new NameEventArgs(value));
				}
			}
			
			public void SetName6(string value) 
			{
				if (value != m_name)
				{
					m_name = value;

					EventHandler handler = m_eventTable[ms_nameKey];
					handler(this, EventArgs.Empty);
				}
			}
			
			private string m_name;
		}
		#endregion
		
		// test code
		public UnprotectedEventTest() : base(
			new string[]{"Good1.SetName1", "Good1.SetName3", "Good1.SetName4", "Good1.SetName5", "Good1.SetName6"},
			new string[]{"Bad1.SetName1", "Bad1.SetName2", "Bad1.SetName3", "Bad1.SetName4", "Bad1.SetName6"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new UnprotectedEventRule(cache, reporter);
		}
	} 
}

