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
using Smokey.Framework;

#if OLD
namespace Smokey.Internal.Rules
{	
	internal static class NamingUtils
	{			
		private enum Expect {Upper, Lower, Any};
		
		// PascalCase or IO or GetID
		public static bool IsPascalCase(string name)
		{
			return DoIsValidName(name, Expect.Upper);
		}
		
		// camelCase or theID
		public static bool IsCamelCase(string name)
		{
			return DoIsValidName(name, Expect.Lower);
		}
		
		// lines_per_page
		public static bool IsUnderScoreName(string name)
		{			
			if (name.Length > 0 && name[0] == '$')
				return true;
			
			// leading and trailing underscores are never ok
			if (name.Length >= 0 && name[0] == '_')
				return false;
			if (name.Length > 1 && name[name.Length - 1] == '_')
				return false;
				
			if (DoIsHungarian(name))
				return false;
			
			for (int i = 0; i < name.Length; ++i)
			{
				if (char.IsUpper(name[i]))
					return false;
			}
								
			return true;
		}
		
//		public static void UpdateBadNames(string name)
//		{
//			string oldNames = Settings.Get("badNames", string.Empty);
//			Settings.Add("badNames", oldNames + name + " ");
//		}		
		
		#region Private methods
		private static bool DoIsValidName(string name, Expect start)
		{
			Expect expect = start;
			int upperCount = 0;
			
			if (name.StartsWith(".ctor") || name.StartsWith("op_"))
				return true;
			if (name == "<Module>")
				return true;
			if (name == "value__")		// used by enums
				return true;
			if (name.Length > 0 && name[0] == '$')
				return true;
			
			// leading and trailing underscores are never ok
			if (name.Length > 0 && name[0] == '_')
				return false;
			if (name.Length > 1 && name[name.Length - 1] == '_')
				return false;
				
			if (DoIsHungarian(name))
				return false;
			
			for (int i = 0; i < name.Length; ++i)
			{
				switch (expect)
				{
					case Expect.Upper:
						if (!char.IsUpper(name[i]))
							return false;
						++upperCount;
						expect = Expect.Lower;
						break;
						
					case Expect.Lower:
						if (!char.IsLower(name[i]))
						{
							if (++upperCount > 3 || i == 0)	// need to allow stuff like ValidIOMask
								return false;
						}
						else
						{
							upperCount = 0;
							expect = Expect.Any;
						}
						break;
						
					case Expect.Any:
						if (char.IsUpper(name[i]))
						{
							expect = Expect.Lower;
							upperCount = 1;
						}
						else if (name[i] == '_')
							return false;
						break;
				}
			}
					
			return true;
		}

		// Tests selected prefixes from <http://msdn.microsoft.com/archive/default.asp?url=/archive/en-us/dnarw98bk/html/variablenameshungariannotation.asp>.
		private static bool DoIsHungarian(string name)
		{
			string[] prefixes = new string[]{"b", "c", "dw", "s", "sz", "lpsz"};
			
			foreach (string prefix in prefixes)
			{
				if (name.StartsWith(prefix))
					if (name.Length > prefix.Length)
						if (char.IsUpper(name[prefix.Length]))
							return true;
			}
					
			return false;
		}
		#endregion
	}
}
#endif
