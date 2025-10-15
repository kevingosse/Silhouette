using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming
// ReSharper disable UnassignedReadonlyField
#pragma warning disable CA1069

namespace Silhouette;

public readonly record struct ModuleId(nint Value)
{
    public override string ToString() => Value.ToString("x2");
}

public readonly record struct ObjectId(nint Value)
{
    public override string ToString() => Value.ToString("x2");
}

public readonly record struct GCHandleId(nint Value)
{
    public override string ToString() => Value.ToString("x2");
}

public readonly record struct AppDomainId(nint Value)
{
    public override string ToString() => Value.ToString("x2");
}

public readonly record struct AssemblyId(nint Value)
{
    public override string ToString() => Value.ToString("x2");
}

public readonly record struct ClassId(nint Value)
{
    public override string ToString() => Value.ToString("x2");
}

public readonly record struct FunctionId(nint Value)
{
    public override string ToString() => Value.ToString("x2");
}

public readonly record struct ReJITId(nint Value)
{
    public override string ToString() => Value.ToString("x2");
}

public readonly record struct ThreadId(nuint Value)
{
    public override string ToString() => Value.ToString("x2");
}

public readonly record struct ProcessId(nint Value)
{
    public override string ToString() => Value.ToString("x2");
}

public readonly record struct ContextId(nint Value)
{
    public override string ToString() => Value.ToString("x2");
}

public readonly record struct MdToken(int Value)
{
    public static implicit operator int(MdToken token) => token.Value;
    public static implicit operator MdToken(int value) => new(value);

    public bool IsTypeRef() => (Value & 0xFF000000) == 0x01000000;
    public bool IsTypeDef() => (Value & 0xFF000000) == 0x02000000;

    public override string ToString() => Value.ToString("x2");
}

public readonly record struct MdModule(MdToken Token)
{
    public int Value => Token.Value;
    public override string ToString() => Value.ToString("x2");
}

public readonly record struct MdTypeDef(MdToken Token)
{
    public int Value => Token.Value;
    public override string ToString() => Value.ToString("x2");
}
public readonly record struct MdMethodDef(MdToken Token)
{
    public int Value => Token.Value;
    public override string ToString() => Value.ToString("x2");
}

public readonly record struct MdFieldDef(MdToken Token)
{
    public int Value => Token.Value;
    public override string ToString() => Value.ToString("x2");
}

public readonly record struct MdInterfaceImpl(MdToken Token)
{
    public int Value => Token.Value;
    public override string ToString() => Value.ToString("x2");
}

public readonly record struct MdTypeRef(MdToken Token)
{
    public int Value => Token.Value;
    public override string ToString() => Value.ToString("x2");
}

public readonly record struct MdParamDef(MdToken Token)
{
    public int Value => Token.Value;
    public override string ToString() => Value.ToString("x2");
}

public readonly record struct MdMemberRef(MdToken Token)
{
    public int Value => Token.Value;
    public override string ToString() => Value.ToString("x2");
}

public readonly record struct MdPermission(MdToken Token)
{
    public int Value => Token.Value;
    public override string ToString() => Value.ToString("x2");
}

public readonly record struct MdProperty(MdToken Token)
{
    public int Value => Token.Value;
    public override string ToString() => Value.ToString("x2");
}

public readonly record struct MdEvent(MdToken Token)
{
    public int Value => Token.Value;
    public override string ToString() => Value.ToString("x2");
}

public readonly record struct MdSignature(MdToken Token)
{
    public int Value => Token.Value;
    public override string ToString() => Value.ToString("x2");
}

public readonly record struct MdModuleRef(MdToken Token)
{
    public int Value => Token.Value;
    public override string ToString() => Value.ToString("x2");
}

public readonly record struct MdTypeSpec(MdToken Token)
{
    public int Value => Token.Value;
    public override string ToString() => Value.ToString("x2");
}

public readonly record struct MdString(MdToken Token)
{
    public int Value => Token.Value;
    public override string ToString() => Value.ToString("x2");
}

public readonly record struct MdCustomAttribute(MdToken Token)
{
    public int Value => Token.Value;
    public override string ToString() => Value.ToString("x2");
}

public readonly record struct MdGenericParam(MdToken Token)
{
    public int Value => Token.Value;
    public override string ToString() => Value.ToString("x2");
}

public readonly record struct MdMethodSpec(MdToken Token)
{
    public int Value => Token.Value;
    public override string ToString() => Value.ToString("x2");
}

public readonly struct MdGenericParamConstraint(MdToken Token)
{
    public int Value => Token.Value;
    public override string ToString() => Value.ToString("x2");
}

public readonly struct CorElementType
{
    public readonly uint Value;
}

public readonly struct HCORENUM
{
    public readonly nint Value;
}

public readonly struct COR_FIELD_OFFSET
{
    public readonly MdFieldDef RidOfField;
    public readonly uint UlOffset;
}

public readonly struct CorIlMap
{
    public readonly uint OldOffset;
    public readonly uint NewOffset;
    public readonly bool fAccurate;
}

public readonly struct CorDebugIlToNativeMap
{
    public readonly uint IlOffset;
    public readonly uint NativeStartOffset;
    public readonly uint NativeEndOffset;
}

/// <summary>
/// Represents a IL methods uniquely by combining the module ID and method token.
/// </summary>
public readonly struct COR_PRF_METHOD
{
    public readonly ModuleId ModuleId;
    public readonly MdMethodDef MethodId;
}

public readonly struct COR_PRF_FRAME_INFO
{
    public readonly nint Value;
}

public readonly struct COR_PRF_CODE_INFO
{
    public readonly nint StartAddress;
    public readonly nint Size;
}

public readonly struct COR_PRF_GC_GENERATION_RANGE
{
    public readonly COR_PRF_GC_GENERATION generation;
    public readonly ObjectId RangeStart;
    public readonly nint RangeLength;
    public readonly nint RangeLengthReserved;
}

public readonly struct COR_PRF_EX_CLAUSE_INFO
{
    public readonly COR_PRF_CLAUSE_TYPE ClauseType;
    public readonly nint ProgramCounter;
    public readonly nint FramePointer;
    public readonly nint ShadowStackPointer;
}

public readonly struct COR_PRF_ELT_INFO
{
    public readonly nint Value;
}

public readonly struct COR_PRF_FUNCTION
{
    public readonly FunctionId FunctionId;
    public readonly ReJITId ReJitId;
}

public readonly struct COR_PRF_FUNCTION_ARGUMENT_INFO
{
    public readonly uint NumRanges;                // number of chunks of arguments
    public readonly uint TotalArgumentSize;    // total size of arguments
    public readonly COR_PRF_FUNCTION_ARGUMENT_RANGE range1;
    public readonly COR_PRF_FUNCTION_ARGUMENT_RANGE range2;
}

public readonly struct COR_PRF_FUNCTION_ARGUMENT_RANGE
{
    /// <summary>
    /// Start address of the range
    /// </summary>
    public readonly nint StartAddress;

    /// <summary>
    /// Contiguous length of the range
    /// </summary>
    public readonly uint Length;
}

public struct COR_IL_MAP
{
    /// <summary>
    /// Old IL offset relative to beginning of function
    /// </summary>
    public uint oldOffset;

    /// <summary>
    /// New IL offset relative to beginning of function
    /// </summary>
    public uint newOffset;

    /// <summary>
    /// Put here for compatibility with the Debugger structure.
    /// </summary>
    public int fAccurate;
}

/// <summary>
/// COR_PRF_CODEGEN_FLAGS controls various flags and hooks for a specific
/// method.A combination of COR_PRF_CODEGEN_FLAGS is provided by the
/// profiler in its call to ICorProfilerFunctionControl::SetCodegenFlags()
/// when rejitting a method.
/// </summary>
[Flags]
public enum COR_PRF_CODEGEN_FLAGS : uint
{
    COR_PRF_CODEGEN_DISABLE_INLINING = 0x0001,
    COR_PRF_CODEGEN_DISABLE_ALL_OPTIMIZATIONS = 0x0002,
    COR_PRF_CODEGEN_DEBUG_INFO = 0x0003
}


[Flags]
public enum COR_PRF_MONITOR : uint
{
    // These flags represent classes of callback events
    COR_PRF_MONITOR_NONE = 0x00000000,

    // MONITOR_FUNCTION_UNLOADS controls the
    // FunctionUnloadStarted callback.
    COR_PRF_MONITOR_FUNCTION_UNLOADS = 0x00000001,

    // MONITOR_CLASS_LOADS controls the ClassLoad*
    // and ClassUnload* callbacks.
    // See the comments on those callbacks for important
    // behavior changes in V2.
    COR_PRF_MONITOR_CLASS_LOADS = 0x00000002,

    // MONITOR_MODULE_LOADS controls the
    // ModuleLoad*, ModuleUnload*, and ModuleAttachedToAssembly
    // callbacks.
    COR_PRF_MONITOR_MODULE_LOADS = 0x00000004,

    // MONITOR_ASSEMBLY_LOADS controls the
    // AssemblyLoad* and AssemblyUnload* callbacks
    COR_PRF_MONITOR_ASSEMBLY_LOADS = 0x00000008,

    // MONITOR_APPDOMAIN_LOADS controls the
    // AppDomainCreation* and AppDomainShutdown* callbacks
    COR_PRF_MONITOR_APPDOMAIN_LOADS = 0x00000010,

    // MONITOR_JIT_COMPILATION controls the
    // JITCompilation*, JITFunctionPitched, and JITInlining
    // callbacks.
    COR_PRF_MONITOR_JIT_COMPILATION = 0x00000020,


    // MONITOR_EXCEPTIONS controls the ExceptionThrown,
    // ExceptionSearch*, ExceptionOSHandler*, ExceptionUnwind*,
    // and ExceptionCatcher* callbacks.
    COR_PRF_MONITOR_EXCEPTIONS = 0x00000040,

    // MONITOR_GC controls the GarbageCollectionStarted/Finished,
    // MovedReferences, SurvivingReferences,
    // ObjectReferences, ObjectsAllocatedByClass,
    // RootReferences*, HandleCreated/Destroyed, and FinalizeableObjectQueued
    // callbacks.
    COR_PRF_MONITOR_GC = 0x00000080,

    // MONITOR_OBJECT_ALLOCATED controls the
    // ObjectAllocated callback.
    COR_PRF_MONITOR_OBJECT_ALLOCATED = 0x00000100,

    // MONITOR_THREADS controls the ThreadCreated,
    // ThreadDestroyed, ThreadAssignedToOSThread,
    // and ThreadNameChanged callbacks.
    COR_PRF_MONITOR_THREADS = 0x00000200,

    // CORECLR DEPRECATION WARNING: Remoting no longer exists in coreclr
    // MONITOR_REMOTING controls the Remoting*
    // callbacks.
    COR_PRF_MONITOR_REMOTING = 0x00000400,

    // MONITOR_CODE_TRANSITIONS controls the
    // UnmanagedToManagedTransition and
    // ManagedToUnmanagedTransition callbacks.
    COR_PRF_MONITOR_CODE_TRANSITIONS = 0x00000800,

    // MONITOR_ENTERLEAVE controls the
    // FunctionEnter*/Leave*/Tailcall* callbacks
    COR_PRF_MONITOR_ENTERLEAVE = 0x00001000,

    // MONITOR_CCW controls the COMClassicVTable*
    // callbacks.
    COR_PRF_MONITOR_CCW = 0x00002000,

    // CORECLR DEPRECATION WARNING: Remoting no longer exists in coreclr
    // MONITOR_REMOTING_COOKIE controls whether
    // a cookie will be passed to the Remoting* callbacks
    COR_PRF_MONITOR_REMOTING_COOKIE = 0x00004000 | COR_PRF_MONITOR_REMOTING,

    // CORECLR DEPRECATION WARNING: Remoting no longer exists in coreclr
    // MONITOR_REMOTING_ASYNC controls whether
    // the Remoting* callbacks will monitor async events
    COR_PRF_MONITOR_REMOTING_ASYNC = 0x00008000 | COR_PRF_MONITOR_REMOTING,

    // MONITOR_SUSPENDS controls the RuntimeSuspend*,
    // RuntimeResume*, RuntimeThreadSuspended, and
    // RuntimeThreadResumed callbacks.
    COR_PRF_MONITOR_SUSPENDS = 0x00010000,

    // MONITOR_CACHE_SEARCHES controls the
    // JITCachedFunctionSearch* callbacks.
    // See the comments on those callbacks for important
    // behavior changes in V2.
    COR_PRF_MONITOR_CACHE_SEARCHES = 0x00020000,

    // NOTE: ReJIT is now supported again.  The profiler must set this flag on
    // startup in order to use RequestReJIT or RequestRevert.  If the profiler specifies
    // this flag, then the profiler must also specify COR_PRF_DISABLE_ALL_NGEN_IMAGES
    COR_PRF_ENABLE_REJIT = 0x00040000,

    // V2 MIGRATION WARNING: DEPRECATED
    // Inproc debugging is no longer supported. ENABLE_INPROC_DEBUGGING
    // has no effect.
    COR_PRF_ENABLE_INPROC_DEBUGGING = 0x00080000,

    // V2 MIGRATION NOTE: DEPRECATED
    // The runtime now always tracks IL-native maps; this flag is thus always
    // considered to be set.
    COR_PRF_ENABLE_JIT_MAPS = 0x00100000,

    // DISABLE_INLINING tells the runtime to disable all inlining
    COR_PRF_DISABLE_INLINING = 0x00200000,

    // DISABLE_OPTIMIZATIONS tells the runtime to disable all code optimizations
    COR_PRF_DISABLE_OPTIMIZATIONS = 0x00400000,

    // ENABLE_OBJECT_ALLOCATED tells the runtime that the profiler may want
    // object allocation notifications.  This must be set during initialization if the profiler
    // ever wants object notifications (using COR_PRF_MONITOR_OBJECT_ALLOCATED)
    COR_PRF_ENABLE_OBJECT_ALLOCATED = 0x00800000,

    // MONITOR_CLR_EXCEPTIONS controls the ExceptionCLRCatcher*
    // callbacks.
    COR_PRF_MONITOR_CLR_EXCEPTIONS = 0x01000000,

    // All callback events are enabled with this flag
    COR_PRF_MONITOR_ALL = 0x0107FFFF,

    // ENABLE_FUNCTION_ARGS enables argument tracing through FunctionEnter2.
    COR_PRF_ENABLE_FUNCTION_ARGS = 0X02000000,

    // ENABLE_FUNCTION_RETVAL enables retval tracing through FunctionLeave2.
    COR_PRF_ENABLE_FUNCTION_RETVAL = 0X04000000,

    // ENABLE_FRAME_INFO enables retrieval of exact ClassIDs for generic functions using
    // GetFunctionInfo2 with a COR_PRF_FRAME_INFO obtained from FunctionEnter2.
    COR_PRF_ENABLE_FRAME_INFO = 0X08000000,

    // ENABLE_STACK_SNAPSHOT enables the used of DoStackSnapshot calls.
    COR_PRF_ENABLE_STACK_SNAPSHOT = 0X10000000,

    // USE_PROFILE_IMAGES causes the native image search to look for profiler-enhanced
    // images.  If no profiler-enhanced image is found for a given assembly the
    // runtime will fallback to JIT for that assembly.
    COR_PRF_USE_PROFILE_IMAGES = 0x20000000,

    // COR_PRF_DISABLE_TRANSPARENCY_CHECKS_UNDER_FULL_TRUST will disable security
    // transparency checks normally done during JIT compilation and class loading for
    // full trust assemblies. This can make some instrumentation easier to perform.
    COR_PRF_DISABLE_TRANSPARENCY_CHECKS_UNDER_FULL_TRUST
                                        = 0x40000000,

    // Prevents all NGEN images (including profiler-enhanced images) from loading.  If
    // this and COR_PRF_USE_PROFILE_IMAGES are both specified,
    // COR_PRF_DISABLE_ALL_NGEN_IMAGES wins.
    COR_PRF_DISABLE_ALL_NGEN_IMAGES = 0x80000000,

    // The mask for valid mask bits
    COR_PRF_ALL = 0x8FFFFFFF,

    // COR_PRF_REQUIRE_PROFILE_IMAGE represents all flags that require profiler-enhanced
    // images.
    COR_PRF_REQUIRE_PROFILE_IMAGE = COR_PRF_USE_PROFILE_IMAGES |
                                          COR_PRF_MONITOR_CODE_TRANSITIONS |
                                          COR_PRF_MONITOR_ENTERLEAVE,

    COR_PRF_ALLOWABLE_AFTER_ATTACH = COR_PRF_MONITOR_THREADS |
                                          COR_PRF_MONITOR_MODULE_LOADS |
                                          COR_PRF_MONITOR_ASSEMBLY_LOADS |
                                          COR_PRF_MONITOR_APPDOMAIN_LOADS |
                                          COR_PRF_ENABLE_STACK_SNAPSHOT |
                                          COR_PRF_MONITOR_GC |
                                          COR_PRF_MONITOR_SUSPENDS |
                                          COR_PRF_MONITOR_CLASS_LOADS |
                                          COR_PRF_MONITOR_EXCEPTIONS |
                                          COR_PRF_MONITOR_JIT_COMPILATION |
                                          COR_PRF_ENABLE_REJIT,

    COR_PRF_ALLOWABLE_NOTIFICATION_PROFILER
                                        = COR_PRF_MONITOR_FUNCTION_UNLOADS |
                                              COR_PRF_MONITOR_CLASS_LOADS |
                                              COR_PRF_MONITOR_MODULE_LOADS |
                                              COR_PRF_MONITOR_ASSEMBLY_LOADS |
                                              COR_PRF_MONITOR_APPDOMAIN_LOADS |
                                              COR_PRF_MONITOR_JIT_COMPILATION |
                                              COR_PRF_MONITOR_EXCEPTIONS |
                                              COR_PRF_MONITOR_OBJECT_ALLOCATED |
                                              COR_PRF_MONITOR_THREADS |
                                              COR_PRF_MONITOR_CODE_TRANSITIONS |
                                              COR_PRF_MONITOR_CCW |
                                              COR_PRF_MONITOR_SUSPENDS |
                                              COR_PRF_MONITOR_CACHE_SEARCHES |
                                              COR_PRF_DISABLE_INLINING |
                                              COR_PRF_DISABLE_OPTIMIZATIONS |
                                              COR_PRF_ENABLE_OBJECT_ALLOCATED |
                                              COR_PRF_MONITOR_CLR_EXCEPTIONS |
                                              COR_PRF_ENABLE_STACK_SNAPSHOT |
                                              COR_PRF_USE_PROFILE_IMAGES |
                                              COR_PRF_DISABLE_ALL_NGEN_IMAGES,

    // MONITOR_IMMUTABLE represents all flags that may only be set during initialization.
    // Trying to change any of these flags elsewhere will result in a
    // failed HRESULT.
    COR_PRF_MONITOR_IMMUTABLE = COR_PRF_MONITOR_CODE_TRANSITIONS |
                                          COR_PRF_MONITOR_REMOTING |
                                          COR_PRF_MONITOR_REMOTING_COOKIE |
                                          COR_PRF_MONITOR_REMOTING_ASYNC |
                                          COR_PRF_ENABLE_INPROC_DEBUGGING |
                                          COR_PRF_ENABLE_JIT_MAPS |
                                          COR_PRF_DISABLE_OPTIMIZATIONS |
                                          COR_PRF_DISABLE_INLINING |
                                          COR_PRF_ENABLE_OBJECT_ALLOCATED |
                                          COR_PRF_ENABLE_FUNCTION_ARGS |
                                          COR_PRF_ENABLE_FUNCTION_RETVAL |
                                          COR_PRF_ENABLE_FRAME_INFO |
                                          COR_PRF_USE_PROFILE_IMAGES |
                                          COR_PRF_DISABLE_TRANSPARENCY_CHECKS_UNDER_FULL_TRUST |
                                          COR_PRF_DISABLE_ALL_NGEN_IMAGES
}

/// <summary>
/// Additional flags the profiler can specify via SetEventMask2 when loading
/// </summary>
[Flags]
public enum COR_PRF_HIGH_MONITOR : uint
{
    COR_PRF_HIGH_MONITOR_NONE = 0x00000000,

    // CORECLR DEPRECATION WARNING: This flag is no longer checked by the runtime
    COR_PRF_HIGH_ADD_ASSEMBLY_REFERENCES = 0x00000001,

    COR_PRF_HIGH_IN_MEMORY_SYMBOLS_UPDATED = 0x00000002,

    COR_PRF_HIGH_MONITOR_DYNAMIC_FUNCTION_UNLOADS = 0x00000004,

    COR_PRF_HIGH_DISABLE_TIERED_COMPILATION = 0x00000008,

    COR_PRF_HIGH_BASIC_GC = 0x00000010,

    // Enables the MovedReferences/MovedReferences2 callback for compacting GCs only.
    COR_PRF_HIGH_MONITOR_GC_MOVED_OBJECTS = 0x00000020,

    COR_PRF_HIGH_REQUIRE_PROFILE_IMAGE = 0,

    // Enables the large object allocation monitoring according to the LOH threshold.
    COR_PRF_HIGH_MONITOR_LARGEOBJECT_ALLOCATED = 0x00000040,

    COR_PRF_HIGH_MONITOR_EVENT_PIPE = 0x00000080,

    // Enables the pinned object allocation monitoring.
    COR_PRF_HIGH_MONITOR_PINNEDOBJECT_ALLOCATED = 0x00000100,

    COR_PRF_HIGH_ALLOWABLE_AFTER_ATTACH = COR_PRF_HIGH_IN_MEMORY_SYMBOLS_UPDATED |
                                                      COR_PRF_HIGH_MONITOR_DYNAMIC_FUNCTION_UNLOADS |
                                                      COR_PRF_HIGH_BASIC_GC |
                                                      COR_PRF_HIGH_MONITOR_GC_MOVED_OBJECTS |
                                                      COR_PRF_HIGH_MONITOR_LARGEOBJECT_ALLOCATED |
                                                      COR_PRF_HIGH_MONITOR_EVENT_PIPE,

    COR_PRF_HIGH_ALLOWABLE_NOTIFICATION_PROFILER
                                        = COR_PRF_HIGH_IN_MEMORY_SYMBOLS_UPDATED |
                                              COR_PRF_HIGH_MONITOR_DYNAMIC_FUNCTION_UNLOADS |
                                              COR_PRF_HIGH_DISABLE_TIERED_COMPILATION |
                                              COR_PRF_HIGH_BASIC_GC |
                                              COR_PRF_HIGH_MONITOR_GC_MOVED_OBJECTS |
                                              COR_PRF_HIGH_MONITOR_LARGEOBJECT_ALLOCATED |
                                              COR_PRF_HIGH_MONITOR_EVENT_PIPE,

    // MONITOR_IMMUTABLE represents all flags that may only be set during initialization.
    // Trying to change any of these flags elsewhere will result in a
    // failed HRESULT.
    COR_PRF_HIGH_MONITOR_IMMUTABLE = COR_PRF_HIGH_DISABLE_TIERED_COMPILATION
}

public enum COR_PRF_FINALIZER_FLAGS
{
    None = 0,
    COR_PRF_FINALIZER_CRITICAL = 1
}

public enum COR_PRF_JIT_CACHE
{
    COR_PRF_CACHED_FUNCTION_FOUND,
    COR_PRF_CACHED_FUNCTION_NOT_FOUND
}

public enum COR_PRF_TRANSITION_REASON
{
    COR_PRF_TRANSITION_CALL,
    COR_PRF_TRANSITION_RETURN
}

public enum COR_PRF_SUSPEND_REASON
{
    COR_PRF_SUSPEND_OTHER = 0,
    COR_PRF_SUSPEND_FOR_GC = 1,
    COR_PRF_SUSPEND_FOR_APPDOMAIN_SHUTDOWN = 2,
    COR_PRF_SUSPEND_FOR_CODE_PITCHING = 3,
    COR_PRF_SUSPEND_FOR_SHUTDOWN = 4,
    COR_PRF_SUSPEND_FOR_INPROC_DEBUGGER = 6,
    COR_PRF_SUSPEND_FOR_GC_PREP = 7,
    COR_PRF_SUSPEND_FOR_REJIT = 8,
    COR_PRF_SUSPEND_FOR_PROFILER = 9
}

[Flags]
public enum CorOpenFlags : uint
{
    ofRead = 0x00000000,
    ofWrite = 0x00000001,
    ofReadWriteMask = 0x00000001,
    ofCopyMemory = 0x00000002,
    ofCacheImage = 0x00000004,
    ofManifestMetadata = 0x00000008,
    ofReadOnly = 0x00000010,
    ofTakeOwnership = 0x00000020,
    ofNoTypeLib = 0x00000080,
    ofNoTransform = 0x00001000,
    ofReserved1 = 0x00000100,
    ofReserved2 = 0x00000200,
    ofReserved = 0xffffff40
}

[Flags]
public enum CorILMethodFlags
{
    CorILMethod_InitLocals = 0x0010,           // call default constructor on all local vars
    CorILMethod_MoreSects = 0x0008,           // there is another attribute after this one

    CorILMethod_CompressedIL = 0x0040,           // Not used.

    // Indicates the format for the COR_ILMETHOD header
    CorILMethod_FormatShift = 3,
    CorILMethod_FormatMask = (1 << CorILMethod_FormatShift) - 1,
    CorILMethod_TinyFormat = 0x0002,         // use this code if the code size is even
    CorILMethod_SmallFormat = 0x0000,
    CorILMethod_FatFormat = 0x0003,
    CorILMethod_TinyFormat1 = 0x0006         // use this code if the code size is odd
}

public enum COR_PRF_STATIC_TYPE
{
    COR_PRF_FIELD_NOT_A_STATIC = 0,
    COR_PRF_FIELD_APP_DOMAIN_STATIC = 0x1,
    COR_PRF_FIELD_THREAD_STATIC = 0x2,
    COR_PRF_FIELD_CONTEXT_STATIC = 0x4,
    COR_PRF_FIELD_RVA_STATIC = 0x8
}

public enum COR_PRF_GC_GENERATION
{
    COR_PRF_GC_GEN_0 = 0,
    COR_PRF_GC_GEN_1 = 1,
    COR_PRF_GC_GEN_2 = 2,
    COR_PRF_GC_LARGE_OBJECT_HEAP = 3,
    COR_PRF_GC_PINNED_OBJECT_HEAP = 4
}

public enum COR_PRF_CLAUSE_TYPE
{
    COR_PRF_CLAUSE_NONE = 0,
    COR_PRF_CLAUSE_FILTER = 1,
    COR_PRF_CLAUSE_CATCH = 2,
    COR_PRF_CLAUSE_FINALLY = 3
}

public enum COR_PRF_RUNTIME_TYPE
{
    COR_PRF_DESKTOP_CLR = 0x1,
    COR_PRF_CORE_CLR = 0x2
}

/// <summary>
/// COR_PRF_GC_REASON describes the reason for a given GC.
/// </summary>
public enum COR_PRF_GC_REASON
{
    COR_PRF_GC_INDUCED = 1,     // Induced by GC.Collect
    COR_PRF_GC_OTHER = 0        // Anything else
}

/// <summary>
/// COR_PRF_GC_ROOT_KIND describes the kind of GC root exposed by
/// the RootReferences2 callback.
/// </summary>
public enum COR_PRF_GC_ROOT_KIND
{
    COR_PRF_GC_ROOT_STACK = 1,        // Variables on the stack
    COR_PRF_GC_ROOT_FINALIZER = 2,    // Entry in the finalizer queue
    COR_PRF_GC_ROOT_HANDLE = 3,        // GC Handle
    COR_PRF_GC_ROOT_OTHER = 0        //Misc. roots
}

/// <summary>
/// COR_PRF_GC_ROOT_FLAGS describes properties of a GC root
/// exposed by the RootReferences callback.
/// </summary>
public enum COR_PRF_GC_ROOT_FLAGS
{
    COR_PRF_GC_ROOT_PINNING = 0x1,    // Prevents GC from moving the object
    COR_PRF_GC_ROOT_WEAKREF = 0x2,    // Does not prevent collection
    COR_PRF_GC_ROOT_INTERIOR = 0x4,   // Refers to a field of the object rather than the object itself
    COR_PRF_GC_ROOT_REFCOUNTED = 0x8 // Whether it prevents collection depends on a refcount - if not,
                                     // COR_PRF_GC_ROOT_WEAKREF will be set also
}

/// <summary>
/// Enum for specifying how much data to pass back with a stack snapshot
/// </summary>
public enum COR_PRF_SNAPSHOT_INFO : uint
{
    COR_PRF_SNAPSHOT_DEFAULT = 0x0,

    // Return a register context for each frame
    COR_PRF_SNAPSHOT_REGISTER_CONTEXT = 0x1,

    // Use a quicker stack walk algorithm based on the EBP frame chain. This is available
    // on x86 only.
    COR_PRF_SNAPSHOT_X86_OPTIMIZED = 0x2
}

public struct COR_DEBUG_IL_TO_NATIVE_MAP
{
    public uint IlOffset;
    public uint NativeStartOffset;
    public uint NativeEndOffset;
}

public unsafe struct COR_PRF_EVENTPIPE_PROVIDER_CONFIG
{
    public char* ProviderName;
    public ulong Keywords;
    public uint LoggingLevel;
    // filterData expects a semicolon delimited string that defines key value pairs
    // such as "key1=value1;key2=value2;". Quotes can be used to escape the '=' and ';'
    // characters. These key value pairs will be passed in the enable callback to event
    // providers
    public char* FilterData;
}

public readonly struct EVENTPIPE_SESSION
{
    public readonly ulong Value;
}

public readonly struct EVENTPIPE_PROVIDER
{
    public readonly nint Value;
}

public readonly struct EVENTPIPE_EVENT
{
    public readonly nint Value;
}

public unsafe struct COR_PRF_EVENTPIPE_PARAM_DESC
{
    public uint Type;
    // Used if type == ArrayType
    public uint ElementType;
    public char* Name;
}

public struct COR_PRF_EVENT_DATA
{
    public ulong Ptr;
    public uint Size;
    public uint Reserved;
}

/// <summary>
/// COR_PRF_NONGC_GENERATION_RANGE describes a range of memory in the GetNonGCHeapBounds function.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct COR_PRF_NONGC_HEAP_RANGE
{
    /// <summary>
    /// The start of the range
    /// </summary>
    public nint RangeStart;

    /// <summary>
    /// The used length of the range
    /// </summary>
    public nint RangeLength;

    /// <summary>
    /// The amount of memory reserved for the range (including rangeLength)
    /// </summary>
    public nint RangeLengthReserved;
}

public enum COR_PRF_HANDLE_TYPE
{
    COR_PRF_HANDLE_TYPE_WEAK = 0x1,
    COR_PRF_HANDLE_TYPE_STRONG = 0x2,
    COR_PRF_HANDLE_TYPE_PINNED = 0x3
}

/// <summary>
/// COR_PRF_REJIT_FLAGS contains values used to control the behavior of RequestReJITWithInliners.
/// </summary>
public enum COR_PRF_REJIT_FLAGS : uint
{
    /// <summary>
    /// ReJITted methods will be prevented from being inlined
    /// </summary>
    COR_PRF_REJIT_BLOCK_INLINING = 0x1,

    /// <summary>
    /// This flag controls whether the runtime will call GetReJITParameters
    /// on methods that are ReJITted because they inline a method that was requested
    /// for ReJIT
    /// </summary>
    COR_PRF_REJIT_INLINING_CALLBACKS = 0x2
}

/// <summary>
/// MethodSemantic attr bits, used by DefineProperty, DefineEvent.
/// </summary>
[Flags]
public enum CorMethodSemanticsAttr : uint
{
    /// <summary>
    /// Setter for property
    /// </summary>
    msSetter = 0x0001,
    /// <summary>
    /// Getter for property
    /// </summary>
    msGetter = 0x0002,
    /// <summary>
    /// Other method for property or event
    /// </summary>
    msOther = 0x0004,
    /// <summary>
    /// AddOn method for event
    /// </summary>
    msAddOn = 0x0008,
    /// <summary>
    /// RemoveOn method for event
    /// </summary>
    msRemoveOn = 0x0010,
    /// <summary>
    /// Fire method for event
    /// </summary>
    msFire = 0x0020
}

/// <summary>
/// MethodImpl attr bits, used by DefineMethodImpl.
/// </summary>
public enum CorMethodImpl : uint
{
    /// <summary>
    /// Flags about code type.
    /// </summary>
    miCodeTypeMask = 0x0003,
    /// <summary>
    /// Method impl is IL.
    /// </summary>
    miIL = 0x0000,
    /// <summary>
    /// Method impl is native.
    /// </summary>
    miNative = 0x0001,
    /// <summary>
    /// Method impl is OPTIL
    /// </summary>
    miOPTIL = 0x0002,
    /// <summary>
    /// Method impl is provided by the runtime.
    /// </summary>
    miRuntime = 0x0003,

    /// <summary>
    /// Flags specifying whether the code is managed or unmanaged.
    /// </summary>
    miManagedMask = 0x0004,
    /// <summary>
    /// Method impl is unmanaged, otherwise managed.
    /// </summary>
    miUnmanaged = 0x0004,
    /// <summary>
    /// Method impl is managed.
    /// </summary>
    miManaged = 0x0000,

    /// <summary>
    /// Indicates method is defined; used primarily in merge scenarios.
    /// </summary>
    miForwardRef = 0x0010,
    /// <summary>
    /// Indicates method sig is not to be mangled to do HRESULT conversion.
    /// </summary>
    miPreserveSig = 0x0080,
    /// <summary>
    /// Reserved for internal use.
    /// </summary>
    miInternalCall = 0x1000,

    /// <summary>
    /// Method is single threaded through the body.
    /// </summary>
    miSynchronized = 0x0020,
    /// <summary>
    /// Method may not be inlined.
    /// </summary>
    miNoInlining = 0x0008,
    /// <summary>
    /// Method should be inlined if possible.
    /// </summary>
    miAggressiveInlining = 0x0100,
    /// <summary>
    /// Method may not be optimized.
    /// </summary>
    miNoOptimization = 0x0040,
    /// <summary>
    /// Method may contain hot code and should be aggressively optimized.
    /// </summary>
    miAggressiveOptimization = 0x0200,

    /// <summary>
    /// These are the flags that are allowed in MethodImplAttribute's Value
    /// property. This should include everything above except the code impl
    /// flags (which are used for MethodImplAttribute's MethodCodeType field).
    /// </summary>
    miUserMask = miManagedMask | miForwardRef | miPreserveSig |
                             miInternalCall | miSynchronized |
                             miNoInlining | miAggressiveInlining |
                             miNoOptimization | miAggressiveOptimization,

    /// <summary>
    /// Range check value
    /// </summary>
    miMaxMethodImplVal = 0xffff
}

/// <summary>
/// PinvokeMap attr bits, used by DefinePinvokeMap.
/// </summary>
[Flags]
public enum CorPinvokeMap : uint
{
    /// <summary>
    /// Pinvoke is to use the member name as specified.
    /// </summary>
    pmNoMangle = 0x0001,

    // Use this mask to retrieve the CharSet information.
    pmCharSetMask = 0x0006,
    pmCharSetNotSpec = 0x0000,
    pmCharSetAnsi = 0x0002,
    pmCharSetUnicode = 0x0004,
    pmCharSetAuto = 0x0006,


    pmBestFitUseAssem = 0x0000,
    pmBestFitEnabled = 0x0010,
    pmBestFitDisabled = 0x0020,
    pmBestFitMask = 0x0030,

    pmThrowOnUnmappableCharUseAssem = 0x0000,
    pmThrowOnUnmappableCharEnabled = 0x1000,
    pmThrowOnUnmappableCharDisabled = 0x2000,
    pmThrowOnUnmappableCharMask = 0x3000,

    pmSupportsLastError = 0x0040,   // Information about target function. Not relevant for fields.

    // None of the calling convention flags is relevant for fields.
    pmCallConvMask = 0x0700,
    pmCallConvWinapi = 0x0100,   // Pinvoke will use native callconv appropriate to target windows platform.
    pmCallConvCdecl = 0x0200,
    pmCallConvStdcall = 0x0300,
    pmCallConvThiscall = 0x0400,   // In M9, pinvoke will raise exception.
    pmCallConvFastcall = 0x0500,

    pmMaxValue = 0xFFFF
}

public enum CorElementTypes : uint
{
    ELEMENT_TYPE_END = 0x00,
    ELEMENT_TYPE_VOID = 0x01,
    ELEMENT_TYPE_BOOLEAN = 0x02,
    ELEMENT_TYPE_CHAR = 0x03,
    ELEMENT_TYPE_I1 = 0x04,
    ELEMENT_TYPE_U1 = 0x05,
    ELEMENT_TYPE_I2 = 0x06,
    ELEMENT_TYPE_U2 = 0x07,
    ELEMENT_TYPE_I4 = 0x08,
    ELEMENT_TYPE_U4 = 0x09,
    ELEMENT_TYPE_I8 = 0x0a,
    ELEMENT_TYPE_U8 = 0x0b,
    ELEMENT_TYPE_R4 = 0x0c,
    ELEMENT_TYPE_R8 = 0x0d,
    ELEMENT_TYPE_STRING = 0x0e,

    // every type above PTR will be simple type
    ELEMENT_TYPE_PTR = 0x0f,     // PTR <type>
    ELEMENT_TYPE_BYREF = 0x10,     // BYREF <type>

    // Please use ELEMENT_TYPE_VALUETYPE. ELEMENT_TYPE_VALUECLASS is deprecated.
    ELEMENT_TYPE_VALUETYPE = 0x11,     // VALUETYPE <class Token>
    ELEMENT_TYPE_CLASS = 0x12,     // CLASS <class Token>
    ELEMENT_TYPE_VAR = 0x13,     // a class type variable VAR <number>
    ELEMENT_TYPE_ARRAY = 0x14,     // MDARRAY <type> <rank> <bcount> <bound1> ... <lbcount> <lb1> ...
    ELEMENT_TYPE_GENERICINST = 0x15,     // GENERICINST <generic type> <argCnt> <arg1> ... <argn>
    ELEMENT_TYPE_TYPEDBYREF = 0x16,     // TYPEDREF  (it takes no args) a typed reference to some other type

    ELEMENT_TYPE_I = 0x18,     // native integer size
    ELEMENT_TYPE_U = 0x19,     // native unsigned integer size
    ELEMENT_TYPE_FNPTR = 0x1b,     // FNPTR <complete sig for the function including calling convention>
    ELEMENT_TYPE_OBJECT = 0x1c,     // Shortcut for System.Object
    ELEMENT_TYPE_SZARRAY = 0x1d,     // Shortcut for single dimension zero lower bound array
                                     // SZARRAY <type>
    ELEMENT_TYPE_MVAR = 0x1e,     // a method type variable MVAR <number>

    // This is only for binding
    ELEMENT_TYPE_CMOD_REQD = 0x1f,     // required C modifier : E_T_CMOD_REQD <mdTypeRef/mdTypeDef>
    ELEMENT_TYPE_CMOD_OPT = 0x20,     // optional C modifier : E_T_CMOD_OPT <mdTypeRef/mdTypeDef>

    // This is for signatures generated internally (which will not be persisted in any way).
    ELEMENT_TYPE_INTERNAL = 0x21,     // INTERNAL <typehandle>

    // Note that this is the max of base type excluding modifiers
    ELEMENT_TYPE_MAX = 0x22,     // first invalid element type


    ELEMENT_TYPE_MODIFIER = 0x40,
    ELEMENT_TYPE_SENTINEL = 0x01 | ELEMENT_TYPE_MODIFIER, // sentinel for varargs
    ELEMENT_TYPE_PINNED = 0x05 | ELEMENT_TYPE_MODIFIER
}

/// <summary>
/// Param attr bits, used by DefineParam.
/// </summary>
public enum CorParamAttr
{
    pdIn = 0x0001,     // Param is [In]
    pdOut = 0x0002,     // Param is [out]
    pdOptional = 0x0010,     // Param is optional

    // Reserved flags for Runtime use only.
    pdReservedMask = 0xf000,
    pdHasDefault = 0x1000,     // Param has default value.
    pdHasFieldMarshal = 0x2000,     // Param has FieldMarshal.

    pdUnused = 0xcfe0
}

public enum CorGenericParamAttr
{
    gpVarianceMask = 0x0003,
    gpNonVariant = 0x0000,
    gpCovariant = 0x0001,
    gpContravariant = 0x0002,

    gpSpecialConstraintMask = 0x001C,
    gpNoSpecialConstraint = 0x0000,
    gpReferenceTypeConstraint = 0x0004,
    gpNotNullableValueTypeConstraint = 0x0008,
    gpDefaultConstructorConstraint = 0x0010
}

public enum CorPEKind
{
    peNot = 0x00000000,
    peILonly = 0x00000001,
    pe32BitRequired = 0x00000002,
    pe32Plus = 0x00000004,
    pe32Unmanaged = 0x00000008,
    pe32BitPreferred = 0x00000010
}

public enum CorSaveSize
{
    cssAccurate = 0x0000,
    cssQuick = 0x0001,
    cssDiscardTransientCAs = 0x0002
}

[StructLayout(LayoutKind.Sequential)]
public struct COR_SECATTR
{
    public MdMemberRef tkCtor;         // Ref to constructor of security attribute.
    public IntPtr pCustomAttribute;  // Blob describing ctor args and field/property values.
    public uint cbCustomAttribute;  // Length of the above blob.
}

public readonly record struct ObjectHandleId(nint Value);

public readonly record struct ClassIdInfo(ModuleId ModuleId, MdTypeDef TypeDef);
public readonly record struct ClassIdInfo2(ModuleId ModuleId, MdTypeDef TypeDef, ClassId ParentClassId);
public readonly record struct TypeDefProps(int TypeDefFlags, MdToken Extends);
public readonly record struct TypeDefPropsWithName(string TypeName, int TypeDefFlags, MdToken Extends);
public readonly record struct FunctionInfo(ClassId ClassId, ModuleId ModuleId, MdToken Token);
public readonly record struct ModuleInfo(nint BaseLoadAddress, AssemblyId AssemblyId);
public readonly record struct ModuleInfoWithName(string ModuleName, nint BaseLoadAddress, AssemblyId AssemblyId);
public readonly record struct ModuleInfo2(nint BaseLoadAddress, AssemblyId AssemblyId, uint ModuleFlags);
public readonly record struct ModuleInfoWithName2(string ModuleName, nint BaseLoadAddress, AssemblyId AssemblyId, uint ModuleFlags);
public readonly record struct AppDomainInfo(string AppDomainName, ProcessId ProcessId);
public readonly record struct AssemblyInfo(AppDomainId AppDomainId, ModuleId ModuleId);
public readonly record struct AssemblyInfoWithName(string AssemblyName, AppDomainId AppDomainId, ModuleId ModuleId);
public readonly record struct StringLayout(uint BufferLengthOffset, uint StringLengthOffset, uint BufferOffset);
public readonly record struct StringLayout2(uint StringLengthOffset, uint BufferOffset);
public readonly record struct CodeInfo(nint Start, uint Size);
public readonly record struct ArrayClassInfo(CorElementType BaseElemType, ClassId BaseClassId, uint Rank);
public readonly record struct TokenAndMetaData(Guid Riid, nint Import, MdToken Token);
public readonly record struct ILFunctionBody(nint MethodHeader, uint MethodSize);
public readonly record struct FunctionLeave3Info(COR_PRF_FRAME_INFO FrameInfo, COR_PRF_FUNCTION_ARGUMENT_RANGE RetvalRange);
public readonly record struct RuntimeInformation(ushort ClrInstanceId, COR_PRF_RUNTIME_TYPE RuntimeType, ushort MajorVersion, ushort MinorVersion, ushort BuildNumber, ushort QFEVersion);
public readonly record struct FunctionFromIP(FunctionId FunctionId, ReJITId ReJitId);
public readonly record struct EventMask2(COR_PRF_MONITOR EventsLow, COR_PRF_HIGH_MONITOR EventsHigh);
public readonly record struct NgenModuleMethodsInliningThisMethod(INativeEnumerator<COR_PRF_METHOD> Enumerator, bool IncompleteData);
public readonly record struct ScopeProps(string Name, Guid Mvid);
public readonly record struct InterfaceImplProps(MdTypeDef Class, MdToken Interface);
public readonly record struct TypeRefProps(string TypeName, MdToken ResolutionScope);
public readonly record struct ResolvedTypeRef(nint IScope, MdTypeDef TypeDef);
public readonly record struct EventProps(MdTypeDef Class, uint EventFlags, MdToken EventType, MdMethodDef AddOn, MdMethodDef RemoveOn, MdMethodDef Fire);
public readonly record struct ClassLayout(uint PackSize, uint ClassSize);
public readonly record struct DynamicFunctionInfo(ModuleId ModuleId, NativePointer<byte> Signature);
public readonly record struct DynamicFunctionInfoWithName(ModuleId ModuleId, NativePointer<byte> Signature, string Name);
public readonly record struct MethodProps(MdTypeDef Class, uint Attributes, NativePointer<byte> Signature, uint RVA, uint ImplementationFlags);
public readonly record struct MethodPropsWithName(string Name, MdTypeDef Class, uint Attributes, NativePointer<byte> Signature, uint Rva, uint ImplementationFlags);
public readonly record struct MemberRefProps(MdToken Token, NativePointer<byte> Signature);
public readonly record struct MemberRefPropsWithName(string Name, MdToken Token, NativePointer<byte> Signature);
public readonly record struct MetadataRva(uint Rva, CorMethodImpl Flags);
public readonly record struct PermissionSetProps(uint Action, NativePointer<byte> Permission);
public readonly record struct PInvokeMap(CorPinvokeMap Flags, MdModuleRef ImportDll);
public readonly record struct PInvokeMapWithName(string ImportName, CorPinvokeMap Flags, MdModuleRef ImportDll);
public readonly record struct CustomAttributeProps(MdToken Object, MdToken Type, NativePointer<byte> Value);
public readonly record struct MemberProps(MdTypeDef Class, uint Attributes, NativePointer<byte> Signature, uint CodeRva, uint ImplementationFlags, CorElementTypes CPlusTypeFlag, NativePointer<byte> Value);
public readonly record struct MemberPropsWithName(string Name, MdTypeDef Class, uint Attributes, NativePointer<byte> Signature, uint CodeRva, uint ImplementationFlags, CorElementTypes CPlusTypeFlag, NativePointer<byte> Value);
public readonly record struct FieldProps(MdTypeDef Class, uint Attributes, NativePointer<byte> Signature, CorElementTypes CPlusTypeFlag, NativePointer<byte> Value);
public readonly record struct FieldPropsWithName(string Name, MdTypeDef Class, uint Attributes, NativePointer<byte> Signature, CorElementTypes CPlusTypeFlag, NativePointer<byte> Value);
public readonly record struct PropertyProps(MdTypeDef Class, uint Flags, NativePointer<byte> Signature, CorElementTypes CPlusTypeFlag, NativePointer<byte> DefaultValue, MdMethodDef Setter, MdMethodDef Getter);
public readonly record struct ParamProps(MdMethodDef Method, uint Index, CorParamAttr Attributes, CorElementTypes CPlusTypeFlag, NativePointer<byte> Value);
public readonly record struct ParamPropsWithName(string Name, MdMethodDef Method, uint Index, CorParamAttr Attributes, CorElementTypes CPlusTypeFlag, NativePointer<byte> Value);
public readonly record struct GenericParamProps(uint ParamSeq, CorGenericParamAttr ParamFlags, MdToken Owner, uint Reserved);
public readonly record struct GenericParamPropsWithName(uint ParamSeq, CorGenericParamAttr ParamFlags, MdToken Owner, uint Reserved, string Name);
public readonly record struct MethodSpecProps(MdToken Parent, NativePointer<byte> Signature);
public readonly record struct GenericParamConstraintProps(MdGenericParam GenericParam, MdToken ConstraintType);
public readonly record struct PEKind(CorPEKind Kind, uint Machine);

public readonly record struct NativePointer<T>(nint Ptr, int Length)
{
    public unsafe Span<T> AsSpan => new((void*)Ptr, Length);
}