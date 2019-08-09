using System;
using System.Text;
using Serilog;

namespace NESCore
{
    public enum Flags {Negative, Overflow, Unused, Break, Decimal, IRQ, Zero, Carry};

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
        /// Z = zero flag (1 when all bits of a result are 0)
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
                0x00 => Invalid(opcode),
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

        #region ORA. Logical OR on the acumulator, set the zero and negative flags


        bool Ora(byte param, int cycles, int pcIncrease)
        {
            LogInstruction(pcIncrease - 1, $"ORA #${param:X}");
            A = (byte)(A | param);

            Bit.Val(ref P, Flags.Zero, A == 0);
            Bit.Val(ref P, Flags.Negative, Bit.Test(A, 7));

            cyclesThisSec += cycles;
            PC += (ushort)pcIncrease;

            return true;
        }

        bool OraImmediate() => Ora(Ram.Byte(PC + 1), 2, 2);
        bool OraZPage() => Ora(Ram.ZPageParam(), 2, 2);
        bool OraZPageX() => Ora(Ram.ZPageXParam(), 4, 2);
        bool OraAbsolute() => Ora(Ram.AbsoluteParam(), 4, 3);
        bool OraAbsoluteX() => Ora(Ram.AbsoluteXParam(true), 4, 3);
        bool OraAbsoluteY() => Ora(Ram.AbsoluteYParam(true), 4, 3);
        bool OraIndirectX() => Ora(Ram.IndirectXParam(), 6,2);
        bool OraIndirectY() => Ora(Ram.IndirectYParam(true), 5, 2);



        #endregion



        private void LogInstruction(int numParams, string mnemonic)
        {
            var sb = new StringBuilder();
            sb.Append($"{PC:X} {currentOpcode:X} ", PC, currentOpcode);

            for (var i = 1; i <= numParams; i++) {
                sb.Append($"{Ram.Byte(PC + i):X} ");
            }

            sb.Append('\t');
            sb.Append(mnemonic);
            sb.Append("\t\t\t");
            sb.Append($"A:{A:X} X:{X:X} Y:{Y:X} P:{P:X} SP:{SP:X} CYC:{cyclesThisSec}");

            Log.Debug(sb.ToString());
        }
    }

}
