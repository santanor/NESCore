namespace NESCore;

public static class OpcodeMetadata
{
    public static ushort[] Timings =
    {
        7, 6, 2, 8, 3, 3, 5, 5, 3, 2, 2, 2, 4, 4, 6, 6,
        2, 5, 2, 8, 4, 4, 6, 6, 2, 4, 2, 7, 4, 4, 7, 7,
        6, 6, 2, 8, 3, 3, 5, 5, 4, 2, 2, 2, 4, 4, 6, 6,
        2, 5, 2, 8, 4, 4, 6, 6, 2, 4, 2, 7, 4, 4, 7, 7,
        6, 6, 2, 8, 3, 3, 5, 5, 3, 2, 2, 2, 3, 4, 6, 6,
        2, 5, 2, 8, 4, 4, 6, 6, 2, 4, 2, 7, 4, 4, 7, 7,
        6, 6, 2, 8, 3, 3, 5, 5, 4, 2, 2, 2, 5, 4, 6, 6,
        2, 5, 2, 8, 4, 4, 6, 6, 2, 4, 2, 7, 4, 4, 7, 7,
        2, 6, 2, 6, 3, 3, 3, 3, 2, 2, 2, 2, 4, 4, 4, 4,
        2, 6, 2, 6, 4, 4, 4, 4, 2, 5, 2, 5, 5, 5, 5, 5,
        2, 6, 2, 6, 3, 3, 3, 3, 2, 2, 2, 2, 4, 4, 4, 4,
        2, 5, 2, 5, 4, 4, 4, 4, 2, 4, 2, 4, 4, 4, 4, 4,
        2, 6, 2, 8, 3, 3, 5, 5, 2, 2, 2, 2, 4, 4, 6, 6,
        2, 5, 2, 8, 4, 4, 6, 6, 2, 4, 2, 7, 4, 4, 7, 7,
        2, 6, 2, 8, 3, 3, 5, 5, 2, 2, 2, 2, 4, 4, 6, 6,
        2, 5, 2, 8, 4, 4, 6, 6, 2, 4, 2, 7, 4, 4, 7, 7
    };

    public static ushort[] Size =
    {
        //  0  1  2  3  4  5  6  7  8  9  A  B  C  D  E  F
        2, 2, 0, 2, 2, 2, 2, 2, 1, 2, 1, 3, 3, 3, 3, 3, // 0x00
        2, 2, 0, 2, 2, 2, 2, 2, 1, 3, 1, 3, 3, 3, 3, 3, // 0x01
        0, 2, 0, 2, 2, 2, 2, 2, 1, 2, 1, 3, 3, 3, 3, 3, // 0x02
        2, 2, 0, 2, 2, 2, 2, 2, 1, 3, 1, 3, 3, 3, 3, 3, // 0x03
        0, 2, 0, 2, 2, 2, 2, 2, 1, 2, 1, 3, 0, 3, 3, 3, // 0x04
        2, 2, 0, 2, 2, 2, 2, 2, 1, 3, 1, 3, 3, 3, 3, 3, // 0x05
        1, 2, 0, 2, 2, 2, 2, 2, 1, 2, 1, 3, 0, 3, 3, 3, // 0x06
        2, 2, 0, 2, 2, 2, 2, 2, 1, 3, 1, 3, 3, 3, 3, 3, // 0x07
        2, 2, 0, 2, 2, 2, 2, 2, 1, 0, 1, 3, 3, 3, 3, 3, // 0x08
        2, 2, 0, 2, 2, 2, 2, 2, 1, 3, 1, 3, 0, 3, 0, 3, // 0x09
        2, 2, 2, 2, 2, 2, 2, 2, 1, 2, 1, 3, 3, 3, 3, 3, // 0x0A
        2, 2, 0, 2, 2, 2, 2, 2, 1, 3, 1, 3, 3, 3, 3, 3, // 0x0B
        2, 2, 0, 2, 2, 2, 2, 2, 1, 2, 1, 3, 3, 3, 3, 3, // 0x0C
        2, 2, 0, 2, 2, 2, 2, 2, 1, 3, 1, 3, 3, 3, 3, 3, // 0x0D
        2, 2, 0, 2, 2, 2, 2, 2, 1, 2, 1, 2, 3, 3, 3, 3, // 0x0E
        2, 2, 0, 2, 2, 2, 2, 2, 1, 3, 1, 3, 3, 3, 3, 3 // 0x0F
    };
}