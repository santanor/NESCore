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

    }
}
