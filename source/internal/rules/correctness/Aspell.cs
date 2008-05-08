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

using Smokey.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace Smokey.Internal.Rules
{
	using AspellConfig = IntPtr;
	using AspellCanHaveError = IntPtr;
	using AspellSpeller = IntPtr;

	internal class AspellException : InvalidOperationException
	{
		public AspellException(string mesg) : base(mesg)
		{
		}

		protected AspellException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
	
	internal class Aspell 
	{
		// If Aspell can't load this will throw an exception the first
		// time. Thereafter it will return null.
		public static Aspell Instance
		{
			get 
			{
				if (ms_instance == null && !ms_tried)
				{
					ms_tried = true;
					ms_instance = new Aspell();
				}
				
				return ms_instance;
			}
		}

		// Returns a list of misspelled words in the text.		
		public List<string> Check(string text)
		{
			List<string> words = new List<string>();
			
			int index = 0;
			while (index < text.Length)
			{
				if (DoIsWordStart(text, index))
				{
					int count = DoGetWordLength(text, index);
					string word = text.Substring(index, count);
					index += count;
					
					count = DoSkipMarkup(text, index);
					if (count == 0)
					{
						word = word.Trim('\'', '-');
						if (words.IndexOf(word) < 0 && !CheckWord(word))
							words.Add(word);
					}
					else
					{
						index += count;
					}
				}
				else
				{
					int count = Math.Max(1, DoSkipMarkup(text, index));
					index += count;
				}
			}
			
			return words;
		}
		
		// Returns true if the word is spelled OK.		
		public bool CheckWord(string word)
		{
			// If the word is mixed case or has numbers call it good.
			for (int i = 1; i < word.Length; ++i)
			{
				if (char.IsUpper(word[i]) || char.IsNumber(word[i]))
					return true;
			}
			
			// Or if it's in our ignore list.
			int index = m_ignore.BinarySearch(word);
			if (index >= 0)
				return true;
			
			int result = NativeMethods.aspell_speller_check(m_speller, word, -1);
	        GC.KeepAlive(this);
			return result == 1;
		}
				
		#region Private methods
		~Aspell()
		{
			if (m_speller != IntPtr.Zero)
				NativeMethods.delete_aspell_speller(m_speller);
		}
		
		private Aspell()
		{
			AspellConfig config = NativeMethods.new_aspell_config();
			
			AspellCanHaveError result = NativeMethods.new_aspell_speller(config);
			if (NativeMethods.aspell_error_number(result) != 0)
			{
				IntPtr ptr = NativeMethods.aspell_error_message(result);
				throw new AspellException(Marshal.PtrToStringAnsi(ptr));
			}
			
			m_speller = NativeMethods.to_aspell_speller(result);
				
			m_ignore.Add("exe");		// make sure this list stays sorted
			m_ignore.Add("mdb");
			m_ignore.Add("stdout");
			m_ignore.Add("xml");
			
			string path = Settings.Get("ignoreList", string.Empty);
			if (path.Length > 0)
				DoAddIgnoreFile(path);
		}
		
		private static int DoSkipMarkup(string text, int index)
		{
			int count = 0;
			
			if (index < text.Length && text[index] == '<')
			{
				while (index + count < text.Length && text[index + count] != '>')
				{
					++count;
				}

				if (index + count < text.Length && text[index + count] == '>')
					++count;
				else
					count = 0;
			}
			
			return count;
		}
		
		private void DoAddIgnoreFile(string path)
		{
			using (StreamReader stream = new StreamReader(path)) 
			{
				string text = stream.ReadToEnd();
				string[] words = text.Split();
				foreach (string word in words)
				{
					if (word.Length > 0)
					{
						int i = m_ignore.BinarySearch(word);
						if (i < 0)
							m_ignore.Insert(~i, word);
					}
				}
			}	
		}
		
		private static bool DoIsWordStart(string text, int index)
		{
			char ch = text[index];
			switch (char.GetUnicodeCategory(ch))
			{
				case UnicodeCategory.LowercaseLetter:
				case UnicodeCategory.TitlecaseLetter:
				case UnicodeCategory.UppercaseLetter:
					return true;
			}
			
			return false;
		}

		private static int DoGetWordLength(string text, int index)
		{
			int count = 0;
			
			while (index + count < text.Length)
			{
				char ch = text[index + count];
				switch (char.GetUnicodeCategory(ch))
				{
					case UnicodeCategory.DashPunctuation:
					case UnicodeCategory.DecimalDigitNumber:
					case UnicodeCategory.LowercaseLetter:
					case UnicodeCategory.NonSpacingMark:
					case UnicodeCategory.TitlecaseLetter:
					case UnicodeCategory.UppercaseLetter:
						++count;
						break;
						
					case UnicodeCategory.OtherPunctuation:
						if (ch == '\'')
							++count;
						else
							return count;
						break;
						
					default:
						return count;
				}
			}
			
			return count;
		}
		#endregion	
		
		#region Fields
		private AspellSpeller m_speller;
		private List<string> m_ignore = new List<string>();
		
		static Aspell ms_instance;
		static bool ms_tried;
		#endregion
	} 
}
