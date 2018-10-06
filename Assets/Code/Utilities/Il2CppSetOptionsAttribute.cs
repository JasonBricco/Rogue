using System;

public enum Option
{
    NullChecks = 1,
    ArrayBoundsChecks = 2,
    DivideByZeroChecks = 3,
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
public class Il2CppSetOptionsAttribute : Attribute
{
    public Option Option { get; private set; }
    public object Value { get; private set; }

    public Il2CppSetOptionsAttribute(Option option, object value)
    {
        Option = option;
        Value = value;
    }
}
