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
			Id = id;
			Name = name;
		}
		
		public string Id {get; private set;}
		public string Name {get; private set;}
	}
}