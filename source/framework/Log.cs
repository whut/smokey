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
using Smokey.Internal;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Smokey.Framework
{
	/// <summary>Simple hierarchical logging class.</summary>
	/// 
	/// <remarks>Configuration is done via the smokey.exe.config file. 
	/// Something like the following works:
	///
	/// <code>&lt;?xml version = "1.0" encoding = "utf-8" ?&gt;
	/// &lt;configuration&gt;
	///     &lt;appSettings&gt;
	///         &lt;add key = "logfile" value = "/Users/jessejones/Source/Smokey/smokey.log"/&gt; &lt;!-- use a full path or nunit will drop the log file somewhere unexpected, or "stdout", or "stderr" --&gt;
	///         &lt;add key = "topic:object" value = "Info"/&gt;        &lt;!-- may be off, Error, Warning, Info, Trace, or Debug --&gt;
	///         &lt;add key = "topic:Rules" value = "Trace"/&gt;        &lt;!-- get more detailed logging for the rules --&gt;
	///     &lt;/appSettings&gt;
	/// &lt;/configuration&gt;</code>
	///
	/// <para>Log level is set using types. If the level isn't set for a type, its base
	/// classes are checked until we find one which is set or reach System.Object.
	/// The convention for logging using the object topic is to call a log method
	/// with true. If you don't have an instance you can pass null in for the 
	/// instance and cast it to the right type.</para></remarks>
    //
	// Didn't see a lot of C# logging classes when I googled, but there is log4net
	// which seems to have a lot of functionality.
	[DisableRule("C1026", "NoStaticRemove")]
	public static class Log
	{		
		/// <summary>Log message severity.</summary>
		public enum Level
		{
			Off,		
			Error, 		// component or method failed
			Warning,	// unexpected condition, but the operation can proceed
			Info,		// an important event (these should be used sparingly)
			Trace,		// detailed tracking of a component
			Debug,		// very detailed tracking, compiled out in release
		}
		
		/// <summary>Note that if this is called twice it has no effect.</summary>
		public static void Init()
		{
			Init(false);
		}
						
		public static void Init(bool append)
		{
			if (!ms_inited)
			{				
				ms_inited = true;
				
				try
				{
					string path = Settings.Get("logfile", null);
					
					if (path != null)
					{
						if (path == "stdout")
							ms_log = Console.Out;
						else if (path == "stderr")
							ms_log = Console.Error;
						else
							ms_log = new StreamWriter(path, append);
						ms_levels[typeof(object)] = Level.Warning;	// default level
						
						if (append)
							DoWriteLine(Level.Off, typeof(object), new string('-', 55));
						
						string prefix = "topic:";
						foreach (KeyValuePair<string, string> entry in Settings.Entries(prefix))
						{			
							DoAddLevel(entry.Key.Substring(prefix.Length), entry.Value);
						}
					}
				}
				catch (Exception e)
				{
					Console.Error.WriteLine("Couldn't initialize the logger: {0}", e.Message);
					ms_log = null;
				}
			}
		}
						
		public static void Flush()
		{
			if (ms_log != null)
				ms_log.Flush();
		}
				
		public static Level LogLevel<T>()
		{			
			return DoGetLevel(typeof(T));
		}

		public static bool AutoFlush
		{
			get {return ms_autoFlush;}
			set {ms_autoFlush = value;}
		}
		
		public static int NumErrors
		{
			get {return ms_numErrors;}
		}
		
		public static int NumWarnings
		{
			get {return ms_numWarnings;}
		}
		
		public static void Indent()
		{
			++ms_indentlevel;
			ms_indentText = new string(' ', 3*ms_indentlevel);
		}
		
		public static void Unindent()
		{
			--ms_indentlevel;
			ms_indentText = new string(' ', 3*ms_indentlevel);
		}
		
		public static void ErrorLine<T>(T instance)												{++ms_numErrors; DoWriteLine(Level.Error, DoGetType(instance));}
		public static void ErrorLine<T>(T instance, string arg1)								{++ms_numErrors; DoLog(Level.Error, DoGetType(instance), arg1);}
		public static void ErrorLine<T, A1>(T instance, A1 arg1)								{++ms_numErrors; DoLog(Level.Error, DoGetType(instance), arg1);}
		public static void ErrorLine<T, A1>(T instance, string format, A1 arg1)					{++ms_numErrors; DoLog(Level.Error, DoGetType(instance), format, arg1);}
		public static void ErrorLine<T, A1, A2>(T instance, string format, A1 arg1, A2 arg2)	{++ms_numErrors; DoLog(Level.Error, DoGetType(instance), format, arg1, arg2);}
		public static void ErrorLine<T>(T instance, string format, params object[] args)		{++ms_numErrors; DoLog(Level.Error, DoGetType(instance), format, args);}
				
		public static void WarningLine<T>(T instance)											{++ms_numWarnings; DoWriteLine(Level.Warning, DoGetType(instance));}
		public static void WarningLine<T>(T instance, string arg1)								{++ms_numWarnings; DoLog(Level.Warning, DoGetType(instance), arg1);}
		public static void WarningLine<T, A1>(T instance, A1 arg1)								{++ms_numWarnings; DoLog(Level.Warning, DoGetType(instance), arg1);}
		public static void WarningLine<T, A1>(T instance, string format, A1 arg1)				{++ms_numWarnings; DoLog(Level.Warning, DoGetType(instance), format, arg1);}
		public static void WarningLine<T, A1, A2>(T instance, string format, A1 arg1, A2 arg2)	{++ms_numWarnings; DoLog(Level.Warning, DoGetType(instance), format, arg1, arg2);}
		public static void WarningLine<T>(T instance, string format, params object[] args)		{++ms_numWarnings; DoLog(Level.Warning, DoGetType(instance), format, args);}
		
		public static void InfoLine<T>(T instance)												{DoWriteLine(Level.Info, DoGetType(instance));}
		public static void InfoLine<T>(T instance, string arg1)									{DoLog(Level.Info, DoGetType(instance), arg1);}
		public static void InfoLine<T, A1>(T instance, A1 arg1)									{DoLog(Level.Info, DoGetType(instance), arg1);}
		public static void InfoLine<T, A1>(T instance, string format, A1 arg1)					{DoLog(Level.Info, DoGetType(instance), format, arg1);}
		public static void InfoLine<T, A1, A2>(T instance, string format, A1 arg1, A2 arg2)		{DoLog(Level.Info, DoGetType(instance), format, arg1, arg2);}
		public static void InfoLine<T>(T instance, string format, params object[] args)			{DoLog(Level.Info, DoGetType(instance), format, args);}
		
		public static void TraceLine<T>(T instance)												{DoWriteLine(Level.Trace, DoGetType(instance));}
		public static void TraceLine<T>(T instance, string arg1)								{DoLog(Level.Trace, DoGetType(instance), arg1);}
		public static void TraceLine<T, A1>(T instance, A1 arg1)								{DoLog(Level.Trace, DoGetType(instance), arg1);}
		public static void TraceLine<T, A1>(T instance, string format, A1 arg1)					{DoLog(Level.Trace, DoGetType(instance), format, arg1);}
		public static void TraceLine<T, A1, A2>(T instance, string format, A1 arg1, A2 arg2)	{DoLog(Level.Trace, DoGetType(instance), format, arg1, arg2);}
		public static void TraceLine<T>(T instance, string format, params object[] args)		{DoLog(Level.Trace, DoGetType(instance), format, args);}

#if DEBUG					
		[Conditional("DEBUG")]
		public static void DebugLine<T>(T instance)												{DoWriteLine(Level.Debug, DoGetType(instance));}
		[Conditional("DEBUG")]
		public static void DebugLine<T>(T instance, string arg1)								{DoLog(Level.Debug, DoGetType(instance), arg1);}
		[Conditional("DEBUG")]
		public static void DebugLine<T, A1>(T instance, A1 arg1)								{DoLog(Level.Debug, DoGetType(instance), arg1);}
		[Conditional("DEBUG")]
		public static void DebugLine<T, A1>(T instance, string format, A1 arg1)					{DoLog(Level.Debug, DoGetType(instance), format, arg1);}
		[Conditional("DEBUG")]
		public static void DebugLine<T, A1, A2>(T instance, string format, A1 arg1, A2 arg2)	{DoLog(Level.Debug, DoGetType(instance), format, arg1, arg2);}
		[Conditional("DEBUG")]
		public static void DebugLine<T>(T instance, string format, params object[] args)		{DoLog(Level.Debug, DoGetType(instance), format, args);}
#else	// Conditional doesn't work on 1.2.5 with template methods...
		[Conditional("DEBUG")]
		public static void DebugLine(object instance)											{}
		[Conditional("DEBUG")]
		public static void DebugLine(object instance, string arg1)								{}
		[Conditional("DEBUG")]		
		public static void DebugLine(object instance, string format, params object[] args)		{}
#endif

		#region Private Methods	-----------------------------------------------
		// Converting arguments to strings can be quite expensive so we use these helpers to
		// avoid the conversion if we're not going to actually log anything.
		private static void DoLog(Level level, Type topic, string arg1)								
		{				
			Level currentLevel = DoGetLevel(topic);
			if (level <= currentLevel)
				DoWriteLine(level, topic, arg1);
		}
		
		private static void DoLog<A1>(Level level, Type topic, A1 arg1)								
		{
			Level currentLevel = DoGetLevel(topic);
			if (level <= currentLevel)
				DoWriteLine(level, topic, arg1.ToString());
		}
		
		private static void DoLog<A1>(Level level, Type topic, string format, A1 arg1)					
		{
			Level currentLevel = DoGetLevel(topic);
			if (level <= currentLevel)
				DoWriteLine(level, topic, string.Format(format, arg1));
		}
		
		private static void DoLog<A1, A2>(Level level, Type topic, string format, A1 arg1, A2 arg2)	
		{
			Level currentLevel = DoGetLevel(topic);
			if (level <= currentLevel)
				DoWriteLine(level, topic, string.Format(format, arg1, arg2));
		}

		private static void DoLog(Level level, Type topic, string format, params object[] args)		
		{
			Level currentLevel = DoGetLevel(topic);
			if (level <= currentLevel)
				DoWriteLine(level, topic, string.Format(format, args));
		}

		// These are the two methods that do the actual logging.
		private static void DoWriteLine(Level level, Type topic)
		{			
			Level currentLevel = DoGetLevel(topic);
			
			if (level <= currentLevel)
				ms_log.WriteLine(); 
		}
		
		private static void DoWriteLine(Level level, Type topic, string message)
		{			
			if (message.IndexOf(Environment.NewLine) >= 0)
			{
				if (ms_lineIndent == null)
					ms_lineIndent = new string(' ', 8 + 14 + 1);
					
				message = message.Replace(Environment.NewLine, Environment.NewLine + ms_lineIndent + ms_indentText);
			}
			
			string topicName = topic.Name;
			if (topicName.Length > 14)
				topicName = topicName.Substring(0, 6) + "-" + topicName.Substring(topicName.Length - 7);
			
			string line = string.Format("{0,-8}{1,-14}{2} {3}", level, topicName, ms_indentText, message);
			ms_log.WriteLine(line);
		
			if (ms_autoFlush)
				ms_log.Flush();
		}
		
		// Misc helpers.
		private static Type DoGetType<T>(T instance)
		{
			if (instance != null)
				return instance.GetType();		// use the dynamic type if possible
			else
				return typeof(T);
		}
		
		private static void DoAddLevel(string key, string value)
		{
			Type topic = DoGetType(key);
			Level level = Level.Warning;
			
			if (topic != null)
			{
				try
				{
					level = (Level) Enum.Parse(typeof(Level), value);
				}
				catch (ArgumentException)
				{
					Console.Error.WriteLine("{0} isn't a valid level (for topic {1})", value, key);
				}
	
				ms_levels[topic] = level;
				DoWriteLine(Level.Off, typeof(object), string.Format("set {0} level to {1}", topic.FullName, level));
			}
			else
				Console.Error.WriteLine("{0} isn't a valid type name", key);
		}

		private static Type DoGetType(string key)
		{
			Type topic = Type.GetType(key);
			
			if (topic == null && !key.Contains(","))
			{
				foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
				{
					topic = assembly.GetType(key);
					if (topic != null)
						break;
				}
			}
			
			return topic;
		}

		private static Level DoGetLevel(Type topic)
		{			
			Level level;
			
			if (!ms_levels.TryGetValue(topic, out level))
			{
				level = DoComputeLevel(topic);
				ms_levels[topic] = level;
			}
			
			return level;
		}

		private static Level DoComputeLevel(Type topic)
		{
			Level level = Level.Off;
			
			topic = topic.BaseType;
			while (topic != null && level == Level.Off)
			{
				Unused.Value = ms_levels.TryGetValue(topic, out level);
				topic = topic.BaseType;
			}
			
			return level;
		}
		#endregion

		#region Fields --------------------------------------------------------
		private static bool ms_inited;
		private static TextWriter ms_log;
		private static Dictionary<Type, Level> ms_levels = new Dictionary<Type, Level>();
		private static bool ms_autoFlush;
		private static int ms_indentlevel;
		private static string ms_indentText = string.Empty;
		private static string ms_lineIndent;
		private static int ms_numWarnings;
		private static int ms_numErrors;
		#endregion
	}
}