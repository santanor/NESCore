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
        // This goes to the PPU registers
        if (address >= PPUCTRL && address <= 0x3FFF)
        {
            Ppu.ReadRegister(address & 0x2007);// This doesn't do the writing to memory
        }
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
    ///     Push a byte on top of the stack
    /// </summary>
    /// <param name="value"></param>
    public static void PushByte(byte b)
    {
        var bankPointer = Cpu.SP + 0x100; // The stack is between 0x100 and 0x1FF
        WriteByte(bankPointer, b);
        Cpu.SP -= 1;
    }
    
    /// <summary>
    ///     Push a word on top of the stack, internally the word is flipped to reflect
    ///     the correct endian-ess
    /// </summary>
    /// <param name="value"></param>
    public static void PushWord(ushort value)
    {
        var bankPointer = (ushort) (Cpu.SP + 0xFF); // 100 - 1 but in hex -> 99 is 0xFF;
        WriteWord(bankPointer, value);
        Cpu.SP -= 2;
    }

    
    /// <summary>
    ///     Pulls the top-most byte from the stack
    /// </summary>
    /// <returns></returns>
    public static byte PopByte()
    {
        var value = Bus.Byte((ushort) (Cpu.SP + 1 + 0x100));
        Cpu.SP += 1;
        return value;
    }

    /// <summary>
    ///     Pops the 2 top-most bytes from the stack and returns them as a word
    /// </summary>
    /// <returns></returns>
    public static ushort PopWord()
    {
        var value = Bus.Word((ushort) (Cpu.SP + 1 + 0x100));
        Cpu.SP += 2;
        return value;
    }
    
    /// <summary>
    ///  Reads a byte from the specified VRAM memory location
    /// </summary>
    /// <param name="address"></param>
    /// <returns></returns>
    public static byte VByte(ushort address)
    {
        if (address <= 0x1FFF)
        {
            return Cartridge.VByte(address);
        }
        if (address <= 0x3EFF) // Nametables and mirrors of nametables
        {
            return Cartridge.Byte(address);
        }
        if (address <= 0x3FFF) // Palette RAM and mirrors 
        {
            return (byte) (address - 0x3F00); // So that indexes are 0-based
        }
        
        // Mirrors of $3F00-$3F1F
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
        if (addr >= PPUCTRL && addr <= 0x3FFF)
        {
            Ppu.WriteRegister(addr & 0x2007, value);// This doesn't do the writing to memory
        }
        else if (addr <= 0x1FFF)
        {
            Ram.WriteByte(addr & 0x07FF, value);
        }
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
        if (address <= 0x2FFF & Cartridge != null)
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