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
	internal sealed class NewLineLiteralRule : Rule
	{				
		public NewLineLiteralRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "PO1000")
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

			m_offset = -1;
		}
		
		public void VisitLoad(LoadString load)
		{
			if (m_offset < 0)
			{
				// We don't want to force people to exclude rules for code that
				// isn't actually broken: there should be some sort of work around
				// that they can use. Unfortunately there really isn't a way to do
				// this with verbatim strings so if a string has more than three
				// new lines we'll assume it's a verbatim string and skip it.
				int count = DoCountNewLines(load.Value);
				if (count > 0 && count <= m_maxNewLines)
				{
					m_offset = load.Untyped.Offset;
					Log.DebugLine(this, "found new line at {0:X2}", m_offset); 
				}
			}
		}

		public void VisitEnd(EndMethod method)
		{
			if (m_offset >= 0)
				Reporter.MethodFailed(method.Info.Method, CheckID, m_offset, string.Empty);
		}
		
		private int DoCountNewLines(string text)
		{
			int count = 0;
			
			int i = 0;
			while (i < text.Length && count <= m_maxNewLines)
			{
				if (i + 1 < text.Length && text[i] == '\r' && text[i + 1] == '\n')
				{
					++i;
					++count;
				}
				else if (text[i] == '\r')
				{
					++count;
				}
				else if (text[i] == '\n')
				{
					++count;
				}
				++i;
			}
			
			return count;
		}
				
		private int m_offset;		
		private int m_maxNewLines = 3;
	}
}

