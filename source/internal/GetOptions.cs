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
using Smokey.Framework;

namespace Smokey.Internal
{	
	[DisableRule("R1019", "RequireSerializableCtor")]
	[DisableRule("D1012", "InternalException")]			// this really is an internal exception
	[Serializable]
	internal sealed class MalformedCommandLineException : Exception
	{
		public MalformedCommandLineException(string message) : base(message) 
		{
		}
		
//		public MalformedCommandLineException(string message, Exception innerException) : 
//			base (message, innerException)
//		{
//		}
	}  			

	// Mono.GetOptions is deprecated as of mono 1.2.6 so we provide our own version.
	internal sealed class GetOptions
	{
		// Typical usage is Add("-help", "-h", "-?", "blah blah"). The first string
		// is the primary option key used to identify the option. The middle strings
		// are aliaii for the primary key. The last string is the help text. Note 
		// that the option can have a value if and only if the primary key ends with
		// '='. If the '=' is followed by text then that text is used as the default 
		// value.
		public void Add(params string[] strs)
		{
			DBC.Pre(strs.Length >= 2, "{0} should have both a primary key and a help string", string.Join(", ", strs));
			DBC.Pre(!m_options.ContainsKey(strs[0]), "{0} was already added", strs[0]);
						
			string key = strs[0];
			string value = null;
			int i = key.IndexOf("=");
			if (i >= 0)
			{
				key = strs[0].Substring(0, i);
				if (i + 1 < strs[0].Length)
					value = strs[0].Substring(i + 1);
			}
							
			string help = strs[strs.Length - 1];
			
			string[] aliaii = new string[strs.Length - 2];	// TODO: should assert that there are no = in aliaii
			Array.Copy(strs, 1, aliaii, 0, strs.Length - 2);
				
			m_options[key] = new Option(key, value, help, i >= 0, aliaii);
		}
				
		// Throws an exception for unrecogized options, ambiguous options,
		// and malformed arguments. Note that either '=' or ':' can be used
		// for the argument separator.
		public void Parse(params string[] args)
		{
			DoSanityTest();
			
			m_operands.Clear();
			foreach (Option option in m_options.Values)
				option.Reset();
			
			bool processOptions = true;
			foreach (string arg in args)
			{
				if (arg == "--")
				{
					processOptions = false;
				}
				else if (processOptions && arg.StartsWith("-"))
				{
					string arg2 = arg;
					if (arg.StartsWith("--"))	// this seems kind of cheesy
						arg2 = arg.Remove(0, 1);
						
					string key = arg2;		// TODO: should support grouped short options, eg -lrwx
					string value = null;	// TODO: add an enum to control the option style: linux (-xml), windows (/xml), both, or gnu (--xml), or just use aliaii?
					
					int i = arg2.IndexOfAny(new char[]{'=', ':'});
					if (i >= 0)
					{
						key = arg2.Substring(0, i);
						value = arg2.Substring(i + 1);
					}
					
					Option option = DoFind(key);					
					option.Found = true;
					option.AddValue(value);
				}
				else
				{
					m_operands.Add(arg);
				}
			}
		}

		public bool Has(string key)	// TODO: might want to change this to Count so we can support stuff like -verbose -verbose
		{
			DoValidateKey(key);
			
			Option option;
			if (m_options.TryGetValue(key, out option))
				return option.Found;
			else
				return false;
		}
		
		// Throws an exception if key is missing or there is not exactly one value.
		public string Value(string key)
		{
			DoValidateKey(key);
			
			Option option;
			if (m_options.TryGetValue(key, out option))
			{
				string[] values = option.Values;
				if (values.Length != 1)
					throw new MalformedCommandLineException("Expected only one instance of " + key);
				return values[0];
			}
			else
				throw new MalformedCommandLineException(key + " is missing");
		}
		
		// Used for options which may appear multiple times. Note that this will return an
		// empty array if the key wasn't present.
		public string[] Values(string key)
		{
			DoValidateKey(key);
			
			Option option;
			if (m_options.TryGetValue(key, out option))
				return option.Values;
			else
				return new string[0];
		}
		
		// Operands are the args that don't start with a dash and the args that
		// follow "--".
		public string[] Operands
		{
			get {return m_operands.ToArray();}
		}
		
		public void ShowHelp()
		{
			SortedList<string, string> options = DoBuildHelp();
			
			int width = 0;
			foreach (string key in options.Keys)
			{
				if (key.Length > width)
					width = key.Length;
			}
			
			Console.WriteLine("Options:");
			foreach (KeyValuePair<string, string> entry in options)
			{
				string padding = new string(' ', width - entry.Key.Length);
				Console.WriteLine("   {0}{1}  {2}", entry.Key, padding, entry.Value);
			}
		}
		
		#region Private Methods
		private SortedList<string, string> DoBuildHelp()
		{
			SortedList<string, string> options = new SortedList<string, string>();
			
			foreach (Option option in m_options.Values)
			{
				string key = string.Join(" ", option.Names);
				if (option.RequiresArg)
				{
					if (option.Default != null)
						key = string.Format("{0}={1}", key, option.Default);
					else
						key = string.Format("{0}=PARAM", key);
				}
				
				options.Add(key, option.Help);
			}			
			
			return options;
		}
		
		private void DoSanityTest()
		{
			foreach (Option option in m_options.Values)
			{
				foreach (string name in option.Names)
				{
					Option result = DoFind(name);	// make sure no two options are ambiguous
					DBC.Assert(result != null, "expected to find {0}", name);
				}
			}
		}
		
		// In order to catch common programmer errors like misspelled options we ensure 
		// that all keys passed in to our class match an existing primary key.
		private void DoValidateKey(string key)
		{
			foreach (Option option in m_options.Values)
			{
				if (option.Key == key)
					return;
			}
			
			throw new ArgumentException(key + " isn't a valid primary key");
		}

		private Option DoFind(string arg)
		{
			DBC.Assert(!string.IsNullOrEmpty(arg), "null or empty arg");
			
			Option result = null;
			int best = -1;
			List<string> matches = new List<string>();
			
			foreach (Option option in m_options.Values)
			{
				string alias;
				int num = option.Match(arg, out alias);
				
				if (num > 0 && num >= best)
				{
					if (num > best)
					{
						best = num;
						matches.Clear();
					}
	
					result = option;
					matches.Add(alias);
				}
			}
			
			if (result == null)
				throw new MalformedCommandLineException(arg + " is not a valid option");

			if (matches.Count > 1)
			{
				string mesg = string.Format("{0} matches the {1} options", arg, string.Join(", ", matches.ToArray()));
				throw new MalformedCommandLineException(mesg);
			}
				
			return result;
		}
		#endregion
		
		#region Private Types
		private class Option
		{	
			public Option(string key, string value, string help, bool requiresArg, params string[] aliaii)
			{
				m_key = key;
				m_default = value;
				m_requiresArg = requiresArg;
				m_help = help;
				m_aliaii = aliaii;
			}
			
			public void Reset()
			{
				m_found = false;
				m_values.Clear();
			}
			
			public int Match(string arg, out string a)
			{
				int count = DoMatch(m_key, arg);
				if (count > 0)
					a = m_key;
				else
					a = null;
				
				foreach (string alias in m_aliaii)
				{
					int temp = DoMatch(alias, arg);
					if (temp > count)
					{
						count = temp;
						a = alias;
					}
				}
				
				return count;
			}

			public bool Found
			{
				get {return m_found;}
				set {m_found = value;}
			}
			
			public string Key
			{
				get {return m_key;}
			}
			
			public string Help
			{
				get {return m_help;}
			}
			
			public bool RequiresArg
			{
				get {return m_requiresArg;}
			}
			
			public string Default
			{
				get {return m_default;}
			}
			
			public string[] Values
			{
				get 
				{
					if (m_found || m_default == null)
						return m_values.ToArray();
					else
						return new string[]{m_default};
				}
			}
			
			public string[] Names
			{
				get 
				{
					string[] names = new string[m_aliaii.Length + 1];
					names[0] = m_key;
					Array.Copy(m_aliaii, 0, names, 1, m_aliaii.Length);
					
					return names;
				}
			}
			
			public void AddValue(string value)
			{
				if (value != null && value.Length > 0)
				{
					if (m_requiresArg)
						m_values.Add(value);
					else
						throw new MalformedCommandLineException(m_key + " does not take an argument");
				}
				else
				{
					if (m_requiresArg)
						throw new MalformedCommandLineException(m_key + " requires an argument");	
				}
			}
			
			private static int DoMatch(string option, string arg)
			{
				if (option.StartsWith(arg))
					return arg.Length;
				else
					return 0;
			}
			
			private string m_key;
			private string m_default;
			private bool m_requiresArg;
			private string m_help;
			private string[] m_aliaii;
			
			private bool m_found;
			private List<string> m_values = new List<string>();
		}
		#endregion
		
		#region Fields
		private Dictionary<string, Option> m_options = new Dictionary<string, Option>();
		private List<string> m_operands = new List<string>();
		#endregion
	}
}
