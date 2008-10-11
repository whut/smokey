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

namespace Smokey.Internal.Rules
{	
	internal sealed class DerivedDisposeableRule : Rule
	{				
		public DerivedDisposeableRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "D1064")
		{
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitBegin");
			dispatcher.Register(this, "VisitMethodBegin");
			dispatcher.Register(this, "VisitCall");
			dispatcher.Register(this, "VisitEnd");
		}
				
		public void VisitBegin(BeginMethods begin)
		{
			m_type = begin.Type;
			m_disposable = m_type.BaseImplements("System.IDisposable", Cache) && !m_type.IsCompilerGenerated();
			
			m_hasFinalizer = false;
			m_hasDispose = false;

			m_disposeMethod = null;
			m_hasBaseCall = false;
			
			if (m_disposable)
			{
				Log.DebugLine(this, "-----------------------------------"); 
				Log.DebugLine(this, "{0}", m_type);				
			}
		}

		public void VisitMethodBegin(BeginMethod begin)	
		{
			m_inDispose = false;
			
			if (m_disposable)
			{
				Log.DebugLine(this, "{0:F}", begin.Info.Instructions);			

				MethodDefinition method = begin.Info.Method;
				if (method.Name == "Finalize")
				{	
					MethodDefinition previous = method.GetPreviousMethod(Cache);
					if (previous != method)
					{
						Log.DebugLine(this, "finalizer overrides {0}", previous);
						m_hasFinalizer = true;
					}
				}
				
				if (method.Matches("System.Void", "Dispose"))
				{
					Log.DebugLine(this, "has Dispose()");				
					m_hasDispose = true;
				}

				if (method.Matches("System.Void", "Dispose", "System.Boolean"))
				{
					Log.DebugLine(this, "has Dispose(bool)");				
					m_disposeMethod = method;
					m_inDispose = true;
				}
			}
		}	
		
		// ldarg.0 
        // ldarg.1 
        // call instance void class Smokey.Tests.DerivedDisposeableTest/GoodBase1::Dispose(bool)
		public void VisitCall(Call call)
		{
			if (m_disposable)
			{
				if (!m_hasBaseCall && m_inDispose)
				{
					if (call.Target.Matches("System.Void", "Dispose", "System.Boolean"))
					{
						Log.DebugLine(this, "has base.Dispose(bool) call");		// we don't check the this argument or declrating type, but it would have to be some really whacked code for that to matter		
						m_hasBaseCall = true;
					}
				}
			}
		}

		public void VisitEnd(EndMethods end)
		{
			string details = string.Empty;
			
			if (m_disposeMethod != null && !m_hasBaseCall)
			{
				MethodDefinition pmethod = m_disposeMethod.GetPreviousMethod(Cache);
				if (pmethod != m_disposeMethod && !pmethod.IsAbstract)
					details += "Dispose(bool) does not call base.Dispose(bool)" + Environment.NewLine;
			}
			
			if (m_hasFinalizer)
				details += "Type has a finalizer" + Environment.NewLine;

			if (m_disposable && end.Type.TypeImplements("System.IDisposable"))
				details += "Type reimplements IDisposable" + Environment.NewLine;

			if (m_hasDispose)
				details += "Type has a Dispose() method" + Environment.NewLine;

			details = details.Trim();
			if (details.Length > 0)
			{
				Log.DebugLine(this, "Details: {0}", details);
				Reporter.TypeFailed(end.Type, CheckID, details);
			}
		}
		
		private bool m_disposable;
		private TypeDefinition m_type;
		
		private bool m_hasFinalizer;
		private bool m_hasDispose;

		private MethodDefinition m_disposeMethod;
		private bool m_inDispose;
		private bool m_hasBaseCall;
	}
}
