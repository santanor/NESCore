using System.Reflection.Metadata;

namespace NESCore.Mappers
{
    /// <summary>
    /// Mapper 0
    /// </summary>
    public class NROM : IMapper
    {
        public const ushort FirstRomPage = 0x8000;
        public const ushort SecondRomPage = 0xC000;
        private ROM rom;

        public NROM(ROM rom)
        {
            this.rom = rom;
        }

        public ushort Map(ushort addr)
        {
            if (addr >= 0x8000 && addr <= 0xFFFF)
            {
                return (ushort) (addr & (rom.numPRGPages > 1 ? 0x7FFF : 0x3FFF));
            }

            return 0x0000;
        }
    }
}
