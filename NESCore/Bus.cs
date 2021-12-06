namespace NESCore;

public static class Bus
{
    public static Cartridge Cartridge { get; set; }
    public static CPU Cpu { get; set; }
    public static RAM Ram;
    public static VRAM Vram;
    public static PPU Ppu;


    /// <summary>
    ///     Reads a byte from the specified memory location
    /// </summary>
    /// <param name="address"></param>
    /// <returns></returns>
    public static byte Byte(ushort address)
    {
        //RAM Range
        if (address <= 0x1FFF)
            return Ram.Byte(address & 0x07FF);
        //This goes to the cartridge
        if (address >= 0x4020)
        {
            return Cartridge.Byte(address);
        }

        return 0x00;
    }

    /// <summary>
    ///  Reads a byte from the specified VRAM memory location
    /// </summary>
    /// <param name="address"></param>
    /// <returns></returns>
    public static byte VByte(ushort address)
    {
        if (address <= 0x2FFF)
        {
            return Cartridge.VByte(address);
        }
        if (address <= 0x3FFF) //Mirrors of $2000-$2EFF
        {
            return Vram.Byte(address & 0x2EFF);
        }
        if (address <= 0x3F1F)
        {
            return Vram.Byte(address);
        }
        
        //Mirrors of $3F00-$3F1F
        return Vram.Byte(address & 0x3F1F);
    }

    /// <summary>
    ///  Reads a byte from the specified VRAM memory location
    /// </summary>
    /// <param name="address"></param>
    /// <returns></returns>
    public static byte VByte(int address)
    {
        return VByte((ushort) address);
    }

    public static byte Byte(int address)
    {
        return Byte((ushort) address);
    }

    /// <summary>
    ///     Reads two bytes from the starting position and returns it as a memory address.
    ///     It will do the calculation for you, that's how nice this bad boy is
    /// </summary>
    public static ushort Word(ushort address)
    {
        //RAM Range
        if (address <= 0x1FFF)
            return Ram.Word(address & 0x07FF);
        if (address >= 0x4020)
        {
            return Cartridge.Word(address);
        }

        return 0x00;
    }

    public static ushort Word(int address)
    {
        return Word((ushort) address);
    }

    /// <summary>
    ///     Writes a byte to the specified memory location
    /// </summary>
    /// <param name="addr"></param>
    /// <param name="value"></param>
    public static void WriteByte(ushort addr, byte value)
    {
        if (addr <= 0x1FFF) Ram.WriteByte(addr & 0x07FF, value);
    }

    public static void WriteByte(int addr, byte value)
    {
        WriteByte((ushort)addr, value);
    }

    /// <summary>
    ///     Writes a byte to the specified VRAM  location
    /// </summary>
    /// <param name="addr"></param>
    /// <param name="value"></param>
    public static void VWriteByte(ushort address, byte value)
    {
        if (address <= 0x2FFF)
        {
            Cartridge.WriteCHRByte(address, value);
        }
        else if (address <= 0x3FFF) //Mirrors of $2000-$2EFF
        {
            Vram.WriteByte(address & 0x2EFF, value);
        }
        else if (address <= 0x3F1F)
        {
            Vram.WriteByte(address, value);
        }
        else//Mirrors of $3F00-$3F1F
        {
            Vram.WriteByte(address & 0x3F1F, value);
        }
    }

    /// <summary>
    ///     Writes a byte to the specified VRAM location
    /// </summary>
    /// <param name="addr"></param>
    /// <param name="value"></param>
    public static void VWriteByte(int address, byte value)
    {
        VWriteByte((ushort)address, value);
    }
    /// <summary>
    ///     Writes a word to the specified memory location, the word
    ///     is internally converted and flipped to be inserted into the memory bank
    /// </summary>
    /// <param name="addr"></param>
    /// <param name="value"></param>
    public static void WriteWord(ushort addr, ushort value)
    {
        if (addr <= 0x1FFF) Ram.WriteWord((ushort) (addr & 0x07FF), value);
    }
}