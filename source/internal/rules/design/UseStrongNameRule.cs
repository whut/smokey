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
using System;
using Smokey.Framework;
using Smokey.Framework.Support;

namespace Smokey.Internal.Rules
{	
	internal class UseStrongNameRule : Rule
	{				
		public UseStrongNameRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "D1004")
		{
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitAssembly");
		}
				
		public void VisitAssembly(AssemblyDefinition assembly)
		{				
			if (assembly.Name.PublicKey == null || assembly.Name.PublicKey.Length == 0)
			{
				Log.DebugLine(this, "{0} has null or zero length public key", assembly.Name.Name);				
				Reporter.AssemblyFailed(Cache.Assembly, CheckID, string.Empty);
			}
			else
			{	
				foreach (byte b in assembly.Name.PublicKey)
				{
					if (b != 0)
						return;
				}

				Log.DebugLine(this, "{0} has public key of all zeros", assembly.Name.Name);				
				Reporter.AssemblyFailed(Cache.Assembly, CheckID, string.Empty);
			}
		}
	}
}

