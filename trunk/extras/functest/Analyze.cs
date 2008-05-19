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
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Schema;

namespace FuncTest
{
	// This is the guy who does the real work of running the functional test.
	internal sealed class Analyze
	{		
		public Analyze()
		{
			m_settings.ValidationEventHandler += DoValidationEvent;
			m_settings.ValidationType = ValidationType.Schema;
			m_settings.IgnoreComments = true;

			m_outFile = Path.Combine(Path.GetTempPath(), "ftest.results.xml");
		}
						
		// Returns false if the run wasn't as expected.
		public bool Run(string smokePath, string[] assemblies)
		{
			List<Expected> unexpectedErrors = new List<Expected>();

			bool append = false;
			foreach (string assembly in assemblies)
			{
				DoGetExpected(assembly);
				DoSmoke(smokePath, assembly, append);

				XmlDocument actual = DoGetActual();		
				DoCheckResults(actual, unexpectedErrors);
				append = true;
			}
						
			bool good = DoReportErrors(unexpectedErrors);

			return good;
		}
		
		#region Internal types
		private struct Expected
		{
			public string checkID;
			public string fullName;
			public string typeName;
			
			public Expected(string checkID, string fullName, string typeName)
			{
				this.checkID = checkID;
				this.fullName = fullName;
				this.typeName = typeName;
			}

			public override bool Equals(object rhsObj)
			{
				if (rhsObj == null)			
					return false;
					
				if (GetType() != rhsObj.GetType()) 
					return false;
				
				Expected rhs = (Expected) rhsObj;
				return this == rhs;
			}
				
			public bool Equals(Expected rhs)
			{				
				return this == rhs;
			}

			public static bool operator==(Expected lhs, Expected rhs)
			{				
				return lhs.checkID == rhs.checkID && lhs.fullName == rhs.fullName;
			}
			
			public static bool operator!=(Expected lhs, Expected rhs)
			{
				return !(lhs == rhs);
			}

			public override int GetHashCode()
			{
			int hash;
			
			unchecked
			{
				hash = checkID.GetHashCode() + fullName.GetHashCode();
			}
			
			return hash;
			}
		}
		#endregion
				
		#region Private methods
		private bool DoReportErrors(List<Expected> unexpectedErrors) 
		{
			// didn't get an error we expected to see
			List<Expected> numMissingErrors = new List<Expected>();		
			foreach (Expected error in m_expected)
			{
				if (m_actual.IndexOf(error) < 0)
					numMissingErrors.Add(error);
			}
			
			// got an error we didn't expect to see
			List<Expected> numUnexpectedErrors = new List<Expected>();		
			foreach (Expected error in m_actual)
			{
				if (m_expected.IndexOf(error) < 0 && m_shouldPass.IndexOf(error) < 0 && m_shouldFail.IndexOf(error) < 0)
					numUnexpectedErrors.Add(error);
			}
			
			// error, but shouldFail
			List<Expected> numBrokenShouldFail = new List<Expected>();	
			foreach (Expected error in m_shouldFail)
			{
				if (m_actual.IndexOf(error) >= 0)
					numBrokenShouldFail.Add(error);
			}
			
			// no error, but shouldPass
			List<Expected> numBrokenShouldPass = new List<Expected>();	
			foreach (Expected error in m_shouldPass)
			{
				if (m_actual.IndexOf(error) < 0)
					numBrokenShouldPass.Add(error);
			}
			
			bool passes = numMissingErrors.Count == 0 && numUnexpectedErrors.Count == 0 && numBrokenShouldFail.Count == 0 && numBrokenShouldPass.Count == 0;
			
			if (passes)
			{
				if (m_shouldFail.Count + m_shouldPass.Count == 0)
					Console.WriteLine("Passed, found {0} errors and all were expected", m_actual.Count);
				else if (m_shouldFail.Count + m_shouldPass.Count == 1)
					Console.WriteLine("Passed, found {0} errors but missed one which is known to be broken", m_actual.Count);
				else
					Console.WriteLine("Passed, found {0} errors but missed {1} which are known to be broken", m_actual.Count, m_shouldFail.Count + m_shouldPass.Count);
			}
			else
			{
				Console.WriteLine("FAILED");
				
				if (numMissingErrors.Count > 0)
				{
					if (numMissingErrors.Count == 1)
						Console.WriteLine("Missed one error that was expected:");
					else
						Console.WriteLine("Missed {0} errors that were expected:", numMissingErrors.Count);
					Console.WriteLine(ToString(numMissingErrors));
				}
				
				if (numUnexpectedErrors.Count > 0)
				{
					if (numUnexpectedErrors.Count == 1)
						Console.WriteLine("Got one error that wasn't expected:");
					else
						Console.WriteLine("Got {0} errors that weren't expected:", numUnexpectedErrors.Count);
					Console.WriteLine(ToString(numUnexpectedErrors));
				}
				
				if (numBrokenShouldFail.Count > 0)
				{
					if (numBrokenShouldFail.Count == 1)
						Console.WriteLine("Got one error from a case that we thought was broken:");
					else
						Console.WriteLine("Got {0} errors from cases that we thought were broken:", numBrokenShouldFail.Count);
					Console.WriteLine(ToString(numBrokenShouldFail));
				}
				
				if (numBrokenShouldPass.Count > 0)
				{
					if (numBrokenShouldPass.Count == 1)
						Console.WriteLine("Missed one error from a case that we thought was broken:");
					else
						Console.WriteLine("Missed {0} errors from cases that we thought were broken:", numBrokenShouldPass.Count);
					Console.WriteLine(ToString(numBrokenShouldPass));
				}
			}
						
			return passes;
		}
		
		private string ToString(List<Expected> errors)
		{
			StringBuilder builder = new StringBuilder(80 * errors.Count);
			
			foreach (Expected error in errors)
			{
				builder.Append(string.Format("   {0}/{1}   {2}", error.checkID, error.typeName, error.fullName));
				builder.Append(Environment.NewLine);
			}
			
			return builder.ToString();
		}
		
		private void DoCheckResults(XmlDocument actual, List<Expected> unexpectedErrors)
		{			
			foreach (XmlNode child in actual.ChildNodes)	// need this retarded loop because m_settings.IgnoreComments doesn't appear to do anything
			{
				if (child.Name == "Errors")
				{
					foreach (XmlNode grandchild in child.ChildNodes)
					{
						if (grandchild.Name == "Error")
							DoCheckActualError(grandchild, unexpectedErrors);
					}
				}
			}
		}
							
		private void DoCheckActualError(XmlNode error, List<Expected> unexpectedErrors)
		{
			string checkID = "", fullName = "", typeName = "";
			foreach (XmlNode child in error.ChildNodes)
			{
				if (child.Name == "Location")
				{
					fullName = child.Attributes["name"].Value;
					int i = fullName.IndexOf(' ');
					if (i >= 0)
						fullName = fullName.Substring(i + 1);
				}
				else if (child.Name == "Violation")
				{
					checkID = child.Attributes["checkID"].Value;
					typeName = child.Attributes["typeName"].Value;
				}
			}

//			Console.WriteLine("actual {0} {1}", checkID, fullName);			
																				
			Expected err = new Expected(checkID, fullName, typeName);	
			m_actual.Add(err);
		}
		
		private XmlDocument DoGetActual()
		{			
			Assembly assembly = Assembly.GetExecutingAssembly();	// TODO: this is a little retarded now that we support multiple assemblies
			Stream stream = assembly.GetManifestResourceStream("Smoke.schema.xml");
			XmlSchema schema = XmlSchema.Read(stream, DoValidationEvent);	

			m_settings.Schemas.Add(schema);
			XmlReader reader = XmlReader.Create(m_outFile, m_settings);

			XmlDocument xml = new XmlDocument();
			xml.Load(reader);
			m_settings.Schemas.Remove(schema);

			return xml;
		}

		private void DoGetExpected(string evilPath)
		{			
			Assembly assembly = Assembly.GetExecutingAssembly();
			Stream stream = assembly.GetManifestResourceStream("Expected.schema.xml");
			XmlSchema schema = XmlSchema.Read(stream, DoValidationEvent);
			m_settings.Schemas.Add(schema);
			
			assembly = Assembly.ReflectionOnlyLoadFrom(evilPath);
			string[] names = assembly.GetManifestResourceNames();
			if (names.Length != 1)
				throw new Exception(string.Format("{0} has {1} resources", evilPath, names.Length));
			stream = assembly.GetManifestResourceStream(names[0]);
			XmlReader reader = XmlReader.Create(stream, m_settings);

			XmlDocument xml = new XmlDocument();
			xml.Load(reader);
			m_settings.Schemas.Remove(schema);

			DoGetExpected(xml);
		}

		private void DoGetExpected(XmlDocument xml)
		{			
			foreach (XmlNode child in xml.ChildNodes)	// need this retarded loop because m_settings.IgnoreComments doesn't appear to do anything
			{
				if (child.Name == "Expectations")
				{
					foreach (XmlNode grandchild in child.ChildNodes)
					{
						if (grandchild.Name == "Expected")
						{
							string checkID = grandchild.Attributes["checkID"].Value;
							string fullName = grandchild.Attributes["fullName"].Value;
							string typeName = grandchild.Attributes["typeName"].Value;
							
							bool shouldPass = false, shouldFail = false;
							Expected e = new Expected(checkID, fullName, typeName);

							if (grandchild.Attributes["shouldPass"] != null)
							{
								string value = grandchild.Attributes["shouldPass"].Value;
								shouldPass = value == "true" || value == "1";
							}
							 
							if (grandchild.Attributes["shouldFail"] != null)
							{
								string value = grandchild.Attributes["shouldFail"].Value;
								shouldFail = value == "true" || value == "1";
							}
						
							if (shouldPass)
								m_shouldPass.Add(e);
							else if (shouldFail)
								m_shouldFail.Add(e);
							else
								m_expected.Add(e);
						}
					}
				}
			}
		}

		private static void DoValidationEvent(object sender, ValidationEventArgs e)
		{
			if (e.Severity == XmlSeverityType.Warning)
				Console.WriteLine("{0}", e.Message);
			else 
				throw e.Exception;
		}

		private void DoSmoke(string smokePath, string evilPath, bool append)
		{
			File.Delete(m_outFile);			// make sure we're using the file we think we are
			
			string ap = append ? "-append" : string.Empty;
			string cmdLine = string.Format("--debug {0} -include-check:R1035 -xml -out:{1} {2} {3}", smokePath, m_outFile, ap, evilPath);
			Process process = Process.Start("mono", cmdLine);
			
			if (!process.WaitForExit(60*1000))
				throw new ApplicationException("timed out waiting for " + smokePath);
				
			if (process.ExitCode < 0 || process.ExitCode > 1)
				throw new ApplicationException(smokePath + " failed with error " + process.ExitCode);
		}
		#endregion

		#region Fields
		private XmlReaderSettings m_settings = new XmlReaderSettings();
		private string m_outFile;
		private List<Expected> m_actual = new List<Expected>();		// errors which were reported
		private List<Expected> m_expected = new List<Expected>();	// errors we expect
		private List<Expected> m_shouldFail = new List<Expected>();	// no error, but we'd like to see one
		private List<Expected> m_shouldPass = new List<Expected>();	// error, but there shouldn't be
		#endregion
	}
}

// compare the xml with expected.xml
// report results