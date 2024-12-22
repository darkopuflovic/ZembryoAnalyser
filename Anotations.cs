using System;

namespace ZembryoAnalyser;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class NotifyPropertyChangedInvocatorAttribute : Attribute
{
    public NotifyPropertyChangedInvocatorAttribute() { }
    public NotifyPropertyChangedInvocatorAttribute(string parameterName)
    {
        ParameterName = parameterName;
    }

    [UsedImplicitly]
    public string ParameterName { get; private set; }
}

[AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
[method: UsedImplicitly]
public sealed class UsedImplicitlyAttribute(ImplicitUseKindFlags useKindFlags, ImplicitUseTargetFlags targetFlags) : Attribute
{
    [UsedImplicitly]
    public UsedImplicitlyAttribute() : this(ImplicitUseKindFlags.Default, ImplicitUseTargetFlags.Default) { }

    [UsedImplicitly]
    public UsedImplicitlyAttribute(ImplicitUseKindFlags useKindFlags) : this(useKindFlags, ImplicitUseTargetFlags.Default) { }

    [UsedImplicitly]
    public UsedImplicitlyAttribute(ImplicitUseTargetFlags targetFlags) : this(ImplicitUseKindFlags.Default, targetFlags) { }

    [UsedImplicitly]
    public ImplicitUseKindFlags UseKindFlags { get; private set; } = useKindFlags;

    [UsedImplicitly]
    public ImplicitUseTargetFlags TargetFlags { get; private set; } = targetFlags;
}

[Flags]
public enum ImplicitUseTargetFlags
{
    Default = 1,
    Itself = Default,
    Members = 2,
    WithMembers = Itself | Members
}

[Flags]
public enum ImplicitUseKindFlags
{
    Default = Access | Assign | InstantiatedWithFixedConstructorSignature,
    Access = 1,
    Assign = 2,
    InstantiatedWithFixedConstructorSignature = 4,
    InstantiatedNoFixedConstructorSignature = 8,
}
