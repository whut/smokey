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

namespace Smokey.Internal.Rules
{	
	internal sealed class ThrowDerivedRule : Rule
	{				
		public ThrowDerivedRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "MS1012")
		{
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitBegin");
			dispatcher.Register(this, "VisitThrow");
			dispatcher.Register(this, "VisitEnd");
		}
				
		public void VisitBegin(BeginMethod method)
		{
			Log.DebugLine(this, "-----------------------------------"); 
			Log.DebugLine(this, "{0:F}", method.Info.Instructions);				

			m_offset = -1;
			m_info = method.Info;
		}
		
		public void VisitThrow(Throw thrw)
		{
			if (m_offset < 0)
			{
				NewObj newObj = m_info.Instructions[thrw.Index - 1] as NewObj;
				if (newObj != null)
				{
					string name = newObj.Ctor.ToString();
					if (name.Contains("System.Exception::.ctor") || name.Contains("System.SystemException::.ctor") || name.Contains("System.ApplicationException::.ctor"))
					{
						m_offset = thrw.Untyped.Offset;
						Log.DebugLine(this, "bad throw at {0:X2}", m_offset); 
					}
				}
			}
		}

		public void VisitEnd(EndMethod method)
		{
			if (m_offset >= 0)
			{
				Reporter.MethodFailed(method.Info.Method, CheckID, m_offset, string.Empty);
			}
		}
				
		private int m_offset;
		private MethodInfo m_info;
	}
}

