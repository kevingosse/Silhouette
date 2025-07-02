namespace Silhouette.Interfaces;

[NativeObject]
internal interface ICorProfilerCallback3 : ICorProfilerCallback2
{
    public new static readonly Guid Guid = Guid.Parse("4FD2ED52-7731-4b8d-9469-03D2CC3086C5");
    HResult InitializeForAttach(
        nint pCorProfilerInfoUnk,
        nint pvClientData,
        uint cbClientData);

    HResult ProfilerAttachComplete();

    HResult ProfilerDetachSucceeded();
}