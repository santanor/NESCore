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
        public NES Nes;

        public CPU(NES nes)
        {
            Ram = new RAM(RAM.RAM_SIZE, this);
            Nes = nes;
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
            while (Nes.running)
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
                0x02 => Halt(),
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
                0x12 => Halt(),
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
                0x20 => Invalid(opcode),
                0x21 => Invalid(opcode),
                0x22 => Halt(),
                0x23 => Invalid(opcode),
                0x24 => Invalid(opcode),
                0x25 => Invalid(opcode),
                0x26 => Invalid(opcode),
                0x27 => Invalid(opcode),
                0x28 => Invalid(opcode),
                0x29 => Invalid(opcode),
                0x2A => Invalid(opcode),
                0x2C => Invalid(opcode),
                0x2D => Invalid(opcode),
                0x2E => Invalid(opcode),
                0x2F => Invalid(opcode),
                0x30 => Invalid(opcode),
                0x31 => Invalid(opcode),
                0x32 => Halt(),
                0x33 => Invalid(opcode),
                0x34 => Invalid(opcode),
                0x35 => Invalid(opcode),
                0x36 => Invalid(opcode),
                0x37 => Invalid(opcode),
                0x38 => Invalid(opcode),
                0x39 => Invalid(opcode),
                0x3A => Invalid(opcode),
                0x3B => Invalid(opcode),
                0x3C => Invalid(opcode),
                0x3D => Invalid(opcode),
                0x3E => Invalid(opcode),
                0x3F => Invalid(opcode),
                0x40 => Invalid(opcode),
                0x41 => Invalid(opcode),
                0x42 => Halt(),
                0x43 => Invalid(opcode),
                0x44 => Invalid(opcode),
                0x45 => Invalid(opcode),
                0x46 => Invalid(opcode),
                0x47 => Invalid(opcode),
                0x48 => Invalid(opcode),
                0x49 => Invalid(opcode),
                0x4A => Invalid(opcode),
                0x4B => Invalid(opcode),
                0x4C => Invalid(opcode),
                0x4D => Invalid(opcode),
                0x4E => Invalid(opcode),
                0x4F => Invalid(opcode),
                0x50 => Invalid(opcode),
                0x51 => Invalid(opcode),
                0x52 => Halt(),
                0x53 => Invalid(opcode),
                0x54 => Invalid(opcode),
                0x55 => Invalid(opcode),
                0x56 => Invalid(opcode),
                0x57 => Invalid(opcode),
                0x58 => Invalid(opcode),
                0x59 => Invalid(opcode),
                0x5A => Invalid(opcode),
                0x5B => Invalid(opcode),
                0x5C => Invalid(opcode),
                0x5D => Invalid(opcode),
                0x5E => Invalid(opcode),
                0x5F => Invalid(opcode),
                0x60 => Invalid(opcode),
                0x61 => Invalid(opcode),
                0x62 => Halt(),
                0x63 => Invalid(opcode),
                0x64 => Invalid(opcode),
                0x65 => Invalid(opcode),
                0x66 => Invalid(opcode),
                0x67 => Invalid(opcode),
                0x68 => Invalid(opcode),
                0x69 => Invalid(opcode),
                0x6A => Invalid(opcode),
                0x6B => Invalid(opcode),
                0x6C => Invalid(opcode),
                0x6D => Invalid(opcode),
                0x6E => Invalid(opcode),
                0x6F => Invalid(opcode),
                0x70 => Invalid(opcode),
                0x71 => Invalid(opcode),
                0x72 => Halt(),
                0x73 => Invalid(opcode),
                0x74 => Invalid(opcode),
                0x75 => Invalid(opcode),
                0x76 => Invalid(opcode),
                0x77 => Invalid(opcode),
                0x78 => Invalid(opcode),
                0x79 => Invalid(opcode),
                0x7A => Invalid(opcode),
                0x7B => Invalid(opcode),
                0x7C => Invalid(opcode),
                0x7D => Invalid(opcode),
                0x7E => Invalid(opcode),
                0x7F => Invalid(opcode),
                0x80 => Invalid(opcode),
                0x81 => Invalid(opcode),
                0x82 => Invalid(opcode),
                0x83 => Invalid(opcode),
                0x84 => Invalid(opcode),
                0x85 => Invalid(opcode),
                0x86 => Invalid(opcode),
                0x87 => Invalid(opcode),
                0x88 => Invalid(opcode),
                0x89 => Invalid(opcode),
                0x8A => Invalid(opcode),
                0x8B => Invalid(opcode),
                0x8C => Invalid(opcode),
                0x8D => Invalid(opcode),
                0x8E => Invalid(opcode),
                0x8F => Invalid(opcode),
                0x90 => Invalid(opcode),
                0x91 => Invalid(opcode),
                0x92 => Halt(),
                0x93 => Invalid(opcode),
                0x94 => Invalid(opcode),
                0x95 => Invalid(opcode),
                0x96 => Invalid(opcode),
                0x97 => Invalid(opcode),
                0x98 => Invalid(opcode),
                0x99 => Invalid(opcode),
                0x9A => Invalid(opcode),
                0x9B => Invalid(opcode),
                0x9C => Invalid(opcode),
                0x9D => Invalid(opcode),
                0x9E => Invalid(opcode),
                0x9F => Invalid(opcode),
                0xA0 => Invalid(opcode),
                0xA1 => Invalid(opcode),
                0xA2 => Invalid(opcode),
                0xA3 => Invalid(opcode),
                0xA4 => Invalid(opcode),
                0xA5 => Invalid(opcode),
                0xA6 => Invalid(opcode),
                0xA7 => Invalid(opcode),
                0xA8 => Invalid(opcode),
                0xA9 => Invalid(opcode),
                0xAA => Invalid(opcode),
                0xAB => Invalid(opcode),
                0xAC => Invalid(opcode),
                0xAD => Invalid(opcode),
                0xAE => Invalid(opcode),
                0xAF => Invalid(opcode),
                0xB0 => Invalid(opcode),
                0xB1 => Invalid(opcode),
                0xB2 => Halt(),
                0xB3 => Invalid(opcode),
                0xB4 => Invalid(opcode),
                0xB5 => Invalid(opcode),
                0xB6 => Invalid(opcode),
                0xB7 => Invalid(opcode),
                0xB8 => Invalid(opcode),
                0xB9 => Invalid(opcode),
                0xBA => Invalid(opcode),
                0xBB => Invalid(opcode),
                0xBC => Invalid(opcode),
                0xBD => Invalid(opcode),
                0xBE => Invalid(opcode),
                0xBF => Invalid(opcode),
                0xC0 => Invalid(opcode),
                0xC1 => Invalid(opcode),
                0xC2 => Invalid(opcode),
                0xC3 => Invalid(opcode),
                0xC4 => Invalid(opcode),
                0xC5 => Invalid(opcode),
                0xC6 => Invalid(opcode),
                0xC7 => Invalid(opcode),
                0xC8 => Invalid(opcode),
                0xC9 => Invalid(opcode),
                0xCA => Invalid(opcode),
                0xCB => Invalid(opcode),
                0xCC => Invalid(opcode),
                0xCD => Invalid(opcode),
                0xCE => Invalid(opcode),
                0xCF => Invalid(opcode),
                0xD0 => Invalid(opcode),
                0xD1 => Invalid(opcode),
                0xD2 => Halt(),
                0xD3 => Invalid(opcode),
                0xD4 => Invalid(opcode),
                0xD5 => Invalid(opcode),
                0xD6 => Invalid(opcode),
                0xD7 => Invalid(opcode),
                0xD8 => Invalid(opcode),
                0xD9 => Invalid(opcode),
                0xDA => Invalid(opcode),
                0xDB => Invalid(opcode),
                0xDC => Invalid(opcode),
                0xDD => Invalid(opcode),
                0xDE => Invalid(opcode),
                0xDF => Invalid(opcode),
                0xE0 => Invalid(opcode),
                0xE1 => Invalid(opcode),
                0xE2 => Invalid(opcode),
                0xE3 => Invalid(opcode),
                0xE4 => Invalid(opcode),
                0xE5 => Invalid(opcode),
                0xE6 => Invalid(opcode),
                0xE7 => Invalid(opcode),
                0xE8 => Invalid(opcode),
                0xE9 => Invalid(opcode),
                0xEA => Invalid(opcode),
                0xEB => Invalid(opcode),
                0xEC => Invalid(opcode),
                0xED => Invalid(opcode),
                0xEE => Invalid(opcode),
                0xEF => Invalid(opcode),
                0xF0 => Invalid(opcode),
                0xF1 => Invalid(opcode),
                0xF2 => Halt(),
                0xF3 => Invalid(opcode),
                0xF4 => Invalid(opcode),
                0xF5 => Invalid(opcode),
                0xF6 => Invalid(opcode),
                0xF7 => Invalid(opcode),
                0xF8 => Invalid(opcode),
                0xF9 => Invalid(opcode),
                0xFA => Invalid(opcode),
                0xFB => Invalid(opcode),
                0xFC => Invalid(opcode),
                0xFD => Invalid(opcode),
                0xFE => Invalid(opcode),
                0xFF => Invalid(opcode),
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

        #region ASL Arithmetic Shift Left. shifts all bits left one position. 0 is shifted into bit 0 and the original bit 7 is shifted into the Carry.

        private bool Asl(ref byte param, int cycles, int pcIncrease)
        {
            LogInstruction(pcIncrease -1, $"ASL #${param:X}");

            var shifted = param << 1;
            return true;
        }

        private bool AslAccumulator() => Asl(ref A, 2, 1);
        private bool AslZPage()
        {
            var addr = Ram.ZPage(Ram.Byte(PC + 1));
            var param = Ram.ZPageParam();
            Asl(ref param, 5, 2);

            Ram.WriteByte(addr, param);
            return true;
        }

        private bool AslZPageX()
        {
            var addr = Ram.ZPageX(Ram.Byte(PC + 1));
            var param = Ram.ZPageXParam();
            Asl(ref param, 6, 2);
            Ram.WriteByte(addr, param);

            return true;
        }

        private bool AslAbsolute()
        {
            var addr = Ram.Absolute(Ram.Word(PC + 1));
            var param = Ram.AbsoluteParam();
            Asl(ref param, 6, 3);
            Ram.WriteByte(addr, param);

            return true;
        }

        private bool AslAbsoluteX()
        {
            var addr = Ram.AbsoluteX(Ram.Word(PC + 1));
            var param = Ram.AbsoluteXParam();
            Asl(ref param, 7, 3);
            Ram.WriteByte(addr, param);

            return true;
        }

        #endregion

        #region Halt. Kills the machine, bam, pum ded, gone gurl....

        bool Halt()
        {
            LogInstruction(0, "KILL");
            Nes.running = false;
            return true;
        }

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
