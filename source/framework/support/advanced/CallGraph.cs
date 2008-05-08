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
using Mono.Cecil.Metadata;
using Smokey.Internal;
using System;
using System.Collections.Generic;

namespace Smokey.Framework.Support.Advanced
{			
	/// <summary>List of all the methods every method calls.</summary>
	public class CallGraph 
	{				
		/// <summary>Enumerates over all of the methods that method calls directly.</summary>
		/// <remarks>Note that there may be cycles in the graph and the methods that
		/// are returned may not be defined in the assembly we're checking.</remarks>
		public IEnumerable<MethodReference> Calls(MethodReference method)
		{			
			List<MethodReference> list;
			if (m_graph.TryGetValue(method, out list))
			{
				foreach (MethodReference token in list)
					yield return token;
			}		
		}
				
		/// <summary>Returns all of the methods that method calls directly.</summary>
		/// <remarks>Note that there may be cycles in the graph and the methods that
		/// are returned may not be defined in the assembly we're checking.</remarks>
		public List<MethodReference> GetCalls(MethodReference method)
		{			
			List<MethodReference> list;
			Ignore.Value = m_graph.TryGetValue(method, out list);	// this will return false if the method is defined outside the assembly we're checking
			return list;		
		}
				
		/// <summary>Enumerates over all the methods and the called methods.</summary>
		public IEnumerable<KeyValuePair<MethodReference, List<MethodReference>>> Entries()
		{			
			IEnumerable<KeyValuePair<MethodReference, List<MethodReference>>> e = m_graph;	
			return e;
		}
				
		internal void Add(MethodReference method, MethodReference callee)
		{
			List<MethodReference> list;
			if (!m_graph.TryGetValue(method, out list))
			{
				list = new List<MethodReference>();
				m_graph.Add(method, list);
			}		
			
			if (list.IndexOf(callee) < 0)
				list.Add(callee);
		}
										
		#region Fields --------------------------------------------------------
		private Dictionary<MethodReference, List<MethodReference>> m_graph = new Dictionary<MethodReference, List<MethodReference>>();
		#endregion
	}
}
