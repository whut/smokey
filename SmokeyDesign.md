# Overview #

Originally Smokey followed Gendarme's lead and rules subclassed either TypeRule, MethodRule, or AssemblyRule. This was nice and simple but it lead to serious scaling issues because the method rules each iterated over the instructions in methods.

In order to reduce duplicate work and improve data locality Smokey now uses a simplified version of the [visitor pattern](http://en.wikipedia.org/wiki/Visitor_pattern). There is now one rule base class and subclasses register the objects they want to visit with the RuleDispatcher.

Here's an example of a simple rule:

```
// Fail if MessageBox was used without MessageBoxOptions (this is a localization
// issue because MessageBoxOptions allows you to specify text direction).
internal class MessageBoxOptionsRule : Rule
{                
    public MessageBoxOptionsRule(AssemblyCache cache, IReportViolations reporter) 
        : base(cache, reporter, "G1001")
    {
    }
            
    public override void Register(RuleDispatcher dispatcher) 
    {
        dispatcher.Register(this, "VisitBegin");
        dispatcher.Register(this, "VisitCall");
    }
    
    // Called when RuleDispatcher begins visiting a method.
    public void VisitBegin(BeginMethod begin)
    {
        m_method = begin.Info.Method;
    }
    
    // Called when RuleDispatcher visits a call instruction.
    public void VisitCall(Call call)
    {
        if (call.Target.ToString().Contains("System.Windows.Forms.MessageBox::Show"))
        {
            if (!call.ToString().Contains("System.Windows.Forms.MessageBoxOptions"))
            {
                Reporter.MethodFailed(m_method, CheckID, call.Untyped.Offset, string.Empty);
            }
        }
    }
    
    MethodDefinition m_method;
}
```


# Namespaces #

## App ##

This is a small namespace which contains public AnalyzeAssembly, Error, and Violation classes. These classes allow tools to smoke assemblies without using System.Diagnostics.Process.

## Framework ##

The framework namespace contains classes which are used by Smokey itself and may also be used by rules. Examples include the Log and Settings Classes.

## Framework.Instructions ##

This namespace contains typed wrappers around the [Cecil](http://www.mono-project.com/Cecil) Instruction class (Cecil is the mono assembly reader and writer tool). These typed wrappers allow RuleDispatcher to dispatch via the types and are substantially easier to use than the raw Instruction type.

For example Smokey's LoadArg type represents the Ldarg\_0, Ldarg\_1, Ldarg\_2, and Ldarg\_3, Ldarg, and Ldarg\_S opcodes and provides Arg, Name, and Type properties which are rather painful to retrieve from the Cecil instruction.

## Framework.Support ##

This namespace contains the classes which are commonly used by rules. The key classes are:
  * Rule - this is the base class for rules. It has a few properties the most important of which allows rules to access the AssemblyCache.
  * RuleDispatcher - rules register their methods with this class to visit things like assemblies, types, fields, events, properties, methods, and instructions.
  * AssemblyCache - this caches information about the assembly being tested and the assemblies it depends upon. This is most commonly used to find the definition of types that are referenced in the assembly being tested.
  * CecilExtensions - extension methods for various Cecil classes. This includes things like checking for DisableRuleAttribute, finding where methods were first declared, and subclass testing.

## Framework.Support.Advanced ##

This contains classes which most rules will not have to use. The key classes are:
  * Values.Tracker - this class contains the values of arguments, locals, and the stack at each instruction in a method. The values are of type long? and may be 0 for 0/0.0/null, 1 for non-zero, and null for unknown.
  * CallGraph - a table of all the methods each method in the assembly being tested calls.

## Internal ##

This namespace contains classes which are not used by rules or external driver tools. It includes the classes used to emit the various report types, options processing, and the xml rule parser.

## Internal.Rules ##

This namespace contains the built-in Smokey rules. Note that it's also possible to write CustomRules.

## Tests ##

This namespace contains [nunit](http://www.nunit.org/index.php) unit tests for the built-in rules. There are additional functional tests which test Smokey as a whole and those rules which are not easy to test via a unit test.


# XML #

Each rule must be accompanied by an associated Violation element in an xml file (these files are resources in the Smokey or custom assemblies). Rules are uniquely defined by a checkID. The XML of the rule above looks like this:

```
<Violation checkID = "G1001" severity = "Warning" breaking = "false">
    <Translation lang = "en" typeName = "MessageBoxOptions" category = "Globalization">            
        <Cause>
        A method calls System.Windows.Forms.MessageBox.Show without specifying MessageBoxOptions.
        </Cause>

        <Description>
        To display the text properly for cultures that use a right to left reading order the RightAlign 
        and RtlReading members of MessageBoxOptions must be used.
        </Description>

        <Fix>
        Use the Show overload that takes a MessageBoxOptions and set RightAlign and RtlReading 
        using the containing control.
        </Fix>

        <CSharp>
        using System.Globalization;
        using System.Windows.Forms;
        
        public static class Dialogs
        {                
            public static DialogResult ShowMessageBox(Control owner, string text, string caption)
            {
                MessageBoxOptions options = 0;
                if (owner != null)
                {
                    if (owner.RightToLeft == RightToLeft.Yes)
                        options |= MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign;
                }
                else if (CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft)
                    options |= MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign;
                
                return MessageBox.Show(
                    owner, 
                    text, 
                    caption,
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Information, 
                    MessageBoxDefaultButton.Button1, 
                    options);
            }
        }
        </CSharp>
    </Translation>
</Violation>
```