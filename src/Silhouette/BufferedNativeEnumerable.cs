using System.Collections;

namespace Silhouette;

public readonly ref struct BufferedNativeEnumerable<T>
    where T : unmanaged
{
    private readonly INativeEnumerator<T> _nativeEnumerator;
    private readonly Span<T> _buffer;

    public BufferedNativeEnumerable(INativeEnumerator<T> nativeEnumerator, Span<T> buffer)
    {
        _buffer = buffer;
        _nativeEnumerator = nativeEnumerator;
    }

    public Enumerator GetEnumerator() => new(_nativeEnumerator, _buffer);

    public ref struct Enumerator : IEnumerator<T>
    {
        private readonly Span<T> _buffer;
        private readonly INativeEnumerator<T> _nativeEnumerator;
        private int _index;
        private uint _size;
        private HResult _result;

        public Enumerator(INativeEnumerator<T> nativeEnumerator, Span<T> buffer)
        {
            _buffer = buffer;
            _nativeEnumerator = nativeEnumerator;
            _index = -1;
            _size = 0;
            _result = HResult.S_OK;
        }

        public T Current => _buffer[_index];

        object IEnumerator.Current => Current;

        public void Dispose() { }

        public bool MoveNext()
        {
            _index++;

            if (_index >= _size)
            {
                if (!_result)
                {
                    return false;
                }

                (_result, _size) = _nativeEnumerator.Next(_buffer);

                _index = 0;

                if (!_result)
                {
                    _size = 0;
                }
            }

            return _index < _size;
        }

        public void Reset() => _nativeEnumerator.Reset().ThrowIfFailed();
    }
}
