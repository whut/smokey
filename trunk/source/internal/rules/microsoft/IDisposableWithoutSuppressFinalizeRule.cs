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
	internal class IDisposableWithoutSuppressFinalizeRule : Rule
	{				
		public IDisposableWithoutSuppressFinalizeRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "MS1007")
		{
		}
						
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitBegin");	
			dispatcher.Register(this, "VisitCall");	
			dispatcher.Register(this, "VisitEnd");
		}
				
		public void VisitBegin(BeginMethod method)
		{		
			m_needsCheck = false;
			if (method.Info.Type.Implements("System.IDisposable"))
			{					
				if (DoIsIDispose(method.Info.Method))
				{
					if (DoHasNativeField(method.Info.Type))
						m_needsCheck = true;
				}
			}
			
			if (m_needsCheck)
			{
				Log.DebugLine(this, "-----------------------------------"); 
				Log.DebugLine(this, "{0:F}", method.Info.Instructions);				
				
				m_foundSuppress = false;
			}
		}
		
		// Note that this function just checks Dispose() for calls to GC::SuppressFinalize.
		// It's possible that someone is calling it from a helper function, but this doesn't
		// seem very useful.
		public void VisitCall(Call call)
		{
			if (m_needsCheck && !m_foundSuppress)
			{
				if (call.Target.ToString().Contains("System.GC::SuppressFinalize(System.Object)"))
				{
					m_foundSuppress = true;
					Log.DebugLine(this, "suppress call at {0:X2}", call.Untyped.Offset); 
				}
			}
		}

		public void VisitEnd(EndMethod method)
		{
			if (m_needsCheck && !m_foundSuppress)
				Reporter.MethodFailed(method.Info.Method, CheckID, 0, string.Empty);
		}
						
		private static bool DoHasNativeField(TypeDefinition type)
		{
			bool has = false;
			
			for (int i = 0; i < type.Fields.Count && !has; ++i)
			{
				FieldDefinition field = type.Fields[i];
				
				if (!field.IsStatic && (field.FieldType.FullName == "System.IntPtr" || field.FieldType.FullName == "System.UIntPtr"))
					has = true;
			}
			
			return has;
		}
				
		private static bool DoIsIDispose(MethodDefinition method)
		{
			bool yup = false;
			
			if (method.Name == "Dispose" && !method.ToString().Contains("CompilerGenerated"))
			{
				if ((method.Attributes & MethodAttributes.MemberAccessMask) == MethodAttributes.Public)
				{
					if (method.IsFinal && method.IsVirtual)
						yup = true;
				}
			}
			
			return yup;
		}
				
		private bool m_needsCheck;
		private bool m_foundSuppress;
	}
}

