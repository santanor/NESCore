using System;
using System.IO;

namespace NESCore
{
    public static class Extensions
    {

        /// <summary>
        /// Given a fileStream, returns the next N bytes from where the current stream position is located
        /// </summary>
        /// <returns></returns>
        public static byte[] Nextbytes(this Stream stream, int amount)
        {
            var result = new byte[amount];
            for (var i = 0; i < amount; i++)
            {
                result[i] = (byte)stream.ReadByte();
            }

            return result;
        }

        public static byte[] ToBytes(this short value)
        {
            var result = new byte[2];
            result[0] = (byte)(value & 0x00FF);
            result[1] = (byte)((value & 0xFF00) >> 8);
            return result;
        }

        public static short ToWord(this byte[] value)
        {
            return (short)(value.Length switch
            {
                0 => 0,
                1 => value[0],
                _ => (value[1] << 8) + value[0]
            });
        }

    }
}
