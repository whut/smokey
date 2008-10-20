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
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Smokey.Framework.Instructions;
using Smokey.Framework.Support.Advanced;
using Smokey.Framework.Support.Advanced.Values;

namespace Smokey.Framework.Support
{	
	/// <summary>Precomputed objects used by method rules.</summary>
	public class MethodInfo
	{
		public TypeDefinition Type {get; private set;}
		public MethodDefinition Method {get; private set;}		// will always have a body
		public TypedInstructionCollection Instructions {get; private set;}	

		internal MethodInfo(SymbolTable symbols, TypeDefinition type, MethodDefinition method)
		{
			Type = type;
			Method = method;
			Instructions = new TypedInstructionCollection(symbols, Method);
		}
		
#if TEST
		internal MethodInfo(TypeDefinition type, MethodDefinition method)
		{
			Type = type;
			Method = method;
			Instructions = new TypedInstructionCollection(new SymbolTable(), Method);
		}
#endif
		
		public ControlFlowGraph Graph 
		{
			get
			{
				if (m_graph == null && Method.Body != null)
					m_graph = new ControlFlowGraph(Instructions);
					
				return m_graph;
			}
		}
		
		public Tracker Tracker
		{
			get
			{
				if (m_tracker == null && Method.Body != null)
				{
					m_tracker = new Tracker(Instructions);
					Tracker.Analyze(Graph);
				}
				
				return m_tracker;
			}
		}

		public void Reset()
		{
			Instructions = null;
			m_graph = null;
			m_tracker = null;
		}

		public override bool Equals(object rhsObj)
		{
			if (rhsObj == null)                        // objects may be null
				return false;
			
			if (GetType() != rhsObj.GetType()) 
				return false;
		
			MethodInfo rhs = (MethodInfo) rhsObj;                    
			return Type == rhs.Type && Method == rhs.Method;
		}
			
		public override int GetHashCode()
		{
			int hash;
			
			unchecked
			{
				hash = Type.GetHashCode() + Method.GetHashCode();
			}
			
			return hash;
		}
		
		private ControlFlowGraph m_graph;
		private Tracker m_tracker;
	}
}

