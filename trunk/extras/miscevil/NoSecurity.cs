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

using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Forms;

[assembly: CLSCompliant(true)]	

// D1028/DeclareSecurity
// MS1000/AssemblyInfo
namespace Unsecure
{
	[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | 
		AttributeTargets.Constructor | AttributeTargets.Enum | AttributeTargets.Interface | 
		AttributeTargets.Method | AttributeTargets.Struct, AllowMultiple = true)]
	internal sealed class DisableRuleAttribute : Attribute
	{		
		public DisableRuleAttribute(string id, string name) 
		{
			this.id = id;
			this.name = name;
		}
		
		public string ID
		{
			get {return id;}
		}
	
		public string Name
		{
			get {return name;}
		}
	
		private string id;
		private string name;
	}

	internal static class Ignore
	{			
		public static object Value
		{
			[DisableRule("C1022", "UseSetterValue")]	
			set {}
		}
	}

	internal static class Program
	{
		[Conditional("DEBUG")]
		private static void DoDumpState1()
		{  
			Console.WriteLine("California");
		}
		
		// D1048/GuiUsesConsole
		private static void DoDumpState2()
		{ 
			Console.WriteLine("Montana");
		}
			
		// R1015/STAThread
		// PO1007/ExitCode 
		[DisableRule("G1002", "NonLocalizedGui")]
		public static int Main(string[] args)
		{ 
			int err = 0; 	
			
			try
			{
				DoDumpState1();
				DoDumpState2();
			
				// G1001/MessageBoxOptions
				Ignore.Value = MessageBox.Show("ho", "hum");
			}
			catch (Exception e)
			{
				Console.Error.WriteLine(e.Message);
				err = -2;
			}
			
			return err;
		}
	}
}