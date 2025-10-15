namespace Silhouette.Interfaces;

[NativeObject]
internal interface ICorProfilerInfo15 : ICorProfilerInfo14
{
    public new static readonly Guid Guid = new("B446462D-BD22-41DD-872D-DC714C49EB56");

    /*
     * EnumerateGCHeapObjects is a method that iterates over each object in the GC heap.
     * For each object, it invokes the provided callback function which should return a bool
     * indicating whether or not enumeration should continue.
     * Enumerating the GC heap requires suspending the runtime. The profiler may accomplish this
     * by starting from a state where the runtime is not suspended and by doing one of:
     *
     * From the same thread,
     * Invoking ICorProfilerInfo10::SuspendRuntime()
     * ...
     * Invoking ICorProfilerInfo15::EnumerateGCHeapObjects()
     * ...
     * Invoking ICorProfilerInfo10::ResumeRuntime()
     *
     * or
     *
     * Invoke ICorProfilerInfo15::EnumerateGCHeapObjects() on its own, and leverage its
     * built-in runtime suspension logic.
     *
     * Parameters:
     * - callback: A function pointer to the callback function that will be invoked for each object in the GC heap.
     *            The callback function should accept an ObjectID and a void pointer as parameters and return a BOOL.
     * - callbackState: A void pointer that can be used to pass state information to the callback function.
     *
     * Returns:
     * - HRESULT: A code indicating the result of the operation. If the method succeeds,
     *           it returns S_OK. If it fails, it returns an error code.
     */
    HResult EnumerateGCHeapObjects(IntPtr callback, IntPtr callbackState);
}