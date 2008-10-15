// Copyright (C) 2008 Jesse Jones
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
using Smokey.Framework;
using Smokey.Framework.Support;
using Smokey.Internal;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Smokey.Tests
{
	[TestFixture]
	public class AssemblyCacheTest : BaseTest
	{		
		#region Test cases
		private class NormalBase											
		{		
			public void Print(int foo)
			{
				Console.WriteLine(foo);
			}
		}

		private sealed class DerivedNormal : NormalBase	
		{		
		}
		
		private sealed class DerivedException : Exception
		{
			public DerivedException(string err) : base(err)
			{
			}
		}
		
		private abstract class DerivedCollection : KeyedCollection<int, string>
		{
		}
		
		private class GenericBase<T>											
		{		
			public void Print(T foo)
			{
				Console.WriteLine(foo);
			}
		}

		private sealed class DerivedGeneric : GenericBase<string>	
		{		
			public void PrintInt(int value)
			{
				GenericBase<int> instantiated = new GenericBase<int>();
				instantiated.Print(value);
			}
		}
		#endregion
		
		public AssemblyCacheTest()
		{
			string loc = System.Reflection.Assembly.GetExecutingAssembly().Location;
			m_assembly = AssemblyFactory.GetAssembly(loc);
			
//			Log.ErrorLine(this, "Assembly types:");
//			foreach (var t in m_assembly.MainModule.Types)
//				Log.ErrorLine(this, "   {0}", t);
		}
		
		[Test]
		public void FindTypes()
		{
			string[] types = new[]{"NormalBase", "DerivedNormal", "DerivedException", "DerivedCollection", "GenericBase`1", "DerivedGeneric"};
			string[] derived = new[]{"DerivedNormal", "DerivedException", "DerivedCollection", "DerivedGeneric"};
						
			AssemblyCache cache = new AssemblyCache(m_assembly, DoGetTypes(types));

			foreach (TypeDefinition d in DoGetTypes(derived))
			{
				TypeDefinition b = cache.FindType(d.BaseType);
				if (b == null)
					Assert.Fail("couldn't find {0}", d.BaseType.FullName);
			}
		}
		
		private IEnumerable<TypeDefinition> DoGetTypes(string[] names)
		{
			return names.Select(n => DoGetType(n));
		}
		
		private TypeDefinition DoGetType(string name)
		{
			TypeDefinition type;
			
			try
			{
				string testName = GetType().FullName;
				string fullName = string.Format("{0}/{1}", testName, name);
				type = m_assembly.MainModule.Types[fullName];
			}
			catch
			{
				Console.Error.WriteLine("assembly doesn't have '{0}'", name);
				throw;
			}
			
			return type;
		}
		
		private AssemblyDefinition m_assembly;
	} 
}
