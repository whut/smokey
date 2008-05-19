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
using Smokey.Framework;
using Smokey.Framework.Support;
using System;
using System.Collections.Generic;
using System.Text;

namespace Smokey.Internal.Rules
{	
	internal sealed class PreferredTermRule : Rule
	{				
		public PreferredTermRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "MS1027")
		{
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitType");
			dispatcher.Register(this, "VisitMethod");
		}
				
		public void VisitType(TypeDefinition type)
		{				
			TypeAttributes attrs = type.Attributes & TypeAttributes.VisibilityMask;
			if ((attrs & TypeAttributes.Public) == TypeAttributes.Public ||
				(attrs & TypeAttributes.Public) == TypeAttributes.NestedPublic)
			{
				Log.DebugLine(this, "-----------------------------------"); 
				Log.DebugLine(this, "checking {0}", type.Name);				

				if (DoObsoleteTerm(type.Name))
				{
					Reporter.TypeFailed(type, CheckID, string.Empty);
				}
			}
		}
				
		public void VisitMethod(MethodDefinition method)
		{				
			if (method.PubliclyVisible(Cache))
			{
				Log.DebugLine(this, "-----------------------------------"); 
				Log.DebugLine(this, "checking {0}", method.Name);				

				if (DoObsoleteTerm(method.Name))
				{
					Reporter.MethodFailed(method, CheckID, 0, string.Empty);
				}
				else
				{
					foreach (ParameterDefinition arg in method.Parameters)
					{
						Log.DebugLine(this, "checking {0}", arg.Name);				
						if (DoObsoleteTerm(arg.Name))
						{
							Reporter.MethodFailed(method, CheckID, 0, string.Empty);
							break;
						}
					}
				}
			}
		}
						
		private bool DoObsoleteTerm(string inName)
		{
			string[] names = DoTokenize(inName);
			for (int i = 0; i < names.Length; ++i)
			{
				string name = names[i].ToLower();
				Log.DebugLine(this, "testing {0}", name);				
				
				if (Array.IndexOf(m_obsolete, name) >= 0)
					return true;
					
				if (i + 1 < names.Length)
				{
					name = string.Format("{0}{1}", name, names[i + 1].ToLower());
					Log.DebugLine(this, "   and {0}", name);				

					if (Array.IndexOf(m_obsolete, name) >= 0)
						return true;
				}
			}
			
			return false;
		}
		
		private enum State {NonLetter, Upper, Lower}
		
		private static string[] DoTokenize(string name)
		{
			List<string> tokens = new List<string>();
			
			int i = 0;
			StringBuilder token = new StringBuilder();
			State state = State.NonLetter;
			while (i < name.Length)
			{
				char ch = name[i];
				
				if (state == State.NonLetter)
				{	
					if (char.IsUpper(ch))
						state = State.Upper;
					else if (char.IsLower(ch))
						state = State.Lower;
					else
						++i;
				}
				else if (state == State.Upper)
				{
					if (char.IsUpper(ch))
					{
						token.Append(ch);
						++i;
					}
					else
					{
						if (char.IsLower(ch))
						{
							state = State.Lower;
						}
						else
						{
							tokens.Add(token.ToString());
							token.Length = 0;
		
							state = State.NonLetter;
						}
					}
				}
				else if (state == State.Lower)
				{
					if (char.IsLower(ch))
					{
						token.Append(ch);
						++i;
					}
					else
					{
						tokens.Add(token.ToString());
						token.Length = 0;

						if (char.IsUpper(ch))
						{
							state = State.Upper;
						}
						else
						{
							state = State.NonLetter;
						}
					}
				}
			}
			
			if (token.Length > 0)
				tokens.Add(token.ToString());
			
			return tokens.ToArray();
		}
		
		private string[] m_obsolete = new string[]{
			"complus", "cancelled", "indices", "login", "logout", "signon", 
			"signoff", "writeable"};
	}
}

