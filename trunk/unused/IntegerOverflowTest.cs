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
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Reflection;
using Smokey.Framework.Support;
using Smokey.Internal.Rules;

namespace Smokey.Tests
{
	[TestFixture]
	public class IntegerOverflowTest : MethodTest
	{	
		#region Test classes
		private class GoodCases
		{
			public static int Decrement(int value)
			{
				System.Diagnostics.Debug.Assert(value != int.MinValue, "value must be greater than Int32.MinValue");
								
				return --value;
			}
		}
				
		private class BadCases
		{
			public static int Decrement(int value)
			{								
				return --value;
			}

			private const uint ModAdler = 65521;

			public uint Checksum(byte[] data, int len)
			{
				uint a = 1;
				uint b = 0;
				
				unchecked	
				{
					int i = 0;
					while (len > 0) 
					{
						int tlen = len > 5550 ? 5550 : len;
						len -= tlen;
						do 
						{
							a += data[i++];
							b += a;
						} 
						while (--tlen > 0);
						
						a = (a & 0xffff) + (a >> 16) * (65536 - ModAdler);
						b = (b & 0xffff) + (b >> 16) * (65536 - ModAdler);	
					}
					
					if (a >= ModAdler)
						a -= ModAdler;
					
					b = (b & 0xffff) + (b >> 16) * (65536 - ModAdler);
					
					if (b >= ModAdler)
						b -= ModAdler;
					
					a = (b << 16) | a;
				}
				
				return a;
			}        	
		}
		#endregion
		
		// test code
		public IntegerOverflowTest() : base(
			new string[]{"GoodCases.Decrement"},
			new string[]{"BadCases.Decrement", "BadCases.Checksum"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new IntegerOverflowRule(cache, reporter);
		}
	} 
}
