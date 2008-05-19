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
using Smokey.Framework;

namespace Smokey.Internal
{
	// Debug.Assert is a bit obnoxious on mono: all that it does by default
	// is call the Debug.Listeners which default to writing to /dev/null on
	// non-Windows platforms. It's possible to change this location, but the
	// sensible /dev/stderr doesn't work because the stream isn't seekable.
	// Because we want asserts to be visible we'll simply arrange for all calls
	// to a Debug.Listener to raise an exception.
	// See http://lists.ximian.com/pipermail/mono-list/2004-January/017482.html
	// for more details.
	internal sealed class AssertTraceListener : TraceListener
	{		
		// Note that this may be called multiple times.
		[Conditional("DEBUG")]
		public static void Install()
		{
			if (!ms_installed)
			{
				Ignore.Value = Debug.Listeners.Add(new AssertTraceListener());
				ms_installed = true;
			}
		}
		
		// Throws AssertException.
		public override void Write(string mesg)
		{
//			throw new AssertException(mesg);		// doesn't seem to be called for asserts
		}
		
		// Throws AssertException.
		public override void WriteLine(string mesg)
		{
			if (mesg.Contains("DEBUG ASSERTION FAILED") || ms_inAssert)
			{
				ms_inAssert = true;
				m_mesg += mesg + Environment.NewLine;
				
				if (mesg.Length == 0)					// assert messages are followed by two empty lines
				{
					string temp = m_mesg;
					m_mesg = String.Empty;
					ms_inAssert = false;
					throw new AssertException(temp);
				}
			}
		}
				
		private static bool ms_installed;
		private static bool ms_inAssert;
		private string m_mesg = String.Empty;
	}
}
