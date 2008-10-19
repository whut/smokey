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
using System.IO;
using System.Reflection;
using System.Xml;
using Smokey.Framework.Support;
using Smokey.Internal.Rules;

#if OLD
namespace Smokey.Tests
{
	[TestFixture]
	public class CtorCallsVirtualTest : MethodTest
	{	
		#region Test classes
		private abstract class Animal
		{
			public Animal()
			{
			}
			
			public abstract string GetSpecies();

			public virtual string GetHabitat()
			{
				return "jungle";
			}
		}
		
		private class GoodDog1 : Animal
		{
			public GoodDog1()
			{
				Console.WriteLine("species: {0}", GetSpecies());
			}
			
			public sealed override string GetSpecies()
			{
				return "latin words go here";
			}
		}
		
		private sealed class GoodDog2 : Animal
		{
			public GoodDog2()
			{
				Console.WriteLine("species: {0}", GetSpecies());
			}
			
			public override string GetSpecies()
			{
				return "latin words go here";
			}
		}
		
		private class GoodDog3 : Animal
		{
			public GoodDog3()
			{
				Console.WriteLine("species: {0}", GetBreed());
			}
			
			public override string GetSpecies()
			{
				return "latin words go here";
			}
			
			public string GetBreed()
			{
				return "mongrel";
			}
		}

		private class GoodDog4 : Animal
		{
			public GoodDog4()
			{
				GoodDog3 rhs = new GoodDog3();
				Console.WriteLine("species: {0}", rhs.GetSpecies());
			}
			
			public override string GetSpecies()
			{
				return "latin words go here";
			}
		}
		
		public interface IConfigXmlNode
		{
			string Filename {get;}
			int LineNumber {get;}
		}

		public class BadConfigurationException : SystemException
		{
			string bareMessage;
			string filename;
			int line;
			string source;
			
			public BadConfigurationException (string message, XmlNode node)
				: base (message)
			{
				filename = GetXmlNodeFilename(node);
				line = GetXmlNodeLineNumber(node);
				bareMessage = OriginalString;
			}
			
			public virtual string Filename
			{
				get {return filename;}
			}

			public static string GetXmlNodeFilename (XmlNode node)
			{
				if (!(node is IConfigXmlNode))
					return String.Empty;

				return ((IConfigXmlNode) node).Filename;
			}
	
			public static int GetXmlNodeLineNumber (XmlNode node)
			{
				if (!(node is IConfigXmlNode))
					return 0;

				return ((IConfigXmlNode) node).LineNumber;
			}

			public string OriginalString 
			{
				get {return source ?? ToString();}
			}

			public override string ToString()
			{
				return bareMessage + Filename + line;
			}
		}

		private class BadDog1 : Animal
		{
			public BadDog1()
			{
				Console.WriteLine("species: {0}", GetSpecies());
			}
			
			public override string GetSpecies()
			{
				return "latin words go here";
			}
		}

		private class BadDog2 : Animal
		{
			public BadDog2()
			{
				Console.WriteLine("species: {0}", DoGetSpecies());
			}
			
			public override string GetSpecies()
			{
				return "latin words go here";
			}
			
			private string DoGetSpecies()
			{
				return GetSpecies();
			}
		}

		private class BadDog3 : Animal
		{
			public BadDog3()
			{
				Console.WriteLine("species: {0}", DoGetHabitat());
			}
						
			private string DoGetHabitat()
			{
				return GetHabitat();
			}
		
			public sealed override string GetSpecies()
			{
				return "latin words go here";
			}
		}

		private class BadDog4 : Animal
		{
			public BadDog4()
			{
				Console.WriteLine("species: {0}", DoGetFoo(10));
			}
						
			protected virtual string DoGetFoo(int count)
			{
				return "foo" + count;
			}
		
			public sealed override string GetSpecies()
			{
				return "latin words go here";
			}
		}
		#endregion
		
		// test code
		public CtorCallsVirtualTest() : base(
			new string[]{"GoodDog1.GoodDog1", "GoodDog2.GoodDog2", "GoodDog3.GoodDog3", "GoodDog4.GoodDog4"},
			new string[]{"BadDog1.BadDog1", "BadDog2.BadDog2", "BadConfigurationException.BadConfigurationException", "BadDog3.BadDog3", "BadDog4.BadDog4"},
			new string[]{"Animal.", "GoodDog1.", "GoodDog2.", "GoodDog3.", "GoodDog4.", "BadDog1.", "BadDog2.", "BadConfigurationException.", "BadDog3.", "BadDog4."})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new CtorCallsVirtualRule(cache, reporter);
		}
	} 
}
#endif
