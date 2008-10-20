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
using Smokey.Framework;
using Smokey.Framework.Support;
using Smokey.Framework.Support.Advanced;
using System;
using System.Collections.Generic;

namespace Smokey.Internal.Rules
{	
	internal sealed class ClassPrefixRule : Rule
	{				
		public ClassPrefixRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "MS1029")
		{
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitType");
			dispatcher.Register(this, "VisitFini");
		}
				
		public void VisitType(TypeDefinition type)
		{						
			if (type.IsPublic)
			{
				List<string> names;
				if (!m_table.TryGetValue(type.Namespace, out names))
				{
					names = new List<string>();
					m_table.Add(type.Namespace, names);
				}
				names.Add(type.Name);
			}
		}
		
		public void VisitFini(EndTesting end)
		{						
			Unused.Value = end;
			
			string details = string.Empty;
			
			foreach (KeyValuePair<string, List<string>> entry in m_table)
			{
				if (entry.Value.Count > 4)
				{
					string prefix = DoGetPrefix(entry.Key, entry.Value);
					if (prefix != null)
					{
						if (details.Length > 0)
							details += Environment.NewLine;
						details += string.Format("{0} classes use \"{1}\"", entry.Key, prefix);
					}
				}
			}
			
			if (details.Length > 0)
			{
				Log.DebugLine(this, details); 
				Reporter.AssemblyFailed(Cache.Assembly, CheckID, details);
			}
		}
		
		private string DoGetPrefix(string ns, List<string> names)
		{
			for (int i = 0; i < names.Count/2; ++i)
			{
				string prefix = DoGetPrefix(names[i]);
				if (prefix != null)
				{
					int count = DoCountPrefix(names, prefix, i + 1);
					if (count > 0)
						Log.DebugLine(this, "{0} has {1} classes, {2} of them share {3}", ns, names.Count, count, prefix); 
					if (count > names.Count/2)
					{
						return prefix;
					}
				}
			}
			
			return null;
		}
		
		private string DoGetPrefix(string name)
		{
			string prefix = null;
			
			if (name.Length > 2)
			{
				int count = 0;
				
				// RWSet, RWBag
				if (Char.IsUpper(name[0]) && Char.IsUpper(name[1]))
				{
					while (count < name.Length && Char.IsUpper(name[count]))
						++count;
						
					if (count < name.Length)
						--count;
					else
						count = 0;
				}
				// RogueSet, RogueBag
				else if (Char.IsUpper(name[0]) && Char.IsLower(name[1]))
				{
					++count;
					while (count < name.Length && Char.IsLower(name[count]))
						++count;
						
					if (count == name.Length)
						count = 0;
				}

				if (count > 0 && count < name.Length)
				{
					prefix = name.Substring(0, count);
					if (prefix == "I")
						prefix = null;
					else
						Log.DebugLine(this, "{0} prefix {1}", name, prefix); 
				}
			}
				
			return prefix;
		}
		
		private int DoCountPrefix(List<string> names, string prefix, int startIndex)
		{
			int count = 0;

			for (int i = startIndex; i < names.Count; ++i)
			{
				if (names[i].StartsWith(prefix))
					++count;
			}
			
			return count + 1;			// add one since the original counts
		}
		
		private Dictionary<string, List<string>> m_table = new Dictionary<string, List<string>>();
	}
}

