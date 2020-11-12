namespace NESCore
{
    public class Bus
    {
        public CPU Cpu;
        public PPU Ppu;
        public RAM Ram;
        public Cartridge Rom;
        
        
        /// <summary>
        /// Reads a byte from the specified memory location
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public byte Byte(ushort address)
        {
            //RAM Range
            if (address <= 0x1FFF)
            {
                return Ram.Byte(address & 0x07FF);
            }

            return 0x00;
        }

        public byte Byte(int address) => Byte((ushort) address);

        /// <summary>
        /// Reads two bytes from the starting position and returns it as a memory address.
        /// It will do the calculation for you, that's how nice this bad boy is
        /// </summary>
        public ushort Word(ushort address)
        {
            //RAM Range
            if (address <= 0x1FFF)
            {
                return Ram.Word(address & 0x07FF);
            }

            return 0x00;
        }

        public ushort Word(int address) => Word((ushort) address);
        
        /// <summary>
        /// Writes a byte to the specified memory location
        /// </summary>
        /// <param name="addr"></param>
        /// <param name="value"></param>
        public void WriteByte(ushort addr, byte value)
        {
            if (addr <= 0x1FFF)
            {
                Ram.WriteByte((ushort)(addr & 0x07FF), value);
            }
        }

        /// <summary>
        /// Writes a word to the specified memory location, the word
        /// is internally converted and flipped to be inserted into the memory bank
        /// </summary>
        /// <param name="addr"></param>
        /// <param name="value"></param>
        public void WriteWord(ushort addr, ushort value)
        {
            if (addr <= 0x1FFF)
            {
                Ram.WriteWord((ushort)(addr & 0x07FF), value);
            }
        }
    }
}