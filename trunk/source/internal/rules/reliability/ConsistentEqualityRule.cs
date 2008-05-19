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
using System.Text;
using Smokey.Framework;
using Smokey.Framework.Instructions;
using Smokey.Framework.Support;

namespace Smokey.Internal.Rules
{	
	internal sealed class ConsistentEqualityRule : Rule
	{				
		public ConsistentEqualityRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "R1011")
		{
		}
		
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitType");
		}
				
		// Unfortunately we can't really handle inter-procedural rules efficiently...
		public void VisitType(TypeDefinition type)
		{
			Log.DebugLine(this, "-----------------------------------"); 
			Log.DebugLine(this, "checking {0}", type);

			// Get all of the Equals, operator==, and GetHashCode methods.
			var methods = new List<MethodInfo>();
			methods.AddRange(Cache.FindMethods(type, "Equals"));	
			methods.AddRange(Cache.FindMethods(type, "op_Equality"));
			methods.AddRange(Cache.FindMethods(type, "op_Inequality"));
			methods.AddRange(Cache.FindMethods(type, "GetHashCode"));
			methods.AddRange(Cache.FindMethods(type, "CompareTo"));
			
			if (methods.Count > 1)
			{
				// If the methods are not calling any this methods then figure out
				// which fields and properties they're using.
				var references = new Dictionary<MethodInfo, List<string>>();
				foreach (MethodInfo info in methods)
				{
					Log.DebugLine(this, "{0:F}", info.Instructions);
	
					if (DoMethodCallsNoInstanceMethods(info))
						DoGetReferences(references, info);
				}
				
				// Do some logging.
				foreach (KeyValuePair<MethodInfo, List<string>> entry in references)
				{
					Log.DebugLine(this, "{0} uses {1}", entry.Key.Method, string.Join(" ", entry.Value.ToArray()));
				}
				
				// If the methods are not all referencing the same things we have a problem.
				if (DoReferencesDiffer(references))
				{
					StringBuilder builder = new StringBuilder();
					foreach (KeyValuePair<MethodInfo, List<string>> entry in references)
						builder.AppendLine(string.Format("{0} uses {1}. ", entry.Key.Method, string.Join(" ", entry.Value.ToArray())));
						
					string details = builder.ToString().Trim();
	
					Log.DebugLine(this, "references differ");
					Reporter.TypeFailed(type, CheckID, details);
				}
			}
		}
		
		private bool DoMethodCallsNoInstanceMethods(MethodInfo info)
		{
			bool foundCall = false;
				
			for (int i = 1; i < info.Instructions.Length && !foundCall; ++i)
			{
				Call call = info.Instructions[i] as Call;
				if (call != null && !call.Target.Name.StartsWith("get_"))
				{	
					MethodInfo targetInfo = Cache.FindMethod(call.Target);
					if (targetInfo != null)
					{
						if (call.IsThisCall(Cache, info, call.Index))
						{
							Log.DebugLine(this, "{0} at {1:X2} is a this call", call.Target.Name, call.Untyped.Offset);
							foundCall = true;
						}
					}
					
					// Special case operator== calling Object.Equals with this arg.
					else if (info.Method.Name == "op_Equality")
					{
						if (call.Target.ToString().Contains("System.Object::Equals"))
						{
							if (info.Instructions[i - 1].Untyped.OpCode.Code == Code.Ldarg_0 || info.Instructions[i - 2].Untyped.OpCode.Code == Code.Ldarg_0)
							{
								Log.DebugLine(this, "{0} at {1:X2} is calling Object.Equals with this", call.Target.Name, call.Untyped.Offset);
								foundCall = true;
							}
						}
					}
					
					// Generics suck: when we call a generic method or a method in a generic type we're
					// calling an instantiated version of the method which has a different token. To avoid
					// false positives we need to set foundCall to true so we'll hack up a test here.
					else
					{
						string name = call.Target.ToString();
						int j = name.IndexOf(':');
						if (name.Contains("<") && name.Contains(">") && j > 0)
						{
							name = name.Substring(0, j);
							if (name.Contains(info.Type.FullName))
							{
								foundCall = true;
								Log.DebugLine(this, "{0} at {1:X2} seems to be a generic my call", call.Target.Name, call.Untyped.Offset);
							}
						}
					}
				}
			}
			
			return !foundCall;
		}
						
		private void DoGetReferences(Dictionary<MethodInfo, List<string>> references, MethodInfo info)
		{
			for (int i = 1; i < info.Instructions.Length; ++i)
			{
				do
				{
					LoadField field = info.Instructions[i] as LoadField;
					if (field != null)
					{	
						if (info.Instructions.LoadsThisArg(i - 1))
							DoAddReference(references, info, field.Field.Name, field.Untyped.Offset);
						break;
					}

					LoadFieldAddress addr = info.Instructions[i] as LoadFieldAddress;
					if (addr != null)
					{
						if (info.Instructions.LoadsThisArg(i - 1))
							DoAddReference(references, info, addr.Field.Name, addr.Untyped.Offset);
						break;
					}
				
					Call call = info.Instructions[i] as Call;
					if (call != null && call.Target.Name.StartsWith("get_"))
					{	
						if (info.Instructions.LoadsThisArg(i - 1))
							DoAddReference(references, info, call.Target.Name, call.Untyped.Offset);
						break;
					}
				}
				while (false);
			}
		}
		
		private void DoAddReference(Dictionary<MethodInfo, List<string>> references, MethodInfo info, string name, int offset)
		{
			List<string> names;
			if (!references.TryGetValue(info, out names))
			{
				names = new List<string>();
				references.Add(info, names);
			}
			
			if (names.IndexOf(name) < 0)
			{
				Log.DebugLine(this, "found {0} at {1:X2} for {2}", name, offset, info.Method.Name);
				names.Add(name);
			}
		}
		
		private static bool DoReferencesDiffer(Dictionary<MethodInfo, List<string>> references)
		{
			List<string> previous = null;
			foreach (List<string> current in references.Values)
			{
				if (previous != null && DoReferencesDiffer(previous, current))
					return true;
					
				previous = current;					
			}
			
			return false;
		}
		
		private static bool DoReferencesDiffer(List<string> lhs, List<string> rhs)
		{			
			if (lhs.Count > 0 || rhs.Count > 0)
			{
				if (lhs.Count == rhs.Count)
				{
					foreach (string name in lhs)
						if (rhs.IndexOf(name) < 0)
							return true;
				}
				else
				{
					return true;
				}
			}
			
			return false;
		}
	}
}

