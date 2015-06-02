## Introduction ##

You can disable rules using the Smokey command line, but it's often much nicer to disable them within the code itself using DisableRuleAttribute. To do this you need to define the attribute and then use it to decorate one of your types or methods.


## Defining the Attribute ##

Add the following to your assembly or export it from an assembly you reference.

```
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | 
AttributeTargets.Constructor | AttributeTargets.Enum | AttributeTargets.Interface | 
AttributeTargets.Method | AttributeTargets.Struct, AllowMultiple = true)]
internal sealed class DisableRuleAttribute : Attribute
{      
    public DisableRuleAttribute(string id, string name) 
    {
        Id = id;
        Name = name;
    }

    public string Id {get; private set;}
    public string Name {get; private set;}
}
```


## Using the Attribute ##

Then you can disable rules like so:

```
[DisableRule("P1008", "StructOverrides")]
[DisableRule("MS1019", "LargeStruct")]
[StructLayout(LayoutKind.Sequential, Pack = 2)]
internal struct ControlFontStyleRec    
{
    public Int16        flags;
    public Int16        font;
    public Int16        size;
    public Int16        style;
    public Int16        mode;
    public Int16        just;
    public QdColor      foreColor;
    public QdColor      backColor;
}
```