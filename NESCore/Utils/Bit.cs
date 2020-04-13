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
        public static bool Test(byte b, Flags pos)
        {
            return Test(b, (int) pos);
        }
        
        /// <summary>
        /// Tests whether a given position in a byte is set to one or zero
        /// </summary>
        /// <param name="b">byte to test</param>
        /// <param name="pos">the position within the byte to test, starting at zero</param>
        /// <returns>True or false for one or zero</returns>
        public static bool Test(byte b, int pos)
        {
            var bitSet = (b >> pos);
            return (bitSet & 1) == 1;
        }
        

        public static void Set(ref byte b, Flags bitPos)
        {
            Set(ref b, (int) bitPos);
        }
        
        public static void Set(ref byte b, in int bitPos)
        {
            b |= (byte)(1 << bitPos);
        }

        public static void Clear(ref byte b, Flags bitPos)
        {
            Clear(ref b, (int) bitPos);
        }
        
        public static void Clear(ref byte b, int bitPos)
        {
            b &= (byte)~(1 << bitPos);
        }

        /// <summary>
        /// Sets the bit specific position in the byte to 1|0 acording to the specified value
        /// </summary>
        public static void Val(ref byte b, Flags position, bool value)
        {
            if (value)
            {
                Set(ref b, position);
            }
            else
            {
                Clear(ref b, position);
            }
        }
        
        /// <summary>
        /// Sets the bit specific position in the byte to 1|0 acording to the specified value
        /// </summary>
        public static void Val(ref byte b, int position, bool value)
        {
            if (value)
            {
                Set(ref b, position);
            }
            else
            {
                Clear(ref b, position);
            }
        }
    }
}
