namespace Library;

public sealed class LookaheadBitStream
{
    private readonly Stream _backingStream;

    private int _currentByte;
    private int _bitPosition;

    // For use in PeekBits()
    private int _cache = 0;
    private int _cacheBitsRemaining = 0;

    public LookaheadBitStream(Stream backingStream)
    {
        _backingStream = backingStream;

        _currentByte = backingStream.ReadByte();
        _bitPosition = 0;

        if (_currentByte == -1)
            throw new EndOfStreamException();
    }

    public bool ReadBit()
    {
        if (_cacheBitsRemaining > 0)
        {
            _cacheBitsRemaining--;
            return (_cache & (1 << _cacheBitsRemaining)) != 0;
        }

        if (_bitPosition == 8)
        {
            _bitPosition = 0;
            if ((_currentByte = _backingStream.ReadByte()) == -1)
                throw new EndOfStreamException();
        }

        bool bit = (_currentByte >> 7 - _bitPosition & 1) == 1;
        _bitPosition++;

        return bit;
    }

    public int ReadBits(int n)
    {
        int b = 0;
        for (int i = 0; i < n; i++)
        {
            b <<= 1;
            b |= ReadBit() ? 1 : 0;
        }

        return b;
    }

    public int PeekBits(int n)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(n, 0, nameof(n));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(n, 32, nameof(n));

        int result = ReadBits(n);
        _cache = result;
        _cacheBitsRemaining = n;
        return result;
    }
}
