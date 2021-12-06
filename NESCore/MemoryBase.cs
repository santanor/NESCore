namespace NESCore;

public abstract class MemoryBase
{
    protected byte[] bank;

    protected MemoryBase(int size)
    {
        bank = new byte[size];
    }


    /// <summary>
    ///     Reads a byte from the specified memory location
    /// </summary>
    /// <param name="address"></param>
    /// <returns></returns>
    public byte Byte(ushort address)
    {
        return bank[address];
    }

    public byte Byte(int address)
    {
        return Byte((ushort) address);
    }

    /// <summary>
    ///     Reads two bytes from the starting position and returns it as a memory address.
    ///     It will do the calculation for you, that's how nice this bad boy is
    /// </summary>
    public ushort Word(ushort address)
    {
        var result = new byte[2];
        result[0] = Byte(address);
        result[1] = Byte((ushort) (address + 1));

        return result.ToWord();
    }

    public ushort Word(int address)
    {
        return Word((ushort) address);
    }

    /// <summary>
    ///     Writes a byte to the specified memory location
    /// </summary>
    /// <param name="addr"></param>
    /// <param name="value"></param>
    public void WriteByte(ushort addr, byte value)
    {
        bank[addr] = value;
    }

    /// <summary>
    ///     Writes a byte to the specified memory location
    /// </summary>
    /// <param name="addr"></param>
    /// <param name="value"></param>
    public void WriteByte(int addr, byte value)
    {
        bank[(ushort)addr] = value;
    }

    /// <summary>
    ///     Writes a word to the specified memory location, the word
    ///     is internally converted and flipped to be inserted into the memory bank
    /// </summary>
    /// <param name="addr"></param>
    /// <param name="value"></param>
    public void WriteWord(ushort addr, ushort value)
    {
        var bytes = value.ToBytes();
        for (var i = 0; i < bytes.Length; i++) bank[addr + i] = bytes[i];
    }
}