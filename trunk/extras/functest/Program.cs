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

using Smokey.Internal;
using System;
using System.Reflection;

namespace FuncTest
{
	internal sealed class Program
	{
		public static int Main(string[] args)
		{ 
			int err = 0; 	
			
			try
			{
				GetOptions options = new GetOptions();
				DoParse(options, args);
				
				if (!DoProcessOptions(options))
				{
					if (DoValidate(options)) 
					{
						string[] assemblies = options.Value("-asm").Split(',');
						
						Analyze analyze = new Analyze();
						if (!analyze.Run(options.Value("-exe"), assemblies))
							err = 1;
					}
					else
					{
						err = -2;
					}
				}
			}
			catch (MalformedCommandLineException m)
			{
				Console.Error.WriteLine(m.Message);
				err = -2;
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
				err = -1;
			}
			
			return err;
		}		

		#region Private methods
		private static void DoParse(GetOptions options, string[] args)
		{
			options.Add("-exe=", "Path to the smokey exe");
			options.Add("-asm=", "Comma separated list of assemblies");
			options.Add("-help", "-?", "Show this help list and exit");
			options.Add("-version", "Display version and licensing information and exit");

			options.Parse(args);
		}

		private static bool DoProcessOptions(GetOptions options)
		{
			bool processed = true;
						
			if (options.Has("-help"))
				DoShowHelp(options);
				
			else if (options.Has("-version"))
				DoShowVersion();
								
			else	
				processed = false;
			
			return processed;
		}
				
		private static void DoShowVersion()
		{
			Version version = Assembly.GetExecutingAssembly().GetName().Version;
			Console.WriteLine("smokey functest {0}", version);
		}
		
		private static void DoShowHelp(GetOptions options)
		{
			Version version = Assembly.GetExecutingAssembly().GetName().Version;

			Console.WriteLine("functest {0} - Copyright (C) 2007-2008 Jesse Jones", version);	// TODO: update the year as needed
			Console.WriteLine("Functional test for Smokey.");
			Console.WriteLine();
			
			options.ShowHelp();
		}
		
		private static bool DoValidate(GetOptions options)
		{
			bool valid = true;
			
			// -exe and -asm are required
			if (!options.Has("-exe"))
			{
				Console.Error.WriteLine("-exe is required");
				valid = false;
			}
			if (!options.Has("-asm"))
			{
				Console.Error.WriteLine("-asm is required");
				valid = false;
			}
			
			// should not be any operands
			if (options.Operands.Length > 0)
			{
				Console.Error.WriteLine("operands are not supported");
				valid = false;
			}
			
			return valid;
		}		
		#endregion
	} 
}
