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

namespace Smokey.Internal.Rules
{	
	internal sealed class ArgumentException2Rule : Rule
	{				
		public ArgumentException2Rule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "C1032")
		{
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitBegin");
			dispatcher.Register(this, "VisitNew");
			dispatcher.Register(this, "VisitEnd");
		}
		
		public void VisitBegin(BeginMethod begin)
		{
			Log.DebugLine(this, "-----------------------------------"); 
			Log.DebugLine(this, "{0:F}", begin.Info.Instructions);				

			m_offset = -1;
			m_info = begin.Info;

			m_args.Clear();
			bool badNames = false;
			for (int i = 0; i < begin.Info.Method.Parameters.Count; ++i)
			{
				ParameterReference p = begin.Info.Method.Parameters[i];
				if (!p.Name.StartsWith("A_"))
				{
					Log.DebugLine(this, "adding {0}", p.Name);				
					m_args.Add(p.Name);
				}
				else
				{
					Log.DebugLine(this, "skipping {0}", p.Name);	
					badNames = true;
				}
			}
			
			if (begin.Info.Method.Name.StartsWith("set_"))
			{
				m_args.Add(begin.Info.Method.Name.Substring(4));
				m_args.Add("value");
				Log.DebugLine(this, "added value and {0}", begin.Info.Method.Name.Substring(4));				
			}
			
			if (badNames)
				m_args.Clear();
		}
		
		public void VisitNew(NewObj obj)
		{
			if (m_offset < 0 && m_args.Count > 0)
			{
				if (obj.Ctor.ToString() == "System.Void System.ArgumentNullException::.ctor(System.String)" || obj.Ctor.ToString() == "System.Void System.ArgumentOutOfRangeException::.ctor(System.String)")
				{
					LoadString load = m_info.Instructions[obj.Index - 1] as LoadString;
					if (load != null)
					{
						if (m_args.IndexOf(load.Value) < 0)
						{
							m_offset = obj.Untyped.Offset;						
							Log.DebugLine(this, "bad new at {0:X2}", m_offset);				
						}
					}
				}
			}
		}

		public void VisitEnd(EndMethod end)
		{
			if (m_offset >= 0)
			{
				Reporter.MethodFailed(end.Info.Method, CheckID, m_offset, string.Empty);
			}
		}

		private int m_offset;
		private MethodInfo m_info;
		private List<string> m_args = new List<string>();
	}
}
