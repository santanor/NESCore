using System.IO;
using NESCore.Mappers;

namespace NESCore
{
    public class Cartridge
    {
        public IMapper mapper;
        public ROM Rom;
        
        public byte Byte(ushort addr)
        {
            var mappedAddr = mapper.Map(addr);
            return Rom.prgROM[mappedAddr];
        }
        
        public byte Byte(int addr) => Byte((ushort) addr);

        public ushort Word(int addr) => Word((ushort) addr);
        public ushort Word(ushort addr)
        {
            var result = new byte[2];
            result[0] = Byte(addr);
            result[1] = Byte((ushort)(addr + 1));

            return result.ToWord();
        }

        public static Cartridge FromFile(string path)
        {
            var cartridge = new Cartridge();
            var rom = new ROM();
            
            if (!File.Exists(path))
            {
                return null;
            }

            var reader = File.OpenRead(path);

            rom.nesTitle = reader.Nextbytes(4);
            rom.numPRGPages = (byte)reader.ReadByte();
            rom.numCHRPages = (byte)reader.ReadByte();
            rom.flags6 = (byte)reader.ReadByte();
            rom.flags7 = (byte)reader.ReadByte();
            rom.endOfHeader = reader.Nextbytes(8);

            //Check the third bit to check if the ROM has a trainer
            if (Bit.Test(rom.flags6, 3))
            {
                //if the trainer is there, then it's 512 bytes long. Always.
                rom.trainer = reader.Nextbytes(ROM.TrainerSize);
            }

            rom.prgROM = reader.Nextbytes((int)rom.numPRGPages * ROM.PrgPageSize);
            rom.chrROM = reader.Nextbytes((int)rom.numCHRPages * ROM.ChrPageSize);

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
}