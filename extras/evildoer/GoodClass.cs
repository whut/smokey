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
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;

namespace EvilDoer
{
	internal interface InternalInterface
	{
		void Greet();
	}

	public class GoodInternal : InternalInterface	
	{
		void InternalInterface.Greet()			// explicit implementation
		{
			System.Diagnostics.Debug.WriteLine("hi");
		}

		public void Greet()
		{
		}
	}

	[Serializable]
	public class GoodException : Exception
	{
		public GoodException()
		{
		}
		
		public GoodException(string message) : base(message) 
		{
		}
		
		public GoodException(string message, Exception innerException) : 
			base (message, innerException)
		{
		}
		
		protected GoodException(SerializationInfo info, StreamingContext context) : 
			base(info, context)
		{
		}

		public GoodException(string mesg, string key) : base(mesg)
		{
			this.key = key;
		}
		
		[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			System.Diagnostics.Debug.Assert(info != null, "info is null");
			
			info.AddValue("key", key, typeof(string));
			base.GetObjectData(info, context);
		}

		public string Key {get {return key;}}
		
		private string key;
	}			

	public class GoodClass : IFormattable
	{				
		~GoodClass()
		{
			System.Diagnostics.Debug.WriteLine("bad");
		}
		
		public static string ArrayToStr(int[] array)
		{
			string[] strs = Array.ConvertAll<int, string>(array, delegate(int i) 
				{return i.ToString();});

			return string.Join(" ", strs);
		}

		public static void Rethrow()
		{
			try
			{
				System.Diagnostics.Debug.WriteLine("cool stuff goes here");
				System.Diagnostics.Debug.WriteLine(NameAttribute);
			}
			catch (ApplicationException e)
			{
				Console.Error.WriteLine(e.Message);
				throw;		
			}
		}
		
		public static void Run()
		{
			System.Windows.Forms.Application.Run();
		}

		public static void ThrowInner()
		{
			try
			{
				System.Diagnostics.Debug.WriteLine("more cool stuff");
			}
			catch (ApplicationException e)
			{
				Console.Error.WriteLine(e.Message);
				throw new InvalidOperationException("cool is buggy", e);	
			}
		}

		public static void WriteBlankLine()
		{
			System.Diagnostics.Debug.WriteLine(string.Empty);
		}

		public static void PortableNewLine()
		{
			System.Diagnostics.Debug.WriteLine("hello" + Environment.NewLine + "world");
		}

		public static void UnixNewLine()
		{
			System.Diagnostics.Debug.WriteLine("hello" + '\n' + "world");
		}

		public static void WindowsNewLine()
		{
			System.Diagnostics.Debug.WriteLine("hello" + '\r' + '\n' + "world");
		}

		public static void MacNewLine()
		{
			System.Diagnostics.Debug.WriteLine("hello" + '\r' + "world");
		}

		public static int Square(int x)
		{
			return x * x;
		}
		
		public static List<int> CreateList(int c)
		{
			List<int> result = null;         
			
			if (c <= 0)
				result = new List<int>();
			else 
				result = new List<int>(c);
		
			result.Add(1);
			
			return result;
		}
		
		public override int GetHashCode()
		{
			return name.GetHashCode() ^ address.GetHashCode();
		}

		public override bool Equals(object rhsObj)
		{
			if (rhsObj == null)	
				return false;
			
			GoodClass rhs = rhsObj as GoodClass;
			if ((object) rhs == null)
				return false;
			
			return name == rhs.name && address == rhs.address;
		}
			
		public bool Equals(GoodClass rhs)
		{
			if ((object) rhs == null)
				return false;
			
			return name == rhs.name && address == rhs.address;
		}

		public static bool operator==(GoodClass lhs, GoodClass rhs)
		{
			if (object.ReferenceEquals(lhs, rhs))
				return true;
			
			if ((object) lhs == null || (object) rhs == null)
				return false;
			
			return lhs.name == rhs.name && lhs.address == rhs.address;
		}
		
		public static bool operator!=(GoodClass lhs, GoodClass rhs)
		{
			return !(lhs == rhs);
		}
			
		public override string ToString()
		{
			return ToString("G", null);
		}
		
		public static string OuOfOrder(int x, int y)
		{
			return string.Format("x = {1,3:X2}, y = {0}", x, y);
		}		
		
		public static void ValidTZFile()
		{
			System.Diagnostics.Debug.WriteLine("stuff all'");
		}

		public static void yyYucky()
		{
			System.Diagnostics.Debug.WriteLine("stuff all'");
		}

		public string ToString(string format, IFormatProvider provider)
		{
			if (format == null)
				throw new ArgumentNullException("format");
				
			if (provider != null)
			{
				ICustomFormatter formatter = provider.GetFormat(GetType()) as ICustomFormatter;
				if (formatter != null)
					return formatter.Format(format, this, provider);
			}
			
			StringBuilder builder = new StringBuilder();
			switch (format)
			{	
				case "S":
					Ignore.Value = builder.Append("name = ");
					Ignore.Value = builder.Append(name);
					break;
											
				case "":			// no good way for clients to fix this so we allow one empty string in ToString methods			
				case "G":
				case null:
					Ignore.Value = builder.Append("name = ");
					Ignore.Value = builder.Append(name);
					Ignore.Value = builder.Append(", address = ");
					Ignore.Value = builder.Append(address);
					break;

				default:
					throw new ArgumentException(format + " isn't a valid GoodClass format string");
			}
			
			return builder.ToString();
		}

		private string name = "hello", address = "goodbye";
		const string NameAttribute     = "name";
	}
}