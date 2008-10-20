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
using Smokey.Framework;
using Smokey.Framework.Support;

namespace Smokey.Internal.Rules
{	
	internal sealed class LargeStructRule : Rule
	{				
		public LargeStructRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "MS1019")
		{
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitType");
		}
				
		public void VisitType(TypeDefinition type)
		{			
			bool passed = true;
			
			Log.DebugLine(this, "-----------------------------------"); 
			Log.DebugLine(this, "checking {0}", type);				

			int size = 0;

			if (type.IsValueType)
				size = DoGetStructSize(Cache, type, 0);
				
			passed = size <= 16;
			if (!passed)
			{
				string details = string.Format("{0} is {1} bytes or more", type.Name, size);
				Log.DebugLine(this, details);
				Reporter.TypeFailed(type, CheckID, details);
			}
		}

		// There doesn't seem to be a way to get this via Cecil: all of the obvious
		// candidates like TypeDefinition::PackingSize, TypeDefinition::ClassSize,
		// and FieldDefinition::Offset return zero.
		private int DoGetStructSize(AssemblyCache cache, TypeDefinition type, int level)
		{
			int size = 0;
			DBC.Assert(level < 100, "LargeStructRule didn't terminate for type {0}", type.FullName);
//			Console.WriteLine(prefix + type.FullName);
			
			// For each field,
			foreach (FieldDefinition field in type.Fields)
			{
				Log.WarningLine(this, "checking field of type {0}{1}", new string(' ', 3*level), field.FieldType.FullName);
				
				// if it isn't static,
				if (!field.IsStatic)
				{
					// if it's a value type,
					if (field.FieldType.IsValueType)
					{
						// if it's a standard system struct then we know the size,
						int temp = DoGetSystemTypeSize(field.FieldType.FullName);
						if (temp > 0)
							size += temp;
						else 
						{
							// otherwise if it's one of our types we can figure the
							// size out,
							TypeDefinition t = cache.FindType(field.FieldType);
							if (t != null && t.FullName != type.FullName)	// TODO: shouldn't need the name check (but mscorlib.dll infinitely recurses w/o it)
							{
								if (t.IsEnum || !t.IsValueType)
									size += 4;
								else
									size += DoGetStructSize(cache, t, level + 1);
							}
							
							// if it's not one of our types then we appear to be hosed
							// (this should rarely happen now that we load dependant
							// assemblies).
							else
							{
								Log.TraceLine(this, "couldn't find the size for {0}", field.FieldType.FullName);
								size += 1;		
							}
						}
					}
					
					// if it's not a value type then for our purposes its size is 4 
					// (pointers may be 8 bytes on a 64-bit platform but we don't want
					// to report errors just because they happen to be running on a 
					// 64-bit system).
					else
						size += 4;
				}
			}
						
			return size;
		}

		private static int DoGetSystemTypeSize(string fullName)
		{
			int size = 0;
			
			switch (fullName)
			{
				case "System.Boolean":
				case "System.Byte":
				case "System.SByte":
					size += 1;
					break;
										
				case "System.Char":
				case "System.Int16":
				case "System.UInt16":
					size += 2;
					break;
					
				case "System.Int32":
				case "System.IntPtr":
				case "System.Single":
				case "System.UInt32":
				case "System.UIntPtr":
					size += 4;
					break;					
					
				case "System.Int64":
				case "System.UInt64":
				case "System.Double":
					size += 8;
					break;					
			}
			
			return size;
		}
	}
}

