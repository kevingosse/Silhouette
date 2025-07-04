﻿using System.ComponentModel;

namespace Silhouette;

public readonly struct HResult : IEquatable<HResult>
{
    public const int S_OK = 0;
    public const int S_FALSE = 1;
    public const int E_ABORT = unchecked((int)0x80004004);
    public const int E_FAIL = unchecked((int)0x80004005);
    public const int E_INVALIDARG = unchecked((int)0x80070057);
    public const int E_NOTIMPL = unchecked((int)0x80004001);
    public const int E_NOINTERFACE = unchecked((int)0x80004002);
    public const int CORPROF_E_UNSUPPORTED_CALL_SEQUENCE = unchecked((int)0x80131363);

    public bool IsOK => Code == S_OK;

    public int Code { get; }

    public HResult(int hr) => Code = hr;

    public static implicit operator HResult(int hr) => new(hr);

    /// <summary>
    /// Helper to convert to int for comparisons.
    /// </summary>
    public static implicit operator int(HResult hr) => hr.Code;

    /// <summary>
    /// This makes "if (hr)" equivalent to SUCCEEDED(hr).
    /// </summary>
    public static implicit operator bool(HResult hr) => hr.Code >= 0;

    public static bool operator ==(HResult left, HResult right) => left.Equals(right);

    public static bool operator !=(HResult left, HResult right) => !left.Equals(right);

    public static string ToString(int code)
    {
        return code switch
        {
            S_OK => "S_OK",
            S_FALSE => "S_FALSE",
            E_ABORT => "E_ABORT",
            E_FAIL => "E_FAIL",
            E_INVALIDARG => "E_INVALIDARG",
            E_NOTIMPL => "E_NOTIMPL",
            E_NOINTERFACE => "E_NOINTERFACE",
            CORPROF_E_UNSUPPORTED_CALL_SEQUENCE => "CORPROF_E_UNSUPPORTED_CALL_SEQUENCE",
            _ => $"{code:x8}"
        };
    }

    public override string ToString() => ToString(Code);

    public void ThrowIfFailed()
    {
        if (Code < 0)
        {
            throw new Win32Exception(this);
        }
    }

    public override bool Equals(object obj)
    {
        return obj is HResult other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Code;
    }

    public bool Equals(HResult other)
    {
        return Code == other.Code;
    }
}

public readonly struct HResult<T> : IEquatable<HResult<T>>
{
    public HResult(HResult error, T result)
    {
        Error = error;
        Result = result;
    }

    public HResult Error { get; }

    public T Result { get; }

    public static implicit operator HResult<T>(T t) => new(HResult.S_OK, t);

    public static implicit operator HResult<T>(HResult error) => new(error, default);

    public static bool operator ==(HResult<T> left, HResult<T> right) => left.Equals(right);

    public static bool operator !=(HResult<T> left, HResult<T> right)=> !left.Equals(right);

    public T ThrowIfFailed()
    {
        if (Error.Code < 0)
        {
            throw new Win32Exception(Error);
        }

        return Result;
    }

    public void Deconstruct(out T result)
    {
        result = Result;
    }

    public void Deconstruct(out HResult error, out T result)
    {
        error = Error;
        result = Result;
    }

    public bool Equals(HResult<T> other)
    {
        return Error.Equals(other.Error) && EqualityComparer<T>.Default.Equals(Result, other.Result);
    }

    public override bool Equals(object obj)
    {
        return obj is HResult<T> other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Error, Result);
    }
}
