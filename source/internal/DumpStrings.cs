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
using Smokey.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Smokey.Internal
{
	// Prints the names of methods in an assembly which may require localization.
	internal static class DumpStrings
	{			
		[Conditional("DEBUG")]
		public static void Dump(string path)
		{
			AssemblyDefinition assembly = AssemblyFactory.GetAssembly(path);

			foreach (ModuleDefinition module in assembly.Modules) 
			{
				foreach (TypeDefinition type in module.Types)
				{	
					bool added = false;
					
					if (type.IsPublic || type.IsNestedPublic)
					{							
						List<MethodDefinition> methods = new List<MethodDefinition>();
						foreach (MethodDefinition m1 in type.Constructors)
							methods.Add(m1);
						foreach (MethodDefinition m2 in type.Methods)
							methods.Add(m2);
						
						foreach (MethodDefinition method in methods)
						{
							if (DoValidMethod(method))
							{									
								string[] args = DoGetArgs(method);
								if (args.Length > 0)
								{
									added = true;
									Console.WriteLine("{0}   [{1}]", method, string.Join(", ", args));
								}
							}
							else if (DoValidProp(method))
							{
								added = true;
								Console.WriteLine(method);
							}
						}			
						
						if (added)
							Console.WriteLine();
					}
				}
			}
		}
		
		private static bool DoValidMethod(MethodDefinition method)
		{
			if (method.IsPublic)
			{
				if (!method.Name.StartsWith("set_"))
				{
					return true;
				}
			}
			
			return false;
		}
		
		private static string[] DoGetArgs(MethodDefinition method)
		{
			List<string> args = new List<string>();
			
			foreach (ParameterDefinition p in method.Parameters)
			{
				if (p.ParameterType.FullName == "System.String")
				{
					string name = p.Name.Replace("_", string.Empty).ToLower();
					if (Array.IndexOf(ms_names, name) >= 0)
					{
						args.Add(p.Name);
					}
				}
			}
			
			return args.ToArray();
		}

		private static bool DoValidProp(MethodDefinition method)
		{
			if (method.IsPublic)
			{
				if (method.Name.StartsWith("set_"))
				{
					if (method.Parameters.Count == 1)
						if (method.Parameters[0].ParameterType.FullName == "System.String")
							return true;
				}
			}
			
			return false;
		}
		
		private static string[] ms_names = new string[]{"caption", "displayname", "displaytext", "header", 
			"initialtext", "label", "message", "text", "title"};
	}
}