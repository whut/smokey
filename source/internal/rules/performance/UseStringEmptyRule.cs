// Copyright (C) 2007 Jesse Jones
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//	Jesse Jones <jesjones@mindspring.com>
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
	internal sealed class UseStringEmptyRule : Rule
	{				
		public UseStringEmptyRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "P1002")
		{
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitBegin");
			dispatcher.Register(this, "VisitLoad");
			dispatcher.Register(this, "VisitEnd");
		}
				
		public void VisitBegin(BeginMethod method)
		{
			Log.DebugLine(this, "-----------------------------------"); 
			Log.DebugLine(this, "{0:F}", method.Info.Instructions);				

			m_max = DoIsIFormattableToString(method.Info.Method) ? 1 : 0;
			Log.DebugLine(this, "m_max = {0}", m_max); 
			
			m_offset = -1;
			m_count = 0;
		}
		
		public void VisitLoad(LoadString load)
		{
			if (m_count <= m_max)
			{
				if (load.Value.Length == 0)
				{
					if (m_offset < 0)
						m_offset = load.Untyped.Offset;

					++m_count;
					Log.DebugLine(this, "found empty string at {0:X2}", load.Untyped.Offset); 
				}
			}
		}

		public void VisitEnd(EndMethod method)
		{
			if (m_count > m_max)
				Reporter.MethodFailed(method.Info.Method, CheckID, m_offset, string.Empty);
		}
				
		private static bool DoIsIFormattableToString(MethodDefinition method)
		{
			bool equals = false;
			
			if (method.Parameters.Count == 2 && method.IsVirtual)
			{
				if (method.Name == "System.IFormattable.ToString" || method.Name == "ToString")
					if (method.Parameters[0].ParameterType.FullName == "System.String")
						if (method.Parameters[1].ParameterType.FullName == "System.IFormatProvider")
							equals = true;
			}
			
			return equals;
		}

		private int m_offset;
		private int m_max;
		private int m_count;
	}
}

