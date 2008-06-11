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
using Mono.Cecil.Metadata;
using System;
using System.Collections.Generic;
using Smokey.Framework.Instructions;
using Smokey.Framework.Support;
using Smokey.Internal;

namespace Smokey.Framework.Support.Advanced
{			
	/// <summary>Tracks various properties of methods.</summary>
	[DisableRule("D1041", "CircularReference")]	// Smokey.Framework.Support.Advanced.ClassifyMethod <-> Smokey.Framework.Support.RuleDispatcher  
	public class ClassifyMethod 
	{				
		internal void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitAssembly", "ClassifyMethod", "I1000");
			dispatcher.Register(this, "VisitBegin", "ClassifyMethod", "I1000");
			dispatcher.Register(this, "VisitNewObj", "ClassifyMethod", "I1000");
		}
		
		/// <summary>Returns all of the thread callback methods in the assembly.</summary>
		/// <remarks>This includes callbacks used with Thread, ThreadPool, Timer
		/// and async callbacks.</remarks>
		public IEnumerable<MethodReference> ThreadRoots()
		{			
			foreach (MethodReference method in m_threadRoots)
				yield return method;
		}
																						
		#region Visitors
		public void VisitAssembly(AssemblyDefinition assembly)
		{
			Unused.Arg(assembly);
			
			Log.DebugLine(this, "++++++++++++++++++++++++++++++++++"); 
			m_threadRoots.Clear();		// need to do this here for unit tests
		}
				
		public void VisitBegin(BeginMethod begin)
		{
			DBC.Pre(begin != null, "begin is null");
				
			Log.DebugLine(this, "-----------------------------------"); 
			Log.DebugLine(this, "{0:F}", begin.Info.Instructions);				

			m_info = begin.Info;	
		
			MethodDefinition method = begin.Info.Method;
			if (method.Name == "Finalize")
			{
				Log.DebugLine(this, "found finalizer: {0}", method);	
				m_threadRoots.Add(method);
			}
			else if (method.ReturnType.ReturnType.ToString() == "System.Void")
			{
				if (method.Parameters.Count == 1 && method.Parameters[0].ParameterType.FullName == "System.IAsyncResult")
				{
					Log.DebugLine(this, "found asynchronous thread function: {0}", method);	
					m_threadRoots.Add(method);
				}
			}
		}

        // ldftn    System.Void Smokey.Tests.StaticSetterTest/GoodCase::DoThread(System.Object)
        // newobj   System.Void System.Threading.ParameterizedThreadStart::.ctor(System.Object,System.IntPtr)
		//
        // ldftn    System.Void Smokey.Tests.StaticSetterTest/BadCase3::DoThread()
        // newobj   System.Void System.Threading.ThreadStart::.ctor(System.Object,System.IntPtr)
        //
        // ldftn    System.Void Smokey.Tests.StaticSetterTest/BadCase5::DoCallback(System.Object)
        // newobj   System.Void System.Threading.WaitCallback::.ctor(System.Object,System.IntPtr)
        //
        // ldftn    System.Void Smokey.Tests.StaticSetterTest/BadCase7::DoCallback(System.Object)
        // newobj   System.Void System.Threading.TimerCallback::.ctor(System.Object,System.IntPtr)
		public void VisitNewObj(NewObj newobj)
		{		
			if (newobj.Ctor.ToString().Contains("System.Threading"))	// optimize common case where it's not a threading call
			{
				if (newobj.Ctor.ToString().Contains("System.Threading.ParameterizedThreadStart::.ctor") ||
					newobj.Ctor.ToString().Contains("System.Threading.ThreadStart::.ctor") ||
					newobj.Ctor.ToString().Contains("System.Threading.WaitCallback::.ctor") ||
					newobj.Ctor.ToString().Contains("System.Threading.TimerCallback::.ctor"))
				{
					LoadFunctionAddress fptr = m_info.Instructions[newobj.Index - 1] as LoadFunctionAddress;
					if (fptr != null)
					{
						Log.DebugLine(this, "found thread function: {0}", fptr.Method);	
						m_threadRoots.Add(fptr.Method);
					}
				}
			}
		}
		#endregion
				
		#region Fields
		private MethodInfo m_info;
		private List<MethodReference> m_threadRoots = new List<MethodReference>();
		#endregion
	}
}
