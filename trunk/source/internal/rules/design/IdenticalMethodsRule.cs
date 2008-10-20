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
using Smokey.Framework.Instructions;
using Smokey.Framework.Support;
using Smokey.Framework.Support.Advanced;
using System;
using System.Collections.Generic;
using System.Text;

namespace Smokey.Internal.Rules
{	
	internal sealed class IdenticalMethodsRule : Rule
	{				
		public IdenticalMethodsRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "D1042")
		{
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitAssembly");
			dispatcher.Register(this, "VisitBegin");
			dispatcher.Register(this, "VisitFini");
		}
		
		public void VisitAssembly(AssemblyDefinition assembly)
		{			
			Unused.Value = assembly;
			
			Log.DebugLine(this, "+++++++++++++++++++++++++++++++++++"); 
			m_table.Clear();		// need this for unit test
		}

		public void VisitBegin(BeginMethod begin)
		{			
			if (begin.Info.Method.Body != null && begin.Info.Method.Body.Instructions.Count >= 40)
			{
				Log.DebugLine(this, "-----------------------------------"); 
				Log.DebugLine(this, "{0:F}", begin.Info.Instructions);				
			
				int hash = DoGetHash(begin.Info.Method);
				Log.DebugLine(this, "{0} instructions, hash is {1}", begin.Info.Method.Body.Instructions.Count, hash);
				
				List<MethodCapture> methods;
				if (!m_table.TryGetValue(hash, out methods))
				{
					methods = new List<MethodCapture>();
					m_table.Add(hash, methods);
				}
				
				methods.Add(new MethodCapture(begin.Info));
			}
		}

		public void VisitFini(EndTesting end)
		{
			Unused.Value = end;
			
			string header = "Match:  ";
			
			foreach (List<MethodCapture> methods in m_table.Values)
			{
				List<int> found = new List<int>();
						
				for (int i = 0; i < methods.Count; ++i)
				{
					List<string> matches = new List<string>();

					for (int j = i + 1; j < methods.Count; ++j)
					{
						if (found.IndexOf(j) < 0)
						{
							Log.DebugLine(this, "checking {0} and {1}", methods[i].Method, methods[j].Method);
						
							if (DoMatch(methods[i], methods[j]))
							{
								Log.DebugLine(this, "   matched");
								matches.Add(methods[j].Method.ToString());
								found.Add(j);
							}
						}
					}
					
					if (matches.Count > 0)
					{						
						string[] strs = Array.ConvertAll(matches.ToArray(), s => s + "   ");
						string details = string.Format("{0}{1}", header, string.Join(Environment.NewLine, strs));
						
						Log.DebugLine(this, details);
						Reporter.MethodFailed(methods[i].Method, CheckID, 0, details);
					}
				}
			}
		}
		
		private static int DoGetHash(MethodDefinition method)
		{
			int hash = 0;
			
			unchecked
			{
				hash += (int) method.ReturnType.MetadataToken.RID;
				foreach (ParameterDefinition p in method.Parameters)
				{
					hash += (int) p.ParameterType.MetadataToken.RID;
				}

				hash += method.Body.CodeSize;
				hash += method.Body.MaxStack;
				hash += method.Body.Scopes.Count;
				hash += method.Body.ExceptionHandlers.Count;
				foreach (VariableDefinition v in method.Body.Variables)
				{
					hash += (int) v.VariableType.MetadataToken.RID;
				}				
			}
			
			return hash;
		}
		
		private bool DoMatch(MethodCapture lhs, MethodCapture rhs)
		{
			if (lhs.Method.ReturnType.MetadataToken != rhs.Method.ReturnType.MetadataToken)
			{
				Log.DebugLine(this, "   return type differs");
				return false;
			}
			
			if (lhs.Method.Parameters.Count != rhs.Method.Parameters.Count)
			{
				Log.DebugLine(this, "   numParams differs");
				return false;
			}
			
			for (int i = 0; i < lhs.Method.Parameters.Count; ++i)
				if (lhs.Method.Parameters[i].ParameterType.MetadataToken != rhs.Method.Parameters[i].ParameterType.MetadataToken)
				{
					Log.DebugLine(this, "   param types differ");
					return false;
				}
					

			if (lhs.Instructions.Length != rhs.Instructions.Length)
			{
				Log.DebugLine(this, "   numInstructions differs");
				return false;
			}
			
			for (int i = 0; i < lhs.Instructions.Length; ++i)
			{
				TypedInstruction left = lhs.Instructions[i];
				TypedInstruction right = rhs.Instructions[i];
				
				if (!DoMatch(left, right))
				{
					Log.DebugLine(this, "   {0} != {1} at {2:X2} and {3:X2}", left, right, left.Untyped.Offset, right.Untyped.Offset);
					return false;
				}
			}
						
			return true;
		}
		
		private bool DoMatch(TypedInstruction lhs, TypedInstruction rhs)
		{
			if (lhs.Untyped.OpCode.Code != rhs.Untyped.OpCode.Code)
				return false;
				
			if (lhs.Untyped.Operand != null || rhs.Untyped.Operand != null)
			{
				if (lhs.Untyped.Operand != null && rhs.Untyped.Operand != null)
				{
					if (!DoMatchOperand(lhs.Untyped.Operand, rhs.Untyped.Operand))
						return false;
				}
				else 
					return false;
			}
						
			return true;
		}
		
		[DisableRule("R1009", "CompareFloats")]
		[DisableRule("P1004", "AvoidUnboxing")]
		private bool DoMatchOperand(object lhs, object rhs)
		{
			if (lhs.GetType() != rhs.GetType())
				return false;
				
			if (lhs is int)
			{
				int i1 = (int) lhs;
				int i2 = (int) rhs;
				return i1 == i2;
			}
			
			if (lhs is long)
			{
				long l1 = (long) lhs;
				long l2 = (long) rhs;
				return l1 == l2;
			}
			
			if (lhs is float)
			{
				float i1 = (float) lhs;
				float i2 = (float) rhs;
				return i1 == i2;
			}
			
			if (lhs is double)
			{
				double i1 = (double) lhs;
				double i2 = (double) rhs;
				return i1 == i2;
			}
			
			if (lhs is SByte)
			{
				SByte i1 = (SByte) lhs;
				SByte i2 = (SByte) rhs;
				return i1 == i2;
			}
			
			if (lhs is string)
			{
				string s1 = (string) lhs;
				string s2 = (string) rhs;
				return s1 == s2;
			}
			
			if (lhs is FieldReference)
			{
				FieldReference f1 = (FieldReference) lhs;
				FieldReference f2 = (FieldReference) rhs;
				return f1.Name == f2.Name;
			}
			
			if (lhs is Instruction)
			{
				Instruction i1 = (Instruction) lhs;
				Instruction i2 = (Instruction) rhs;
				return i1.Offset == i2.Offset;
			}
			
			if (lhs is Instruction[])			// used for switch
			{
				Instruction[] i1s = (Instruction[]) lhs;
				Instruction[] i2s = (Instruction[]) rhs;
				return i1s.Length == i2s.Length;	// if the elements differ we'll find out about it soon enough
			}
			
			if (lhs is MethodReference)
			{
				MethodReference m1 = (MethodReference) lhs;
				MethodReference m2 = (MethodReference) rhs;
				
				string s1 = m1.ToString().Replace(m1.DeclaringType.Name, "self");
				string s2 = m2.ToString().Replace(m2.DeclaringType.Name, "self");
				
				return s1 == s2;
			}
			
			if (lhs is ParameterDefinition)
			{
				ParameterDefinition p1 = (ParameterDefinition) lhs;
				ParameterDefinition p2 = (ParameterDefinition) rhs;
				return p1.Sequence == p2.Sequence;
			}
			
			if (lhs is TypeReference)
			{
				TypeReference t1 = (TypeReference) lhs;
				TypeReference t2 = (TypeReference) rhs;
				return t1.MetadataToken == t2.MetadataToken;
			}
			
			if (lhs is VariableDefinition)
			{
				VariableDefinition v1 = (VariableDefinition) lhs;
				VariableDefinition v2 = (VariableDefinition) rhs;
				return v1.Index == v2.Index;
			}
			
			DBC.Fail("unexpected {0}", lhs.GetType());
			
			return false;
		}
		
		// Need to save this stuff because the dispatcher will free up MethodInfo
		// instructions after visiting it.
		private sealed class MethodCapture
		{
			public MethodCapture(MethodInfo info)
			{
				Method = info.Method;
				Instructions = info.Instructions;
			}
			
			public MethodDefinition Method {get; private set;}		// will always have a body
			public TypedInstructionCollection Instructions {get; private set;}	
		}
				
		private Dictionary<int, List<MethodCapture>> m_table = new Dictionary<int, List<MethodCapture>>();
	}
}

