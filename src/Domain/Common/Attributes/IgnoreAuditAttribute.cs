namespace Domain.Common.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public sealed class IgnoreAuditAttribute : Attribute
{
}