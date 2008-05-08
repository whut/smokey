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
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Smokey.Framework.Support.Advanced;

namespace Smokey.Framework.Instructions
{	
	/// <summary>Contains all of the TypedInstructionCollection in a method.</summary>
	[DisableRule("D1041", "CircularReference")]	// Smokey.Framework.Instructions.TypedInstructionCollection <-> Smokey.Framework.Support.Advanced.TryCatchCollection  
	[DisableRule("D1045", "GodClass")]
	public class TypedInstructionCollection : IFormattable, IEnumerable<TypedInstruction>
	{		
		internal TypedInstructionCollection(SymbolTable symbols, MethodDefinition method)
		{
			Profile.Start("TypedInstructionCollection ctor");
			m_method = method;
			m_symbols = symbols;
			
			if (method.Body != null)
			{
				m_instructions = new List<TypedInstruction>(method.Body.Instructions.Count);
				m_offsets = new Dictionary<int, int>(method.Body.Instructions.Count);
				
				for (int index = 0; index < method.Body.Instructions.Count; ++index)
				{
					Instruction untyped = method.Body.Instructions[index];
					TypedInstruction instruction = DoGetTyped(method, untyped, index);
					
					m_offsets.Add(untyped.Offset, m_instructions.Count);
					m_instructions.Add(instruction);
				}
	
				DoFixBranches();
			}
			else
			{
				m_instructions = new List<TypedInstruction>();
				m_offsets = new Dictionary<int, int>();
			}
			
			m_tryCatches = new TryCatchCollection(this);
			Profile.Stop("TypedInstructionCollection ctor");
		}
		
		/// <summary>The method the instructions belong to.</summary>
		public MethodDefinition Method {get {return m_method;}}
		
		/// <summary>Returns the index and length for all try, catch, and finally
		/// blocks in the method.</summary>
		public TryCatchCollection TryCatchCollection
		{
			get {return m_tryCatches;}
		}
		
		/// <summary>Can use foreach to enumerate through all of the instructions.</summary>
		public IEnumerator<TypedInstruction> GetEnumerator()
		{
			foreach (TypedInstruction instruction in m_instructions)
				yield return instruction;
		}
			
		IEnumerator IEnumerable.GetEnumerator()
		{
			return m_instructions.GetEnumerator();
		}
		
		public int Length
		{
			get {return m_instructions.Count;}
		}
		
		/// <summary>Can random access the instructions (via an index).</summary>
		public TypedInstruction this[int index]
		{
			get {return m_instructions[index];}
		}

		/// <summary>Can get the instruction index for an offset.</summary>
		public int OffsetToIndex(int offset)
		{
			return m_offsets[offset];
		}

		/// <summary>Returns all instructions of type T.</summary>
		public IEnumerable<T> Match<T>() where T: TypedInstruction
		{
			foreach (TypedInstruction instruction in m_instructions)
			{
				T instance = instruction as T;
				if (instance != null)
					yield return instance;
			}
		}
		
		/// <summary>Returns true if the instruction at loads argument zero onto the
		/// stack (this can be done via Ldarg_0 or Ldobj).</summary>
		public bool LoadsThisArg(int index)
		{
			DBC.Pre(index >= 0 && index < m_instructions.Count, "index is out of range");
			bool loadsThis = false;

			if (index >= 0)
			{
				TypedInstruction candidate = m_instructions[index];
				if (candidate.Untyped.OpCode.Code == Code.Ldarg_0)
					loadsThis = true;
				
				else if (index > 0 && candidate.Untyped.OpCode.Code == Code.Ldobj)	// used with structs
					loadsThis = m_instructions[index - 1].Untyped.OpCode.Code == Code.Ldarg_0;
			}
				
			return loadsThis;
		}

		#region Overrides
		public override string ToString()
		{
			return ToString("G", null);
		}
		
		/// <summary>G prints something like "System.Void Smokey.Foo() instructions".
		/// F prints the instructions.</summary>
		/// <exception cref="System.ArgumentException">Thrown if the format string is invalid.</exception>
		public string ToString(string format, IFormatProvider provider)
		{
			if (provider != null)
			{
				ICustomFormatter formatter = provider.GetFormat(GetType()) as ICustomFormatter;
				if (formatter != null)
					return formatter.Format(format, this, provider);
			}
			
			string result;
			switch (format)
			{	
				case "F":
					result = DoGetFlatString();
					break;
										
				case "":			
				case "G":
				case null:
					result = string.Format("{0} Instructions", m_method);
					break;

				default:
					throw new ArgumentException(format + " isn't a valid TypedInstructionCollection format string");
			}
			
			return result;
		}
		#endregion 

		#region Private methods	
		private string DoGetFlatString()
		{
			StringBuilder builder = new StringBuilder(30 * m_instructions.Count);

			builder.Append(m_method.ToString());
			builder.Append(Environment.NewLine);
						
			int index = 0, indent = 0;
			while (index < m_instructions.Count)
			{
				index += DoAppendInstructionString(builder, index, indent);
			}
			
			return builder.ToString();
		}
		
		private int DoAppendInstructionString(StringBuilder builder, int index, int indent)
		{				
			int count = DoAppendTryString(builder, index, indent);
							
			if (count == 0)
				count = DoRealAppendInstructionString(builder, index, indent);
			
			return count;
		}
		
		private int DoRealAppendInstructionString(StringBuilder builder, int index, int indent)
		{											
			builder.Append(' ', 3 * indent);
			builder.Append(m_instructions[index].ToString());
			builder.Append(Environment.NewLine);
			
			return 1;
		}
		
		private int DoAppendTryString(StringBuilder builder, int startIndex, int indent)
		{				
			var blocks = DoGetTry(startIndex);
			blocks.Sort((a, b) => a.Try.Length.CompareTo(b.Try.Length));
			
			int count = 0;
			if (blocks.Count > 0)
			{				
				count = DoAppendOneTryString(builder, blocks, startIndex, blocks.Count - 1, indent);
			}
			
			return count;
		}
		
		private int DoAppendOneTryString(StringBuilder builder, List<TryCatch> blocks, int startIndex, int index, int indent)
		{							
			builder.Append(' ', 3 * indent);
			builder.Append("try");
			builder.Append(Environment.NewLine);
			++indent;
			
			int count = 0;
			if (index > 0)
				count += DoAppendOneTryString(builder, blocks, startIndex, index - 1, indent);
				
			int i = startIndex + count;
			if (i < startIndex + blocks[index].Try.Length && index == 0)
			{
				i += DoRealAppendInstructionString(builder, i, indent);
				++count;
			}
			
			while (i < startIndex + blocks[index].Try.Length)
			{
				int temp = DoAppendInstructionString(builder, i, indent);
				i += temp;
				count += temp;
			}
			--indent;
			
			foreach (CatchBlock cb in blocks[index].Catchers)
			{
				count += DoAppendCatchString(builder, cb, indent);
			}
						
			if (blocks[index].Finally.Length > 0)
				count += DoAppendFinallyString(builder, blocks[index].Finally, indent);

			else if (blocks[index].Fault.Length > 0)
				count += DoAppendFaultString(builder, blocks[index].Fault, indent);

			return count;
		}
		
		private int DoAppendCatchString(StringBuilder builder, CatchBlock block, int indent)
		{				
			builder.Append(' ', 3 * indent);
			builder.Append("catch ");			
			builder.Append(block.CatchType.ToString());
			builder.Append(Environment.NewLine);
			++indent;
			
			int index = block.Index;
			while (index < block.Index + block.Length)
			{
				index += DoAppendInstructionString(builder, index, indent);
			}
			--indent;
			
			return block.Length;
		}
		
		private int DoAppendFinallyString(StringBuilder builder, Block block, int indent)
		{				
			builder.Append(' ', 3 * indent);
			builder.Append("finally");
			builder.Append(Environment.NewLine);
			++indent;
			
			int index = block.Index;
			while (index < block.Index + block.Length)
			{
				index += DoAppendInstructionString(builder, index, indent);
			}
			--indent;
			
			return block.Length;
		}
				
		private int DoAppendFaultString(StringBuilder builder, Block block, int indent)
		{				
			builder.Append(' ', 3 * indent);
			builder.Append("fault");
			builder.Append(Environment.NewLine);
			++indent;
			
			int index = block.Index;
			while (index < block.Index + block.Length)
			{
				index += DoAppendInstructionString(builder, index, indent);
			}
			--indent;
			
			return block.Length;
		}
				
		private List<TryCatch> DoGetTry(int index)
		{
			var blocks = new List<TryCatch>();
			
			if (m_tryCatches != null)
			{
				foreach (TryCatch tc in m_tryCatches)
				{
					if (tc.Try.Index == index)
						blocks.Add(tc);
				}
			}
			
			return blocks;
		}
						
		private TypedInstruction DoGetTyped(MethodDefinition method, Instruction untyped, int index)
		{
			TypedInstruction instruction = null;
						
			switch (untyped.OpCode.Code)
			{
				case Code.Add:
				case Code.Add_Ovf:
				case Code.Add_Ovf_Un:
				case Code.And:
				case Code.Div:
				case Code.Div_Un:
				case Code.Mul:
				case Code.Mul_Ovf:
				case Code.Mul_Ovf_Un:
				case Code.Or:
				case Code.Rem:
				case Code.Rem_Un:
				case Code.Shl:
				case Code.Shr:
				case Code.Shr_Un:
				case Code.Sub:
				case Code.Sub_Ovf:
				case Code.Sub_Ovf_Un:
				case Code.Xor:
					instruction = new BinaryOp(untyped, index);
					break;

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
				case Code.Brfalse:
				case Code.Brfalse_S:
				case Code.Brtrue:
				case Code.Brtrue_S:
					instruction = new ConditionalBranch(untyped, index);
					break;
					
				case Code.Box:
					instruction = new Box(untyped, index);
					break;
																									
				case Code.Br:
				case Code.Br_S:
				case Code.Leave:
				case Code.Leave_S:
					instruction = new UnconditionalBranch(untyped, index);
					break;
					
				case Code.Call:
				case Code.Callvirt:
					instruction = new Call(untyped, index);
					break;
					
				case Code.Castclass:
				case Code.Isinst:
					instruction = new CastClass(untyped, index);
					break;
					
				case Code.Ceq:
					instruction = new Ceq(untyped, index);
					break;
										
				case Code.Cgt:
				case Code.Cgt_Un:
				case Code.Clt:
				case Code.Clt_Un:
					instruction = new Compare(untyped, index);
					break;
										
				case Code.Conv_I1:
				case Code.Conv_I2:
				case Code.Conv_I4:
				case Code.Conv_I8:
				case Code.Conv_R4:
				case Code.Conv_R8:
				case Code.Conv_U4:
				case Code.Conv_U8:
				case Code.Conv_R_Un:
				case Code.Conv_Ovf_I1_Un:
				case Code.Conv_Ovf_I2_Un:
				case Code.Conv_Ovf_I4_Un:
				case Code.Conv_Ovf_I8_Un:
				case Code.Conv_Ovf_U1_Un:
				case Code.Conv_Ovf_U2_Un:
				case Code.Conv_Ovf_U4_Un:
				case Code.Conv_Ovf_U8_Un:
				case Code.Conv_Ovf_I_Un:
				case Code.Conv_Ovf_U_Un:
				case Code.Conv_Ovf_I1:
				case Code.Conv_Ovf_U1:
				case Code.Conv_Ovf_I2:
				case Code.Conv_Ovf_U2:
				case Code.Conv_Ovf_I4:
				case Code.Conv_Ovf_U4:
				case Code.Conv_Ovf_I8:
				case Code.Conv_Ovf_U8:
				case Code.Conv_U2:
				case Code.Conv_U1:
				case Code.Conv_I:
				case Code.Conv_Ovf_I:
				case Code.Conv_Ovf_U:
				case Code.Conv_U:
					instruction = new Conv(untyped, index);
					break;

				case Code.Endfilter:
				case Code.Endfinally:
				case Code.Ret:
				case Code.Rethrow:
					instruction = new End(untyped, index);
					break;
										
				case Code.Initobj:
					instruction = new InitObj(untyped, index);
					break;
										
				case Code.Ldarg_0:
				case Code.Ldarg_1:
				case Code.Ldarg_2:
				case Code.Ldarg_3:
				case Code.Ldarg:
				case Code.Ldarg_S:
					instruction = new LoadArg(method, untyped, index);
					break;

				case Code.Ldarga:
				case Code.Ldarga_S:
					instruction = new LoadArgAddress(method, untyped, index);
					break;

				case Code.Ldc_I4_M1:
				case Code.Ldc_I4_0:
				case Code.Ldc_I4_1:
				case Code.Ldc_I4_2:
				case Code.Ldc_I4_3:
				case Code.Ldc_I4_4:
				case Code.Ldc_I4_5:
				case Code.Ldc_I4_6:
				case Code.Ldc_I4_7:
				case Code.Ldc_I4_8:
				case Code.Ldc_I4_S:
				case Code.Ldc_I4:
				case Code.Ldc_I8:
					instruction = new LoadConstantInt(untyped, index);
					break;
										
				case Code.Ldc_R4:
				case Code.Ldc_R8:
					instruction = new LoadConstantFloat(untyped, index);
					break;
										
				case Code.Ldelema:
				case Code.Ldtoken:
					instruction = new LoadPointer(untyped, index);
					break;

				case Code.Ldelem_I1:
				case Code.Ldelem_U1:
				case Code.Ldelem_I2:
				case Code.Ldelem_U2:
				case Code.Ldelem_I4:
				case Code.Ldelem_U4:
				case Code.Ldelem_I8:
				case Code.Ldelem_I:
				case Code.Ldelem_R4:
				case Code.Ldelem_R8:
				case Code.Ldelem_Ref:
				case Code.Ldelem_Any:
				case Code.Ldind_I1:
				case Code.Ldind_U1:
				case Code.Ldind_I2:
				case Code.Ldind_U2:
				case Code.Ldind_I4:
				case Code.Ldind_U4:
				case Code.Ldind_I8:
				case Code.Ldind_I:
				case Code.Ldind_R4:
				case Code.Ldind_R8:
				case Code.Ldind_Ref:
				case Code.Ldlen:
					instruction = new Load(untyped, index);
					break;
					
				case Code.Ldfld:
					instruction = new LoadField(untyped, index);
					break;

				case Code.Ldflda:
					instruction = new LoadFieldAddress(untyped, index);
					break;
					
				case Code.Ldftn:
				case Code.Ldvirtftn:
					instruction = new LoadFunctionAddress(untyped, index);
					break;

				case Code.Ldloc_0:
				case Code.Ldloc_1:
				case Code.Ldloc_2:
				case Code.Ldloc_3:
				case Code.Ldloc:
				case Code.Ldloc_S:
					instruction = new LoadLocal(m_symbols, method, untyped, index);
					break;

				case Code.Ldloca:
				case Code.Ldloca_S:
					instruction = new LoadLocalAddress(m_symbols, method, untyped, index);
					break;

				case Code.Ldnull:
					instruction = new LoadNull(untyped, index);
					break;
					
				case Code.Ldsfld:
					instruction = new LoadStaticField(untyped, index);
					break;

				case Code.Ldsflda:
					instruction = new LoadStaticFieldAddress(untyped, index);
					break;

				case Code.Ldstr:
					instruction = new LoadString(untyped, index);
					break;
					
				case Code.Newarr:
					instruction = new NewArr(untyped, index);
					break;
					
				case Code.Newobj:
					instruction = new NewObj(untyped, index);
					break;
					
				case Code.Starg:
				case Code.Starg_S:
					instruction = new StoreArg(method, untyped, index);
					break;

				case Code.Stelem_I:
				case Code.Stelem_I1:
				case Code.Stelem_I2:
				case Code.Stelem_I4:
				case Code.Stelem_I8:
				case Code.Stelem_R4:
				case Code.Stelem_R8:
				case Code.Stelem_Ref:
				case Code.Stelem_Any:
				case Code.Stind_I:
				case Code.Stind_I1:
				case Code.Stind_I2:
				case Code.Stind_I4:
				case Code.Stind_I8:
				case Code.Stind_R4:
				case Code.Stind_R8:
				case Code.Stobj:
					instruction = new Store(untyped, index);
					break;

				case Code.Stfld:
					instruction = new StoreField(untyped, index);
					break;

				case Code.Stloc_0:
				case Code.Stloc_1:
				case Code.Stloc_2:
				case Code.Stloc_3:
				case Code.Stloc:
				case Code.Stloc_S:
					instruction = new StoreLocal(m_symbols, method, untyped, index);
					break;
					
				case Code.Stsfld:
					instruction = new StoreStaticField(untyped, index);
					break;

				case Code.Switch:
					instruction = new Switch(untyped, index);
					break;
					
				case Code.Throw:
					instruction = new Throw(untyped, index);
					break;
										
				case Code.Unbox:
				case Code.Unbox_Any:
					instruction = new Unbox(untyped, index);
					break;
					
				default:
					instruction = new CatchAll(untyped, index);
					break;
			}
			 
			return instruction;
		}

		private void DoFixBranches()
		{
			foreach (TypedInstruction instruction in m_instructions)
			{
				Branch branch = instruction as Branch;
				if (branch != null)
				{
					Instruction untyped = (Instruction) instruction.Untyped.Operand;
					branch.Target = m_instructions[m_offsets[untyped.Offset]];
				}

				Switch sw = instruction as Switch;
				if (sw != null)
				{
					List<TypedInstruction> targets = new List<TypedInstruction>();
					
					foreach (Instruction i in (Instruction[]) sw.Untyped.Operand)
					{
						targets.Add(m_instructions[m_offsets[i.Offset]]);
					}					

					sw.Targets = targets.ToArray();
				}
			}
		}
		#endregion
		
		#region Fields
		private SymbolTable m_symbols;
		private MethodDefinition m_method;
		private List<TypedInstruction> m_instructions;
		private Dictionary<int, int> m_offsets;			// offset -> index
		private TryCatchCollection m_tryCatches;
		#endregion
	}
}
