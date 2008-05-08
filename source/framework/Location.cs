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
using System.IO;

namespace Smokey.Framework
{	
	/// <summary>Represents the location of an error. Contents will vary according to
	/// the rule type.</summary>
	public struct Location
	{		
		/// <summary>For example: /Users/jessejones/Source/Smokey/extras/evildoer/EvilPoint.cs.</summary>
		/// <remarks>May be null.</remarks>
		public string File
		{
			get {return m_file;}
			set {m_file = value;}
		}
				
		/// <summary>Start of the code, often points to opening brace.</summary>
		/// <remarks>May be -1.</remarks>
		public int Line
		{
			get {return m_line;}
			set {m_line = value;}
		}
				
		/// <summary>For example: System.Boolean EvilDoer.EvilPoint::Equals(EvilDoer.EvilPoint).</summary>
		public string Name
		{
			get {return m_name;}
			set {m_name = value;}
		}
				
		/// <summary>Rule specific optional data (eg misspelled words).</summary>
		public string Details
		{
			get {return m_details;}
			set {m_details = value;}
		}
				
		public override bool Equals(object rhsObj)
		{
			if (rhsObj == null)                        // objects may be null
				return false;
			
			if (GetType() != rhsObj.GetType()) 
				return false;
		
			Location rhs = (Location) rhsObj;                    
			return this == rhs;
		}
				
		public static bool operator==(Location lhs, Location rhs)
		{
			return lhs.Line == rhs.Line && lhs.Name == rhs.Name;
		}
		
		public static bool operator!=(Location lhs, Location rhs)
		{
			return !(lhs == rhs);
		}    

		public override int GetHashCode()
		{
			int hash;
			
			unchecked
			{
				hash = Line.GetHashCode() + Name.GetHashCode();
			}
			
			return hash;
		}

		private string m_file;		
		private int m_line;		
		private string m_name;		
		private string m_details;	
	}
}