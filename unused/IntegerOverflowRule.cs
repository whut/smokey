// This is tricky to implement well. For example, tlen below should not be
// considered to overflow, but to prevent this we probably need to test for
// the expression used in a compare instead of merely the instruction.
	
	<Violation checkID = "C1007" severity = "Warning">
		<Translation lang = "en" typeName = "IntegerOverflow" category = "Correctness">			
			<Cause>
			A method performs an integer operation which may over or underflow. 
			</Cause>
		
			<Description>
			If the assembly is compiled with -checked, or the method is inside a checked
			block, integer over or underflow will result in a System.OverflowException being
			thrown. If not the value will wrap around zero usually leading to unexpected
			behavior.
			</Description>
	
			<Fix>
			If over/underflow is safe then disable the rule with DisableRuleAttribute
			(see the README) and place the code in an unchecked block. Othewise test
			the value to ensure that it is in range before doing the operation.
			</Fix>
	
			<CSharp>
			public static class Algorithms
			{
				public static int Decrement(int value)
				{
					// Here we verify that the value is OK before decrementing it. Because
					// this is a check that our caller can easily perform we use an assert
					// instead of throwing an ArgumentException.
					System.Diagnostics.Debug.Assert(value != int.MinValue, "value must be greater than Int32.MinValue");
									
					return --value;
				}

				private const uint ModAdler = 65521;
				
				// In this case overflow is OK and even expected. So we disable the 
				// IntegerOverflow rule and ensure that the code executes in an unchecked 
				// block.
				[DisableRule("C1007", "IntegerOverflow")]
				public uint Checksum(byte[] data, int len)
				{
					uint a = 1;
					uint b = 0;
					
					unchecked	// need this in case the assembly is compiled with -checked
					{
						int i = 0;
						while (len > 0) 
						{
							int tlen = len > 5550 ? 5550 : len;
							len -= tlen;
							do 
							{
								a += data[i++];	// these two lines may overflow
								b += a;
							} 
							while (--tlen > 0);
							
							a = (a & 0xffff) + (a >> 16) * (65536 - ModAdler);
							b = (b & 0xffff) + (b >> 16) * (65536 - ModAdler);	
						}
						
						if (a >= ModAdler)
							a -= ModAdler;
						
						b = (b & 0xffff) + (b >> 16) * (65536 - ModAdler);
						
						if (b >= ModAdler)
							b -= ModAdler;
						
						a = (b << 16) | a;
					}
					
					return a;
				}        	
			}
			</CSharp>
		</Translation>
	</Violation>

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
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Smokey.Internal.Rules
{	
	internal class IntegerOverflowRule : Rule
	{				
		public IntegerOverflowRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "C1007")
		{
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitBegin");
			dispatcher.Register(this, "VisitOp");
			dispatcher.Register(this, "VisitCompare");
			dispatcher.Register(this, "VisitBranch");
			dispatcher.Register(this, "VisitEnd");
		}
				
		public void VisitBegin(BeginMethod begin)
		{
			Log.DebugLine(this, "-----------------------------------"); 
			Log.DebugLine(this, "{0:F}", begin.Info.Instructions);				

			m_info = begin.Info;
			m_overflow.Clear();
			m_checked.Clear();			
		}
		
		public void VisitOp(BinaryOp op)
		{
			switch (op.Untyped.OpCode.Code)
			{
				case Code.Add:
				case Code.Add_Ovf:
				case Code.Add_Ovf_Un:
				case Code.Mul:
				case Code.Mul_Ovf:
				case Code.Mul_Ovf_Un:
				case Code.Sub:
				case Code.Sub_Ovf:
				case Code.Sub_Ovf_Un:
				case Code.Shl:
				case Code.Shr:
				case Code.Shr_Un:
					int i1 = m_info.Tracker.GetStackIndex(op.Index, 0);
					int i2 = m_info.Tracker.GetStackIndex(op.Index, 1);
					
					string n1 = DoGetName(i1);
					string n2 = DoGetName(i2);
					
					if (n1 != null && !m_overflow.ContainsKey(n1))
						m_overflow.Add(n1, op.Untyped.Offset);
					if (n2 != null && !m_overflow.ContainsKey(n2))
						m_overflow.Add(n2, op.Untyped.Offset);
					break;
			}
		}

		public void VisitCompare(Compare op)
		{
			int i1 = m_info.Tracker.GetStackIndex(op.Index, 0);
			int i2 = m_info.Tracker.GetStackIndex(op.Index, 1);
			
			string n1 = DoGetName(i1);
			string n2 = DoGetName(i2);
			
			if (n1 != null && m_checked.IndexOf(n1) < 0)
				m_checked.Add(n1);
			if (n2 != null && m_checked.IndexOf(n2) < 0)
				m_checked.Add(n2);
		}

		public void VisitBranch(ConditionalBranch op)
		{
			switch (op.Untyped.OpCode.Code)
			{
				case Code.Beq:
				case Code.Beq_S:
				case Code.Bge:
				case Code.Bge_S:
				case Code.Bge_Un:
				case Code.Bge_Un_S:
				case Code.Bgt:
				case Code.Bgt_S:
				case Code.Bgt_Un:
				case Code.Bgt_Un_S:
				case Code.Ble:
				case Code.Ble_S:
				case Code.Ble_Un:
				case Code.Ble_Un_S:
				case Code.Blt:
				case Code.Blt_S:
				case Code.Blt_Un:
				case Code.Blt_Un_S:
				case Code.Bne_Un:
				case Code.Bne_Un_S:
					int i1 = m_info.Tracker.GetStackIndex(op.Index, 0);
					int i2 = m_info.Tracker.GetStackIndex(op.Index, 1);
					
					string n1 = DoGetName(i1);
					string n2 = DoGetName(i2);
					
					if (n1 != null && m_checked.IndexOf(n1) < 0)
						m_checked.Add(n1);
					if (n2 != null && m_checked.IndexOf(n2) < 0)
						m_checked.Add(n2);
					break;
			}
		}

		public void VisitEnd(EndMethod end)
		{
			if (m_overflow.Count > 0)
			{
				foreach (string name in m_checked)
				{
					Log.DebugLine(this, "{0} is checked", name);
					m_overflow.Remove(name);
				}

				if (m_overflow.Count > 0)
				{
					string key = null;
					foreach (KeyValuePair<string, int> entry in m_overflow)
					{
						Log.DebugLine(this, "potential overflow of {0} at {1:X2}", entry.Key, entry.Value);

						if (key == null)
							key = entry.Key;
						else if (entry.Value < m_overflow[key])
							key = entry.Key;							
					}

					string details = key + " over/underflows";
					Log.DebugLine(this, details);
					
					Reporter.MethodFailed(end.Info.Method, CheckID, m_overflow[key], details);
				}
			}
		}
		
		private string DoGetName(int index)
		{
			if (index >= 0)
			{
				TypedInstruction instruction = m_info.Instructions[index];
				
				do
				{
					LoadArg arg = instruction as LoadArg;
					if (arg != null)
					{
						return arg.Name;
					}

					LoadLocal local = instruction as LoadLocal;
					if (local != null)
					{
						return local.Name;
					}
				}
				while (false);
			}
			
			return null;
		}
				
		private MethodInfo m_info;
		private Dictionary<string, int> m_overflow = new Dictionary<string, int>();
		private List<string> m_checked = new List<string>();
	}
}
