// Copyright (C) 2007 Jesse Jones
//
// Authors:
//	Jb Evain <jbevain@gmail.com>
//	Jesse Jones <jesjones@mindspring.com>
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

	<Violation checkID = "P1001" severity = "Nitpick">
		<Translation lang = "en" typeName = "MethodCanBeMadeStatic" category = "Performance">			
			<Cause>
			An instance method calls no instance methods and refers to no instance fields.
			</Cause>
	
			<Description>
			Instance methods require a this pointer so that they can reference fields
			associated with the object. When an instance method is called the caller
			must push the this pointer onto the stack. But if the method never makes
			use of the this pointer it's pointless to push it.
			
			Now this doesn't sound like much, but dense code is very important on modern
			processors and if saving a few bytes means your time critical code now fits into
			the LI cache it will be a big win.
			</Description>
	
			<Fix>
			Make the method static (or use -severity:Warning).
			</Fix>
	
			<CSharp>
			using System.IO;
			using System.Xml;
			using System.Xml.Schema;
			
			internal class LoadXml
			{
				public LoadXml(Stream schemaStream)
				{		
					m_settings = new XmlReaderSettings();
					m_settings.Schemas.Add(XmlSchema.Read(schemaStream, DoValidationEvent));
					m_settings.ValidationEventHandler += DoValidationEvent;
					m_settings.ValidationType = ValidationType.Schema;
				}
				
				public XmlDocument Load(Stream xmlStream)
				{
					XmlDocument xml = new XmlDocument();
					xml.Load(XmlReader.Create(xmlStream, m_settings));
					
					return xml;
				}
				
				// This method doesn't use instance methods/fields so it should be static.
				private static void DoValidationEvent(object sender, ValidationEventArgs e)
				{
					if (e.Severity == XmlSeverityType.Warning)
						Console.WriteLine(e.Message);
					else 
						throw e.Exception;
				}
				
				private XmlReaderSettings m_settings;
			} 
			</CSharp>
		</Translation>
	</Violation>

namespace Smokey.Internal.Rules
{	
	internal class MethodCanBeMadeStaticRule : Rule
	{				
		public MethodCanBeMadeStaticRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "P1001")
		{
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitBegin");
			dispatcher.Register(this, "VisitLoadArg");
			dispatcher.Register(this, "VisitEnd");
		}
				
		public void VisitBegin(BeginMethod method)
		{
			m_needsCheck = DoNeedsCheck(method.Info);

			if (m_needsCheck)
			{
				Log.DebugLine(this, "-----------------------------------"); 
				Log.DebugLine(this, "{0:F}", method.Info.Instructions);				

				m_foundThis = false;
			}
		}
		
		public void VisitLoadArg(LoadArg load)
		{
			if (m_needsCheck && !m_foundThis)
			{
				if (load.Arg == 0)
				{
					m_foundThis = true;
					Log.DebugLine(this, "found this pointer at {0:X2}", load.Untyped.Offset); 
				}
			}
		}

		public void VisitEnd(EndMethod method)
		{
			if (m_needsCheck)
			{
				if (!m_foundThis)
					Reporter.MethodFailed(method.Info.Method, CheckID, 0, string.Empty);
			}
		}

		private static bool DoNeedsCheck(MethodInfo info)
		{
			bool needs = false;
			
			if (!info.Method.IsStatic && !info.Method.IsVirtual)
			{
				if (!info.Method.ToString().Contains("CompilerGenerated") && !info.Method.ToString().Contains("_AnonymousMethod"))
				{
					if (!info.Type.Namespace.StartsWith("System") && !info.Type.Namespace.StartsWith("Mono"))
					{
						needs = true; 
					}
					
					// If the type is in the System or Mono namespaces then it can only
					// be made static if it's access is private or internal to the assembly.
					else if ((info.Method.Attributes & MethodAttributes.MemberAccessMask) == MethodAttributes.Private)
					{
						needs = true;
					}
					else if ((info.Method.Attributes & MethodAttributes.MemberAccessMask) == MethodAttributes.FamANDAssem)
					{
						needs = true;
					}
				}
			}
			
			return needs;
		}
				
		private bool m_needsCheck;
		private bool m_foundThis;
	}
}

