using System.Buffers.Binary;

namespace Library;

public sealed class LookaheadBitStream
{
    struct State
    {
        public int CurrentByte, BitPosition;
        public long BytePosition;
    }

    private readonly Stream _backingStream;

    private int _currentByte;
    private int _bitPosition;

    private readonly bool _streamCanSeek = false;

    public LookaheadBitStream(Stream backingStream)
    {
        _backingStream = backingStream;
        _streamCanSeek = backingStream.CanSeek;

        _currentByte = backingStream.ReadByte();
        _bitPosition = 0;

        if (_currentByte == -1)
            throw new EndOfStreamException();
    }

    private State GetState() => new() { BitPosition = _bitPosition, CurrentByte = _currentByte, BytePosition = _backingStream.Position };

    private void UseState(State state)
    {
        _bitPosition = state.BitPosition;
        _currentByte = state.CurrentByte;
        _backingStream.Position = state.BytePosition;
    }

    public bool ReadBit()
    {
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

    private readonly Queue<int> _buffer = new();

    public int PeekBits(int n)
    {
        if (_streamCanSeek)
        {
            State state = this.GetState();
            int r;
            try
            {
                r = this.ReadBits(n);
            }
            catch
            {
                this.UseState(state);
                throw;
            }
            this.UseState(state);
            return r;
        }

        while (_buffer.Count * 8 - _bitPosition < n)
        {
            int nextByte = _backingStream.ReadByte();
            if (nextByte == -1)
                throw new EndOfStreamException();

            _buffer.Enqueue(nextByte);
        }

        int tempBitPos = _bitPosition;
        int tempCurrentByte = _currentByte;
        var tempBuffer = new Queue<int>(_buffer);

        int b = 0;

        for (int i = 0; i < n; i++)
        {
            b <<= 1;

            if (tempBitPos == 8)
            {
                tempBitPos = 0;
                tempCurrentByte = tempBuffer.Dequeue();
            }

            bool bit = (tempCurrentByte & (1 << tempBitPos)) != 0;
            tempBitPos++;

            if (bit)
                b |= 1;
        }

        return b;
    }
}
