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
        public int Speed;

        public RAM Ram;

        private bool running;

        private Action[] opcodes;

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

            running = true;
            
            CreateOpcodeArray();
        }

        private void CreateOpcodeArray()
        {
            opcodes = new Action[]
            {
                Break, //0x00
                OraIndirectX, //0x01
                Halt, //0x02
                Invalid, //0x03
                Invalid, //0x04
                OraZPage, //0x05
                AslZPage, //0x06
                Invalid, //0x07
                Invalid, //0x08
                OraImmediate, //0x09
                AslAccumulator, //0x0A
                Invalid, //0x0B
                Invalid, //0x0C
                OraAbsolute, //0x0D
                AslAbsolute, //0x0E
                Invalid, //0x0F
                Invalid, //0x10
                OraIndirectY, //0x11
                Halt, //0x12
                Invalid, //0x13
                Invalid, //0x14
                OraZPageX, //0x15
                AslZPageX, //0x16
                Invalid, //0x17
                Invalid, //0x18
                OraAbsoluteY, //0x19
                Invalid, //0x1A
                Invalid, //0x1B
                Invalid, //0x1C
                OraAbsoluteX, //0x1D
                AslAbsoluteX, //0x1E
                Invalid, //0x20
                AndIndirectX, //0x21
                Halt, //0x22
                Invalid, //0x23
                Invalid, //0x24
                AndZPage, //0x25
                Invalid, //0x26
                Invalid, //0x27
                Invalid, //0x28
                AndImmediate, //0x29
                Invalid, //0x2A
                Invalid, //0x2C
                AndAbsolute, //0x2D
                Invalid, //0x2E
                Invalid, //0x2F
                Invalid, //0x30
                AndIndirectY, //0x31
                Halt, //0x32
                Invalid, //0x33
                Invalid, //0x34
                AndZPageX, //0x35
                Invalid, //0x36
                Invalid, //0x37
                Invalid, //0x38
                AndAbsoluteY, //0x39
                Invalid, //0x3A
                Invalid, //0x3B
                Invalid, //0x3C
                AndAbsoluteX, //0x3D
                Invalid, //0x3E
                Invalid, //0x3F
                Invalid, //0x40
                Invalid, //0x41
                Halt, //0x42
                Invalid, //0x43
                Invalid, //0x44
                Invalid, //0x45
                Invalid, //0x46
                Invalid, //0x47
                Invalid, //0x48
                Invalid, //0x49
                Invalid, //0x4A
                Invalid, //0x4B
                Invalid, //0x4C
                Invalid, //0x4D
                Invalid, //0x4E
                Invalid, //0x4F
                Invalid, //0x50
                Invalid, //0x51
                Halt, //0x52
                Invalid, //0x53
                Invalid, //0x54
                Invalid, //0x55
                Invalid, //0x56
                Invalid, //0x57
                Invalid, //0x58
                Invalid, //0x59
                Invalid, //0x5A
                Invalid, //0x5B
                Invalid, //0x5C
                Invalid, //0x5D
                Invalid, //0x5E
                Invalid, //0x5F
                Invalid, //0x60
                AdcIndirectX, //0x61
                Halt, //0x62
                Invalid, //0x63
                Invalid, //0x64
                AdcZPage, //0x65
                Invalid, //0x66
                Invalid, //0x67
                Invalid, //0x68
                AdcImmediate, //0x69
                Invalid, //0x6A
                Invalid, //0x6B
                Invalid, //0x6C
                AdcAbsolute, //0x6D
                Invalid, //0x6E
                Invalid, //0x6F
                Invalid, //0x70
                AdcIndirectY, //0x71
                Halt, //0x72
                Invalid, //0x73
                Invalid, //0x74
                AdcZPageX, //0x75
                Invalid, //0x76
                Invalid, //0x77
                Invalid, //0x78
                AdcAbsoluteY, //0x79
                Invalid, //0x7A
                Invalid, //0x7B
                Invalid, //0x7C
                AdcAbsoluteX, //0x7D
                Invalid, //0x7E
                Invalid, //0x7F
                Invalid, //0x80
                Invalid, //0x81
                Invalid, //0x82
                Invalid, //0x83
                Invalid, //0x84
                Invalid, //0x85
                Invalid, //0x86
                Invalid, //0x87
                Invalid, //0x88
                Invalid, //0x89
                Invalid, //0x8A
                Invalid, //0x8B
                Invalid, //0x8C
                Invalid, //0x8D
                Invalid, //0x8E
                Invalid, //0x8F
                Invalid, //0x90
                Invalid, //0x91
                Halt, //0x92
                Invalid, //0x93
                Invalid, //0x94
                Invalid, //0x95
                Invalid, //0x96
                Invalid, //0x97
                Invalid, //0x98
                Invalid, //0x99
                Invalid, //0x9A
                Invalid, //0x9B
                Invalid, //0x9C
                Invalid, //0x9D
                Invalid, //0x9E
                Invalid, //0x9F
                Invalid, //0xA0
                Invalid, //0xA1
                Invalid, //0xA2
                Invalid, //0xA3
                Invalid, //0xA4
                Invalid, //0xA5
                Invalid, //0xA6
                Invalid, //0xA7
                Invalid, //0xA8
                Invalid, //0xA9
                Invalid, //0xAA
                Invalid, //0xAB
                Invalid, //0xAC
                Invalid, //0xAD
                Invalid, //0xAE
                Invalid, //0xAF
                Invalid, //0xB0
                Invalid, //0xB1
                Halt, //0xB2
                Invalid, //0xB3
                Invalid, //0xB4
                Invalid, //0xB5
                Invalid, //0xB6
                Invalid, //0xB7
                Invalid, //0xB8
                Invalid, //0xB9
                Invalid, //0xBA
                Invalid, //0xBB
                Invalid, //0xBC
                Invalid, //0xBD
                Invalid, //0xBE
                Invalid, //0xBF
                Invalid, //0xC0
                Invalid, //0xC1
                Invalid, //0xC2
                Invalid, //0xC3
                Invalid, //0xC4
                Invalid, //0xC5
                Invalid, //0xC6
                Invalid, //0xC7
                Invalid, //0xC8
                Invalid, //0xC9
                Invalid, //0xCA
                Invalid, //0xCB
                Invalid, //0xCC
                Invalid, //0xCD
                Invalid, //0xCE
                Invalid, //0xCF
                Invalid, //0xD0
                Invalid, //0xD1
                Halt, //0xD2
                Invalid, //0xD3
                Invalid, //0xD4
                Invalid, //0xD5
                Invalid, //0xD6
                Invalid, //0xD7
                Invalid, //0xD8
                Invalid, //0xD9
                Invalid, //0xDA
                Invalid, //0xDB
                Invalid, //0xDC
                Invalid, //0xDD
                Invalid, //0xDE
                Invalid, //0xDF
                Invalid, //0xE0
                Invalid, //0xE1
                Invalid, //0xE2
                Invalid, //0xE3
                Invalid, //0xE4
                Invalid, //0xE5
                Invalid, //0xE6
                Invalid, //0xE7
                Invalid, //0xE8
                Invalid, //0xE9
                Invalid, //0xEA
                Invalid, //0xEB
                Invalid, //0xEC
                Invalid, //0xED
                Invalid, //0xEE
                Invalid, //0xEF
                Invalid, //0xF0
                Invalid, //0xF1
                Halt, //0xF2
                Invalid, //0xF3
                Invalid, //0xF4
                Invalid, //0xF5
                Invalid, //0xF6
                Invalid, //0xF7
                Invalid, //0xF8
                Invalid, //0xF9
                Invalid, //0xFA
                Invalid, //0xFB
                Invalid, //0xFC
                Invalid, //0xFD
                Invalid, //0xFE
                Invalid, //0xFF
            };

        }

        public void Run()
        {
            while (running)
            {
                Cycle();
            }
        }

        public void Stop() => running = false;

        public void Cycle()
        {
            opcodes[Ram.Byte(PC)].Invoke();
        }

        /// <summary>
        /// Invalid Opcode, logs the error to file
        /// </summary>
        private void Invalid()
        {
            Log.Error($"Unkown OPcode: {Ram.Byte(PC):X}");
        }

        private void Break()
        {
            LogInstruction(0, $"BRK");
            Bit.Set(ref P, Flags.Break);
            Bit.Set(ref P, Flags.IRQ);
            cyclesThisSec += 7;
            PC++;
        }

        #region ORA. Logical OR on the acumulator, set the zero and negative flags


        void Ora(byte param, int cycles, int pcIncrease)
        {
            LogInstruction(pcIncrease - 1, $"ORA #${param:X}");
            A = (byte)(A | param);

            Bit.Val(ref P, Flags.Zero, A == 0);
            Bit.Val(ref P, Flags.Negative, Bit.Test(A, 7));

            cyclesThisSec += cycles;
            PC += (ushort)pcIncrease;
        }

        void OraImmediate() => Ora(Ram.Byte(PC + 1), 2, 2);
        void OraZPage() => Ora(Ram.ZPageParam(), 2, 2);
        void OraZPageX() => Ora(Ram.ZPageXParam(), 4, 2);
        void OraAbsolute() => Ora(Ram.AbsoluteParam(), 4, 3);
        void OraAbsoluteX() => Ora(Ram.AbsoluteXParam(true), 4, 3);
        void OraAbsoluteY() => Ora(Ram.AbsoluteYParam(true), 4, 3);
        void OraIndirectX() => Ora(Ram.IndirectXParam(), 6,2);
        void OraIndirectY() => Ora(Ram.IndirectYParam(true), 5, 2);



        #endregion

        #region ASL Arithmetic Shift Left. shifts all bits left one position. 0 is shifted into bit 0 and the original bit 7 is shifted into the Carry.

        private void Asl(ref byte param, int cycles, int pcIncrease)
        {
            LogInstruction(pcIncrease -1, $"ASL #${param:X}");

            var shifted = param << 1;
        }

        private void AslAccumulator() => Asl(ref A, 2, 1);
        private void AslZPage()
        {
            var addr = Ram.ZPage(Ram.Byte(PC + 1));
            var param = Ram.ZPageParam();
            Asl(ref param, 5, 2);

            Ram.WriteByte(addr, param);
        }

        private void AslZPageX()
        {
            var addr = Ram.ZPageX(Ram.Byte(PC + 1));
            var param = Ram.ZPageXParam();
            Asl(ref param, 6, 2);
            Ram.WriteByte(addr, param);
        }

        private void AslAbsolute()
        {
            var addr = Ram.Absolute(Ram.Word(PC + 1));
            var param = Ram.AbsoluteParam();
            Asl(ref param, 6, 3);
            Ram.WriteByte(addr, param);
        }

        private void AslAbsoluteX()
        {
            var addr = Ram.AbsoluteX(Ram.Word(PC + 1));
            var param = Ram.AbsoluteXParam();
            Asl(ref param, 7, 3);
            Ram.WriteByte(addr, param);
        }

        #endregion

        #region Halt. Kills the machine, bam, pum ded, gone gurl....

        void Halt()
        {
            LogInstruction(0, "KILL");
            Stop();
        }

        #endregion

        #region ADC Add with Carry

        private void Adc(byte value, int cycles, ushort pcIncrease)
        {
            LogInstruction(pcIncrease - 1, $"ADC #${value:X}");
            var carry = Bit.Test(P, Flags.Carry) ? 1 : 0;
            var result = A + carry + value;
            carry = (byte) ((result & 0x100) >> 8);

            Bit.Val(ref P, Flags.Carry, carry > 0);

            // If operands same source sign but different result sign
            var isOverflown = ((A ^ result) & (value ^ result) & 0x80);
            A = (byte) result;

            Bit.Val(ref P, Flags.Zero, A == 0);
            Bit.Val(ref P, Flags.Overflow, isOverflown > 0);
            Bit.Val(ref P, Flags.Negative, Bit.Test(A, 7));

            PC += pcIncrease;
            cyclesThisSec += cycles;
        }

        private void AdcImmediate() => Adc(Ram.Byte(PC + 1), 2, 2);
        private void AdcZPage() => Adc(Ram.ZPageParam(), 3, 2);
        private void AdcZPageX() => Adc(Ram.ZPageXParam(), 4, 2);
        private void AdcAbsolute() => Adc(Ram.AbsoluteParam(), 4, 3);
        private void AdcAbsoluteX() => Adc(Ram.AbsoluteXParam(true), 4, 3);
        private void AdcAbsoluteY() => Adc(Ram.AbsoluteYParam(true), 4, 3);
        private void AdcIndirectX() => Adc(Ram.IndirectXParam(), 6, 2);
        private void AdcIndirectY() => Adc(Ram.IndirectYParam(true), 5, 2);
        
        #endregion

        #region AND Bitwise AND with accumulator

        private void And(byte value, int cycles, ushort pcIncrease)
        {
            LogInstruction(pcIncrease - 1, $"ADC #${value:X}");
            A &= value;
            Bit.Val(ref P, Flags.Zero, A == 0);
            Bit.Val(ref P, Flags.Negative, Bit.Test(A, 7));

            cyclesThisSec += cycles;
            PC += pcIncrease;
        }

        private void AndImmediate() => And(Ram.Byte(PC + 1), 2, 2);
        private void AndZPage() => And(Ram.ZPageParam(), 3, 2);
        private void AndZPageX() => And(Ram.ZPageXParam(), 4, 2);
        private void AndAbsolute() => And(Ram.AbsoluteParam(), 4, 3);
        private void AndAbsoluteX() => And(Ram.AbsoluteXParam(true), 4, 3);
        private void AndAbsoluteY() => And(Ram.AbsoluteYParam(true), 4, 3);
        private void AndIndirectX() => And(Ram.IndirectXParam(), 6, 2);
        private void AndIndirectY() => And(Ram.IndirectYParam(true), 5, 2);
        
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
