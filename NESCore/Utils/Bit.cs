namespace NESCore
{
    public static class Bit
    {

        /// <summary>
        /// Tests whether a given position in a byte is set to one or zero
        /// </summary>
        /// <param name="b">byte to test</param>
        /// <param name="pos">the position within the byte to test, starting at zero</param>
        /// <returns>True or false for one or zero</returns>
        public static bool Test(byte b, int pos)
        {
            var bitSet = (b >> (pos));
            return (bitSet & 1) == 1;
        }

    }
}
