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

using EvilDoer;
using System;

namespace Hungarian
{
	[DisableRule("MO1000", "UseMonoNaming")]
	public class Good1
	{
		public void Alpha(int arg1, int arg2)
		{
			m_foo5 = arg1;
			m_bar6 = arg2;
		}
		
		public int Beta(int arg3, int arg4)
		{
			return m_foo5 + m_bar6 + arg3 + arg4;
		}
		
		private int m_foo5;
		private int m_bar6;
	}

	public static class Good2
	{
		public static int Alpha(int a, int b)
		{
			int nitems1 = a + b;
			int stritems2 = a * b;
			int wItems3 = a / b;
			int uItems4 = a % b;
			int pchItems5 = a + a;
			
			return nitems1 + stritems2 + wItems3 + uItems4 + pchItems5;
		}
	}

	[DisableRule("MO1000", "UseMonoNaming")]
	public class Bad1
	{
		public void Alpha(int bArg1, int cArg2)
		{
			m_foo5 = bArg1;
			m_szBar6 = cArg2;
		}
		
		public int Beta(int dwArg3, int lpszF)
		{
			return m_foo5 + m_szBar6 + dwArg3 + lpszF;
		}
		
		private int m_foo5;
		private int m_szBar6;
	}

	[DisableRule("MO1000", "UseMonoNaming")]
	public class Bad2
	{
		public void Alpha(int bArg1, int cArg2)
		{
			_foo5 = bArg1;
			_szBar6 = cArg2;
		}
		
		public int Beta(int dwArg3, int lpszF)
		{
			return _foo5 + _szBar6 + dwArg3 + lpszF;
		}
		
		private int _foo5;
		private int _szBar6;
	}

	[DisableRule("MO1000", "UseMonoNaming")]
	public class Bad3
	{
		public void Alpha(int bArg1, int cArg2)
		{
			m_foo5 = bArg1;
			ms_szBar6 = cArg2;
		}
		
		public int Beta(int dwArg3, int lpszF)
		{
			return m_foo5 + ms_szBar6 + dwArg3 + lpszF;
		}
		
		private int m_foo5;
		private int ms_szBar6;
	}

	public static class Bad4
	{
		public static int Alpha(int a, int b)
		{
			int cItems1 = a + b;
			int strItems2 = a * b;
			int wItems3 = a / b;
			int uItems4 = a % b;
			int pchItems5 = a + a;
			
			return cItems1 + strItems2 + wItems3 + uItems4 + pchItems5;
		}
	}
}
