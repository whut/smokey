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
using Mono.Cecil.Cil;

using Smokey.Framework;
using Smokey.Framework.Support;

using System;
using System.Net;
using System.Reflection;

namespace Smokey.Internal.Rules
{		
	[DisableRule("R1000", "DisposableFields")]
	internal sealed class DownloadFile
	{
		public DownloadFile()
		{
			m_client.DownloadStringCompleted += this.DoCompleted;
		}
		
		public void BeginDownload(string addr)
		{			
			try
			{
				Uri uri = new Uri(addr);
				m_client.DownloadStringAsync(uri);
			}
			catch (Exception e)
			{
				Log.ErrorLine(this, "version download failed: {0}", e);		
			}
		}
		
		public bool Complete
		{
			get 
			{
				lock (m_lock)
				{
					return m_complete;
				}
			}
		}
		
		public string Contents
		{
			get 
			{
				DBC.Pre(Complete, "download isn't complete");
				
				lock (m_lock)
				{
					return m_contents;
				}
			}
		}
		
		public void Cancel()
		{
			m_client.CancelAsync();
		}
		
		private void DoCompleted(object sender, DownloadStringCompletedEventArgs e)
		{
			Unused.Arg(sender);
			
			lock (m_lock)
			{
				m_contents = e.Result;
				m_complete = true;
			}
		}
		
		private WebClient m_client = new WebClient();
		private object m_lock = new object();
			private bool m_complete;
			private string m_contents;
	}
	
	internal sealed class NewVersionRule : Rule
	{				
		public NewVersionRule(AssemblyCache cache, IReportViolations reporter) 
			: base(cache, reporter, "M1003")
		{
		}
				
		public override void Register(RuleDispatcher dispatcher) 
		{
			dispatcher.Register(this, "VisitBegin");
			dispatcher.Register(this, "VisitEnd");
		}
				
		public void VisitBegin(BeginTesting begin)
		{		
			Unused.Arg(begin);
			
			Log.DebugLine(this, "starting download");	
			
			m_startTime = DateTime.Now;
			m_downloader.BeginDownload("http://home.comcast.net/~jesse98/public/SmokeyVersion");
		}		
		
		public void VisitEnd(EndTesting end)
		{			
			Unused.Arg(end);
			
			if (m_downloader.Complete)
			{
				string[] parts = m_downloader.Contents.Trim().Split('.');
				int newMajor, newMinor;
				
				if (parts.Length == 2 && int.TryParse(parts[0], out newMajor) && int.TryParse(parts[1], out newMinor))
				{
					Version installed = Assembly.GetExecutingAssembly().GetName().Version;
	
					if (newMajor > installed.Major || (newMajor == installed.Major && newMinor > installed.Minor))
					{
						string details = string.Empty;
						details += "Latest Version:    " + m_downloader.Contents + Environment.NewLine;
						details += "Installed Version: " + installed.Major + "." + installed.Minor;
						Log.DebugLine(this, details);
						
						Reporter.AssemblyFailed(Cache.Assembly, CheckID, details);
					}
					else
					{
						Log.DebugLine(this, "installed version is OK");		
						Log.DebugLine(this, "installed: {0}", installed);		
						Log.DebugLine(this, "latest: {0}.{1}", newMajor, newMinor);		
					}
				}
				else
				{
					Log.ErrorLine(this, "Bad download: '{0}'", m_downloader.Contents);		
				}
			}
			else
			{
				TimeSpan duration = DateTime.Now - m_startTime;
				Log.WarningLine(this, "NewVersionRule wasn't able to download the version number within {0:0.000} seconds", duration.TotalSeconds);		

				m_downloader.Cancel();
			}
		}		
		
		private DownloadFile m_downloader = new DownloadFile();
		private DateTime m_startTime;
	}
}

