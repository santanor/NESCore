using System;
using Serilog;

namespace NESCore
{
    public class CPU
    {
        /// <summary>
        /// Accumulator, deal with carry, overflow and so on...
        /// </summary>
        public byte A;

        /// <summary>
        /// General purpose register
        /// </summary>
        public byte X;

        /// <summary>
        /// General purpose register
        /// </summary>
        public byte Y;

        /// <summary>
        /// The opcode of this cycle
        /// </summary>
        public byte currentOpcode;

        /// <summary>
        /// Counter of elapsed cycles (Hz) this current second.
        /// </summary>
        public int cyclesThisSec;

        /// <summary>
        /// Flags of the status register:
        /// The processor status register has 8 bits, where 7 are used as flags: (From 7 to 0)
        /// N = negative flag (1 when result is negative)
        /// V = overflow flag (1 on signed overflow)
        /// # = unused (always 1)
        /// B = break flag (1 when interrupt was caused by a BRK)
        /// D = decimal flag (1 when CPU in BCD mode)
        /// I = IRQ flag (when 1, no interrupts will occur (exceptions are IRQs forced by BRK and NMIs))
        ///   * Z = zero flag (1 when all bits of a result are 0)
        /// C = carry flag (1 on unsigned overflow)
        /// </summary>
        public byte P;

        /// <summary>
        /// Program Counter
        /// </summary>
        public ushort PC;

        /// <summary>
        /// Stack Pointer, from 0x100 to 0x1FF address
        /// </summary>
        public byte SP;

        /// <summary>
        /// Speed of the CPU in Hz. Used to slow down the emulation to match the NES's clock speed
        /// </summary>
        public int speed;

        public RAM Ram;

        public CPU()
        {
            Ram = new RAM(RAM.RAM_SIZE, this);
        }

        public void PowerUp()
        {
            X = 0x00;
            A = 0x00;
            Y = 0x00;
            P = 0x34;
            SP = 0xFD;
            Ram.WriteByte(0x4017, 0x00);

            for (ushort i = 0x4000; i <= 0x4013; i++)
            {
                Ram.WriteByte(i, 0x00);
            }
        }

        public void Run()
        {
            while (true)
            {
                Cycle();
            }
        }

        public void Cycle()
        {
            var opcode = Ram.Byte(PC);

            var pageCrossed = opcode switch
            {
                0x00 => Invalid(opcode),         // Breakpoint
                0x01 => OraIndirectX(),
                0x02 => Invalid(opcode),
                0x03 => Invalid(opcode),
                0x04 => Invalid(opcode),
                0x05 => OraZPage(),
                0x06 => Invalid(opcode),
                0x07 => Invalid(opcode),
                0x08 => Invalid(opcode),
                0x09 => OraImmediate(),
                0x0A => Invalid(opcode),
                0x0B => Invalid(opcode),
                0x0C => Invalid(opcode),
                0x0D => OraAbsolute(),
                0x0E => Invalid(opcode),
                0x0F => Invalid(opcode),
                0x10 => Invalid(opcode),
                0x11 => OraIndirectY(),
                0x12 => Invalid(opcode),
                0x13 => Invalid(opcode),
                0x14 => Invalid(opcode),
                0x15 => OraZPageX(),
                0x16 => Invalid(opcode),
                0x17 => Invalid(opcode),
                0x18 => Invalid(opcode),
                0x19 => OraAbsoluteY(),
                0x1A => Invalid(opcode),
                0x1B => Invalid(opcode),
                0x1C => Invalid(opcode),
                0x1D => OraAbsoluteX(),
                0x1E => Invalid(opcode),
                0x1F => Invalid(opcode),
                _ => Invalid(opcode)
            };

            if (pageCrossed)
            {
                cyclesThisSec++;
            }
        }

        /// <summary>
        /// Invalid Opcode, logs the error to file
        /// </summary>
        private bool Invalid(byte opcode)
        {
            Log.Error($"Unkown OPcode: {opcode:X}");
            return true;
        }

        bool OraIndirectX()
        {
            return true;
        }

        bool OraIndirectY()
        {
            return true;
        }

        bool OraAbsoluteX()
        {
            return true;
        }

        bool OraAbsoluteY()
        {
            return true;
        }

        bool OraAbsolute()
        {
            return true;
        }

        bool OraZPageX()
        {
            return true;
        }

        bool OraZPageY()
        {
            return true;
        }

        bool OraZPage()
        {
            return true;
        }

        bool OraImmediate()
        {
            return true;
        }
    }
}
