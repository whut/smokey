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
using System.Collections;

namespace EvilDoer
{
	// D1006/ImplementGenericCollection
	// D1016/TypedList
	public class EmptyCollection : IList
	{
		// IEnumerable
		public IEnumerator GetEnumerator()
		{	
			return null;
		}
		
		// ICollection
		public int Count 
		{
			get {return 0;}
		}

		public bool IsSynchronized 
		{ 
			get {return false;}
		}

		public object SyncRoot 
		{ 
			get {return null;}
		}

		public void CopyTo(Array array, int index)
		{
		}
		
		// IList
		public bool IsFixedSize 
		{ 
			get {return true;}
		}

		public bool IsReadOnly 
		{ 
			get {return true;}
		}

		public object this[int index] 
		{ 
			get {return null;}
			set {Unused.Value = index; Unused.Value = value;}
		}

		public int Add(object value)
		{
			return -1;
		}

		public void Clear()
		{
		}

		public bool Contains(object value)
		{
			return false;
		}

		public int IndexOf(object value)
		{
			return -1;
		}

		public void Insert(int index, object value)
		{
		}

		public void Remove(object value)
		{
		}

		public void RemoveAt(int index)
		{
		}
	}

	// D1017/TypedDictionary
	public class EmptyDictionary : IDictionary
	{
		// IEnumerable
		IEnumerator IEnumerable.GetEnumerator()
		{	
			return null;
		}
		
		// ICollection
		public int Count 
		{
			get {return 0;}
		}

		public bool IsSynchronized 
		{ 
			get {return false;}
		}

		public object SyncRoot 
		{ 
			get {return null;}
		}

		public void CopyTo(Array array, int index)
		{
		}
		
		// IDictionary
		public bool IsFixedSize 
		{ 
			get {return true;}
		}

		public bool IsReadOnly 
		{ 
			get {return true;}
		}

		public object this[object key] 
		{ 
			get {return null;}
			set {Unused.Value = key; Unused.Value = value;}
		}

		public ICollection Keys 
		{ 
			get {return null;}
		}

		public ICollection Values 
		{ 
			get {return null;}
		}

		public void Add(object key, object value)
		{
		}

		public void Clear()
		{
		}

		public bool Contains(object key)
		{
			return false;
		}

		public IDictionaryEnumerator GetEnumerator()
		{
			return null;
		}

		public void Remove(object key)
		{
		}
	}
}