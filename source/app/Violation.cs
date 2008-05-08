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
using System.Diagnostics;
using Smokey.Framework;

namespace Smokey.App
{
	public enum Severity {Error, Warning, Nitpick};

	/// <summary>Information about a rule violation.</summary>
	/// <remarks>This info is loaded from xml files
	// and consists of the constant data associated with an error. Note that
	/// the xml is embedded in the Smokey exe as resources.</remarks>
	public class Violation : IComparable<Violation>
	{					
		/// <summary>Code used to identify the violation, eg "C1001". Unlike the
		/// other strings this will never be localized.</summary>
		public readonly string CheckID;
		
		/// <summary>Word used to identify the violation, eg "InfiniteRecursion".</summary>
		public readonly string TypeName;	

		/// <summary>Violation class, eg "Usage".</summary>
		public readonly string Category;	
		
		/// <summary>Importance of the violation.</summary>
		public readonly Severity Severity;
				
		/// <summary>True if fixing the violation is likely to break binary compatibility
		/// with client assemblies.</summary>
		public readonly bool Breaking;
				
		/// <summary>The reason the code failed.</summary>
		public readonly string Cause;	
		
		/// <summary>Why the failure is a problem.</summary>
		public readonly string Description;	
		
		/// <summary>What to do to fix the problem.</summary>
		public readonly string Fix;	
		
		/// <summary>C# code that satisfies the rule.</summary>
		public readonly string Csharp;	
				
		[DisableRule("D1047", "TooManyArgs")]	
		internal Violation(string checkID, string typeName, string category, 
						Severity severity, bool breaking, string cause, string description,
						string fix, string csharp) 
		{
			DBC.FastAssert(checkID != null, "checkID cannot be null");

			CheckID = checkID;
			TypeName = typeName;
			Category = category;
			Severity = severity;	
			Breaking = breaking;	
			Cause = cause;
			Description = description;
			Fix = fix;
			Csharp = csharp;
		}

		public int CompareTo(Violation rhs)
		{
			return string.Compare(CheckID, rhs.CheckID);
		}	

		public override bool Equals(object rhsObj)
		{
			if (rhsObj == null)        
				return false;
			
			Violation rhs = rhsObj as Violation;
			return CheckID == rhs.CheckID;
		}
        
		public override int GetHashCode()
		{
			return CheckID.GetHashCode();
		}
	}
}