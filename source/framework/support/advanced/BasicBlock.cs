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

using System;
using System.Collections.Generic;
using System.Text;
using Smokey.Framework.Instructions;

#if OLD
namespace Smokey.Framework.Support.Advanced
{			
	/// <summary>A sequence of instructions with no branches in (except for possibly 
	/// the first instruction) and no branches out (except possibly for the
	/// last instruction).</summary>
	public class BasicBlock : IFormattable, IEquatable<BasicBlock>
	{	
		/// <summary>The first instruction in the basic block. </summary>
		public readonly TypedInstruction First;
		
		/// <summary>The last instruction in the basic block. May be equal to First.</summary>
		public readonly TypedInstruction Last;
		
		/// <summary>This will always be a positive number within rules, but may be zero
		/// or even negative within DataFlow.</summary>
		public readonly int Length;
				
		/// <summary>The blocks to which execution may flow after this block executes. </summary>
		public BasicBlock[] Next
		{
			get {return m_next;}
			internal set {m_next = value;}
		}
		
		/// <summary>If this block is inside a try or catch block and either exits
		/// the method or leaves the block then, if the try has a finally
		/// block, this will point to the first block within it. Otherwise
		/// it will be null.</summary>
		public BasicBlock Finally
		{
			get {return m_finally;}
			internal set {m_finally = value;}
		}
		
		/// <summary>This isn't generated normally by C#, but can appear in compiler 
		/// generated code. It works like a finally block except that it's
		/// only used if an exception is thrown. It's also exclusive with a
		/// finally block.</summary>
		public BasicBlock Fault
		{
			get {return m_fault;}
			internal set {m_fault = value;}
		}
		
		internal int SafeIndex
		{
			get
			{
				if (Length > 0)
					return First.Index;
				else
					return Next[0].SafeIndex;
			}
		}
								
		#region Overrides
		public override bool Equals(object rhsObj)
		{		
			BasicBlock rhs = rhsObj as BasicBlock;
			return this == rhs;
		}
		
		public bool Equals(BasicBlock rhs)
		{		
			return this == rhs;
		}
		
		[DisableRule("C1021", "RecursiveEquality")]
		public static bool operator==(BasicBlock lhs, BasicBlock rhs)
		{
			if (object.ReferenceEquals(lhs, rhs))
				return true;
		
			if ((object) lhs == null || (object) rhs == null)
				return false;
				
			if (lhs.Length == rhs.Length)
			{
				if (lhs.Length < 0)
				{
					bool equals = lhs.Next.Length == rhs.Next.Length;
					for (int i = 0; i < lhs.Next.Length && equals; ++i)
						equals = lhs.Next[i] == rhs.Next[i];
					return equals;
				}
				
				else if (lhs.First.Index == rhs.First.Index && lhs.Last.Index == rhs.Last.Index)
					return true;
			}
			
			return false;
		}
		
		public static bool operator!=(BasicBlock lhs, BasicBlock rhs)
		{
			return !(lhs == rhs);
		}
				
		public override int GetHashCode()
		{
			int hash = 0;
			
			unchecked
			{
				if (Length > 0)
				{
					hash = First.Index + Last.Index;
				}
				else
				{
					hash = Length;
					foreach (BasicBlock block in Next)
						hash += block.GetHashCode();
				}
			}
			
			return hash;
		}

		public override string ToString()
		{
			return ToString("G", null);
		}
		
		/// <summary>S prints something like "00 - 0A".
		/// G prints something like "00 - 0A => 1F 2A".
		/// T prints, using "G", every block reachable from this.</summary>
		/// <exception cref="System.ArgumentException">Thrown if the format string is invalid.</exception>
		public string ToString(string format, IFormatProvider provider)
		{
			if (provider != null)
			{
				ICustomFormatter formatter = provider.GetFormat(GetType()) as ICustomFormatter;
				if (formatter != null)
					return formatter.Format(format, this, provider);
			}
			
			StringBuilder builder = new StringBuilder();
			switch (format)
			{	
				case "S":
					if (Length > 0)
					{
						builder.Append(First.Untyped.Offset.ToString("X2"));
						builder.Append(" - ");
						builder.Append(Last.Untyped.Offset.ToString("X2"));
					}
					else
						builder.Append(Length.ToString());
					break;
					
				case "T":
					DoGetTreeString(builder, this);
					break;
					
				case "":			
				case "G":
				case null:
					DoGetFlatString(builder, this);
					break;

				default:
					throw new ArgumentException(format + " isn't a valid BasicBlock format string");
			}
			
			return builder.ToString();
		}
		#endregion 

		#region Internal methods	
		internal BasicBlock(TypedInstruction first, TypedInstruction last)
		{
			First = first;
			Last = last;
			Length = Last.Index - First.Index + 1;
		}
				
//		internal BasicBlock(TypedInstruction first, TypedInstruction last, BasicBlock fnlly, BasicBlock fault)
//		{
//			First = first;
//			Last = last;
//			Finally = fnlly;
//			Fault = fault;
//			Length = Last.Index - First.Index + 1;
//		}
				
		internal BasicBlock(int length, BasicBlock[] next)
		{
			Length = length;
			m_next = next;
		}
		#endregion 

		#region Private methods	
		private static void DoGetTreeString(StringBuilder builder, BasicBlock block)
		{			
			Dictionary<int, BasicBlock> table = new Dictionary<int, BasicBlock>();
			DoGetBlocks(table, block);
			
			List<BasicBlock> blocks = new List<BasicBlock>(table.Values);
			blocks.Sort(delegate (BasicBlock lhs, BasicBlock rhs)
			{
				return lhs.SafeIndex.CompareTo(rhs.SafeIndex);
			});
			
			foreach (BasicBlock b in blocks)
			{
				DoGetFlatString(builder, b);
				builder.Append(Environment.NewLine);
			}
		}

		private static void DoGetBlocks(Dictionary<int, BasicBlock> table, BasicBlock block)
		{			
			if (!table.ContainsKey(block.First.Index))
			{
				table.Add(block.First.Index, block);
	
				if (block.Next != null)
					foreach (BasicBlock n in block.Next)
						DoGetBlocks(table, n);
			}
		}

		private static void DoGetFlatString(StringBuilder builder, BasicBlock block)
		{			
			if (block.Length > 0)
			{
				builder.Append(block.First.Untyped.Offset.ToString("X2"));
				builder.Append(" - ");
				builder.Append(block.Last.Untyped.Offset.ToString("X2"));
			}
			else
				builder.Append(block.Length.ToString());
			builder.Append(" => ");
			
			if (block.Next != null && block.Next.Length > 0)
			{
				for (int i = 0; i < block.Next.Length; ++i)
				{
					BasicBlock n = block.Next[i];
					
					if (i + 1 == block.Next.Length && block.Finally != null)
					{
						builder.Append(block.Finally.First.Untyped.Offset.ToString("X2"));
						builder.Append('/');
					}
					
					if (n.Length > 0)
						builder.Append(n.First.Untyped.Offset.ToString("X2"));
					else
					{
						builder.Append(n.Length.ToString());
						builder.Append('/');
						if (n.Next[0].Length > 0)
							builder.Append(n.Next[0].First.Untyped.Offset.ToString("X2"));
						else
							builder.Append(n.Next[0].Length.ToString());
					}
					builder.Append(' ');
				}
			}
			else if (block.Finally != null)
			{
				builder.Append(block.Finally.First.Untyped.Offset.ToString("X2"));
				builder.Append("/-");
			}
			else if (block.Fault != null)
			{
				builder.Append(block.Fault.First.Untyped.Offset.ToString("X2"));
				builder.Append("/-");
			}
		}
		#endregion 
		
		#region Fields
		private BasicBlock[] m_next;	
		private BasicBlock m_finally;
		private BasicBlock m_fault;
		#endregion 
	}
}
#endif
