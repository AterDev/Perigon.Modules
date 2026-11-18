namespace Perigon.AspNetCore.Attributes;

/// <summary>
/// Supplies the runtime target name for an object written to the system log.
/// Implement this interface when a log description uses a dynamic field value.
/// </summary>
public interface ILogDescriptionTarget
{
    string? LogTargetName { get; }
}
