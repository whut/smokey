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
using Mono.Cecil.Metadata;
using Mono.CompilerServices.SymbolWriter;

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;

namespace Smokey.Framework
{
	using MethodEntries = Dictionary<MetadataToken, SymbolTable.MethodInfo>;
		
	// Uses the (undocumented) Mono.CompilerServices to extract file and line
	// info from mdb files, and Cecil to process the meta data in the image
	// file. See /Users/jessejones/New_Files/mono-1.2.6/mcs/class/Mono.CompilerServices.SymbolWriter/MonoSymbolTable.cs
	internal sealed class SymbolTable
	{				
#if TEST
		public SymbolTable()
		{		
		}
#endif

		[DisableRule("P1011", "UseDefaultInit")]		// we reset m_methodEntries to null
		public SymbolTable(string imagePath)
		{		
			Profile.Start("SymbolTable ctor");
			try
			{
				if (File.Exists(imagePath + ".mdb"))	// MonoSymbolFile.ReadSymbolFile uses the Assembly.Location which isn't always right
					DoLoadImage(imagePath);
				else
					Console.WriteLine("mdb file is missing so there will be no file and line numbers");
			}
			catch (Exception e)
			{
				Log.ErrorLine(this, "Couldn't load the {0} symbol file.", imagePath);
				Log.ErrorLine(this, e.Message);
				m_methodEntries = null;
			}
			Profile.Stop("SymbolTable ctor");
		}
		
		public Location Location(TypeDefinition type, string details)
		{
			Location location = new Location();
			
			location.File = "<unknown>";			
			location.Line = -1;					// TODO: not sure how to set this, could get close by using the first method...
			location.Name = "Type: " + type.FullName;
			location.Details = details;
			
			if (m_methodEntries != null)
				DoLocation(type, details, ref location);
					
			return 	location;
		}
		
		public Location Location(MethodDefinition method, int offset, string details)
		{
			Location location = new Location();
			
			location.Name = "Method: " + method.ToString();			
			location.Line = -1;
			location.Details = details;

			if (m_methodEntries != null)
				DoLocation(method, offset, details, ref location);
					
			return location;
		}
		
		public string LocalName(MethodDefinition method, int index)
		{
			string name = "V_" + index;

			if (m_methodEntries != null)
				name = DoLocalName(method, index);
								
			return name;
		}
				
		public bool HaveLocalNames(MethodDefinition method)
		{
			MethodInfo info;
			if (m_methodEntries != null && m_methodEntries.TryGetValue(method.MetadataToken, out info))
			{	
				return !info.Entry.LocalNamesAmbiguous;
			}
			
			return false;
		}
				
		#region Private Methods -----------------------------------------------
		private void DoLoadImage(string imagePath)	// note that we're careful only to use Mono.CompilerServices.SymbolWriter within helper methods so the CLR does not load it unless we need it
		{
			if (m_methodEntries == null)
				m_methodEntries = new MethodEntries();
				
			Assembly assembly = Assembly.LoadFrom(imagePath);	// note that a reflection only load doesn't seem to work
			MonoSymbolFile symbols = MonoSymbolFile.ReadSymbolFile(assembly);	// use this ctor so that we can verify that the mdb file matches the assembly
			
			for (int i = 1; i <= symbols.MethodCount; ++i)		// 1-based...
			{
				MethodEntry method = symbols.GetMethod(i);
				m_methodEntries.Add(new MetadataToken(method.Token), new MethodInfo(method));
			}
		}

		private void DoLocation(TypeDefinition type, string details, ref Location location)
		{
			List<MetadataToken> tokens = new List<MetadataToken>();
			foreach (MethodDefinition method in type.Methods)
				tokens.Add(method.MetadataToken);
			foreach (MethodDefinition method in type.Constructors)
				tokens.Add(method.MetadataToken);
			
			if (tokens.Count > 0)
			{
				List<string> files = new List<string>();
				foreach (MetadataToken token in tokens)
				{
					MethodInfo info;
					if (m_methodEntries.TryGetValue(token, out info))
						if (files.IndexOf(info.File) < 0)	// classes may be defined in multiple files...
							files.Add(info.File);
				}
				location.File = string.Join(" ", files.ToArray());	
			}
		}
		
		private void DoLocation(MethodDefinition method, int offset, string details, ref Location location)
		{
			MethodInfo info;
			if (m_methodEntries.TryGetValue(method.MetadataToken, out info))
			{
				location.File = info.File;	
				
				if (info.Lines.Length > 0)
				{					
					int index = Array.BinarySearch(info.Lines, new LineInfo(offset, 0));
					if (index >= 0)
						location.Line = info.Lines[index].Line;
					else if (~index - 1 >= 0 && ~index - 1 < info.Lines.Length)
						location.Line = info.Lines[~index - 1].Line;
					else
						location.Line = -1;
				}
				else
					location.Line = -1;
			}
		}
		
		private string DoLocalName(MethodDefinition method, int index)
		{
			string name = "V_" + index;
			
			MethodInfo info;
			if (m_methodEntries.TryGetValue(method.MetadataToken, out info))
			{	
				if (!info.Entry.LocalNamesAmbiguous)
				{
					foreach (LocalVariableEntry local in info.Entry.Locals)
					{
						if (local.Index == index)
							return local.Name;
					}

					Log.TraceLine(this, "failed to find variable {0}", index);
				}
				else
					Log.TraceLine(this, "{0} local variable names are ambiguous", info.Entry.SourceFile.FileName);
			}
			else
				Log.TraceLine(this, "failed to find entries for {0}", method);
			
			return name;
		}
		#endregion
		
		#region Private Types -------------------------------------------------
		private struct LineInfo : IComparable<LineInfo>
		{
			public int Offset;
			public int Line;
			
			public LineInfo(int offset, int line)
			{
				Offset = offset;
				Line = line;
			}

			public int CompareTo(LineInfo rhs)
			{
				if (Offset < rhs.Offset)
					return -1;
				else if (Offset > rhs.Offset)
					return +1;
				else
					return 0;
			}	
	
			public static bool operator==(LineInfo lhs, LineInfo rhs)
			{
				return lhs.Offset == rhs.Offset;
			}
			
			public static bool operator!=(LineInfo lhs, LineInfo rhs)
			{
				return !(lhs == rhs);
			}    

			public override bool Equals(object rhsObj)
			{
				if (rhsObj == null)  
					return false;
				
				if (GetType() != rhsObj.GetType()) 
					return false;
			
				LineInfo rhs = (LineInfo) rhsObj;                    
				return Offset == rhs.Offset;
			}
					
			public override int GetHashCode()
			{
				return Offset;
			}
		}

		private struct MethodInfo
		{
			public string File;
			public LineInfo[] Lines;	// used to map offsets into a method to line numbers in a source file
			public MethodEntry Entry;
			
			public MethodInfo(MethodEntry entry)
			{
				Entry = entry;
				File = entry.SourceFile.FileName;			
				
				List<LineInfo> lines = new List<LineInfo>(entry.LineNumbers.Length);
				foreach (LineNumberEntry line in entry.LineNumbers)
				{
					lines.Add(new LineInfo(line.Offset, line.Row));
				}
				
				Lines = lines.ToArray();
				lines.Sort();
			}

			public static bool operator==(MethodInfo lhs, MethodInfo rhs)
			{
				return lhs.Entry == rhs.Entry;
			}
			
			public static bool operator!=(MethodInfo lhs, MethodInfo rhs)
			{
				return !(lhs == rhs);
			}    

			public override bool Equals(object rhsObj)
			{
				if (rhsObj == null)   
					return false;
				
				if (GetType() != rhsObj.GetType()) 
					return false;
			
				MethodInfo rhs = (MethodInfo) rhsObj;                    
				return Entry == rhs.Entry;
			}
				
			public override int GetHashCode()
			{
				return Entry.GetHashCode();
			}
		}
		#endregion
		
		#region Fields --------------------------------------------------------
		private MethodEntries m_methodEntries;
		#endregion
	}
}