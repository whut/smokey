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
using Smokey.Framework;
using Smokey.Framework.Instructions;
using Smokey.Framework.Support;

#if OLD
namespace Smokey.Internal.Rules
{	
	internal sealed class UseIEquatableRule : Rule
	{				
		public UseIEquatableRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "P1023")
		{
		}
		
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitBegin");
			dispatcher.Register(this, "VisitMethodBegin");
			dispatcher.Register(this, "VisitEnd");
		}

		public void VisitBegin(BeginMethods begin)
		{
			Log.DebugLine(this, "-----------------------------------"); 
			Log.DebugLine(this, "{0}", begin.Type);				

			m_foundEquals = false;					
		}

		public void VisitMethodBegin(BeginMethod begin)
		{			
			if (!m_foundEquals)
			{
				if (begin.Info.Method.Matches("System.Boolean", "Equals", begin.Info.Type.FullName))
				{
					if (begin.Info.Method.IsPublic)
					{
						Log.DebugLine(this, "found an Equals(T) method");
						m_foundEquals = true;
					}
				}
			}
		}

		public void VisitEnd(EndMethods end)
		{
			if (m_foundEquals)
			{
//				for (int i = 0; i < end.Type.Interfaces.Count; ++i)
//				{
//					TypeReference t = end.Type.Interfaces[i];
//					Log.DebugLine(this, "implements: {0}", t.FullName);
//				}

				if (!end.Type.TypeImplements("System.IEquatable"))
				{
					Log.DebugLine(this, "doesn't implement IEquatable");
					Reporter.TypeFailed(end.Type, CheckID, string.Empty);
				}
			}
		}
		
		private bool m_foundEquals;
	}
}
#endif
