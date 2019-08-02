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
    public class RAM
    {
        public const int RAM_SIZE = 65536;

        private byte[] bank;

        private CPU cpu;

        public RAM(int size, CPU cpu)
        {
            this.cpu = cpu;
            bank = new byte[size];
        }

        /// <summary>
        /// Reads a byte from the specified memory location
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public byte Byte(ushort address)
        {
            return bank[address];
        }

        /// <summary>
        /// Reads two bytes from the starting position and returns it as a memory address.
        /// It will do the calculation for you, that's how nice this bad boy is
        /// </summary>
        public short Word(ushort address)
        {
            var result = new byte[2];
            result[0] = Byte(address);
            result[1] = Byte((ushort)(address + 1));

            return result.ToWord();
        }

        /// <summary>
        /// Writes a byte to the specified memory location
        /// </summary>
        /// <param name="addr"></param>
        /// <param name="value"></param>
        public void WriteByte(ushort addr, byte value)
        {
            bank[addr] = value;
        }

        public void WriteWord(ushort addr, short value)
        {
            var bytes = value.ToBytes();
            for (var i = 0; i < bytes.Length; i++)
            {
                bank[addr + i] = bytes[i];
            }
        }

        public void PushByte(byte value)
        {
            var bankPointer = (ushort)(cpu.SP + 0x100);
            WriteByte(bankPointer, value);
            cpu.SP -= 1;
        }

        public void PushWord(short value)
        {
            var bankPointer = (ushort)(cpu.SP + 0xFF); // 100 - 1 but in hex -> 99 is 0xFF;
            WriteWord(bankPointer, value);
            cpu.SP -= 2;
        }

        public byte PopByte()
        {
            var value = Byte((ushort)(cpu.SP + 1 + 0x100));
            cpu.SP += 1;
            return value;
        }

        public short PopWord()
        {
            var value = Word((ushort)(cpu.SP + 1 + 0x100));
            cpu.SP += 2;
            return value;
        }

        /// <summary>
        /// Clears the contents of the bank, setting them to 0
        /// </summary>
        public void Zero()
        {
            Array.Fill<byte>(bank, 0);
        }
    }
}
