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

using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Text;

namespace Smokey.Framework.Support.Advanced
{	
	// Classes used with DataFlow to track the state of arguments, locals, and
	// the stack.
	namespace Values
	{	
		/// <summary>Value and index of an item on the stack.</summary>
		public struct StackEntry
		{
			/// <summary>The value at some point in the stack.</summary>
			public readonly long? Value;
			
			/// <summary>The index of the instruction that pushed the value. Will be
			/// -1 if there's not a single index that set the value.</summary>
			public readonly int Index;
			
			internal StackEntry(long? value, int index)
			{
				Value = value;
				Index = index;
			}
		
			public override bool Equals(object rhsObj)
			{
				if (rhsObj == null)                        // objects may be null
					return false;
				
				if (GetType() != rhsObj.GetType()) 
					return false;
			
				StackEntry rhs = (StackEntry) rhsObj;                    
				return this == rhs;
			}
					
			public static bool operator==(StackEntry lhs, StackEntry rhs)
			{
				return lhs.Value == rhs.Value && lhs.Index == rhs.Index;
			}
			
			public static bool operator!=(StackEntry lhs, StackEntry rhs)
			{
				return !(lhs == rhs);
			}    

			public override int GetHashCode()
			{
				int hash;
				
				unchecked
				{
					hash = Value.GetHashCode() + Index.GetHashCode();
				}
				
				return hash;
			}
		}
		
		/// <summary>State of arguments, locals, and the stack at a particular point in 
		/// the method.</summary>
		public struct State	: IEquatable<State>
		{			
			internal State(long?[] args, long?[] locals, List<StackEntry> stack)
			{
				DBC.Pre(args != null, "args is null");
				DBC.Pre(locals != null, "locals is null");
				DBC.Pre(stack != null, "stack is null");
				
				m_args = args;
				m_locals = locals;
				m_stack = stack;
			}
			
			// If the method has a this pointer it will be the first argument.
			public long?[] Arguments	
			{
				get {return m_args;}
			}
						
			public long?[] Locals	
			{
				get {return m_locals;}
			}
			
			// Top of the stack is the last value.
			public List<StackEntry> Stack	
			{
				get {return m_stack;}
			}
						
			public override string ToString()
			{
				StringBuilder builder = new StringBuilder();
				
				if (m_args != null)
				{
					for (int i = 0; i < m_args.Length; ++i)
					{
						builder.Append("A_");
						builder.Append(i.ToString());
						builder.Append(" = ");
						builder.Append(DoValueToString(m_args[i]));
						builder.Append(' ');
					} 
				
					for (int i = 0; i < m_locals.Length; ++i)
					{
						builder.Append("V_");
						builder.Append(i.ToString());
						builder.Append(" = ");
						builder.Append(DoValueToString(m_locals[i]));
						builder.Append(' ');
					} 
				
					builder.Append("S = ");
					for (int i = 0; i < m_stack.Count; ++i)
					{
						builder.Append(DoValueToString(m_stack[i].Value));
						builder.Append(' ');
					} 			
				}
				else
					builder.Append("top");
					
				return builder.ToString();
			}

			#region Equality
			public override bool Equals(object rhsObj)
			{
				if (rhsObj == null)					
					return false;
				
				if (GetType() != rhsObj.GetType()) 
					return false;
			
				State rhs = (State) rhsObj;					
				return this == rhs;
			}
				
			public bool Equals(State rhs)
			{					
				return this == rhs;
			}

			public static bool operator==(State lhs, State rhs)
			{				
				return DoIsEquals(lhs.m_args, rhs.m_args) &&
						DoIsEquals(lhs.m_locals, rhs.m_locals) &&
						DoIsEquals(lhs.m_stack, rhs.m_stack);
			}
			
			public static bool operator!=(State lhs, State rhs)
			{
				return !(lhs == rhs);
			}
			
			public override int GetHashCode()
			{
				int hash = 0;
				
				unchecked
				{
					foreach (long? value in m_args)
						hash += value.GetHashCode();
	
					foreach (long? value in m_locals)
						hash += value.GetHashCode();
	
					foreach (StackEntry value in m_stack)
						hash += value.Value.GetHashCode();
				}
				
				return hash;
			}
			#endregion
			
			#region Private methods
			private static string DoValueToString(long? value)
			{
				if (value.HasValue)
					return value.Value.ToString();
				else
					return "?";		// indeterminate
			}
			
			private static bool DoIsEquals(long?[] lhs, long?[] rhs)
			{
				bool equals = lhs.Length == rhs.Length;
				
				for (int i = 0; i < lhs.Length && equals; ++i)
				{
					if (lhs[i].HasValue && rhs[i].HasValue)
						equals = lhs[i].Value == rhs[i].Value;
	
					else if (!lhs[i].HasValue && !rhs[i].HasValue)
						equals = true;
	
					else
						equals = false;
				}
				
				return equals;
			}
			
			private static bool DoIsEquals(List<StackEntry> lhs, List<StackEntry> rhs)
			{
				bool equals = lhs.Count == rhs.Count;
				
				for (int i = 0; i < lhs.Count && equals; ++i)
				{
					if (lhs[i].Value.HasValue && rhs[i].Value.HasValue)
						equals = lhs[i].Value.Value == rhs[i].Value.Value;
	
					else if (!lhs[i].Value.HasValue && !rhs[i].Value.HasValue)
						equals = true;
	
					else
						equals = false;
				}
				
				return equals;
			}
			#endregion
			
			#region Fields
			private readonly long?[] m_args;
			private readonly long?[] m_locals;
			private readonly List<StackEntry> m_stack;	// last element is the top of the stack
			#endregion
		}
	}
}

