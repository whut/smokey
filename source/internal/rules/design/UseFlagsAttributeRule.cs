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
	internal sealed class UseFlagsAttributeRule : Rule
	{				
		public UseFlagsAttributeRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "D1019")
		{
			long mask = 1;
			for (int i = 0; i < 63; ++i)
			{
				m_powers.Add(mask);
				mask = mask << 1;
			}
			m_powers.Add(mask);
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitBegin");
			dispatcher.Register(this, "VisitField");
			dispatcher.Register(this, "VisitEnd");
		}
				
		public void VisitBegin(BeginType begin)
		{
			m_needsCheck = false;
			
			if (begin.Type.IsEnum)
			{
				Log.DebugLine(this, "-----------------------------------"); 
				Log.DebugLine(this, "{0}", begin.Type);				

				m_needsCheck = true;
				m_values.Clear();
				
				for (int i = 0; i < begin.Type.CustomAttributes.Count && m_needsCheck; ++i)
				{
					string name = begin.Type.CustomAttributes[i].Constructor.ToString();
					if (name.Contains("System.FlagsAttribute"))
					{
						Log.DebugLine(this, "has Flags already");				
						m_needsCheck = false;
					}
				}
			}
		}
		
		public void VisitField(FieldDefinition field)
		{			
			if (m_needsCheck && field.IsStatic)
			{
				if (field.HasConstant && field.Constant != null)
				{
					try
					{
						IConvertible convertible = (IConvertible) field.Constant;
						long value = convertible.ToInt64(null);
						if (value != 0 && !m_values.Contains(value))
						{
							m_values.Add(value);
						}
					}
					catch (Exception)
					{
						Log.TraceLine(this, "couldn't convert enum values to longs");				
						m_needsCheck = false;
					}
				}
			}
		}

		public void VisitEnd(EndType end)
		{
			if (m_needsCheck && m_values.Count > 2)
			{
				m_values.Sort();
				
				int numSequential = DoGetNumSequential();	
				Log.DebugLine(this, "numSequential: {0}", numSequential);				

				if (numSequential < m_values.Count && numSequential < 3)
				{
					List<long> masks = new List<long>();
					foreach (long value in m_values)
					{
						if (m_powers.IndexOf(value) < 0)
						{
							masks.Add(value);
						}
					}
					
					for (int i = 0; i < masks.Count; ++i)
					{
						for (int j = i+1; j < masks.Count; ++j)
						{
							if ((masks[i] & masks[j]) != 0)	// masks aren't disjoint so probably not a bitfield
							{
								Log.DebugLine(this, "{0:X} and {1:X} aren't disjoint", masks[i], masks[j]);				
								return;
							}
						}
					}
					
					Reporter.TypeFailed(end.Type, CheckID, string.Empty);
				}
			}
		}
		
		private int DoGetNumSequential()
		{
			int count = 0;
			
			int index = 0;
			while (index < m_values.Count)
			{
				int candidate = DoGetNumSequential(index);
				count = Math.Max(count, candidate);
				
				index += candidate;
			}
			
			return count;
		}
		
		private int DoGetNumSequential(int index)
		{
			int count = 1;
			
			while (index+1 < m_values.Count && m_values[index+1] == m_values[index]+1)
			{
				++index;
				++count;
			}
			
			return count;
		}
		
		private bool m_needsCheck;
		private List<long> m_values = new List<long>();
		private List<long> m_powers = new List<long>();
	}
}

