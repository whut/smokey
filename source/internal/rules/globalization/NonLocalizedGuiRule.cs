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
using Smokey.Framework.Instructions;
using Smokey.Framework.Support;
using Smokey.Framework.Support.Advanced;

using System;
using System.IO;
using System.Collections.Generic;

namespace Smokey.Internal.Rules
{	
	internal sealed class NonLocalizedGuiRule : Rule
	{				
		public NonLocalizedGuiRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "G1002")
		{
			m_enabled = Settings.Get("*localized*", "true") == "true";
			Log.TraceLine(this, "enabled: {0}", m_enabled);
			
			string custom = Settings.Get("localize", string.Empty);
			foreach (string name in custom.Split(';'))
			{
				Log.TraceLine(this, "using custom: {0}", name);
				m_custom.Add(" " + name + "(");		// add some goo so we match only what we should
			}
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			if (m_enabled)
			{
				dispatcher.Register(this, "VisitBegin");
				dispatcher.Register(this, "VisitCall");
				dispatcher.Register(this, "VisitNew");
				dispatcher.Register(this, "VisitEnd");
			}
		}
		
		public void VisitBegin(BeginMethod begin)
		{
			Log.DebugLine(this, "-----------------------------------"); 
			Log.DebugLine(this, "{0:F}", begin.Info.Instructions);				

			m_offset = -1;
			m_bad = null;
			m_info = begin.Info;
		}
				
		public void VisitCall(Call call)
		{
			if (m_offset < 0)
			{				
				if (DoMatchProperty(call.Target))
				{
					DoCheck(call, call.Target, 0);
				}
				else if (DoMatchMethod(call.Target))
				{
					MethodDefinition method = DoFindMethod(call.Target);
					if (method != null)
					{
						Log.DebugLine(this, "found method call at {0:X2}", call.Untyped.Offset);				
						for (int i = 0; i < method.Parameters.Count && m_offset < 0; ++i)
						{
							ParameterDefinition p = method.Parameters[i];
							Log.DebugLine(this, "checking parameter{0}: {1}", i, p.Name);				
							if (DoIsValidArg(p))
							{
								Log.DebugLine(this, "found parameter at {0}", i);				
								int nth = method.Parameters.Count - i - 1;		
								DoCheck(call, call.Target, nth);
							}
						}
					}
				}
			}
		}
		
		public void VisitNew(NewObj newer)
		{
			if (m_offset < 0)
			{				
				if (DoMatchCtor(newer.Ctor))
				{
					MethodDefinition method = DoFindCtor(newer.Ctor);
					if (method != null)
					{
						Log.DebugLine(this, "found ctor at {0:X2}", newer.Untyped.Offset);				
						for (int i = 0; i < method.Parameters.Count && m_offset < 0; ++i)
						{
							ParameterDefinition p = method.Parameters[i];
							Log.DebugLine(this, "checking parameter{0}: {1}", i, p.Name);				
							if (DoIsValidArg(p))
							{
								Log.DebugLine(this, "found parameter at {0}", i);				
								int nth = method.Parameters.Count - i - 1;		
								DoCheck(newer, newer.Ctor, nth);
							}
						}
					}
				}
			}
		}
		
		private void DoCheck(TypedInstruction call, MethodReference target, int nth)
		{
			int index = m_info.Tracker.GetStackIndex(call.Index, nth);
			if (index >= 0)
			{
				LoadString load = m_info.Instructions[index] as LoadString;
				if (load != null)
				{
					m_offset = call.Untyped.Offset;
					m_bad = target.ToString();
					Log.DebugLine(this, "bad call at {0:X2}", m_offset);				
				}
			}
		}

		public void VisitEnd(EndMethod end)
		{
			if (m_offset >= 0)
			{
				string details = "Method: " + m_bad;
				Reporter.MethodFailed(end.Info.Method, CheckID, m_offset, details);
			}
		}
		
		private bool DoMatchProperty(MethodReference method)
		{
			if (method.Name.StartsWith("set_"))
			{
				if (method.Parameters.Count == 1)
				{
					if (method.Parameters[0].ParameterType.FullName == "System.String")
					{
						string name = method.ToString();
			
						foreach (string pattern in m_properties)
						{
							if (name.Match(pattern))
								return true;
						}
					}
				}
			}
			
			return false;
		}
		
		private bool DoMatchMethod(MethodReference method)
		{
			string name = method.ToString();

			if (!method.Name.StartsWith("set_"))
			{	
				foreach (string pattern in m_methods)
				{
					if (name.Match(pattern))
						return true;
				}
			}
			
			foreach (string custom in m_custom)
			{
				if (name.Contains(custom))
					return true;
			}
			
			return false;
		}
		
		private bool DoMatchCtor(MethodReference method)
		{
			string name = method.ToString();

			if (name.StartsWith("System.Void System.Windows.Forms.") || name.StartsWith("System.Void Gtk."))
			{
				if (name.Contains("::.ctor("))
					return true;
			}
			
			foreach (string custom in m_custom)
			{
				if (name.Contains(custom))
					return true;
			}
						
			return false;
		}
		
		private bool DoIsValidArg(ParameterDefinition p)
		{
			if (p.ParameterType.FullName == "System.String")
			{
				string name = p.Name.Replace("_", string.Empty).ToLower();
				return Array.IndexOf(m_args, name) >= 0;
			}
			
			return false;
		}
						
		private MethodDefinition DoFindMethod(MethodReference mr)
		{
			MethodDefinition md;
			if (!m_methodTable.TryGetValue(mr, out md))
			{
				TypeDefinition type = Cache.FindType(mr.DeclaringType);
				if (type != null)
				{
					md = type.Methods.GetMethod(mr.Name, mr.Parameters);
					if (md != null)
						m_methodTable.Add(mr, md);
				}
			}
			
			return md;
		}
				
		private MethodDefinition DoFindCtor(MethodReference mr)
		{
			MethodDefinition md;
			if (!m_ctorTable.TryGetValue(mr, out md))
			{
				TypeDefinition type = Cache.FindType(mr.DeclaringType);
				if (type != null)
				{
					md = type.Constructors.GetConstructor(false, mr.Parameters);
					if (md != null)
						m_ctorTable.Add(mr, md);
				}
			}
			
			return md;
		}
		
		private bool m_enabled;
		private int m_offset;
		private string m_bad;
		private MethodInfo m_info;
		private Dictionary<MethodReference, MethodDefinition> m_methodTable = new Dictionary<MethodReference, MethodDefinition>();
		private Dictionary<MethodReference, MethodDefinition> m_ctorTable = new Dictionary<MethodReference, MethodDefinition>();
		private List<string> m_custom = new List<string>();
		private string[] m_properties = new string[]		
		{
			"System.Void System.Windows.Forms.*::set_BalloonTipText(System.String)",
			"System.Void System.Windows.Forms.*::set_BalloonTipTitle(System.String)",
			"System.Void System.Windows.Forms.*::set_Caption(System.String)",
			"System.Void System.Windows.Forms.*::set_Description(System.String)",
			"System.Void System.Windows.Forms.*::set_ErrorText(System.String)",
			"System.Void System.Windows.Forms.*::set_HeaderText(System.String)",
			"System.Void System.Windows.Forms.*::set_ShortcutKeyDisplayString(System.String)",
			"System.Void System.Windows.Forms.*::set_Text(System.String)",
			"System.Void System.Windows.Forms.*::set_Title(System.String)",
			"System.Void System.Windows.Forms.*::set_ToolTipText(System.String)",
			"System.Void Gtk.*::set_Comments(System.String)",
			"System.Void Gtk.*::set_Copyright(System.String)",
			"System.Void Gtk.*::set_CustomTabLabel(System.String)",
			"System.Void Gtk.*::set_Label(System.String)",
			"System.Void Gtk.*::set_MenuLabel(System.String)",
			"System.Void Gtk.*::set_SecondaryText(System.String)",
			"System.Void Gtk.*::set_ShortLabel(System.String)",
			"System.Void Gtk.*::set_TabLabel(System.String)",
			"System.Void Gtk.*::set_TearoffTitle(System.String)",
			"System.Void Gtk.*::set_Title(System.String)",
			"System.Void Gtk.*::set_TranslatorCredits(System.String)",
			"System.Void Gtk.*::set_WebsiteLabel(System.String)",
			"System.Void Shan.*::set_Text(System.String)",
			"System.Void Shan.*::set_Title(System.String)",
		};
		private string[] m_methods = new string[]
		{
			"System.Void System.Windows.Forms.Help::ShowPopup(System.Windows.Forms.Control,System.String,System.Drawing.Point)",
			"System.Windows.Forms.DialogResult System.Windows.Forms.MessageBox::Show(*",
			"System.Void System.Windows.Forms.NotifyIcon::ShowBalloonTip(System.Int32,System.String,System.String,System.Windows.Forms.ToolTipIcon)",
			"System.Void System.Windows.Forms.TextRenderer::DrawText(*",
			"System.Void System.Windows.Forms.ToolTip::Show(*",
			"Gtk.Button Gtk.*::NewWithLabel(System.String)",
			"Gtk.CellView Gtk.*::NewWithText(System.String)",
			"System.Void Shan.Architecture.IErrorHandler::Add(*",
			"System.Void Shan.Architecture.IText::Set(System.Int32,System.Int32,System.String)",
			"System.Int32 Shan.Architecture.IText::Append(System.String)",
			"System.Void Shan.Controls.IListBox::Add(System.String)",
		};
		private string[] m_args = new string[]
		{
			"caption", "displayname", "displaytext", "header", "initialtext", 
			"label", "message", "text", "title"
		};
	}
}

