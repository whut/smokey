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
using System.Collections;
using System.Windows.Forms;

namespace EvilDoer
{
	[DisableRule("MS1018", "VisibleFields")]
	public class NonLocalized
	{
		public void Good1(string s)
		{
			m_control.Text = s;
		}
		
		[DisableRule("G1001", "MessageBoxOptions")]
		public object Good2(string s, string t)
		{
			return MessageBox.Show(t, s);
		}
		
		public object Good3(string s)
		{
			return new MenuItem(s);
		}
		
		// G1002/NonLocalizedGui
		public void Bad1()
		{
			m_control.Text = "hello";
		}
		
		// G1002/NonLocalizedGui
		[DisableRule("G1001", "MessageBoxOptions")]
		public object Bad2(string s)
		{
			return MessageBox.Show(s, "goodbye");
		}
		
		// G1002/NonLocalizedGui
		public object Bad3(string s)
		{
			return MessageBox.Show("goodbye", s);
		}
		
		// G1002/NonLocalizedGui
		public object Bad4()
		{
			return new MenuItem("bah");
		}
		
		public Control m_control;
	}		
}