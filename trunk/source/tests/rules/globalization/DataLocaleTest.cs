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
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using Smokey.Framework.Support;
using Smokey.Internal.Rules;

namespace Smokey.Tests
{
	[TestFixture]
	public class DataLocaleTest : MethodTest
	{	
		#region Test classes
		private class Cases
		{
			public System.Data.DataTable GoodCase1()
			{
				System.Data.DataTable table = new System.Data.DataTable("Customers");
				table.Locale = CultureInfo.InvariantCulture;
				
				System.Data.DataColumn keyColumn = table.Columns.Add("ID", typeof(int));
				keyColumn.AllowDBNull = false;
				keyColumn.Unique = true;
				
				table.Columns.Add("LastName", typeof(string));
				table.Columns.Add("FirstName", typeof(string));

				return table;
			}
			
			public System.Data.DataSet GoodCase2()
			{
				System.Data.DataSet table = new System.Data.DataSet("Customers");
				table.Locale = CultureInfo.InvariantCulture;
				
				return table;
			}
			
			public System.Data.DataTable GoodCase3(System.Data.DataTable table)
			{				
				System.Data.DataColumn keyColumn = table.Columns.Add("ID", typeof(int));
				keyColumn.AllowDBNull = false;
				keyColumn.Unique = true;
				
				table.Columns.Add("LastName", typeof(string));
				table.Columns.Add("FirstName", typeof(string));

				return table;
			}
			
			public System.Data.DataTable BadCase1()
			{
				System.Data.DataTable table = new System.Data.DataTable("Customers");
				
				System.Data.DataColumn keyColumn = table.Columns.Add("ID", typeof(int));
				keyColumn.AllowDBNull = false;
				keyColumn.Unique = true;
				
				table.Columns.Add("LastName", typeof(string));
				table.Columns.Add("FirstName", typeof(string));

				return table;
			}
			
			public System.Data.DataSet BadCase2()
			{
				System.Data.DataSet table = new System.Data.DataSet("Customers");
				
				return table;
			}
			
			public System.Data.DataSet BadCase3()
			{
				System.Data.DataSet table = new System.Data.DataSet();
				
				return table;
			}
		}		
		#endregion
		
		// test code
		public DataLocaleTest() : base(
			new string[]{"Cases.GoodCase1", "Cases.GoodCase2", "Cases.GoodCase3"},
			new string[]{"Cases.BadCase1", "Cases.BadCase2", "Cases.BadCase3"})	
		{
		}
						
		protected override Rule OnCreate(AssemblyCache cache, IReportViolations reporter)
		{
			return new DataLocaleRule(cache, reporter);
		}
	} 
}

