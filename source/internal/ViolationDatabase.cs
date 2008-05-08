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
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;
using Smokey.App;
using Smokey.Framework;

namespace Smokey.Internal
{	
	// Loads Violation objects from xml.
	internal static class ViolationDatabase
	{							
		// Use Init instead of a type ctor so we have more control over when it's executed.
		public static void Init()
		{
			Assembly assembly = Assembly.GetExecutingAssembly();
			Stream stream = assembly.GetManifestResourceStream("Schema.xml");
			
			XmlSchema schema = XmlSchema.Read(stream, DoValidationEvent);

			ms_settings = new XmlReaderSettings();
			Ignore.Value = ms_settings.Schemas.Add(schema);
			ms_settings.ValidationEventHandler += DoValidationEvent;
			ms_settings.ValidationType = ValidationType.Schema;
			ms_settings.IgnoreComments = true;

			foreach (string file in ms_xmlFiles)
			{
				Log.TraceLine(true, "reading {0}", file);
				stream = assembly.GetManifestResourceStream(file);
				LoadXML(file, stream);
			}
		}
		
		public static void LoadCustom(Assembly assembly)
		{					
			string file = "Rules.xml";
			Log.TraceLine(true, "reading {0}/{1}", assembly.Location, file);
			Stream stream = assembly.GetManifestResourceStream(file);
			LoadXML(file, stream);
		}
				
		// Name is used for logging.
		public static void LoadXML(string name, Stream stream)
		{					
			try
			{
				Log.DebugLine(true, "loading {0}", name);
				XmlReader reader = XmlReader.Create(stream, ms_settings);
	
				XmlDocument xml = new XmlDocument();
				xml.Load(reader);
				
				foreach (XmlNode child in xml.ChildNodes)	// need this retarded loop because ms_settings.IgnoreComments doesn't appear to do anything
				{
					if (child.Name == "Violations")
						DoViolations(child);			
				}
			}
			catch (Exception e)
			{
				throw new ArgumentException("Failed to load " + name, e);
			}
		}
				
		public static Violation Get(string checkID)
		{
			int index = ms_entries.BinarySearch(new Entry(checkID));
			if (index >= 0)
			{
				Violation violation = ms_entries[index].Violation;
				DBC.FastPost(violation.CheckID == checkID, "looked up a bad violation");
				return violation;
			}
			else
				throw new ArgumentException("Couldn't find the " + checkID + " xml element.");
		}
				
		public static bool IsValid(string checkID)
		{
			int index = ms_entries.BinarySearch(new Entry(checkID));
			return index >= 0;
		}
				
		public static IEnumerable<Violation> Violations
		{
			get
			{
				foreach (Entry entry in ms_entries)
					yield return entry.Violation;
			}
		}

		public static string[] XmlFiles()
		{
			return (string[]) ms_xmlFiles.Clone();
		}
				
		#region Private methods
		private static void DoViolations(XmlNode violations)
		{
			foreach (XmlNode child in violations.ChildNodes)
			{
				if (child.Name == "Violation")
					DoViolation(child);			
			}
		}
		
		private static void DoViolation(XmlNode violation)
		{
			string checkID = violation.Attributes["checkID"].Value;
			Severity severity = (Severity) Enum.Parse(typeof(Severity), violation.Attributes["severity"].Value);
			bool breaking = violation.Attributes["breaking"].Value.ToLower() == "true";
			
			foreach (XmlNode child in violation.ChildNodes)
			{
				if (child.Name == "Translation")
				{
					// Get the next translation.
					Entry entry = DoTranslation(child, checkID, severity, breaking);
								
#if DEBUG
					// Make sure it's not a duplicate of one we've seen already.
					string key = string.Format("{0}/{1}", checkID, entry.Lang);		
					
					int i = ms_checkIDs.BinarySearch(key);
					if (i < 0)
						ms_checkIDs.Insert(~i, key);
					else 
						DBC.Assert(false, "{0} is already defined", key);
#endif
					
					// If the checkID is new, or the language is better than
					// our current translation use the new translation.
					int index = ms_entries.BinarySearch(entry);
					if (index < 0)
					{
						Log.DebugLine(true, "adding {0}", checkID);
						ms_entries.Insert(~index, entry);
					}
					else if (DoLeftIsBetter(entry, ms_entries[index]))
					{
						Log.DebugLine(true, "overriding {0} ({1} is better than {2})", checkID, entry.Lang, ms_entries[index].Lang);
						ms_entries[index] = entry;
					}
				}
			}			
		}
				
		private static Entry DoTranslation(XmlNode translation, string checkID, Severity severity, bool breaking)
		{
			string lang = translation.Attributes["lang"].Value;
			string typeName = translation.Attributes["typeName"].Value;
			string category = translation.Attributes["category"].Value;
			
			string cause = string.Empty, description = string.Empty, fix = string.Empty, csharp = string.Empty;

			foreach (XmlNode child in translation.ChildNodes)
			{
				if (child.Name == "Cause")
					cause = child.InnerText.Trim();			
				else if (child.Name == "Description")
					description = child.InnerText.Trim();
				else if (child.Name == "Fix")
					fix = child.InnerText.Trim();
				else if (child.Name == "CSharp")
				{
					int i = child.InnerText.IndexOf(NewLine.Windows);	// first line is normally so we have to get rid of it or Reformat.Code doesnt work right
					if (i < 0)
						i = child.InnerText.IndexOf(NewLine.Unix);
					if (i < 0)
						i = child.InnerText.IndexOf(NewLine.Mac);
				
					if (i < 0)
						csharp = child.InnerText;
					else
						csharp = child.InnerText.Substring(i + 1);
				}
			}
			
			Entry entry = new Entry(lang, new Violation(checkID, typeName, category, severity, breaking, cause, description, fix, csharp));
			
			return entry;
		}
		
		private static bool DoLeftIsBetter(Entry lhs, Entry rhs)
		{
			// If the two entries have the same language neither is better.
			if (lhs.Lang == rhs.Lang)
			{
				Log.WarningLine(true, "{0} has two entries for the {1} language", lhs.Violation.CheckID, lhs.Lang);
				return false;
			}
			
			// If the lhs is an exact match for the current language it is better.
			else if (lhs.Lang == CultureInfo.CurrentCulture.TwoLetterISOLanguageName)
			{
				return true;
			}
			
			// If current lanuage isn't set and the lhs is english then use it. This
			// will happen if neither LC_ALL and LANG is set.
			else if (CultureInfo.CurrentCulture.TwoLetterISOLanguageName == "iv" && lhs.Lang == "en")
			{
				return true;
			}
			
			// Otherwise the rhs is better.
			return false;
		}
		
		private static void DoValidationEvent(object sender, ValidationEventArgs e)
		{
			if (e.Severity == XmlSeverityType.Warning)
				Console.WriteLine("{0}", e.Message);
			else 
				throw e.Exception;
		}
		#endregion

		#region Internal types
		private struct Entry : IComparable<Entry>
		{
			public string Lang;
			public Violation Violation;
			
			public Entry(string checkID)
			{
				Lang = "*";
				this.Violation = new Violation(checkID, null, null, Severity.Warning, false, null, null, null, null);
			}

			public Entry(string lang, Violation violation)
			{
				Lang = lang;
				Violation = violation;
			}

			public int CompareTo(Entry rhs)
			{
				return Violation.CompareTo(rhs.Violation);
			}	
		
			public override bool Equals(object rhsObj)
			{
				if (rhsObj == null)                        // objects may be null
					return false;
				
				if (GetType() != rhsObj.GetType()) 
					return false;
			
				Entry rhs = (Entry) rhsObj;                    
				return Violation == rhs.Violation;
			}
					
			public override int GetHashCode()
			{
				return Violation.GetHashCode();
			}

			public static bool operator==(Entry lhs, Entry rhs)
			{
				return lhs.Violation == rhs.Violation;
			}
			
			public static bool operator!=(Entry lhs, Entry rhs)
			{
				return !(lhs == rhs);
			}    
		}
		#endregion

		#region Fields
		private static XmlReaderSettings ms_settings;
		private static List<Entry> ms_entries = new List<Entry>();
#if DEBUG
		private static List<string> ms_checkIDs = new List<string>();	// check id + "/" + lang, used to find duplicate definitions
#endif
		private static readonly string[] ms_xmlFiles = new string[]{
			"Correctness.xml", "Design.xml", "Globalization.xml", "Microsoft.xml", "Misc.xml", 
			"Mono.xml", "Performance.xml", "Portability.xml", "Reliability.xml",
			"Security.xml"};
		#endregion
	}
}