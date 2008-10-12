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
	internal struct CodeBlock
	{
		public readonly MethodInfo Info;
		public readonly int Index;
		
		public CodeBlock(MethodInfo info, int index)
		{
			Info = info;
			Index = index;
		}

		public override bool Equals(object rhsObj)
		{
			if (rhsObj == null)                        // objects may be null
				return false;
			
			if (GetType() != rhsObj.GetType()) 
				return false;
			
			CodeBlock rhs = (CodeBlock) rhsObj;                    
			return this == rhs;
		}
		
		public static bool operator==(CodeBlock lhs, CodeBlock rhs)
		{
			return lhs.Info == rhs.Info && lhs.Index == rhs.Index;
		}
		
		public static bool operator!=(CodeBlock lhs, CodeBlock rhs)
		{
			return !(lhs == rhs);
		}
		
		public override int GetHashCode()
		{
			int hash;
			
			unchecked
			{
				hash = Info.GetHashCode() + Index.GetHashCode();
			}
			
			return hash;
		}
	}
	
	internal sealed class IdenticalCodeBlocksRule : Rule
	{				
		public IdenticalCodeBlocksRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "D1049")
		{
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitBegin");
			dispatcher.Register(this, "VisitMethod");
			dispatcher.Register(this, "VisitBranch");
			dispatcher.Register(this, "VisitEnd");
		}
		
		// One possible improvement is to use a real hash of the block instead of simply
		// the block length. This would significantly cut down on false matches, but it
		// also require us to revisit instructions which tends to blow. From examination
		// it appears that we compare very few blocks so a hash doesn't seem terribly
		// useful.
		public void VisitBegin(BeginMethods begin)
		{			
			Unused.Value = begin;
			
			m_table.Clear();
		}

		public void VisitMethod(BeginMethod begin)
		{			
			m_info = begin.Info;
			
			if (m_info.Method.Body != null && m_info.Method.Body.Instructions.Count > m_minBlockSize)
			{
				Log.DebugLine(this, "-----------------------------------"); 
				Log.DebugLine(this, "{0:F}", m_info.Instructions);				

				foreach (TryCatch tc in m_info.Instructions.TryCatchCollection)
				{
//					DoAddBlock(tc.Try);				// we should probably do this but we'd have to special case nested trys
					DoAddBlock(tc.Finally);
					DoAddBlock(tc.Fault);

					foreach (CatchBlock b in tc.Catchers)
					{
						DoAddBlock(b);
					}
				}
			}
		}
		
		// might want a foreach test
		// make sure it works within a method (maybe if and handler)
		public void VisitBranch(Branch branch)
		{			
			if (branch.Untyped.OpCode.Code != Code.Leave && branch.Untyped.OpCode.Code != Code.Leave_S)
			{
				if (branch.Target.Index > branch.Index)
					DoAddBlock(branch.Index + 1, branch.Target.Index - branch.Index - 1);
				else
					DoAddBlock(branch.Target.Index, branch.Index - branch.Target.Index);
			}
		}

		public void VisitEnd(EndMethods end)
		{
			Unused.Value = end;
			
			foreach (KeyValuePair<int, List<CodeBlock>> entry in m_table)
			{
				Log.DebugLine(this, "there are {0} blocks of length {1}", entry.Value.Count, entry.Key);

				while (entry.Value.Count > 1)
				{
					string details = string.Empty;
					CodeBlock block = entry.Value[0];
					
					for (int i = 1; i < entry.Value.Count;)
					{
						if (DoMatch(entry.Value[0], entry.Value[i], entry.Key))
						{
							details += entry.Value[i].Info.Method.ToString() + " ";
							DoSanitize(entry.Value[i].Info.Method);
						}
						else
							++i;
					}
					
					if (details.Length > 0)
					{	
						details = "Match:  " + details;
						Log.DebugLine(this, details);
						
						int offset = block.Info.Instructions[block.Index].Untyped.Offset;
						Reporter.MethodFailed(block.Info.Method, CheckID, offset, details);
					}
					
					DoSanitize(block.Info.Method);
				}
			}
		}
		
		// We only want to report methods once so once we report it we need to remove
		// the other instances from our table.
		private void DoSanitize(MethodDefinition method)
		{
			foreach (KeyValuePair<int, List<CodeBlock>> entry in m_table)
			{
				for (int i = 0; i < entry.Value.Count;)
				{
					if (entry.Value[i].Info.Method.MetadataToken == method.MetadataToken)
						entry.Value.RemoveAt(i);
					else
						++i;
				}
			}
		}
		
		private void DoAddBlock(Block block)
		{
			DoAddBlock(block.Index, block.Length - 1);	// have to ignore the leave instructions because they can go to different places
		}

		private void DoAddBlock(CatchBlock block)
		{
			DoAddBlock(block.Index, block.Length - 1);	// ditto
		}

		private void DoAddBlock(int index, int length)
		{
			if (length >= m_minBlockSize)
			{
				List<CodeBlock> blocks;
				if (!m_table.TryGetValue(length, out blocks))
				{
					blocks = new List<CodeBlock>();
					m_table.Add(length, blocks);
				}
				
				blocks.Add(new CodeBlock(m_info, index));
				Log.DebugLine(this, "{0} length block at {1:X2}", length, m_info.Instructions[index].Untyped.Offset);
			}
		}

		private bool DoMatch(CodeBlock lhs, CodeBlock rhs, int count)
		{
			Log.DebugLine(this, "   lhs: {0} at {1:X2}", lhs.Info.Method, lhs.Info.Instructions[lhs.Index].Untyped.Offset);
			Log.DebugLine(this, "   rhs: {0} at {1:X2}", rhs.Info.Method, rhs.Info.Instructions[rhs.Index].Untyped.Offset);
			Log.DebugLine(this, "   count: {0}", count);
			
			for (int i = 0; i < count; ++i)
			{
				TypedInstruction left = lhs.Info.Instructions[lhs.Index + i];
				TypedInstruction right = rhs.Info.Instructions[rhs.Index + i];
				
				LoadLocal load1 = left as LoadLocal;
				LoadLocal load2 = right as LoadLocal;
				LoadLocalAddress load1b = left as LoadLocalAddress;
				LoadLocalAddress load2b = right as LoadLocalAddress;
				StoreLocal store1 = left as StoreLocal;
				StoreLocal store2 = right as StoreLocal;

				if (load1 != null && load2 != null)
				{
					// If we have real names for the local variables (from the mdb file) then
					// life is good and we can compare the names. If not we're stuck: we can't
					// simply use the variable index because we'll get false positives.
					if (!load1.RealName)
					{												
						Log.DebugLine(this, "   can't match local name");
						return false;
					}
					// This should be a temporary in which case we can't use the variable index.
					else if (load1.Name.StartsWith("V_") && load1.Type.FullName != load2.Type.FullName)
					{
						Log.DebugLine(this, "   {0} != {1} ({2} and {3})", left, right, load1.Type.FullName, load2.Type.FullName);
						return false;
					}
					else if (!load1.Name.StartsWith("V_") && load1.Name != load2.Name)
					{												
						Log.DebugLine(this, "   {0} != {1} ({2} and {3})", left, right, load1.Name, load2.Name);
						return false;
					}
				}	
				else if (load1b != null && load2b != null)
				{
					if (!load1b.RealName)
					{												
						Log.DebugLine(this, "   can't match local name");
						return false;
					}
					else if (load1b.Name.StartsWith("V_") && load1b.Type.FullName != load2b.Type.FullName)
					{
						Log.DebugLine(this, "   {0} != {1} ({2} and {3})", left, right, load1b.Type.FullName, load2b.Type.FullName);
						return false;
					}
					else if (!load1b.Name.StartsWith("V_") && load1b.Name != load2b.Name)	
					{												
						Log.DebugLine(this, "   {0} != {1} ({2} and {3})", left, right, load1b.Name, load2b.Name);
						return false;
					}
				}	
				else if (store1 != null && store2 != null)
				{
					if (!store1.RealName)
					{												
						Log.DebugLine(this, "   can't match local name");
						return false;
					}
					else if (store1.Name.StartsWith("V_") && store1.Type.FullName != store2.Type.FullName)
					{
						Log.DebugLine(this, "   {0} != {1} ({2} and {3})", left, right, store1.Type.FullName, store2.Type.FullName);
						return false;
					}
					else if (!store1.Name.StartsWith("V_") && store1.Name != store2.Name)
					{
						Log.DebugLine(this, "   {0} != {1} ({2} and {3})", left, right, store1.Name, store2.Name);
						return false;
					}
				}
				else if (!left.Untyped.Matches(right.Untyped))
				{
					Log.DebugLine(this, "   {0} != {1}", left, right);
					return false;
				}
			}
						
			return true;
		}
										
		private int m_minBlockSize = 40;
		private MethodInfo m_info;
		private Dictionary<int, List<CodeBlock>> m_table = new Dictionary<int, List<CodeBlock>>();
	}
}

