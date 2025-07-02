# Bug Report: Silhouette .NET Profiler Library

## Bug #1: HResult Error Handling Inconsistency (CRITICAL)
**File:** `src/Silhouette/HResult.cs`  
**Lines:** 15, 30, 52-57

### Issue
There's an inconsistency in how error codes are handled between the `IsOK` property and the implicit boolean operator:

- `IsOK` property only returns `true` for `S_OK` (line 15: `Code == S_OK`)
- Implicit boolean operator returns `true` for any success code (line 30: `Code >= 0`)
- `ThrowIfFailed()` method uses `IsOK`, causing it to throw exceptions for warning codes like `S_FALSE`

### Problem
This means `ThrowIfFailed()` will throw exceptions for success codes with warnings (like `S_FALSE`), which is incorrect behavior. `S_FALSE` is a valid success code that indicates "success but with additional information."

### Fix Required
Change the `ThrowIfFailed()` method to use the same logic as the implicit boolean operator:

```csharp
public void ThrowIfFailed()
{
    if (Code < 0) // Changed from !IsOK
    {
        throw new Win32Exception(this);
    }
}
```

---

## Bug #2: Incorrect Error Messages in Exception Handlers (HIGH)
**File:** `src/ManagedDotnetProfiler/CorProfiler.cs`  
**Lines:** 565, 597, 629

### Issue
Several exception handler methods have incorrect error messages that reference the wrong method names:

1. **Line 565** - `ExceptionSearchFunctionLeave()`:
   ```csharp
   Error($"ExceptionSearchFunctionLeave called without a matching ExceptionSearchFilterEnter");
   ```
   Should reference `ExceptionSearchFunctionEnter`, not `ExceptionSearchFilterEnter`.

2. **Line 597** - `ExceptionUnwindFinallyLeave()`:
   ```csharp
   Error($"ExceptionUnwindFinallyLeave called without a matching ExceptionSearchFilterEnter");
   ```
   Should reference `ExceptionUnwindFinallyEnter`, not `ExceptionSearchFilterEnter`.

3. **Line 629** - `ExceptionUnwindFunctionLeave()`:
   ```csharp
   Error($"ExceptionUnwindFunctionLeave called without a matching ExceptionSearchFilterEnter");
   ```
   Should reference `ExceptionUnwindFunctionEnter`, not `ExceptionSearchFilterEnter`.

### Problem
These incorrect error messages will confuse developers when debugging, making it harder to identify the actual mismatched method pairs.

---

## Bug #3: Copy-Paste Error in ExceptionOSHandlerLeave (MEDIUM)
**File:** `src/ManagedDotnetProfiler/CorProfiler.cs`  
**Line:** 513

### Issue
```csharp
protected override unsafe HResult ExceptionOSHandlerLeave(nint* _)
{
    Error("The profiling API never raises the event ExceptionOSHandlerEnter");
    return HResult.S_OK;
}
```

The error message says "ExceptionOSHandlerEnter" but should say "ExceptionOSHandlerLeave".

### Problem
This copy-paste error makes it unclear which method is actually never called by the profiling API.

---

## Bug #4: ClassFactory QueryInterface Returns Wrong Error Code (MEDIUM)
**File:** `src/Silhouette/ClassFactory.cs`  
**Line:** 32

### Issue
```csharp
public HResult QueryInterface(in Guid guid, out nint ptr)
{
    if (guid == Silhouette.Interfaces.IClassFactory.Guid)
    {
        ptr = IClassFactory;
        return HResult.S_OK;
    }

    ptr = nint.Zero;
    return HResult.E_NOTIMPL; // Should be E_NOINTERFACE
}
```

### Problem
According to COM conventions, `QueryInterface` should return `E_NOINTERFACE` when the requested interface is not supported, not `E_NOTIMPL`. The `E_NOTIMPL` error code indicates the method itself is not implemented.

### Fix Required
```csharp
return HResult.E_NOINTERFACE;
```

---

## Bug #5: Potential Race Condition in Garbage Collection Counting (LOW)
**File:** `src/ManagedDotnetProfiler/CorProfiler.cs`  
**Lines:** 701, 707

### Issue
While using `Interlocked.Increment` and `Interlocked.Decrement` for `_garbageCollectionsInProgress` is thread-safe, the check `if (count < 0)` after decrementing could theoretically be racy if there are more `GarbageCollectionFinished` calls than `GarbageCollectionStarted` calls.

### Problem
If the counter goes negative, it will stay negative, and subsequent calls will continue to report errors even if the calls become balanced again.

---

## Summary
- **1 Critical Bug**: HResult error handling that could cause incorrect exception throwing
- **3 High Priority Bugs**: Copy-paste errors in error messages that hinder debugging
- **1 Medium Priority Bug**: COM interface implementation not following conventions
- **1 Low Priority Bug**: Potential race condition in GC counting

The most critical bug is the HResult error handling inconsistency, as it could cause the profiler to fail unexpectedly when receiving valid warning codes from the .NET runtime.