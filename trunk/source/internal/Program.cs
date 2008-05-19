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

using Smokey.App;
using Smokey.Framework;

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Smokey.Internal
{
	// Main entry point for Smokey.
	internal static class Program
	{
		public static int Main(string[] inArgs)
		{ 
			int err = 0; 	
			
			try
			{
				Profile.Start("App");
				
				DoPreOptionsInit();
				string[] args = DoExpandProfile(inArgs);

				GetOptions options = new GetOptions();
				DoParse(options, args);
				
				if (!DoProcessOptions(options))
				{
					if (DoValidate(options)) 
					{
						DoPostOptionsInit(options, args);
						if (!DoRun(options))
							err = 1;
					}
					else
					{
						err = 2;
					}
				}
				
				Profile.Stop("App");
#if PROFILE
				Console.WriteLine(Profile.GetResults());
#endif
			}
			catch (MalformedCommandLineException m)
			{
				Console.Error.WriteLine(m.Message);
				err = 2;
			}
			catch (Exception e)
			{
				Exception n = e;
				int indent = 0;
				while (n != null)
				{
					Console.Error.WriteLine("{0}{1}", new string(' ', 3*indent), n.Message);
					n = n.InnerException;
					++indent;
				}

				Console.Error.WriteLine("exception stack trace:");
				Console.Error.WriteLine(e.StackTrace);

				err = 3;
			}
			finally
			{
#if DEBUG
				if (Log.NumErrors > 0 && Log.NumWarnings > 0)
					Console.Error.WriteLine("There were {0} errors and {1} warnings!!!", Log.NumErrors, Log.NumWarnings);

				else if (Log.NumErrors > 0)
					Console.Error.WriteLine("There were {0} errors!!!", Log.NumErrors.ToString());

				else if (Log.NumWarnings > 0)
					Console.Error.WriteLine("There were {0} warnings!!!", Log.NumWarnings.ToString());				
#endif
				Log.Flush();
			}
			
			return err;
		}
				
		#region Private Methods -----------------------------------------------
		private static bool DoProcessOptions(GetOptions options)
		{
			bool processed = true;
						
			if (options.Has("-help"))
				DoShowHelp(options);
				
			else if (options.Has("-version"))
				DoShowVersion();
				
			else if (options.Has("-usage"))
				DoShowUsage();
				
#if DEBUG
			else if (options.Has("-check-xml"))
			{
				CheckXml checker = new CheckXml();
				checker.Check();
			}
			else if (options.Has("-generate-html-violations"))
			{
				ViolationDatabase.Init();
	
				HtmlViolations html = new HtmlViolations();
				html.Write(options.Value("-generate-html-violations"));
			}
			else if (options.Has("-dump-strings"))
			{
				string assemblyPath = options.Operands[0];
				DumpStrings.Dump(assemblyPath);
			}
#endif
			else	
				processed = false;
			
			return processed;
		}
		
		private static void DoParse(GetOptions options, string[] args)
		{
			options.Add("-append", "Append to log files");
			options.Add("-exclude-check=", "Don't do checks if the checkID contains PARAM");
			options.Add("-exclude-name=", "Don't check types/methods where the full name contains PARAM");
			options.Add("-help", "-?", "Show this help list and exit");
			options.Add("-html", "Generate an html report");
			options.Add("-ignore-breaking", "Skip rules where the fix may break binary compatibility");
			options.Add("-include-check=", "Undo -exclude-check");
			options.Add("-include-name=", "Undo -exclude-name");
			options.Add("-include-breaking", "Undo -ignore-breaking");
			options.Add("-include-localized", "Undo -not-localized");
			options.Add("-not-localized", "Disable localization rules");
			options.Add("-out=stdout", "Path to write the report to");
			options.Add("-profile=", "Use a named option set");
			options.Add("-quiet", "Don't print progress");
			options.Add("-only-type=", "Only check types where the full name contains PARAM");
			options.Add("-set=", "Change a setting, e.g. -set:maxBoxes:0");
			options.Add("-severity=Nitpick", "Only check rules with this severity or higher");
			options.Add("-text", "Generate a text report, this is the default");
			options.Add("-usage", "Show usage syntax and exit");
			options.Add("-verbose", "-V", "Progress prints names and rules");
			options.Add("-version", "Display version and licensing information and exit");
			options.Add("-xml", "Generate an xml report");			
#if DEBUG			
			options.Add("-check-xml", "Compiles the C# code examples in the xml and reports any errors");
			options.Add("-generate-html-violations=", "Generates html docs from the xml");
			options.Add("-dump-strings", "Dump setters and ctors with string parameters");
#endif

			options.Parse(args);
		}
		
		private static bool DoValidate(GetOptions options)
		{
			bool valid = true;
			
			// can't use both -verbose and -quiet
			if (options.Has("-verbose") && options.Has("-quiet"))
			{
				Console.Error.WriteLine("can't use both -verbose and -quiet");
				valid = false;
			}
			
			// -text, -html, and -xml are exclusive
			if (options.Has("-text") && (options.Has("-html") || options.Has("-xml")))
			{
				Console.Error.WriteLine("-text, -html, and -xml are exclusive options");
				valid = false;
			}
			if (options.Has("-html") && options.Has("-xml"))
			{
				Console.Error.WriteLine("-text, -html, and -xml are exclusive options");
				valid = false;
			}
			
			// -exclude-name values must be valid
			string[] names = options.Values("-exclude-name");
			if (!DoTypeMethodsValid(names, "-exclude-name"))
			{
				valid = false;
			}
						
			// -include-name values must be valid
			names = options.Values("-include-name");
			if (!DoTypeMethodsValid(names, "-include-name"))
			{
				valid = false;
			}
			
			// -severity must be legit
			foreach (string severity in options.Values("-severity"))
			{
				if (severity != "Error" && severity != "Warning" && severity != "Nitpick")
				{
					Console.Error.WriteLine("severity must be Error, Warning, or Nitpick");
					valid = false;
				}
			}
			
			// -set must be well formed
			string[] values = options.Values("-set");
			foreach (string value in values)
			{
				if (!value.Contains(":"))
				{
					Console.Error.WriteLine("-set:{0} is missing a colon", value);
					valid = false;
				}
			}
						
			// must have one assembly
			if (options.Operands.Length != 1)	
			{
				Console.Error.WriteLine("one assembly must be specified");
				valid = false;
			}
			
			return valid;
		}		

		private static bool DoTypeMethodsValid(string[] strs, string option)
		{
			bool valid = true;
			
			for (int i = 0; i < strs.Length && valid; ++i)
			{
				if (strs[i].Contains("*"))
				{
					Console.Error.WriteLine("{0} doesn't support wild cards", option);
					valid = false;
				}
			}
			
			return valid;
		}
		
		private static void DoShowUsage()
		{
			Console.WriteLine("Simple usage is like this:");
			Console.WriteLine("   mono smokey.exe MyCoolApp.exe");
			Console.WriteLine();
			
			Console.WriteLine("A more complex example is:");
			Console.WriteLine("   mono smokey.exe -severity:Warning -exclude-check:P1001 MyCoolApp.exe");
			Console.WriteLine("which will skip the rules with severity Nitpick and skip the MethodCanBeMadeStatic rule.");
			Console.WriteLine();

			Console.WriteLine("-exclude-check, -exclude-name, and -only-type may appear multiple times.");
			Console.WriteLine("Note that it's usually better to use the DisableRuleAttribute instead of -exclude-name, see the README for details.");
		}
		
		private static void DoShowVersion()
		{
			Version version = Assembly.GetExecutingAssembly().GetName().Version;
			Console.WriteLine("Smokey {0}", version);
		}
		
		private static void DoShowHelp(GetOptions options)
		{
			Version version = Assembly.GetExecutingAssembly().GetName().Version;

			Console.WriteLine("Smokey {0} - Copyright (C) 2007-2008 Jesse Jones", version);	// TODO: update the year as needed
			Console.WriteLine("Analyzes assemblies and reports problems with the code.");
			Console.WriteLine();
			Console.WriteLine("Usage: mono smokey [options] assembly");
			Console.WriteLine();
			
			options.ShowHelp();

			Console.WriteLine();
			DoShowProfileHelp();
		}
		
		private static void DoShowProfileHelp()
		{
			Console.WriteLine("Profiles:");
			
			int width = "system".Length;
			foreach (KeyValuePair<string, string> entry in Settings.Entries("profile:"))
			{
				int i = entry.Key.IndexOf(':');
				string name = entry.Key.Substring(i + 1);
				if (name.Length > width)
					width = name.Length;
			}
			
			bool foundSystem = false;
			foreach (KeyValuePair<string, string> entry in Settings.Entries("profile:"))
			{
				int i = entry.Key.IndexOf(':');
				string name = entry.Key.Substring(i + 1);
				if (name == "system")
					foundSystem = true;

				string padding = new string(' ', width - name.Length);
				Console.WriteLine("   {0}{1}  {2}", name, padding, entry.Value);
			}
			
			if (!foundSystem)
				Console.WriteLine("   {0}   {1}", "system", string.Join(" ", ms_systemProfile));
		}
		
		private static string[] DoExpandProfile(string[] args)
		{	
			List<string> result = new List<string>(args.Length);
			
			foreach (string arg in args)
			{				
				if (arg.StartsWith("-profile") || arg.StartsWith("--profile"))
				{
					int i = arg.IndexOf('=');
					if (i < 0)
						i = arg.IndexOf(':');
					if (i >= 0)
					{
						string profile = arg.Substring(i + 1);
						DoAppendProfile(result, profile);
					}
					else
					{
						throw new MalformedCommandLineException(string.Format("Expected '=' or ':' in '{0}'.", arg));
					}
				}
				else
					result.Add(arg);
			}
			
			return result.ToArray();
		}
		
		private static void DoAppendProfile(List<string> result, string name)
		{
			string key = "profile:" + name;
			if (Settings.Has(key))
			{
				string[] args = Settings.Get(key, string.Empty).Split(new char[]{' ', '\t'}, StringSplitOptions.RemoveEmptyEntries);
				result.AddRange(args);
			}
			else if (name == "system")
			{
				result.AddRange(ms_systemProfile);
			}
			else
			{
				throw new MalformedCommandLineException(string.Format("'{0}' is not a valid profile name.", name));
			}
		}
				
		private static void DoPreOptionsInit()
		{
			Settings.Add("naming", "mono");

			NameValueCollection nv = ConfigurationManager.AppSettings;
			for (int i = 0; i < nv.Count; ++i)
				Settings.Add(nv.GetKey(i), nv.Get(i));
		}
		
		private static void DoPostOptionsInit(GetOptions options, string[] args)
		{				
			if (options.Has("-not-localized"))
				Settings.Add("*localized*", "false");

			string paths = Settings.Get("custom", string.Empty);
			foreach (string path in paths.Split(':'))
			{
				if (path.Length > 0)
					Ignore.Value = System.Reflection.Assembly.LoadFrom(path);	// need to load these before we init the logger
			}

			Log.Init(options.Has("-append"));
			Log.InfoLine(true, "started up on {0}, version {1}", DateTime.Now, Assembly.GetExecutingAssembly().GetName().Version);
			Log.InfoLine(true, "arguments are '{0}'", string.Join(", ", args));

#if DEBUG
			AssertTraceListener.Install();
#endif
						
			foreach (string entry in options.Values("-set"))
			{
				string key = entry.Split(':')[0];
				string value = entry.Split(':')[1];
				Settings.Add(key, value);
			}
			
			ViolationDatabase.Init();
		}
		
		private static bool DoRun(GetOptions options)
		{ 
			Progress progress = null;
			if (!options.Has("-quiet"))
				progress = new Progress(options.Has("-verbose"));
				
			Watchdog watcher = new Watchdog(options.Has("-verbose"));
			
			Error[] errors = null;
			try
			{
				DateTime startTime = DateTime.Now;

				AnalyzeAssembly analyzer = new AnalyzeAssembly(delegate (string name)
				{
					progress.Add(name);
					watcher.Add(name);
				});
				
				List<string> excluded = new List<string>(options.Values("-exclude-check"));
				if (excluded.IndexOf("R1035") < 0)		// ValidateArgs2 is disabled by default
					excluded.Add("R1035");
				
				analyzer.ExcludedChecks = excluded.Except(options.Values("-include-check"));
				analyzer.ExcludeNames = options.Values("-exclude-name").Except(options.Values("-include-name"));
				
				string[] onlyType = options.Values("-only-type");
				
				string[] severities = options.Values("-severity");
				Severity severity = (Severity) Enum.Parse(typeof(Severity), severities[severities.Length - 1]);
				bool ignoreBreaks = options.Has("-ignore-breaking") && !options.Has("-include-breaking");
				errors = analyzer.Analyze(options.Operands[0], onlyType, severity, ignoreBreaks);
				TimeSpan elapsed = DateTime.Now - startTime;
				
				string assemblyPath = options.Operands[0];
				string outFile = options.Value("-out");
				
				if (options.Has("-xml"))
					XmlReport.Report(assemblyPath, outFile, errors);
				else if (options.Has("-html"))
					HtmlReport.Report(assemblyPath, outFile, errors, analyzer.NumRules, elapsed);
				else
					TextReport.Report(assemblyPath, outFile, errors, analyzer.NumRules, elapsed);
			}
			finally
			{
				if (progress != null)
					progress.Dispose();
				watcher.Dispose();
			}
			
			int count = errors.Count(x => x.Violation.Severity != Severity.Nitpick);
						
			return count == 0;
		}
		#endregion	

		#region Fields --------------------------------------------------------
		private static string[] ms_systemProfile = new string[]
		{
			"-exclude-check=D1006",		// ImplementGenericCollection
			"-exclude-check=D1011",		// TypedEnumerator
			"-exclude-check=D1037",		// DontExit1
			"-exclude-check=D1038",		// DontExit2
			"-exclude-check=MS1010",	// ReservedExceptions
			"-exclude-check=P1003",		// AvoidBoxing
			"-exclude-check=P1004",		// AvoidUnboxing
			"-exclude-check=P1005",		// StringConcat
			"-exclude-check=P1007",		// NonGenericCollections
			"-exclude-check=PO1001",	// DllImportPath
			"-exclude-check=PO1002",	// DllImportExtension
			"-exclude-check=PO1009",	// WinFormsVoodoo
			"-ignore-breaking",
			"-not-localized",
			"-set:dictionary:/resources/SysIgnore.txt",
		};
		#endregion	
	} 
}
