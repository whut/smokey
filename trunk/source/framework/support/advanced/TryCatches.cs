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
using Smokey.Framework.Instructions;

namespace Smokey.Framework.Support.Advanced
{	
	/// <summary>used by TryCatch and CatchBlock.</summary>
	public struct Block
	{
		public int Length	{get {return m_length;} internal set {m_length = value;}}
		public int Index	{get {return m_index;} internal set {m_index = value;}}
	
		public override bool Equals(object rhsObj)
		{
			if (rhsObj == null)                        // objects may be null
				return false;
			
			if (GetType() != rhsObj.GetType()) 
				return false;
		
			Block rhs = (Block) rhsObj;                    
			return this == rhs;
		}

		public static bool operator==(Block lhs, Block rhs)
		{
			return lhs.Index == rhs.Index && lhs.Length == rhs.Length;
		}
		
		public static bool operator!=(Block lhs, Block rhs)
		{
			return !(lhs == rhs);
		}
				
		public override int GetHashCode()
		{
			int hash;
			
			unchecked
			{
				hash = Index.GetHashCode() + Length.GetHashCode();
			}
			
			return hash;
		}
		
		private int m_index;
		private int m_length;
	}
	
	/// <summary>TryCatch returns these.</summary>
	public struct CatchBlock
	{
		public int Index				{get {return m_index;} internal set {m_index = value;}}
		public int Length				{get {return m_length;} internal set {m_length = value;}}
		public TypeReference CatchType	{get {return m_catchType;} internal set {m_catchType = value;}}
	
		public override bool Equals(object rhsObj)
		{
			if (rhsObj == null)                        // objects may be null
				return false;
			
			if (GetType() != rhsObj.GetType()) 
				return false;
		
			CatchBlock rhs = (CatchBlock) rhsObj;                    
			return this == rhs;
		}
				
		public override int GetHashCode()
		{
			int hash;
			
			unchecked
			{
				hash = Index.GetHashCode() + Length.GetHashCode();
			}
			
			return hash;
		}
		
		public static bool operator==(CatchBlock lhs, CatchBlock rhs)
		{
			return lhs.Index == rhs.Index && lhs.Length == rhs.Length;
		}
		
		public static bool operator!=(CatchBlock lhs, CatchBlock rhs)
		{
			return !(lhs == rhs);
		}    

		private int m_index;
		private int m_length;
		private TypeReference m_catchType;
	}
	
	/// <summary>TryCatchCollection returns these.</summary>
	public class TryCatch				// class because we rely on aliasing
	{
		public Block Try					{get {return m_try;} internal set {m_try = value;}}
		public List<CatchBlock> Catchers	{get {return m_catchers;} internal set {m_catchers = value;}}
		public Block Finally				{get {return m_finally;} internal set {m_finally = value;}}
		public Block Fault					{get {return m_fault;} internal set {m_fault = value;}}
	
		public override bool Equals(object rhsObj)
		{
			if (rhsObj == null)                        // objects may be null
				return false;
			
			if (GetType() != rhsObj.GetType()) 
				return false;
		
			TryCatch rhs = (TryCatch) rhsObj;                    
			return Try == rhs.Try;
		}
				
		public override int GetHashCode()
		{
			return Try.GetHashCode();
		}

		private Block m_try;
		private List<CatchBlock> m_catchers;
		private Block m_finally;			// may have zero length
		private Block m_fault;				// may have zero length
	}
	
	/// <summary>Used to find the try, catch, and finally blocks in a method.</summary>
	[DisableRule("D1041", "CircularReference")]	// Smokey.Framework.Instructions.TypedInstructionCollection <-> Smokey.Framework.Support.Advanced.TryCatchCollection  
	public class TryCatchCollection : IEnumerable<TryCatch>
	{
		internal TryCatchCollection(TypedInstructionCollection instructions)
		{
			if (instructions.Method.Body != null)
				DoGetTryCatches(instructions);
		}
		
		public IEnumerator<TryCatch> GetEnumerator()
		{
			foreach (TryCatch tc in m_trys)
				yield return tc;
		}
			
		IEnumerator IEnumerable.GetEnumerator()
		{
			return m_trys.GetEnumerator();
		}
		
		public int Length {get {return m_trys.Count;}}
		
		public TryCatch this[int index]
		{
			get {return m_trys[index];}
		}
				
		/// <summary>Returns a TryCatch if the try block, catch block, or finally block
		/// starts at index. If a suitable TryCatch cannot be found null is returned.</summary>
		public TryCatch HandlerStartsAt(int index)
		{
			DBC.Pre(index >= 0, "index is negative");

			for (int i = 0; i < m_trys.Count; ++i)
			{
				if (m_trys[i].Try.Index == index)
					return m_trys[i];
					
				for (int j = 0; j < m_trys[i].Catchers.Count; ++j)
				{
					if (m_trys[i].Catchers[j].Index == index)
						return m_trys[i];
				}
	
				if (m_trys[i].Finally.Index == index && m_trys[i].Finally.Length > 0)
					return m_trys[i];
	
				else if (m_trys[i].Fault.Index == index && m_trys[i].Fault.Length > 0)
					return m_trys[i];
			}
			
			return null;
		}
				
		#region Private methods
		// There will be separate handlers for each catch block and for the
		// finally block (if present). Handlers that share the same try block
		// will have equal [TryStart, TryEnd) values. The handler block will
		// be in [HandlerStart, HandlerEnd). 
		private void DoGetTryCatches(TypedInstructionCollection instructions)
		{
			if (instructions.Method.Body.ExceptionHandlers.Count == 0)
				return;
				
			Log.DebugLine(this, "----------------------------------------");
			Log.DebugLine(this, "{0:F}", instructions);
			foreach (ExceptionHandler handler in instructions.Method.Body.ExceptionHandlers)
			{
				Log.DebugLine(this, "handler.TryStart.Offset {0:X2}", handler.TryStart.Offset);
				Log.DebugLine(this, "handler.TryEnd.Offset {0:X2}", handler.TryEnd.Offset);

				if (handler.Type == ExceptionHandlerType.Catch)
				{
					Log.DebugLine(this, "handler.CatchStart.Offset {0:X2}", handler.HandlerStart.Offset);
					Log.DebugLine(this, "handler.CatchEnd.Offset {0:X2}", handler.HandlerEnd.Offset);
				}
				else if (handler.Type == ExceptionHandlerType.Finally)
				{
					Log.DebugLine(this, "handler.FinallyStart.Offset {0:X2}", handler.HandlerStart.Offset);
					Log.DebugLine(this, "handler.FinallyEnd.Offset {0:X2}", handler.HandlerEnd.Offset);
				}
				else if (handler.Type == ExceptionHandlerType.Fault)
				{
					Log.DebugLine(this, "handler.FaultStart.Offset {0:X2}", handler.HandlerStart.Offset);
					Log.DebugLine(this, "handler.FaultEnd.Offset {0:X2}", handler.HandlerEnd.Offset);
				}
			}
			
			// First add the catch blocks and the try blocks they're associated with.
			var tries = new Dictionary<long, TryCatch>();
			foreach (ExceptionHandler handler in instructions.Method.Body.ExceptionHandlers)
			{
				if (handler.Type == ExceptionHandlerType.Catch)
				{
					TryCatch tc = new TryCatch();
					tc.Try = DoGetBlock(instructions, handler.TryStart.Offset, handler.TryEnd.Offset);
					tc.Catchers = new List<CatchBlock>();
					m_trys.Add(tc);

					Block b = DoGetBlock(instructions, handler.HandlerStart.Offset, handler.HandlerEnd.Offset);
					tries.Add(10000L * handler.TryStart.Offset + handler.HandlerEnd.Offset, tc);
					
					CatchBlock block = new CatchBlock();
					block.Index = b.Index;
					block.Length = b.Length;
					block.CatchType = handler.CatchType;
					
					tc.Catchers.Add(block);
				}
			}
			
			// Then add the finally blocks and their try blocks if they weren't added already.
			foreach (ExceptionHandler handler in instructions.Method.Body.ExceptionHandlers)
			{
				if (handler.Type == ExceptionHandlerType.Finally || handler.Type == ExceptionHandlerType.Fault)
				{
					long key = 10000L * handler.TryStart.Offset + handler.TryEnd.Offset;
					
					TryCatch tc;
					if (!tries.TryGetValue(key, out tc))
					{
						tc = new TryCatch();
						tc.Try = DoGetBlock(instructions, handler.TryStart.Offset, handler.TryEnd.Offset);
						tc.Catchers = new List<CatchBlock>();
						m_trys.Add(tc);
					}

					if (handler.Type == ExceptionHandlerType.Finally)
						tc.Finally = DoGetBlock(instructions, handler.HandlerStart.Offset, handler.HandlerEnd.Offset);
					else
						tc.Fault = DoGetBlock(instructions, handler.HandlerStart.Offset, handler.HandlerEnd.Offset);
				}
			}
				
			foreach (TryCatch tc in m_trys)
			{
				Log.DebugLine(this, "try {0:X2} to {1:X2}", instructions[tc.Try.Index].Untyped.Offset, instructions[tc.Try.Index + tc.Try.Length - 1].Untyped.Offset);
				foreach (CatchBlock c in tc.Catchers)
				{
					Log.DebugLine(this, "   catch {0:X2} to {1:X2}", instructions[c.Index].Untyped.Offset, instructions[c.Index + c.Length - 1].Untyped.Offset);
				}
				if (tc.Finally.Length > 0)
					Log.DebugLine(this, "   finally {0:X2} to {1:X2}", instructions[tc.Finally.Index].Untyped.Offset, instructions[tc.Finally.Index + tc.Finally.Length - 1].Untyped.Offset);
				else if (tc.Fault.Length > 0)
					Log.DebugLine(this, "   finally {0:X2} to {1:X2}", instructions[tc.Fault.Index].Untyped.Offset, instructions[tc.Fault.Index + tc.Fault.Length - 1].Untyped.Offset);
			}
		}
		
		private static Block DoGetBlock(TypedInstructionCollection instructions, int startOffset, int endOffset)
		{
			int startIndex = instructions.OffsetToIndex(startOffset);
			int endIndex = instructions.OffsetToIndex(endOffset);

			Block block = new Block();
			block.Index = startIndex;
			block.Length = endIndex - startIndex;
			
			return block;
		}
		#endregion
		
		#region Fields
		private List<TryCatch> m_trys = new List<TryCatch>();
		#endregion
	}
}
