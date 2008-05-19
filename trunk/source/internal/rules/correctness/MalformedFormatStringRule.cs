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
using Smokey.Framework;
using Smokey.Framework.Instructions;
using Smokey.Framework.Support;

namespace Smokey.Internal.Rules
{		
	internal sealed class MalformedFormatStringRule : Rule
	{				
		public MalformedFormatStringRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "C1002")
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
			m_info = begin.Info;
		}
		
		public void VisitCall(Call call)
		{
			if (m_offset < 0)
			{
				// string.Format("x = {0}, y = {1}", x, y)
				int numArgs = call.Target.Parameters.Count;
				if (numArgs > 1)							// ignore stuff like string.Format(Localize("x = {0}"), x);
				{
					for (int arg = 0; arg < numArgs; ++arg)
					{
						LoadString load = DoGetStringArg(call, arg);
						
						if (load != null)
						{
							int numFormatArgs = DoCountFormatArgs(load.Value);
							if (numFormatArgs > 0)
							{
								if (arg + 1 < numArgs && call.Target.Parameters[arg+1].ParameterType.FullName == "System.Object[]")
								{
									int arrayLen = DoGetArrayLen(load.Index + 1, call.Index - 1);
									
									if (numFormatArgs != arrayLen)
									{
										m_offset = call.Untyped.Offset;
										Log.DebugLine(this, "format has {0} arguments, but there are {1} params arguments at {2:X2}", numFormatArgs.ToString(), arrayLen, m_offset.ToString()); 
									}
								}
								else if (numFormatArgs != numArgs - arg - 1)
								{
									m_offset = call.Untyped.Offset;
									Log.DebugLine(this, "format has {0} arguments, but there are {1} actual arguments at {2:X2}", numFormatArgs.ToString(), numArgs - arg - 1, m_offset.ToString()); 
								}
							}
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
			
		private int DoGetArrayLen(int firstIndex, int lastIndex)
		{
			// Find the newarr instruction.
			while (firstIndex <= lastIndex && m_info.Instructions[firstIndex].Untyped.OpCode.Code != Code.Newarr)
				++firstIndex;
				
			// Loop through all of the instructions from the newarr to the method call and
			// count the number of stelem instructions.
			int count = 0;
			for (int index = firstIndex; index <= lastIndex; ++index)
			{
				switch (m_info.Instructions[index].Untyped.OpCode.Code)
				{
					case Code.Stelem_I:
					case Code.Stelem_I1:
					case Code.Stelem_I2:
					case Code.Stelem_I4:
					case Code.Stelem_I8:
					case Code.Stelem_R4:
					case Code.Stelem_R8:
					case Code.Stelem_Ref:
					case Code.Stelem_Any:
						++count;
						break;
				}
			}
			
			return count;
		}
		
		private LoadString DoGetStringArg(Call call, int arg)
		{
			LoadString result = null;
			
			int numArgs = call.Target.Parameters.Count;
			int entry = numArgs - arg - 1;
			int index = m_info.Tracker.GetStackIndex(call.Index, entry);
			
			if (index >= 0)
				result = m_info.Instructions[index] as LoadString;
			
			return result;
		}
		
		private static int DoCountFormatArgs(string format)
		{
			int maxArg = -1;
			
			int i = 0;
			while (i < format.Length)
			{	
				// {
				if (format[i] == '{')
				{
					++i;
					
					// \d+
					if (i < format.Length && char.IsDigit(format[i]))
					{
						int arg = 0;
						while (i < format.Length && char.IsDigit(format[i]))
						{
							arg = 10*arg + (int) char.GetNumericValue(format[i]);
							++i;
						}
						
						// (,\d+)? (:\w+)?
						while (i < format.Length && format[i] != '}')
							++i;

						// }
						if (i < format.Length && format[i] == '}')
						{
							++i;
							if (arg > maxArg)
								maxArg = arg;
						}
					}
				}
				else
					++i;
			}
			
			return maxArg + 1;
		}
				
		private int m_offset;
		private MethodInfo m_info;
	}
}

