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
using Mono.Cecil.Cil;
using System;
using System.IO;
using System.Collections.Generic;
using Smokey.Framework;
using Smokey.Framework.Instructions;
using Smokey.Framework.Support;

namespace Smokey.Internal.Rules
{	
	internal sealed class SpecialFolderRule : Rule
	{				
		public SpecialFolderRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "PO1003")
		{
			string user = Environment.UserName;
			
			Array values = Enum.GetValues(typeof(Environment.SpecialFolder));			
			foreach (object o in values)
			{
				Environment.SpecialFolder name = (Environment.SpecialFolder) o;
				string path = Environment.GetFolderPath(name);
				if (path.Length > 0)
				{
					path = path.Replace(user, "*");
					if (!m_globs.ContainsKey(name))
						m_globs.Add(name, path.Split(Path.DirectorySeparatorChar));
				}
			}
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitBegin");
			dispatcher.Register(this, "VisitLoad");
			dispatcher.Register(this, "VisitEnd");
		}
				
		public void VisitBegin(BeginMethod method)
		{
			Log.DebugLine(this, "-----------------------------------"); 
			Log.DebugLine(this, "{0:F}", method.Info.Instructions);				

			m_offset = -1;
			m_details = string.Empty;			
		}
		
		public void VisitLoad(LoadString load)
		{
			if (m_offset < 0)
				if (DoMatch(load.Value))
					m_offset = load.Untyped.Offset;						
		}

		public void VisitEnd(EndMethod method)
		{
			if (m_offset >= 0)
			{
				Log.DebugLine(this, m_details);
				Reporter.MethodFailed(method.Info.Method, CheckID, m_offset, m_details);
			}
		}
				
		private bool DoMatch(string path)
		{
			string[] components = path.Split(Path.DirectorySeparatorChar);
			foreach (KeyValuePair<Environment.SpecialFolder, string[]> pair in m_globs)
			{
				bool match = DoMatch(components, pair.Value);

				if (match)
				{
					m_details = string.Format("Use SpecialFolder.{0} with '{1}'", pair.Key, path);
					return true;
				}
			}
			
			return false;
		}
				
		private static bool DoMatch(string[] components, string[] glob)
		{
			bool matches = components.Length >= glob.Length;

			for (int i = 0; i < glob.Length && matches; ++i)
			{
				if (glob[i] != "*")
					if (components[i] != glob[i])
						matches = false;
			}
			
			return matches;
		}
				
		private int m_offset;
		private string m_details;
		private Dictionary<Environment.SpecialFolder, string[]> m_globs = new Dictionary<Environment.SpecialFolder, string[]>();
	}
}
