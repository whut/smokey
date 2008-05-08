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
	/// <summary>Some helpful List related methods.</summary>
	public static class ListExtensions 
	{ 
		/// <summary>Returns a list of all the elements in source which match predicate.</summary>
		///
		/// <remarks>For example, to copy all the strings in a list that start with a 
		/// letter:
		///
		/// <code>List{string} result = names.CopyIf(delegate (string name) 
		/// {
		///     return name.Length &gt; 0 &amp;&amp; char.IsLetter(name[0]);
		/// });</code></remarks>
		public static List<T> CopyIf<T>(this List<T> source, Predicate<T> predicate) 
		{ 
			DBC.Pre(source != null, "source is null");
			DBC.Pre(predicate != null, "predicate is null");
			
			List<T> result = new List<T>(source.Count);
			
			foreach (T element in source)
			{
				if (predicate(element))
					result.Add(element);
			}
			
			return result;
		} 

		/// <summary>Returns a list of all the elements in source which do not match 
		/// predicate.</summary>
		///
		/// <remarks>For example, to remove all the strings in a list that
		/// start with a number:
		///
		/// <code>List{string} result = names.RemoveIf(delegate (string name) 
		/// {
		///     return name.Length &gt; 0 &amp;&amp; char.IsDigit(name[0]);
		/// });</code></remarks>
		public static List<T> RemoveIf<T>(this List<T> source, Predicate<T> predicate) 
		{ 
			DBC.Pre(source != null, "source is null");
			DBC.Pre(predicate != null, "predicate is null");

			List<T> result = new List<T>(source.Count);
			
			foreach (T element in source)
			{
				if (!predicate(element))
					result.Add(element);
			}
			
			return result;
		} 

		/// <summary>Sets state to initialState and for each element in the list
		/// sets state to the result of calling the callback with the current state
		/// and the element.</summary>
		///
		/// <remarks>For example, to convert a list of ints to a string:
		/// <code>string result = ints.Accumulate(string.Empty, (s, e) =&gt;
		/// s + e.ToString("X2") + " ");</code></remarks>
		public delegate S AccumulateCallback<S, T>(S state, T element);
		public static S Accumulate<S, T>(List<T> source, S initialstate, AccumulateCallback<S, T> callback) 
		{ 
			DBC.Pre(source != null, "source is null");
			DBC.Pre(callback != null, "callback is null");
			
			S state = initialstate;
			
			foreach (T element in source)
			{
				state = callback(state, element);
			}
			
			return state;
		} 

		/// <summary>Returns the first element in the list.</summary>
		public static T Front<T>(this List<T> list) 
		{ 
			DBC.Pre(list != null, "list is null");
			DBC.Pre(list.Count > 0, "list is empty");
			
			return list[0];
		} 

		/// <summary>Returns the last element in the list.</summary>
		public static T Back<T>(this List<T> list) 
		{ 
			DBC.Pre(list != null, "list is null");
			DBC.Pre(list.Count > 0, "list is empty");
			
			return list[list.Count - 1];
		} 
	}
} 
