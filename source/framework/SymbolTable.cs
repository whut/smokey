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

using Smokey.Internal;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace Smokey.Framework
{		
	internal sealed class SymbolTable
	{				
		public Location Location(TypeDefinition type, string details)
		{
			var info = DoFindFileLine(type);

			Location location = new Location();			
			location.File = info.First;			
			location.Line = info.Second;
			location.Offset = -1;
			location.Name = "Type: " + type.FullName;
			location.Details = details;
								
			return 	location;
		}
		
		public Location Location(MethodDefinition method, int offset, string details)
		{			
			var info = DoFindFileLine(method, offset);

			Location location = new Location();
			location.File = info.First;			
			location.Line = info.Second;
			location.Name = "Method: " + method.ToString();			
			location.Offset = offset;
			location.Details = details;
					
			return location;
		}
		
		public string LocalName(MethodDefinition method, Instruction instruction, int index)
		{
			string name = "V_" + index;

			if (HaveLocalNames(method))
				name = DoLocalName(method, instruction, index);
								
			return name;
		}
				
		public bool HaveLocalNames(MethodDefinition method)
		{
			return method.Body != null && method.Body.Variables != null;
		}
				
		#region Private Methods -----------------------------------------------
		private string DoLocalName(MethodDefinition method, Instruction instruction, int index)
		{
			string name = "V_" + index;
						
			if (index < method.Body.Variables.Count)
				name = method.Body.Variables[index].Name;
			else
				Log.ErrorLine(this, "Instruction at {0:X2} in {1} has a bad index {2}", instruction.Offset, method, index);
			
			return name;
		}
				
		private Scope DoFindScope(ScopeCollection scopes, Instruction instruction)
		{
			Scope result = null;
			
			for (int i = 0; i < scopes.Count && result == null; ++i)
			{
				Scope candidate = scopes[i];

				if (candidate.Start.Offset <= instruction.Offset && instruction.Offset <= candidate.End.Offset)
					result = candidate;
				else
					result = DoFindScope(candidate.Scopes, instruction);
			}
				
			return result;
		}
		
		private Tuple2<string, int> DoFindFileLine(TypeDefinition type)
		{
			var methods = new List<MethodDefinition>(type.Constructors.Count + type.Methods.Count);
			foreach (MethodDefinition m in type.Constructors)		// these aren't IEnumerable<T> so we need to explicitly loop
				methods.Add(m);
			foreach (MethodDefinition m in type.Methods)
				methods.Add(m);
			
			var infos = from method in methods
							let info = DoGetFileLine(method, 0)
							where info.First != "<unknown>"
							group info by info.First;
			
			Tuple2<string, int> result = Tuple.Make("<unknown>", -1);
			if (infos.Count() == 1)
			{
				// If the methods and constructors are all in a single file then
				// we can get a good line number.
				var lines = from info in infos.First()
							orderby info.Second ascending 
							select info.Second;
				result = Tuple.Make(infos.First().Key, lines.First());
			}
			else if (infos.Count() > 1)
			{
				// But if the type is spread across multiple files we can't
				// return a meaningful line number.
				var files = from info in infos 
							select info.Key;
				result = Tuple.Make(string.Join(" ", files.ToArray()), -1);
			}
			
			return result;
		}
				
		private Tuple2<string, int> DoFindFileLine(MethodDefinition method, int offset)
		{
			Tuple2<string, int> result = DoGetFileLine(method, offset);
			
			if (result.First == "<unknown>")
			{
				TypeDefinition type = method.DeclaringType as TypeDefinition;
				if (type != null)
				{
					var temp = DoFindFileLine(type);
					Tuple.Make(temp.First, -1);
				}
			}
			
			return result;
		}
		
		private Tuple2<string, int> DoGetFileLine(MethodDefinition method, int offset)
		{
			Tuple2<string, int> result = Tuple.Make("<unknown>", -1);

			if (method.Body != null) 
			{
				Instruction instruction = DoFindInstruction(method, offset);
				if (instruction != null)
				{
					while (instruction != null && instruction.SequencePoint == null)
						instruction = instruction.Next;
					
					if (instruction != null)			// unfortunately it's fairly common for methods to have no sequence point
					{
						SequencePoint spt = instruction.SequencePoint;
						string url = spt.Document != null ? spt.Document.Url : null;
						result = Tuple.Make(url ?? "<unknown>", spt.StartLine);		// TODO: maybe StartColumn too
					}
				}
			}
						
			return result;
		}
		
		private Instruction DoFindInstruction(MethodDefinition method, int offset)
		{
			for (int i = 0; i < method.Body.Instructions.Count; ++i)		// might want to do a binary search
			{
				if (method.Body.Instructions[i].Offset == offset)
					return method.Body.Instructions[i];
			}
			
			Log.ErrorLine(this, "Couldn't find the instruction for {0} at offset {1:X2}", method, offset);
			
			return null;
		}
		#endregion
	}
}