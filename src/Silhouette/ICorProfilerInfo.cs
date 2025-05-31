using Silhouette.Interfaces;

namespace Silhouette
{
    public class ICorProfilerInfo : IUnknown, ICorProfilerInfoFactory<ICorProfilerInfo>
    {
        private NativeObjects.ICorProfilerInfoInvoker _impl;

        public ICorProfilerInfo(nint ptr)
        {
            _impl = new(ptr);
        }

        static ICorProfilerInfo ICorProfilerInfoFactory<ICorProfilerInfo>.Create(nint ptr) => new(ptr);
        static Guid ICorProfilerInfoFactory<ICorProfilerInfo>.Guid => Interfaces.ICorProfilerInfo.Guid;

        public HResult QueryInterface(in Guid guid, out nint ptr)
        {
            return _impl.QueryInterface(in guid, out ptr);
        }

        public int AddRef()
        {
            return _impl.AddRef();
        }

        public int Release()
        {
            return _impl.Release();
        }

        public HResult<ClassId> GetClassFromObject(ObjectId objectId)
        {
            var result = _impl.GetClassFromObject(objectId, out var classId);
            return new(result, classId);
        }

        public HResult<ClassId> GetClassFromToken(ModuleId moduleId, MdTypeDef typeDef)
        {
            var result = _impl.GetClassFromToken(moduleId, typeDef, out var classId);
            return new(result, classId);
        }

        public unsafe HResult<CodeInfo> GetCodeInfo(FunctionId functionId)
        {
            var result = _impl.GetCodeInfo(functionId, out var start, out var size);
            return new(result, new((nint)start, size));
        }

        public HResult<COR_PRF_MONITOR> GetEventMask()
        {
            var result = _impl.GetEventMask(out var pdwEvents);
            return new(result, (COR_PRF_MONITOR)pdwEvents);
        }

        public HResult<FunctionId> GetFunctionFromIP(nint ip)
        {
            var result = _impl.GetFunctionFromIP(ip, out var functionId);
            return new(result, functionId);
        }

        public HResult<FunctionId> GetFunctionFromToken(ModuleId moduleId, MdToken token)
        {
            var result = _impl.GetFunctionFromToken(moduleId, token, out var functionId);
            return new(result, functionId);
        }

        public HResult<nint> GetHandleFromThread(ThreadId threadId)
        {
            var result = _impl.GetHandleFromThread(threadId, out var handle);
            return new(result, handle);
        }

        public HResult<uint> GetObjectSize(ObjectId objectId)
        {
            var result = _impl.GetObjectSize(objectId, out var size);
            return new(result, size);
        }

        public HResult<ArrayClassInfo> IsArrayClass(ClassId classId)
        {
            var result = _impl.IsArrayClass(classId, out var baseElemType, out var baseClassId, out var rank);
            return new(result, new(baseElemType, baseClassId, rank));
        }

        public HResult<uint> GetThreadInfo(ThreadId threadId)
        {
            var result = _impl.GetThreadInfo(threadId, out var win32ThreadId);
            return new(result, win32ThreadId);
        }

        public HResult<ThreadId> GetCurrentThreadId()
        {
            var code = _impl.GetCurrentThreadId(out var threadId);
            return new(code, threadId);
        }

        public HResult<ClassIdInfo> GetClassIdInfo(ClassId classId)
        {
            var result = _impl.GetClassIdInfo(classId, out var moduleId, out var typeDefToken);
            return new(result, new(moduleId, typeDefToken));
        }

        public HResult<FunctionInfo> GetFunctionInfo(FunctionId functionId)
        {
            var result = _impl.GetFunctionInfo(functionId, out var classId, out var moduleId, out var token);
            return new(result, new(classId, moduleId, token));
        }

        public HResult SetEventMask(COR_PRF_MONITOR dwEvents)
        {
            return _impl.SetEventMask(dwEvents);
        }

        public unsafe HResult SetEnterLeaveFunctionHooks(void* pFuncEnter, void* pFuncLeave, void* pFuncTailcall)
        {
            return _impl.SetEnterLeaveFunctionHooks(pFuncEnter, pFuncLeave, pFuncTailcall);
        }

        public unsafe HResult SetFunctionIdMapper(void* pFunc)
        {
            return _impl.SetFunctionIdMapper(pFunc);
        }

        public unsafe HResult<TokenAndMetaData> GetTokenAndMetaDataFromFunction(FunctionId functionId)
        {
            var result = _impl.GetTokenAndMetaDataFromFunction(functionId, out var riid, out var ppImport, out var pToken);
            return new(result, new(riid, (nint)ppImport, pToken));
        }

        public unsafe HResult<ModuleInfoWithName> GetModuleInfo(ModuleId moduleId)
        {
            var (result, _) = GetModuleInfo(moduleId, [], out var length);

            if (!result)
            {
                return result;
            }

            Span<char> buffer = stackalloc char[(int)length];

            (result, var moduleInfo) = GetModuleInfo(moduleId, buffer, out _);

            if (!result)
            {
                return result;
            }

            return new(result, new(buffer.WithoutNullTerminator(), moduleInfo.BaseLoadAddress, moduleInfo.AssemblyId));
        }

        public unsafe HResult<ModuleInfo> GetModuleInfo(ModuleId moduleId, Span<char> moduleName, out uint moduleNameLength)
        {
            fixed (char* c = moduleName)
            {
                var result = _impl.GetModuleInfo(moduleId, out var pBaseLoadAddress, (uint)moduleName.Length, out moduleNameLength, c, out var assemblyId);
                return new(result, new(pBaseLoadAddress, assemblyId));
            }
        }

        public unsafe HResult<IMetaDataImport> GetModuleMetaData(ModuleId moduleId, CorOpenFlags dwOpenFlags, Guid riid)
        {
            var result = _impl.GetModuleMetaData(moduleId, dwOpenFlags, &riid, out var ptr);

            return new(result, new(ptr));
        }

        public unsafe HResult<ILFunctionBody> GetILFunctionBody(ModuleId moduleId, MdMethodDef methodId)
        {
            var result = _impl.GetILFunctionBody(moduleId, methodId, out var pMethodHeader, out var methodSize);
            return new(result, new((nint)pMethodHeader, methodSize));
        }

        public unsafe HResult<nint> GetILFunctionBodyAllocator(ModuleId moduleId)
        {
            var result = _impl.GetILFunctionBodyAllocator(moduleId, out var malloc);
            return new(result, (nint)malloc);
        }

        public HResult SetILFunctionBody(ModuleId moduleId, MdMethodDef methodid, nint pbNewILMethodHeader)
        {
            return _impl.SetILFunctionBody(moduleId, methodid, pbNewILMethodHeader);
        }

        public unsafe HResult<AppDomainInfo> GetAppDomainInfo(AppDomainId appDomainId)
        {
            var (result, _) = GetAppDomainInfo(appDomainId, [], out var length);

            if (!result)
            {
                return result;
            }

            Span<char> buffer = stackalloc char[(int)length];

            (result, var processId) = GetAppDomainInfo(appDomainId, buffer, out _);

            if (!result)
            {
                return result;
            }

            return new(result, new(buffer.WithoutNullTerminator(), processId));
        }

        public unsafe HResult<ProcessId> GetAppDomainInfo(AppDomainId appDomainId, Span<char> appDomainName, out uint pcchName)
        {
            fixed (char* c = appDomainName)
            {
                var result = _impl.GetAppDomainInfo(appDomainId, (uint)appDomainName.Length, out pcchName, c, out var processId);
                return new(result, processId);
            }
        }

        public unsafe HResult<AssemblyInfo> GetAssemblyInfo(AssemblyId assemblyId, Span<char> assemblyName, out uint cchName)
        {
            fixed (char* c = assemblyName)
            {
                var result = _impl.GetAssemblyInfo(assemblyId, (uint)assemblyName.Length, out cchName, c, out var appDomainId, out var moduleId);
                return new(result, new(appDomainId, moduleId));
            }
        }

        public unsafe HResult<AssemblyInfoWithName> GetAssemblyInfo(AssemblyId assemblyId)
        {
            var (result, _) = GetAssemblyInfo(assemblyId, [], out var length);

            if (!result)
            {
                return result;
            }

            Span<char> buffer = stackalloc char[(int)length];

            (result, var assemblyInfo) = GetAssemblyInfo(assemblyId, buffer, out _);

            if (!result)
            {
                return result;
            }

            return new(result, new(buffer.WithoutNullTerminator(), assemblyInfo.AppDomainId, assemblyInfo.ModuleId));
        }

        public HResult SetFunctionReJIT(FunctionId functionId)
        {
            return _impl.SetFunctionReJIT(functionId);
        }

        public HResult ForceGC()
        {
            return _impl.ForceGC();
        }

        public unsafe HResult SetILInstrumentedCodeMap(FunctionId functionId, int fStartJit, uint cILMapEntries, CorIlMap* rgILMapEntries)
        {
            return _impl.SetILInstrumentedCodeMap(functionId, fStartJit, cILMapEntries, rgILMapEntries);
        }

        public unsafe HResult<nint> GetInprocInspectionInterface()
        {
            var result = _impl.GetInprocInspectionInterface(out var picd);
            return new(result, (nint)picd);
        }

        public unsafe HResult<nint> GetInprocInspectionIThisThread()
        {
            var result = _impl.GetInprocInspectionIThisThread(out var picd);
            return new(result, (nint)picd);
        }

        public HResult<ContextId> GetThreadContext(ThreadId threadId)
        {
            var result = _impl.GetThreadContext(threadId, out var contextId);
            return new(result, contextId);
        }

        public HResult<uint> BeginInprocDebugging(int thisThreadOnly)
        {
            var result = _impl.BeginInprocDebugging(thisThreadOnly, out var profilerContext);
            return new(result, profilerContext);
        }

        public HResult EndInprocDebugging(uint profilerContext)
        {
            return _impl.EndInprocDebugging(profilerContext);
        }

        public unsafe HResult GetILToNativeMapping(FunctionId functionId, Span<CorDebugIlToNativeMap> map, out uint pcMap)
        {
            fixed (CorDebugIlToNativeMap* cMap = map)
            {
                return _impl.GetILToNativeMapping(functionId, (uint)map.Length, out pcMap, cMap);
            }
        }
    }
}
