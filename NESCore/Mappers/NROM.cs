using System.Reflection.Metadata;

namespace NESCore.Mappers
{
    /// <summary>
    /// Mapper 0
    /// </summary>
    public class NROM
    {
        private NES nes;
        public const ushort FirstRomPage = 0x8000;
        public const ushort SecondRomPage = 0xC000;

        public NROM(NES nes)
        {
            this.nes = nes;
        }

        public void Apply(ROM rom)
        {
            for (ushort i = 0; i < ROM.PrgPageSize; i++)
            {
                nes.Ram.WriteByte((ushort)(FirstRomPage + i), rom.prgROM[i]);
            }

            // if there's only one page then start at 0 to mirror it, otherwise continue
            // with the copy
            var mirrorStartingPoint = rom.numPRGPages == 1 ? 0 : ROM.PrgPageSize;

            for (ushort i = 0; i < ROM.PrgPageSize; i++)
            {
                nes.Ram.WriteByte((ushort)(SecondRomPage + i), rom.prgROM[mirrorStartingPoint + i]);
            }

            //TODO COPY THE VRAM STUFF
        }

    }
}
