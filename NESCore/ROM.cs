using System;
using System.IO;
using System.Linq;
using System.Numerics;

namespace NESCore
{
    public class ROM
    {

        public const int PrgPageSize = 16384;
        public const int ChrPageSize = 8192;
        public const int TrainerSize = 512;

        /// <summary>
        /// Header
        /// Bytes 0-3 of the ROM
        /// </summary>
        public byte[] nesTitle;

        /// <summary>
        /// for iNES format it should always be 0xA1 (26)
        /// </summary>
        public byte fileFormat;

        /// <summary>
        /// Byte4. Number of 16384 byte program ROM pages. Byte4
        /// </summary>
        public uint numPRGPages;

        /// <summary>
        /// Byte5. Number of 8192 byte character ROM pages (0 indicates CHR RAM).
        /// </summary>
        public uint numCHRPages;

        /// <summary>
        /// byte 6
        /// NNNN FTBM
        /// N: Lower 4 bits of the mapper number
        /// F: Four screen mode. 0 = no, 1 = yes. (When set, the M bit has no effect)
        /// T: Trainer.  0 = no trainer present, 1 = 512 byte trainer at 7000-71FFh
        /// B: SRAM at 6000-7FFFh battery backed.  0= no, 1 = yes
        /// M: Mirroring.  0 = horizontal, 1 = vertical.
        /// </summary>
        public byte flags6;

        /// <summary>
        /// byte 7
        /// NNNN xxPV
        /// N: Upper 4 bits of the mapper number
        /// P: Playchoice 10.  When set, this is a PC-10 game
        /// V: Vs. Unisystem.  When set, this is a Vs. game
        /// x: these bits are not used in iNES.
        /// </summary>
        public byte flags7;

        /// <summary>
        /// bytes 8-15.
        /// These bytes are not used, and should be 00h.
        /// </summary>
        public byte[] endOfHeader;

        /// <summary>
        /// Index of the mapper to be used
        /// </summary>
        public int mapper;

        /// <summary>
        /// Actual ROM program
        /// </summary>
        public byte[] prgROM;

        /// <summary>
        /// trainer data (if anything)
        /// </summary>
        public byte[] trainer;

        /// <summary>
        /// Character ROM
        /// </summary>
        public byte[] chrROM;
        
    }
}
