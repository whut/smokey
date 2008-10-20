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
using System;

namespace Smokey.Framework.Support
{				
	// ------------------------------------------------------------------------
	/// <summary>RuleDispatcher calls this before anything else.</summary>
	public sealed class BeginTesting  {}

	/// <summary>RuleDispatcher calls this after everything else.</summary>
	public sealed class EndTesting  {}

	// ------------------------------------------------------------------------
	/// <summary>RuleDispatcher calls this before visiting types.</summary>
	public sealed class BeginTypes  {}

	/// <summary>RuleDispatcher calls this after visiting types.</summary>
	public sealed class EndTypes  {}

	// ------------------------------------------------------------------------
	/// <summary>RuleDispatcher calls this before visiting a type.</summary>
	public sealed class BeginType 
	{
		public TypeDefinition Type 	{get {return m_type;} internal set {m_type = value;}}
		
		private TypeDefinition m_type;
	}

	/// <summary>RuleDispatcher calls this after visiting a type.</summary>
	public sealed class EndType 
	{
		public TypeDefinition Type 	{get {return m_type;} internal set {m_type = value;}}
		
		private TypeDefinition m_type;
	}

	// ------------------------------------------------------------------------
	/// <summary>RuleDispatcher calls this before visiting methods.</summary>
	public sealed class BeginMethods
	{
		public BeginMethods(TypeDefinition type) {m_type = type;}
		
		public TypeDefinition Type 	{get {return m_type;}}
		
		private TypeDefinition m_type;
	}

	/// <summary>RuleDispatcher calls this after visiting methods.</summary>
	public sealed class EndMethods
	{
		public EndMethods(TypeDefinition type) {m_type = type;}
		
		public TypeDefinition Type 	{get {return m_type;}}
		
		private TypeDefinition m_type;
	}

	/// <summary>RuleDispatcher calls this before visiting a method.</summary>
	public sealed class BeginMethod 
	{
		public MethodInfo Info 	{get {return m_info;} internal set {m_info = value;}}
		
		private MethodInfo m_info;
	}

	/// <summary>RuleDispatcher calls this after visiting a method.</summary>
	public sealed class EndMethod 
	{
		public MethodInfo Info 	{get {return m_info;} internal set {m_info = value;}}
		
		private MethodInfo m_info;
	}
}

