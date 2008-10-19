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
using System.IO;
using System.Collections.Generic;
using Smokey.Framework;
using Smokey.Framework.Instructions;
using Smokey.Framework.Support;

#if OLD
namespace Smokey.Internal.Rules
{	
	internal sealed class DataLocaleRule : Rule
	{				
		public DataLocaleRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "G1000")
		{
			m_enabled = Settings.Get("*localized*", "true") == "true";
			Log.TraceLine(this, "enabled: {0}", m_enabled);			
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			if (m_enabled)
			{
				dispatcher.Register(this, "VisitBegin");
				dispatcher.Register(this, "VisitNewObj");
				dispatcher.Register(this, "VisitCall");
				dispatcher.Register(this, "VisitEnd");
			}
		}
				
		public void VisitBegin(BeginMethod begin)
		{
			Log.DebugLine(this, "-----------------------------------"); 
			Log.DebugLine(this, "{0:F}", begin.Info.Instructions);				

			m_offset = -1;
			m_foundCtor = false;
			m_foundLocale = false;
		}
		
		public void VisitNewObj(NewObj newobj)
		{
			if (!m_foundCtor)
			{
				Log.DebugLine(this, "newobj.Ctor.DeclaringType: {0}", newobj.Ctor.DeclaringType.FullName);
				
				if (newobj.Ctor.DeclaringType.IsSameOrSubclassOf("System.Data.DataSet", Cache))
					m_foundCtor = true;

				else if (newobj.Ctor.DeclaringType.IsSameOrSubclassOf("System.Data.DataTable", Cache))
					m_foundCtor = true;

				Log.DebugLine(this, "   m_foundCtor: {0}", m_foundCtor);

				if (m_foundCtor)
					m_offset = newobj.Untyped.Offset;											
			}
		}
		
		public void VisitCall(Call call)
		{
			if (!m_foundLocale)
			{
				if (call.Target.ToString().Contains("System.Data.DataSet::set_Locale"))
					m_foundLocale = true;

				else if (call.Target.ToString().Contains("System.Data.DataTable::set_Locale"))
					m_foundLocale = true;
			}
		}

		public void VisitEnd(EndMethod end)
		{
			if (m_foundCtor && !m_foundLocale)
			{
				Reporter.MethodFailed(end.Info.Method, CheckID, m_offset, string.Empty);
			}
		}
						
		private bool m_enabled;
		private int m_offset;
		private bool m_foundCtor;
		private bool m_foundLocale;
	}
}
#endif
