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

using System;
using System.Collections.Generic;
using System.IO;

namespace Smokey.Internal.Rules
{		
	internal class InconsistentNamespaceRule : Rule
	{				
		public InconsistentNamespaceRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "M1002")
		{
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitType");
		}
				
		public void VisitType(TypeDefinition type)
		{			
			string p = Cache.Symbols.Location(type, string.Empty).File;
			if (!string.IsNullOrEmpty(p))
			{
				Log.DebugLine(this, "-----------------------------------"); 
				Log.DebugLine(this, "type: {0}", type);		

				string[] path = Path.GetDirectoryName(p).Split(Path.DirectorySeparatorChar);
				if (path.Length > 0)
				{
					string[] names = type.Namespace.Split('.');
	
					int i = 0;
					while (true)
					{
						if (!DoComponentsMatch(path, names, i))
						{
							Log.DebugLine(this, "path: '{0}'", string.Join(new string(Path.DirectorySeparatorChar, 1), path));		
							Log.DebugLine(this, "namespace: {0}", string.Join(".", names));		
							
							Log.DebugLine(this, "components at {0} don't match", i);
							if (DoMatchesSibling(path, names, i))
							{
								Log.DebugLine(this, "but a sibling does match");
								Reporter.TypeFailed(type, CheckID, string.Empty);
							}
							break;
						}
						++i;
					}
				}
			}
		}		
		
		private bool DoComponentsMatch(string[] path, string[] names, int index)
		{
			int i = path.Length - index - 1;
			int j = names.Length - index - 1;
			
			if (i >= 0 && j >= 0)
			{
				return path[i].ToLower() == names[j].ToLower();
			}
			
			return false;
		}
		
		// foo/bar/gamma/
		private bool DoMatchesSibling(string[] path, string[] names, int index)
		{
			int i = path.Length - index - 1;
			int j = names.Length - index - 1;
			
			if (i > 0 && j >= 0)
			{
				string[] root = new string[i];
				Array.Copy(path, root, i);

				string p = string.Join(new string(Path.DirectorySeparatorChar, 1), root);
				Log.DebugLine(this, "checking '{0}'", p);
				
				string[] siblings = Directory.GetDirectories(p);
				foreach (string sibling in siblings)
				{
					string[] s = sibling.Split(Path.DirectorySeparatorChar);

					if (s[s.Length - 1].ToLower() == names[j].ToLower())
					{
						return true;
					}
				}
			}
						
			return false;
		}
	}
}

