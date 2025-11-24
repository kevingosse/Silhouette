namespace Silhouette;

[AttributeUsage(AttributeTargets.Class)]
public class ProfilerAttribute : Attribute
{
    public ProfilerAttribute(string guid)
    {
    }
}
