using System.IO;
using NESCore.Mappers;

namespace NESCore;

public class Cartridge
{
    public IMapper mapper;
    public ROM Rom;

    /// <summary>
    /// Returns a byte from the PRG ROM. The mapper can intercept the address and apply some operations to it
    /// </summary>
    /// <param name="addr"></param>
    /// <returns></returns>
    public byte Byte(ushort addr)
    {
        addr = mapper.Map(addr);
        return Rom.prgROM[addr];
    }

    /// <summary>
    /// Returns a byte from the PRG ROM. The mapper can intercept the address and apply some operations to it
    /// </summary>
    /// <param name="addr"></param>
    /// <returns></returns>
    public byte Byte(int addr)
    {
        return Byte((ushort) addr);
    }

    /// <summary>
    /// Returns a byte from the CHR ROM. The mapper can intercept the address and apply some operations to it
    /// </summary>
    /// <param name="addr"></param>
    /// <returns></returns>
    public byte VByte(ushort addr)
    {
        //addr = mapper.Map(addr);
        return Rom.chrROM[addr];
    }

    /// <summary>
    /// Returns a byte from the PRG ROM. The mapper can intercept the address and apply some operations to it
    /// </summary>
    /// <param name="addr"></param>
    /// <returns></returns>
    public byte VByte(int addr)
    {
        return VByte((ushort) addr);
    }

    /// <summary>
    /// Returns a WORD from the PRG ROM. The mapper can intercept the address and apply some operations to it
    /// </summary>
    /// <param name="addr"></param>
    /// <returns></returns>
    public ushort Word(ushort addr)
    {
        addr = mapper.Map(addr);
        
        var result = new byte[2];
        result[0] = Rom.prgROM[addr];
        result[1] = Rom.prgROM[addr+1];

        return result.ToWord();
    }
    
    /// <summary>
    /// Returns a WORD from the PRG ROM. The mapper can intercept the address and apply some operations to it
    /// </summary>
    /// <param name="addr"></param>
    /// <returns></returns>
    public ushort Word(int addr)
    {
        return Word((ushort) addr);
    }

    public void WriteCHRByte(ushort address, byte value)
    {
        address = mapper.Map(address);
        Rom.chrROM[address] = value;
    }
    
    public static Cartridge FromFile(string path)
    {
        var cartridge = new Cartridge();
        var rom = new ROM();

        if (!File.Exists(path)) return null;

        var reader = File.OpenRead(path);

        rom.nesTitle = reader.Nextbytes(4);
        rom.numPRGPages = (byte) reader.ReadByte();
        rom.numCHRPages = (byte) reader.ReadByte();
        rom.flags6 = (byte) reader.ReadByte();
        rom.flags7 = (byte) reader.ReadByte();
        rom.endOfHeader = reader.Nextbytes(8);

        //Check the third bit to check if the ROM has a trainer
        if (Bit.Test(rom.flags6, 3))
            //if the trainer is there, then it's 512 bytes long. Always.
            rom.trainer = reader.Nextbytes(TRAINER_SIZE);

        rom.prgROM = reader.Nextbytes((int) rom.numPRGPages * PRG_PAGE_SIZE);
        rom.chrROM = reader.Nextbytes((int) rom.numCHRPages * CHR_PAGE_SIZE);

        cartridge.Rom = rom;
        cartridge.mapper = cartridge.GetMapperInstance(rom.mapper);

        return cartridge;
    }

    private IMapper GetMapperInstance(int romMapper)
    {
        return romMapper switch
        {
            0 => new NROM(Rom),
            _ => null
        };
    }
}