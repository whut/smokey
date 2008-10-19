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
using System.Collections.Generic;
using System.Text;
using Smokey.Framework;
using Smokey.Framework.Instructions;
using Smokey.Framework.Support;

#if OLD
namespace Smokey.Internal.Rules
{	
	internal sealed class BaseDisposeableRule : Rule
	{				
		public BaseDisposeableRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "D1063")
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
			m_disposable = false;
			
			TypeDefinition baseType = Cache.FindType(begin.Type.BaseType);
			
			if (!begin.Type.IsSealed && !begin.Type.IsNestedPrivate && !begin.Type.IsCompilerGenerated())
				if (begin.Type.TypeImplements("System.IDisposable"))
					if (baseType == null || !baseType.TypeOrBaseImplements("System.IDisposable", Cache))	// don't check types with redundent IDisposable declarations
						if (!begin.Type.IsInterface)
							m_disposable = true;
			m_hasUnaryDispose = false;
			
			if (m_disposable)
			{
				Log.DebugLine(this, "-----------------------------------"); 
				Log.DebugLine(this, "{0}", begin.Type);				
			}
		}

		public void VisitMethodBegin(BeginMethod begin)
		{			
			if (m_disposable)
			{
				MethodDefinition method = begin.Info.Method;
				
				if (method.Matches("System.Void", "Dispose", "System.Boolean"))
					m_hasUnaryDispose = true;
			}		
		}
		
		public void VisitEnd(EndMethods end)
		{
			if (m_disposable)
			{
				string details = string.Empty;	
				if (!m_hasUnaryDispose)				// DisposableRule will verify protected and virtual
					details += "The type is unsealed, but does not have Dispose(bool)." + Environment.NewLine;
	
				details = details.Trim();
				if (details.Length > 0)
				{
					Log.DebugLine(this, "Details: {0}", details);
					Reporter.TypeFailed(end.Type, CheckID, details);
				}
			}
		}
						
		private bool m_disposable;
		private bool m_hasUnaryDispose;
	}
}
#endif
