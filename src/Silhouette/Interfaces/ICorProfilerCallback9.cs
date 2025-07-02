namespace Silhouette.Interfaces;

[NativeObject]
internal interface ICorProfilerCallback9 : ICorProfilerCallback8
{
    public new static readonly Guid Guid = Guid.Parse("27583EC3-C8F5-482F-8052-194B8CE4705A");

    // This event is triggered whenever a dynamic method is garbage collected
    // and subsequently unloaded.
    HResult DynamicMethodUnloaded(FunctionId functionId);
}