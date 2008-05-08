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
using System;

namespace Smokey.Framework.Support
{	
	/// <summary>Base class for all rules.</summary>
	[DisableRule("D1032", "UnusedMethod")]		// rules use reflection to call methods so we'll disable this rule for all methods
	[DisableRule("P1012", "NotInstantiated")]	// rules use reflection to call methods so we'll disable this rule for all methods
	[DisableRule("D1041", "CircularReference")]	// Smokey.Framework.Support.Rule <-> Smokey.Framework.Support.RuleDispatcher  
	public abstract class Rule
	{		
		// Used internally during long operations to prevent the watchdog from
		// killing the process.
		public delegate void KeepAliveCallback(string name);

		protected Rule(AssemblyCache cache, IReportViolations reporter, string checkID)
		{
			DBC.Pre(cache != null, "cache is null");
			DBC.Pre(reporter != null, "reporter is null");
			DBC.Pre(!string.IsNullOrEmpty(checkID), "checkID is null or empty");

			m_cache = cache;
			m_reporter = reporter;
			m_checkID = checkID;
		}
		
		public abstract void Register(RuleDispatcher dispatcher);
	
		public AssemblyCache Cache 			{get {return m_cache;}}
		public IReportViolations Reporter 	{get {return m_reporter;}}
		public string CheckID 				{get {return m_checkID;}}
		public TargetRuntime Runtime 		{get {return m_runtime;} set {m_runtime = value;}}
		
		#region Fields		
		AssemblyCache m_cache;
		private IReportViolations m_reporter;
		private string m_checkID;
		private TargetRuntime m_runtime = TargetRuntime.NET_1_0;
		#endregion
	}
}
