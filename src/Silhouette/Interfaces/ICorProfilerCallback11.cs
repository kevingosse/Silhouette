namespace Silhouette.Interfaces;

[NativeObject]
internal interface ICorProfilerCallback11 : ICorProfilerCallback10
{
    public new static readonly Guid Guid = Guid.Parse("42350846-AAED-47F7-B128-FD0C98881CDE");

    HResult LoadAsNotificationOnly(out int pbNotificationOnly);
}