namespace Silhouette;

/// <summary>
/// Marks a class for source generation of the DllGetClassObject export.
/// The class is expected to inherit from CorProfilerCallbackBase.
/// </summary>
/// <remarks>
/// Usage example:
/// <code>
/// [Profiler("12345678-1234-1234-1234-123456789abc")]
/// public class MyProfiler : CorProfilerCallback11Base { }
/// </code>
/// </remarks>
[AttributeUsage(AttributeTargets.Class)]
public class ProfilerAttribute : Attribute
{
    public ProfilerAttribute(string guid)
    {
        Guid = guid;
    }

    public string Guid { get; }
}
