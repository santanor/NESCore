using NESCore.Mappers;

namespace NESCore
{
    public class NES
    {
        public RAM Ram;
        public CPU Cpu;

        public NES()
        {
            Cpu = new CPU();
            Ram = Cpu.Ram;
            Cpu.PowerUp();
        }

        /// <summary>
        /// Loads the ROM into memory, applying the mapper and copying the necessary bits to where they should go
        /// </summary>
        /// <param name="rom"></param>
        public void LoadROM(ROM rom)
        {
            switch (rom.mapper)
            {
                case 0:
                    var nrom = new NROM(this);
                    nrom.Apply(rom);
                    break;

            }
        }

    }
}
