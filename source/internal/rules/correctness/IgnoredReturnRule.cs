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
using System.Diagnostics;
using Smokey.Framework;
using Smokey.Framework.Instructions;
using Smokey.Framework.Support;

#if OLD
namespace Smokey.Internal.Rules
{	
	internal sealed class IgnoredReturnRule : Rule
	{				
		public IgnoredReturnRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "C1018")
		{
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitBegin");
			dispatcher.Register(this, "VisitCall");
			dispatcher.Register(this, "VisitEnd");
		}
				
		public void VisitBegin(BeginMethod begin)
		{
			Log.DebugLine(this, "-----------------------------------"); 
			Log.DebugLine(this, "{0:F}", begin.Info.Instructions);				

			m_offset = -1;
			m_details = string.Empty;
		}
		
		public void VisitCall(Call call)
		{
			if (m_offset < 0)
			{
				if (call.Target.ReturnType.ReturnType.FullName != "System.Void")
				{
					if (DoValidMethod(call.Target))
					{
						if (call.Untyped.Next != null && call.Untyped.Next.OpCode.Code == Code.Pop)
						{
							m_offset = call.Untyped.Offset;
							m_details += call.Target + " ";
							Log.DebugLine(this, "bad call at {0:X2}", m_offset);				
						}
					}
				}
			}
		}

		public void VisitEnd(EndMethod end)
		{
			if (m_offset >= 0)
			{
				m_details = "Calling: " + m_details;
				Reporter.MethodFailed(end.Info.Method, CheckID, m_offset, m_details);
			}
		}
		
		private bool DoValidMethod(MethodReference method)	
		{
			string fullName = method.ToString();
			
			if (fullName.Contains("Mono.Security.ASN1::Add"))	
				return false;
				
			else if (fullName.Contains("System.Boolean System.Collections.Generic.HashSet`1<T>::Add"))
				return false;

			else if (fullName.Contains("System.Collections.ArrayList::Add"))
				return false;

			else if (fullName.Contains("System.Collections.IList::Add"))
				return false;

			else if (fullName.Contains("System.Collections.IList::Add"))
				return false;

			else if (fullName.Contains("System.Diagnostics.Process::Start()"))	// note that people should generally use the result of the other overloads
				return false;

			else if (fullName.Contains("System.IO.Directory::CreateDirectory"))
				return false;
				
			else if (fullName.Contains("System.Diagnostics.TraceListenerCollection::Add"))
				return false;
				
			else if (fullName.Contains("System.IO.DirectoryInfo::CreateSubdirectory"))
				return false;
								
			else if (fullName.Contains("System.IO.FileStream::Seek"))
				return false;

			else if (fullName.Contains("System.IO.MemoryStream::Seek"))
				return false;

			else if (fullName.Contains("System.IO.Stream::Seek"))
				return false;

			else if (fullName.Contains("System.IO.UnmanagedMemoryStream::Seek"))
				return false;

			else if (fullName.Contains("System.Security.PermissionSet::AddPermission"))
				return false;
				
			else if (fullName.Contains("System.Security.PermissionSet::RemovePermission"))
				return false;
				
			else if (fullName.Contains("System.Security.PermissionSet::SetPermission"))
				return false;
				
			else if (fullName.Contains("System.Reflection.MethodBase::Invoke"))
				return false;
				
			else if (fullName.Contains("System.Runtime.InteropServices.Marshal::AddRef"))
				return false;

			else if (fullName.Contains("System.Runtime.InteropServices.Marshal::Release"))
				return false;

			else if (fullName.Contains("System.Runtime.Remoting.Lifetime.ILease::Renew"))
				return false;

			else if (fullName.Contains("System.Text.StringBuilder::Append"))	
				return false;

			else if (fullName.Contains("System.Text.StringBuilder::Insert"))
				return false;

			else if (fullName.Contains("System.Text.StringBuilder::Remove"))
				return false;

			else if (fullName.Contains("System.Text.StringBuilder::Replace"))
				return false;

			else if (fullName.Contains("System.Threading.Interlocked::Add"))
				return false;

			else if (fullName.Contains("System.Threading.Interlocked::CompareExchange"))
				return false;

			else if (fullName.Contains("System.Threading.Interlocked::Decrement"))
				return false;

			else if (fullName.Contains("System.Threading.Interlocked::Exchange"))
				return false;

			else if (fullName.Contains("System.Threading.Interlocked::Increment"))
				return false;

			else if (fullName.Contains("System.Windows.Forms.ListBox/ObjectCollection::Add"))
				return false;

			else if (fullName.Contains("System.Windows.Forms.Menu/MenuItemCollection::Add"))
				return false;

			else if (fullName.Contains("System.Xml.Schema.XmlSchemaSet::Add"))
				return false;

			else if (method.Name == "BeginInvoke")
			{
				if (method.Parameters.Count > 0)
					if (method.Parameters[0].ParameterType.FullName == "System.Delegate")
						return false;

					else if (method.Parameters[0].ParameterType.FullName == "System.IAsyncResult")
						return false;
			}
				
			return true;
		}
				
		private int m_offset;
		private string m_details;
	}
}
#endif