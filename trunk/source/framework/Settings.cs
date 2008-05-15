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

namespace Smokey.Framework
{
	using Table = Dictionary<string, string>;
	
	/// <summary>Key/value pairs set via the config file or the command line. Used
	/// by some of the rules.</summary>
	public static class Settings
	{		
		/// <summary>Overwrites any existing entries.</summary>
		public static void Add(string key, string value)
		{
			DBC.Pre(!string.IsNullOrEmpty(key), "key is null or empty");
			
			ms_table[key] = value;
			
			DBC.Post(Has(key), "key wasn't added");
		}
		
		public static bool Has(string key)
		{
			DBC.Pre(!string.IsNullOrEmpty(key), "key is null or empty");

			return ms_table.ContainsKey(key);
		}
		
		/// <summary>If the key exists the associated value is returned. Otherwise
		/// backup is returned.</summary>
		public static string Get(string key, string backup)
		{
			DBC.Pre(!string.IsNullOrEmpty(key), "key is null or empty");

			string result;
			if (ms_table.TryGetValue(key, out result))
				return result;
			else
				ms_table.Add(key, backup);	// note that we have to do this to allow variable expansion to work in the xml
				
			return backup;
		}
		
		/// <summary>If the key exists the associated value is returned. Otherwise
		/// backup is returned.</summary>
		public static int Get(string key, int backup)
		{
			DBC.Pre(!string.IsNullOrEmpty(key), "key is null or empty");

			string result;
			if (ms_table.TryGetValue(key, out result))
				return int.Parse(result);
			else
				ms_table.Add(key, backup.ToString());
				
			return backup;
		}
		
		public static KeyValuePair<string, string>[] Entries(string prefix)
		{
			DBC.Pre(prefix != null, "prefix is null");

			List<KeyValuePair<string, string>> entries = new List<KeyValuePair<string, string>>();
			
			foreach (KeyValuePair<string, string> entry in ms_table)
			{
				if (entry.Key.StartsWith(prefix))
					entries.Add(entry);
			}
			
			return entries.ToArray();
		}
		
		#region Fields
		private static Table ms_table = new Table();
		#endregion
	}
}