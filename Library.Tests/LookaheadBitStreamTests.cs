namespace Library.Tests;

public class LookaheadBitStreamTests
{
    [Fact]
    public void TestReadingBits()
    {
        var ms = new MemoryStream([0b00000011, 0b00000010, 0b00000101])
        {
            Position = 0
        };

        var bs = new LookaheadBitStream(ms);
        Assert.Equal(3, bs.ReadBits(8));
        bs.ReadBits(4);
        Assert.Equal(0, bs.ReadBits(2));
        Assert.Equal(2, bs.ReadBits(2));

        Assert.Equal(0, bs.ReadBits(5));
        Assert.Equal(5, bs.ReadBits(3));
    }

    [Fact]
    public void TestBitLookahead()
    {
        var ms = new MemoryStream([0b00000011, 0b00000010, 0b00000101])
        {
            Position = 0
        };

        var bs = new LookaheadBitStream(ms);
        Assert.Equal(3, bs.PeekBits(8));
        Assert.Equal(3, bs.ReadBits(8));
        bs.ReadBits(4);
        Assert.Equal(0, bs.ReadBits(2));
        Assert.Equal(1, bs.PeekBits(1));
        Assert.Equal(2, bs.ReadBits(2));

        Assert.Equal(0, bs.ReadBits(5));
        Assert.Equal(5, bs.ReadBits(3));
    }
}
