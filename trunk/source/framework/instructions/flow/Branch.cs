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

namespace Smokey.Framework.Instructions
{		
	/// <summary>Represents UnconditionalBranch and
	/// and ConditionalBranch.</summary>
	/// 
	/// <remarks>Conditional or unconditional branch. </remarks>
	public class Branch : TypedInstruction
	{		
		internal Branch(Instruction untyped, int index) : base(untyped, index)
		{
		}

		public TypedInstruction Target
		{
			get {return m_target;}
			internal set {m_target = value;}
		}

		protected override string OnOperandToString()
		{
			if (m_target != null)
				return m_target.Untyped.Offset.ToString("X2");
			else
				return "unset";
		}
		
		private TypedInstruction m_target;
	}
}

