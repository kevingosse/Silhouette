namespace Silhouette;

public unsafe class ICorProfilerInfo12 : ICorProfilerInfo11, ICorProfilerInfoFactory<ICorProfilerInfo12>
{
    private NativeObjects.ICorProfilerInfo12Invoker _impl;

    public ICorProfilerInfo12(nint ptr) : base(ptr)
    {
        _impl = new(ptr);
    }

    static ICorProfilerInfo12 ICorProfilerInfoFactory<ICorProfilerInfo12>.Create(nint ptr) => new(ptr);
    static Guid ICorProfilerInfoFactory<ICorProfilerInfo12>.Guid => Interfaces.ICorProfilerInfo12.Guid;

    public HResult<EVENTPIPE_SESSION> EventPipeStartSession(Span<COR_PRF_EVENTPIPE_PROVIDER_CONFIG> providerConfigs, bool requestRundown)
    {
        fixed (COR_PRF_EVENTPIPE_PROVIDER_CONFIG* pProviderConfigs = providerConfigs)
        {
            var result = _impl.EventPipeStartSession((uint)providerConfigs.Length, pProviderConfigs, requestRundown ? 1 : 0, out var session);
            return new(result, session);
        }
    }

    public HResult EventPipeAddProviderToSession(EVENTPIPE_SESSION session, COR_PRF_EVENTPIPE_PROVIDER_CONFIG providerConfig)
    {
        return _impl.EventPipeAddProviderToSession(session, providerConfig);
    }

    public HResult EventPipeStopSession(EVENTPIPE_SESSION session)
    {
        return _impl.EventPipeStopSession(session);
    }

    public HResult<EVENTPIPE_PROVIDER> EventPipeCreateProvider(string providerName)
    {
        fixed (char* pProviderName = providerName)
        {
            var result = _impl.EventPipeCreateProvider(pProviderName, out var provider);
            return new(result, provider);
        }
    }

    public HResult EventPipeGetProviderInfo(EVENTPIPE_PROVIDER provider, Span<char> name, out uint nameLength)
    {
        fixed (char* pName = name)
        {
            return _impl.EventPipeGetProviderInfo(provider, (uint)name.Length, out nameLength, pName);
        }
    }

    public HResult<string> EventPipeGetProviderInfo(EVENTPIPE_PROVIDER provider)
    {
        var result = EventPipeGetProviderInfo(provider, [], out var length);

        if (!result)
        {
            return result;
        }

        Span<char> buffer = stackalloc char[(int)length];

        result = EventPipeGetProviderInfo(provider, buffer, out _);

        if (!result)
        {
            return result;
        }

        return new(result, buffer.WithoutNullTerminator());
    }

    public HResult<EVENTPIPE_EVENT> EventPipeDefineEvent(EVENTPIPE_PROVIDER provider, string eventName, uint eventID, ulong keywords, uint eventVersion,
        uint level, byte opcode, bool needStack, ReadOnlySpan<COR_PRF_EVENTPIPE_PARAM_DESC> paramDescs)
    {
        fixed (COR_PRF_EVENTPIPE_PARAM_DESC* pParamDescs = paramDescs)
        fixed (char* pEventName = eventName)
        {
            var result = _impl.EventPipeDefineEvent(provider, pEventName, eventID, keywords, eventVersion, level, opcode, needStack ? 1 : 0, (uint)paramDescs.Length, pParamDescs, out var @event);
            return new(result, @event);
        }
    }


    public HResult EventPipeWriteEvent(EVENTPIPE_EVENT @event, ReadOnlySpan<COR_PRF_EVENT_DATA> data, in Guid pActivityId, in Guid pRelatedActivityId)
    {
        fixed (COR_PRF_EVENT_DATA* pData = data)
        {
            return _impl.EventPipeWriteEvent(@event, (uint)data.Length, pData, pActivityId, pRelatedActivityId);
        }
    }
}