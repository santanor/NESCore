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
                Bpl, //0x10
                OraIndirectY, //0x11
                Halt, //0x12
                Invalid, //0x13
                Invalid, //0x14
                OraZPageX, //0x15
                AslZPageX, //0x16
                Invalid, //0x17
                Clc, //0x18
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
                BitZPage, //0x24
                AndZPage, //0x25
                Invalid, //0x26
                Invalid, //0x27
                Invalid, //0x28
                AndImmediate, //0x29
                Invalid, //0x2A
                BitAbsolute, //0x2C
                AndAbsolute, //0x2D
                Invalid, //0x2E
                Invalid, //0x2F
                Bmi, //0x30
                AndIndirectY, //0x31
                Halt, //0x32
                Invalid, //0x33
                Invalid, //0x34
                AndZPageX, //0x35
                Invalid, //0x36
                Invalid, //0x37
                Sec, //0x38
                AndAbsoluteY, //0x39
                Invalid, //0x3A
                Invalid, //0x3B
                Invalid, //0x3C
                AndAbsoluteX, //0x3D
                Invalid, //0x3E
                Invalid, //0x3F
                Invalid, //0x40
                EorIndirectX, //0x41
                Halt, //0x42
                Invalid, //0x43
                Invalid, //0x44
                EorZPage, //0x45
                Invalid, //0x46
                Invalid, //0x47
                Invalid, //0x48
                EorImmediate, //0x49
                Invalid, //0x4A
                Invalid, //0x4B
                Invalid, //0x4C
                EorAbsolute, //0x4D
                Invalid, //0x4E
                Invalid, //0x4F
                Bvc, //0x50
                EorIndirectY, //0x51
                Halt, //0x52
                Invalid, //0x53
                Invalid, //0x54
                EorZPageX, //0x55
                Invalid, //0x56
                Invalid, //0x57
                Cli, //0x58
                EorAbsoluteY, //0x59
                Invalid, //0x5A
                Invalid, //0x5B
                Invalid, //0x5C
                EorAbsoluteX, //0x5D
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
                Bvs, //0x70
                AdcIndirectY, //0x71
                Halt, //0x72
                Invalid, //0x73
                Invalid, //0x74
                AdcZPageX, //0x75
                Invalid, //0x76
                Invalid, //0x77
                Sei, //0x78
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
                Bcc, //0x90
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
                Bcs, //0xB0
                Invalid, //0xB1
                Halt, //0xB2
                Invalid, //0xB3
                Invalid, //0xB4
                Invalid, //0xB5
                Invalid, //0xB6
                Invalid, //0xB7
                Clv, //0xB8
                Invalid, //0xB9
                Invalid, //0xBA
                Invalid, //0xBB
                Invalid, //0xBC
                Invalid, //0xBD
                Invalid, //0xBE
                Invalid, //0xBF
                CpyImmediate, //0xC0
                CmpIndirectX, //0xC1
                Invalid, //0xC2
                Invalid, //0xC3
                CpyZPage, //0xC4
                CmpZPage, //0xC5
                DecZPage, //0xC6
                Invalid, //0xC7
                Invalid, //0xC8
                CmpImmediate, //0xC9
                Invalid, //0xCA
                Invalid, //0xCB
                CpyAbsolute, //0xCC
                CmpAbsolute, //0xCD
                DecAbsolute, //0xCE
                Invalid, //0xCF
                Bne, //0xD0
                CmpIndirectY, //0xD1
                Halt, //0xD2
                Invalid, //0xD3
                Invalid, //0xD4
                CmpZPageX, //0xD5
                DecZPageX, //0xD6
                Invalid, //0xD7
                Cld, //0xD8
                CmpAbsoluteY, //0xD9
                Invalid, //0xDA
                Invalid, //0xDB
                Invalid, //0xDC
                CmpAbsoluteX, //0xDD
                DecAbsoluteX, //0xDE
                Invalid, //0xDF
                CpxImmediate, //0xE0
                Invalid, //0xE1
                Invalid, //0xE2
                Invalid, //0xE3
                CpxZPage, //0xE4
                Invalid, //0xE5
                IncZPage, //0xE6
                Invalid, //0xE7
                Invalid, //0xE8
                Invalid, //0xE9
                Invalid, //0xEA
                Invalid, //0xEB
                CpxAbsolute, //0xEC
                Invalid, //0xED
                IncAbsolute, //0xEE
                Invalid, //0xEF
                Beq, //0xF0
                Invalid, //0xF1
                Halt, //0xF2
                Invalid, //0xF3
                Invalid, //0xF4
                Invalid, //0xF5
                IncZPageX, //0xF6
                Invalid, //0xF7
                Sed, //0xF8
                Invalid, //0xF9
                Invalid, //0xFA
                Invalid, //0xFB
                Invalid, //0xFC
                Invalid, //0xFD
                IncAbsoluteX, //0xFE
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
        
        #region BIT test BITs

        private void BIT(byte value, int cycles, ushort pcIncrease)
        {
            LogInstruction(pcIncrease - 1, $"BIT #${value:X}");

            byte tmp = (byte)(A & value);
            Bit.Val(ref P, Flags.Zero, tmp == 0);
            Bit.Val(ref P, Flags.Overflow, Bit.Test(value, 6));
            Bit.Val(ref P, Flags.Negative, Bit.Test(value, 7));

            cyclesThisSec += cycles;
            PC += pcIncrease;
        }

        private void BitZPage() => BIT(Ram.ZPageParam(), 3, 2);
        private void BitAbsolute() => BIT(Ram.AbsoluteParam(), 4, 3);
        #endregion
        
        #region Branch Instructions

        private void TryBranch(Flags flag, bool reqFlagValue, string mnemonic)
        {
            ushort value = Ram.Word(PC + 1); //byte is unsigned but we need a signed char
            LogInstruction(1, $"{mnemonic} ${PC + 2 + value:X}");
            cyclesThisSec += 2;//This is always constant
            PC += 2;
            
            if (Bit.Test(P, flag) == reqFlagValue)
            {
                Ram.CheckPageCrossed((ushort) (PC + value), PC);
                PC += value;
                cyclesThisSec++;
            }
        }

        private void Bpl() => TryBranch(Flags.Negative, false, "BPL");
        private void Bmi() => TryBranch(Flags.Negative, true, "BMI");
        private void Bvc() => TryBranch(Flags.Overflow, false, "BVC");
        private void Bvs() => TryBranch(Flags.Overflow, true, "BVS");
        private void Bcc() => TryBranch(Flags.Carry, false, "BCC");
        private void Bcs() => TryBranch(Flags.Carry, true, "BCS");
        private void Bne() => TryBranch(Flags.Zero, false, "BNE");
        private void Beq() => TryBranch(Flags.Zero, true, "BEQ");
        
        #endregion
        
        #region CMP, CPY, CPX Compare Registers

        private void Cmp(byte register, byte value, int cycles, ushort pcIncrease, string regMnemonic)
        {
            LogInstruction(pcIncrease - 1, $"{regMnemonic} #{value:X}");

            var tmp = (byte)(register - value);

            Bit.Val(ref P, Flags.Carry, register >= value);

            //Need to do this since there are some positive numbers that should trigger this flag. i.e. 0x80
            Bit.Val(ref P, Flags.Negative, Bit.Test(tmp, 7));
            Bit.Val(ref P, Flags.Zero, register == value);

            PC += pcIncrease;
            cyclesThisSec += cycles;
        }

        void CmpImmediate() => Cmp(A, Ram.Byte(PC + 1), 2, 2, "CMP");
        void CmpZPage() => Cmp(A, Ram.ZPageParam(), 3, 2, "CMP");
        void CmpZPageX() => Cmp(A, Ram.ZPageXParam(), 4, 2, "CMP");
        void CmpAbsolute() => Cmp(A, Ram.AbsoluteParam(), 4, 3, "CMP");
        void CmpAbsoluteX() => Cmp(A, Ram.AbsoluteXParam(true), 4, 3, "CMP");
        void CmpAbsoluteY() => Cmp(A, Ram.AbsoluteYParam(true), 4, 3, "CMP");
        void CmpIndirectX() => Cmp(A, Ram.IndirectXParam(), 6, 2, "CMP");
        void CmpIndirectY() => Cmp(A, Ram.IndirectYParam(true), 5, 2, "CMP");
        void CpxImmediate() => Cmp(X, Ram.Byte(PC + 1), 2, 2, "CPX");
        void CpxZPage() => Cmp(X, Ram.ZPageParam(), 3, 2, "CPX");
        void CpxAbsolute() => Cmp(X, Ram.AbsoluteParam(), 4, 3, "CPX");
        void CpyImmediate() => Cmp(Y, Ram.Byte(PC + 1), 2, 2, "CPY");
        void CpyZPage() => Cmp(Y, Ram.ZPageParam(), 3, 2, "CPY");
        void CpyAbsolute() => Cmp(Y, Ram.AbsoluteParam(), 4, 3, "CPY");
        
        #endregion
        
        #region INC/DEC Increment and Decrement Memory

        void DeltaMemory(ushort memAddr, int delta, int cycles, ushort pcIncrease, string mnemonic)
        {
            LogInstruction(pcIncrease -1, $"{mnemonic} ${memAddr:X} = {(memAddr + delta):X}");
            var value = Ram.Byte(memAddr);
            value += (byte)delta;
            Ram.WriteByte(memAddr, value);

            Bit.Val(ref P, Flags.Zero, value == 0);
            Bit.Val(ref P, Flags.Negative, Bit.Test(value, 7));

            PC += pcIncrease;
            cyclesThisSec += cycles;
        }

        void DecZPage() => DeltaMemory(Ram.ZPage(Ram.Byte(PC + 1)), -1, 5, 2, "DEC");
        void DecZPageX() => DeltaMemory(Ram.ZPageX(Ram.Byte(PC + 1)), -1, 6, 2, "DEC");
        void DecAbsolute() => DeltaMemory(Ram.Absolute(Ram.Byte(PC + 1)), -1, 6, 3, "DEC");
        void DecAbsoluteX() => DeltaMemory(Ram.AbsoluteX(Ram.Byte(PC + 1)), -1, 7, 3, "DEC");
        void IncZPage() => DeltaMemory(Ram.ZPage(Ram.Byte(PC + 1)), 1, 5, 2, "INC");
        void IncZPageX() => DeltaMemory(Ram.ZPageX(Ram.Byte(PC + 1)), 1, 6, 2, "INC");
        void IncAbsolute() => DeltaMemory(Ram.Absolute(Ram.Byte(PC + 1)), 1, 6, 3, "INC");
        void IncAbsoluteX() => DeltaMemory(Ram.AbsoluteX(Ram.Byte(PC + 1)), 1, 7, 3, "INC");
        
        #endregion
        
        #region EOR bitwise exclusive OR

        void Eor(byte value, int cycles, ushort pcIncrease)
        {
            LogInstruction(pcIncrease - 1, $"EOR ${value:X}");

            A ^= value;

            Bit.Val(ref P, Flags.Negative, Bit.Test(A, 7));
            Bit.Val(ref P, Flags.Zero, A == 0);

            PC += pcIncrease;
            cyclesThisSec += cycles;
        }

        void EorImmediate() => Eor(Ram.Byte(PC + 1), 2, 2);
        void EorZPage() => Eor(Ram.ZPageParam(), 3, 2);
        void EorZPageX() => Eor(Ram.ZPageXParam(), 4, 2);
        void EorAbsolute() => Eor(Ram.AbsoluteParam(), 4, 3);
        void EorAbsoluteX() => Eor(Ram.AbsoluteXParam(true), 4, 3);
        void EorAbsoluteY() => Eor(Ram.AbsoluteYParam(true), 4, 3);
        void EorIndirectX() => Eor(Ram.IndirectXParam(), 6, 2);
        void EorIndirectY() => Eor(Ram.IndirectYParam(true), 5, 2);
        
        #endregion
        
        #region Flag Processor status instructions

        void SetFlagValue(Flags flag, bool isSet, string mnemonic)
        {
            LogInstruction(0, mnemonic);
            Bit.Val(ref P, flag, isSet);
            cyclesThisSec += 2; //Constant. Always
            PC++; //Constant. Always
        }

        void Clc() => SetFlagValue(Flags.Carry, false, "CLC");
        void Sec() => SetFlagValue(Flags.Carry, true, "SEC");
        void Cli() => SetFlagValue(Flags.IRQ, false, "CLI");
        void Sei() => SetFlagValue(Flags.IRQ, true, "SEI");
        void Clv() => SetFlagValue(Flags.Overflow, false, "CLV");
        void Cld() => SetFlagValue(Flags.Decimal, false, "CLD");
        void Sed() => SetFlagValue(Flags.Decimal, true, "SED");
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
