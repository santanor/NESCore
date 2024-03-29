using System.IO;

namespace NESCore;

public static class Extensions
{
    /// <summary>
    ///     Given a fileStream, returns the next N bytes from where the current stream position is located
    /// </summary>
    /// <returns></returns>
    public static byte[] Nextbytes(this Stream stream, int amount)
    {
        // Create the smallest array possible, which is either the entered amount
        // Or the remaining elements in the stream
        var arrayLength = Math.Min(amount, stream.Length - stream.Position);
        var result = new byte[arrayLength];
        for (var i = 0; i < arrayLength; i++) result[i] = (byte) stream.ReadByte();

        return result;
    }

    public static byte[] ToBytes(this short value)
    {
        var result = new byte[2];
        result[0] = (byte) (value & 0x00FF);
        result[1] = (byte) ((value & 0xFF00) >> 8);
        return result;
    }

    public static byte[] ToBytes(this ushort value)
    {
        var result = new byte[2];
        result[0] = (byte) (value & 0x00FF);
        result[1] = (byte) ((value & 0xFF00) >> 8);
        return result;
    }

    public static ushort ToWord(this byte[] value)
    {
        return (ushort) (value.Length switch
        {
            0 => 0,
            1 => value[0],
            _ => (value[1] << 8) + value[0]
        });
    }
}