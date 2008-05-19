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
using Smokey.Framework.Support;

namespace Smokey.Framework.Instructions
{				
	/// <summary>Represents Call and Callvirt.</summary>
	/// 
	/// <remarks>A call to a method. </remarks>.
	public class Call : TypedInstruction
	{		
		internal Call(Instruction untyped, int index) : base(untyped, index)
		{			
			Target = (MethodReference) untyped.Operand;
		}
		
		/// <summary>The method we're calling.</summary>
		public readonly MethodReference Target;
		
		/// <summary>Returns true if we're calling an instance method with caller's this
		/// pointer, or if we're calling a static method in caller's declaring
		/// type with caller's this pointer as an argument.</summary>
		public bool IsThisCall(AssemblyCache cache, MethodInfo caller, int callIndex)
		{
			DBC.Pre(cache != null, "cache is null");				
			DBC.Pre(caller != null, "caller is null");
				
			if (!m_isThisCall.HasValue)
			{
				m_isThisCall = false;

				MethodInfo target = cache.FindMethod(Target);
				if (target != null && !target.Method.IsConstructor)
				{
					if (target.Method.HasThis)
					{
						int nth = target.Method.Parameters.Count;
						int j = caller.Tracker.GetStackIndex(callIndex, nth);
						
						if (j >= 0)
							m_isThisCall = caller.Instructions.LoadsThisArg(j);
					}
					else if (target.Method.IsStatic)
					{
						for (int i = 0; i < target.Method.Parameters.Count && !m_isThisCall.Value; ++i)
						{
							int j = caller.Tracker.GetStackIndex(callIndex, i);				

							if (j >= 0)
								m_isThisCall = caller.Instructions.LoadsThisArg(j);
						}
					}
				}
			}
			return m_isThisCall.Value;
		}
				
		private bool? m_isThisCall;
	}
}
