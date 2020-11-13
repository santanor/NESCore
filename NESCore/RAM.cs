using System;

namespace NESCore
{
    /// <summary>
    /// RAM MEMORY MAP
    /// Address range  Size  Device
    /// $0000-$07FF    $0800  2KB internal RAM
    /// $0800-$0FFF    $0800  -----------------------
    /// $1000-$17FF    $0800  Mirrors of $0000-$07FF
    /// $1800-$1FFF    $0800  -----------------------
    /// $2000-$2007    $0008  NES PPU registers
    /// $2008-$3FFF    $1FF8  Mirrors of $2000-2007 (repeats every 8 bytes)
    /// $4000-$4017    $0018  NES APU and I/O registers
    /// $4018-$401F    $0008  APU and I/O functionality that is normally disabled. See CPU Test Mode.
    /// $4020-$FFFF    $BFE0  Cartridge space: PRG ROM, PRG RAM, and mapper registers
    /// </summary>
    public class RAM : MemoryBase
    {
        public const int RAM_SIZE = 0x0800;
        public CPU Cpu;

        public RAM() : base(RAM_SIZE)
        {
        }

        public void Zero()
        {
            for (var i = 0; i < RAM_SIZE; i++)
            {
                bank[i] = 0;
            }
        }

    }
}
