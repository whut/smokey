using System;

namespace Smokey.Framework
{
	/// <summary>Used to disable Smokey rules. Note that assemblies can define
	/// this themselves: they don't need to reference the Smokey assembly.</summary>
	[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | 
		AttributeTargets.Constructor | AttributeTargets.Enum | AttributeTargets.Interface | 
		AttributeTargets.Method | AttributeTargets.Struct, AllowMultiple = true)]
	public sealed class DisableRuleAttribute : Attribute
	{		
		public DisableRuleAttribute(string id, string name) 
		{
			m_id = id;
			m_name = name;
		}
		
		public string ID
		{
			get {return m_id;}
		}
	
		public string Name
		{
			get {return m_name;}
		}
	
		private string m_id;
		private string m_name;
	}
}