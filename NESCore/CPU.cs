using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Xml.Xsl;
using Serilog;

namespace NESCore
{
    public enum Flags {Carry, Zero, IRQ, Decimal, Break, Unused, Overflow, Negative};

    public class CPU
    {
        /// <summary>
        /// Accumulator, deal with carry, overflow and so on...
        /// </summary>
        private byte A;

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
        private byte currentOpcode;

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
                AsoIndirectX, //0x03
                Nop, //0x04
                OraZPage, //0x05
                AslZPage, //0x06
                AsoZPage, //0x07
                Php, //0x08
                OraImmediate, //0x09
                AslAccumulator, //0x0A
                Anc, //0x0B
                Nop, //0x0C
                OraAbsolute, //0x0D
                AslAbsolute, //0x0E
                AsoAbsolute, //0x0F
                Bpl, //0x10
                OraIndirectY, //0x11
                Halt, //0x12
                AsoIndirectY, //0x13
                Nop, //0x14
                OraZPageX, //0x15
                AslZPageX, //0x16
                AsoZPageX, //0x17
                Clc, //0x18
                OraAbsoluteY, //0x19
                Nop, //0x1A
                AsoAbsoluteY, //0x1B
                Nop, //0x1C
                OraAbsoluteX, //0x1D
                AslAbsoluteX, //0x1E
                AsoAbsoluteX, //0x1F
                Jsr, //0x20
                AndIndirectX, //0x21
                Halt, //0x22
                RlaIndirectX, //0x23
                BitZPage, //0x24
                AndZPage, //0x25
                RolZPage, //0x26
                RlaZPage, //0x27
                Plp, //0x28
                AndImmediate, //0x29
                RolAccumulator, //0x2A,
                Anc, //0x2B
                BitAbsolute, //0x2C
                AndAbsolute, //0x2D
                RolAbsolute, //0x2E
                RlaAbsolute, //0x2F
                Bmi, //0x30
                AndIndirectY, //0x31
                Halt, //0x32
                RlaIndirectY, //0x33
                Nop, //0x34
                AndZPageX, //0x35
                RolZPageX, //0x36
                RlaZPageX, //0x37
                Sec, //0x38
                AndAbsoluteY, //0x39
                Nop, //0x3A
                RlaAbsoluteY, //0x3B
                Nop, //0x3C
                AndAbsoluteX, //0x3D
                RolAbsoluteX, //0x3E
                RlaAbsoluteX, //0x3F
                Rti, //0x40
                EorIndirectX, //0x41
                Halt, //0x42
                LseIndirectX, //0x43
                Nop, //0x44
                EorZPage, //0x45
                LsrZPage, //0x46
                LseZPage, //0x47
                Pha, //0x48
                EorImmediate, //0x49
                LsrAccumulator, //0x4A
                Alr, //0x4B
                JmpAbsolute, //0x4C
                EorAbsolute, //0x4D
                LsrAbsolute, //0x4E
                LseAbsolute, //0x4F
                Bvc, //0x50
                EorIndirectY, //0x51
                Halt, //0x52
                LseIndirectY, //0x53
                Nop, //0x54
                EorZPageX, //0x55
                LsrZPageX, //0x56
                LseZPageX, //0x57
                Cli, //0x58
                EorAbsoluteY, //0x59
                Nop, //0x5A
                LseAbsoluteY, //0x5B
                Nop, //0x5C
                EorAbsoluteX, //0x5D
                LsrAbsoluteX, //0x5E
                LseAbsoluteX, //0x5F
                Rts, //0x60
                AdcIndirectX, //0x61
                Halt, //0x62
                RraIndirectX, //0x63
                Nop, //0x64
                AdcZPage, //0x65
                RorZPage, //0x66
                RraZPage, //0x67
                Pla, //0x68
                AdcImmediate, //0x69
                RorAccumulator, //0x6A
                Arr, //0x6B
                JmpIndirect, //0x6C
                AdcAbsolute, //0x6D
                RorAbsolute, //0x6E
                RraAbsolute, //0x6F
                Bvs, //0x70
                AdcIndirectY, //0x71
                Halt, //0x72
                RraIndirectY, //0x73
                Nop, //0x74
                AdcZPageX, //0x75
                RorZPageX, //0x76
                RraZPageX, //0x77
                Sei, //0x78
                AdcAbsoluteY, //0x79
                Nop, //0x7A
                RraAbsoluteY, //0x7B
                Nop, //0x7C
                AdcAbsoluteX, //0x7D
                RorAbsoluteX, //0x7E
                RraAbsoluteX, //0x7F
                Nop, //0x80
                StaIndirectX, //0x81
                Nop, //0x82
                AxsIndirectX, //0x83
                StyZPage, //0x84
                StaZPage, //0x85
                StxZPage, //0x86
                AxsZPage, //0x87
                Dey, //0x88
                Nop, //0x89
                Txa, //0x8A
                Halt, //0x8B
                StyAbsolute, //0x8C
                StaAbsolute, //0x8D
                StxAbsolute, //0x8E
                AxsAbsolute, //0x8F
                Bcc, //0x90
                StaIndirectY, //0x91
                Halt, //0x92
                Halt, //0x93
                StyZPageX, //0x94
                StaZPageX, //0x95
                StxZPageY, //0x96
                AxsZPageY, //0x97
                Tya, //0x98
                StaAbsoluteY, //0x99
                Txs, //0x9A
                Halt, //0x9B
                Halt, //0x9C
                StaAbsoluteX, //0x9D
                Halt, //0x9E
                Halt, //0x9F
                LdyImmediate, //0xA0
                LdaIndirectX, //0xA1
                LdxImmediate, //0xA2
                LaxIndirectX, //0xA3
                LdyZPage, //0xA4
                LdaZPage, //0xA5
                LdxZPage, //0xA6
                LaxZPage, //0xA7
                Tay, //0xA8
                LdaImmediate, //0xA9
                Tax, //0xAA
                Halt, //0xAB
                LdyAbsolute, //0xAC
                LdaAbsolute, //0xAD
                LdxAbsolute, //0xAE
                LaxAbsolute, //0xAF
                Bcs, //0xB0
                LdaIndirectY, //0xB1
                Halt, //0xB2
                LaxIndirectY, //0xB3
                LdyZPageX, //0xB4
                LdaZPageX, //0xB5
                LdxZPageY, //0xB6
                LaxZPageY, //0xB7
                Clv, //0xB8
                LdaAbsoluteY, //0xB9
                Tsx, //0xBA
                Halt, //0xBB
                LdyAbsoluteX, //0xBC
                LdaAbsoluteX, //0xBD
                LdxAbsoluteY, //0xBE
                LaxAbsoluteY, //0xBF
                CpyImmediate, //0xC0
                CmpIndirectX, //0xC1
                Nop, //0xC2
                DcmIndirectX, //0xC3
                CpyZPage, //0xC4
                CmpZPage, //0xC5
                DecZPage, //0xC6
                DcmZPage, //0xC7
                Iny, //0xC8
                CmpImmediate, //0xC9
                Dex, //0xCA
                Halt, //0xCB
                CpyAbsolute, //0xCC
                CmpAbsolute, //0xCD
                DecAbsolute, //0xCE
                DcmAbsolute, //0xCF
                Bne, //0xD0
                CmpIndirectY, //0xD1
                Halt, //0xD2
                DcmIndirectY, //0xD3
                Nop, //0xD4
                CmpZPageX, //0xD5
                DecZPageX, //0xD6
                DcmZPageX, //0xD7
                Cld, //0xD8
                CmpAbsoluteY, //0xD9
                Nop, //0xDA
                DcmAbsoluteY, //0xDB
                Nop, //0xDC
                CmpAbsoluteX, //0xDD
                DecAbsoluteX, //0xDE
                DcmAbsoluteX, //0xDF
                CpxImmediate, //0xE0
                SbcIndirectX, //0xE1
                Nop, //0xE2
                InsIndirectX, //0xE3
                CpxZPage, //0xE4
                SbcZPage, //0xE5
                IncZPage, //0xE6
                InsZPage, //0xE7
                Inx, //0xE8
                SbcImmediate, //0xE9
                Nop, //0xEA
                SbcIndirectY, //0xEB
                CpxAbsolute, //0xEC
                SbcAbsolute, //0xED
                IncAbsolute, //0xEE
                InsAbsolute, //0xEF
                Beq, //0xF0
                SbcIndirectY, //0xF1
                Halt, //0xF2
                InsIndirectY, //0xF3
                Nop, //0xF4
                SbcZPageX, //0xF5
                IncZPageX, //0xF6
                InsZPageX, //0xF7
                Sed, //0xF8
                AdcAbsoluteY, //0xF9
                Nop, //0xFA
                InsAbsoluteY, //0xFB
                Nop, //0xFC
                SbcAbsoluteX, //0xFD
                IncAbsoluteX, //0xFE
                InsAbsoluteX, //0xFF
            };

        }

        public void Stop() => running = false;

        public void Cycle()
        {
            currentOpcode = Ram.Byte(PC);
            opcodes[currentOpcode].Invoke();
        }

        /// <summary>
        /// Invalid Opcode, logs the error to file
        /// </summary>
        private void Invalid()
        {
            Log.Error($"Unkown OPcode: {Ram.Byte(PC):X2}");
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
            LogInstruction(pcIncrease - 1, $"ORA #${param:X2}");
            A = (byte)(A | param);

            Bit.Val(ref P, Flags.Zero, A == 0);
            Bit.Val(ref P, Flags.Negative, Bit.Test(A, Flags.Negative));

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
            LogInstruction(pcIncrease -1, $"ASL #${param:X2}");

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
            LogInstruction(pcIncrease - 1, $"ADC #${value:X2}");

            AdcInternal(value);
            
            PC += pcIncrease;
            cyclesThisSec += cycles;
        }
        
        void AdcInternal(byte value)
        {
            var carry = Bit.Test(P, Flags.Carry) ? 1 : 0;
            var result = A + carry + value;
            carry = (byte) ((result & 0x100) >> 8);

            Bit.Val(ref P, Flags.Carry, carry > 0);

            // If operands same source sign but different result sign
            var isOverflown = ((A ^ result) & (value ^ result) & 0x80);
            A = (byte) result;

            Bit.Val(ref P, Flags.Zero, A == 0);
            Bit.Val(ref P, Flags.Overflow, isOverflown > 0);
            Bit.Val(ref P, Flags.Negative, Bit.Test(A, Flags.Negative));
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

        #region SBC Substract With Carry

        void Sbc(byte value, int cycles, ushort pcIncrease)
        {
            LogInstruction(pcIncrease - 1, $"SBC #{value:X2}");

            AdcInternal((byte) ~value);
            
            PC += pcIncrease;
            cyclesThisSec += cycles;
        }

        void SbcImmediate() => Sbc(Ram.Byte(PC + 1), 2, 2);
        void SbcZPage() => Sbc(Ram.ZPageParam(), 3, 2);
        void SbcZPageX() => Sbc(Ram.ZPageXParam(), 4, 2);
        void SbcAbsolute() => Sbc(Ram.AbsoluteParam(), 4, 3);
        void SbcAbsoluteX() => Sbc(Ram.AbsoluteXParam(true), 4, 3);
        void SbcAbsoluteY() => Sbc(Ram.AbsoluteYParam(true), 4, 3);
        void SbcIndirectX() => Sbc(Ram.IndirectXParam(), 6, 2);
        void SbcIndirectY() => Sbc(Ram.IndirectYParam(true), 5, 2);
        
        #endregion
        
        #region AND Bitwise AND with accumulator

        private void And(byte value, int cycles, ushort pcIncrease)
        {
            LogInstruction(pcIncrease - 1, $"ADC #${value:X2}");
            A &= value;
            Bit.Val(ref P, Flags.Zero, A == 0);
            Bit.Val(ref P, Flags.Negative, Bit.Test(A, Flags.Negative));

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
            LogInstruction(pcIncrease - 1, $"BIT #${value:X2}");

            byte tmp = (byte)(A & value);
            Bit.Val(ref P, Flags.Zero, tmp == 0);
            Bit.Val(ref P, Flags.Overflow, Bit.Test(value, Flags.Overflow));
            Bit.Val(ref P, Flags.Negative, Bit.Test(value, Flags.Negative));

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
            LogInstruction(1, $"{mnemonic} ${PC + 2 + value:X2}");
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
            LogInstruction(pcIncrease - 1, $"{regMnemonic} #{value:X2}");

            var tmp = (byte)(register - value);

            Bit.Val(ref P, Flags.Carry, register >= value);

            //Need to do this since there are some positive numbers that should trigger this flag. i.e. 0x80
            Bit.Val(ref P, Flags.Negative, Bit.Test(tmp, Flags.Negative));
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
            LogInstruction(pcIncrease -1, $"{mnemonic} ${memAddr:X2} = {(memAddr + delta):X2}");
            var value = Ram.Byte(memAddr);
            value += (byte)delta;
            Ram.WriteByte(memAddr, value);

            Bit.Val(ref P, Flags.Zero, value == 0);
            Bit.Val(ref P, Flags.Negative, Bit.Test(value, Flags.Negative));

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
            LogInstruction(pcIncrease - 1, $"EOR ${value:X2}");

            EorInternal(value);

            PC += pcIncrease;
            cyclesThisSec += cycles;
        }

        void EorInternal(byte value)
        {
            A ^= value;

            Bit.Val(ref P, Flags.Negative, Bit.Test(A, Flags.Negative));
            Bit.Val(ref P, Flags.Zero, A == 0);
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
        
        #region Jump instructions

        void Jmp(ushort addr, int cycles)
        {
            LogInstruction(2, $"JMP ${addr:X2}");

            PC = addr;
            cyclesThisSec += cycles;
        }

        void JmpAbsolute() => Jmp(Ram.Word(PC + 1), 3);
        void JmpIndirect() => Jmp(Ram.IndirectParam(), 5);

        void Jsr()
        {
            var cachedPc = (ushort) (PC + 0x02);
            var addr = Ram.Absolute(Ram.Word(PC + 1));
            LogInstruction(2, $"JSR #${addr:X2}");
            
            Ram.PushWord(cachedPc); // Stores the address of the next opcode minus one

            PC = addr;
            cyclesThisSec += 6;
        }
        
        #endregion
        
        #region Load Registers

        // ReSharper disable once RedundantAssignment
        void LoadRegister(ref byte register, byte value, int cycles, ushort pcIncrease, string mnemonic)
        {
            LogInstruction(pcIncrease - 1, $"{mnemonic} #${value:X2}");
            register = value;

            Bit.Val(ref P, Flags.Zero, register == 0);
            Bit.Val(ref P, Flags.Negative, Bit.Test(register, Flags.Negative));

            PC += pcIncrease;
            cyclesThisSec += cycles;
        }
        void LdaImmediate() => LoadRegister(ref A, Ram.Byte(PC + 1), 2, 2, "LDA");
        void LdaZPage() => LoadRegister(ref A, Ram.ZPageParam(), 3, 2, "LDA");
        void LdaZPageX() => LoadRegister(ref A, Ram.ZPageXParam(), 4, 2, "LDA");
        void LdaAbsolute() => LoadRegister(ref A, Ram.AbsoluteParam(), 4, 3, "LDA");
        void LdaAbsoluteX() => LoadRegister(ref A, Ram.AbsoluteXParam(true), 4, 3, "LDA");
        void LdaAbsoluteY() => LoadRegister(ref A, Ram.AbsoluteYParam(true), 4, 3, "LDA");
        void LdaIndirectX() => LoadRegister(ref A, Ram.IndirectXParam(), 6, 2, "LDA");
        void LdaIndirectY() => LoadRegister(ref A, Ram.IndirectYParam(true), 5, 2, "LDA");
        void LdxImmediate() => LoadRegister(ref X, Ram.Byte(PC + 1), 2, 2, "LDX");
        void LdxZPage() => LoadRegister(ref X, Ram.ZPageParam(), 3, 2, "LDX");
        void LdxZPageY() => LoadRegister(ref X, Ram.ZPageYParam(), 4, 2, "LDX");
        void LdxAbsolute() => LoadRegister(ref X, Ram.AbsoluteParam(), 4, 3, "LDX");
        void LdxAbsoluteY() => LoadRegister(ref X, Ram.AbsoluteYParam(true), 4, 3, "LDX");
        void LdyImmediate() => LoadRegister(ref Y, Ram.Byte(PC + 1), 2, 2, "LDY");
        void LdyZPage() => LoadRegister(ref Y, Ram.ZPageParam(), 3, 2, "LDY");
        void LdyZPageX() => LoadRegister(ref Y, Ram.ZPageXParam(), 4, 2, "LDY");
        void LdyAbsolute() => LoadRegister(ref Y, Ram.AbsoluteParam(), 4, 3, "LDY");
        void LdyAbsoluteX() => LoadRegister(ref Y, Ram.AbsoluteXParam(true), 4, 3, "LDY");
        
        #endregion
        
        #region LSR Logical Shift Right

        byte Lsr(byte value, int cycles, ushort pcIncrease)
        {
            LogInstruction(pcIncrease - 1, $"LSR #{value:X2}");

            
            PC += pcIncrease;
            cyclesThisSec += cycles;

            return LsrInternal(value);
        }

        byte LsrInternal(byte value)
        {
            Bit.Val(ref P, Flags.Carry, Bit.Test(value, 0));

            var shifted = (byte) (value >> 1);

            Bit.Val(ref P, Flags.Zero, shifted == 0);
            Bit.Val(ref P, Flags.Negative, Bit.Test(shifted, Flags.Negative));

            return shifted;
        }

        void LsrAccumulator()
        {
            A = Lsr(A, 2, 1);
        }

        void LsrZPage()
        {
            var addr = Ram.ZPage(Ram.Byte(PC + 1));
            var data = Ram.ZPageParam();
            data = Lsr(data, 5, 2);
            Ram.WriteByte(addr, data);
        }

        void LsrZPageX()
        {
            var addr = Ram.ZPageX(Ram.Byte(PC + 1));
            var data = Ram.Byte(addr);
            data = Lsr(data, 6, 2);
            Ram.WriteByte(addr, data);
        }

        void LsrAbsolute()
        {
            var addr = Ram.Absolute(Ram.Word(PC + 1));
            var data = Ram.Byte(addr);
            data = Lsr(data, 6, 3);
            Ram.WriteByte(addr, data);
        }

        void LsrAbsoluteX()
        {
            var addr = Ram.AbsoluteX(Ram.Word(PC + 1));
            var data = Ram.Byte(addr);
            data = Lsr(data, 7, 3);
            Ram.WriteByte(addr, data);
        }
        
        #endregion
        
        #region NOP

        void Nop()
        {
            LogInstruction(0, "NOP");
            switch (Ram.Byte(PC)){
                case 0x1A: case 0x3A: case 0x5A: case 0x7A: case 0xDA: case 0xEA: case 0xFA:
                    PC++;
                    cyclesThisSec += 2;
                    break;

                case 0x80: case 0x82: case 0x89: case 0xC2: case 0xE2:
                    PC += 2;
                    cyclesThisSec += 2;
                    break;
                case 0x0C:
                    PC += 3;
                    cyclesThisSec += 4;
                    break;
                case 0x1C: case 0x3C: case 0x5C: case 0x7C: case 0xDC: case 0xFC:
                    PC += 3;
                    Ram.AbsoluteXParam(true);
                    cyclesThisSec += 4;
                    break;
                case 0x04: case 0x44: case 0x64:
                    PC += 2;
                    cyclesThisSec += 3;
                    break;
                case 0x14: case 0x34: case 0x54:  case 0x74: case 0xD4: case 0xF4:
                    PC += 2;
                    cyclesThisSec += 4;
                    break;
            }
        }
        #endregion
        
        #region Register Instructions

        // ReSharper disable once RedundantAssignment
        void TransferRegister(byte source, ref byte destination, string mnemonic)
        {
            LogInstruction(0, mnemonic);
            destination = source;
            Bit.Val(ref P, Flags.Zero, destination == 0);
            Bit.Val(ref P, Flags.Negative, Bit.Test(destination, Flags.Negative));
            PC++;
            cyclesThisSec += 2;
        }

        void DeltaRegister(ref byte register, int delta, string mnemonic)
        {
            LogInstruction(0, mnemonic);
            register += (byte)delta;
            Bit.Val(ref P, Flags.Zero, register == 0);
            Bit.Val(ref P, Flags.Negative, Bit.Test(register, Flags.Negative));
            PC++;
            cyclesThisSec += 2;
        }

        void Tax() => TransferRegister(A, ref X, "TAX");
        void Txa() => TransferRegister(X, ref A, "TXA");
        void Dex() => DeltaRegister(ref X, -1, "DEX");
        void Inx() => DeltaRegister(ref X, 1, "INX");
        void Tay() => TransferRegister(A, ref Y, "TAY");
        void Tya() => TransferRegister(Y, ref A, "TYA");
        void Dey() => DeltaRegister(ref Y, -1, "DEY");
        void Iny() => DeltaRegister(ref Y, 1, "INY");
        
        #endregion
        
        #region ROR/ROL Rotate region

        byte Rol(byte value, int cycles, ushort pcIncrease)
        {
            LogInstruction(pcIncrease - 1, $"ROL ${value:X2}");
            PC += pcIncrease;
            cyclesThisSec += cycles;
            return Rotate(value, RotateDirection.Left);
        }

        byte Ror(byte value, int cycles, ushort pcIncrease)
        {
            LogInstruction(pcIncrease - 1, $"ROR ${value:X2}");
            PC += pcIncrease;
            cyclesThisSec += cycles;
            return Rotate(value, RotateDirection.Right);
        }

        private enum RotateDirection {Left, Right}
        byte Rotate(byte value, RotateDirection direction )
        {
            var cachedFlagC = Bit.Test(P, Flags.Carry);

            var cachedPosition = direction == RotateDirection.Left ? Flags.Negative : Flags.Carry;
            var cached = Bit.Test(value, cachedPosition);

            byte shifted;
            
            if (direction == RotateDirection.Right)
            {
                shifted = (byte) (value >> 1);
            
                Bit.Val(ref shifted, Flags.Negative, cachedFlagC);
                Bit.Val(ref P, Flags.Carry, cached);
            }
            else
            {
                Bit.Val(ref P, Flags.Carry, cached);
                shifted = (byte) (value << 1);

                Bit.Val(ref shifted, 0, cachedFlagC);
            }
            
            Bit.Val(ref P, Flags.Zero, shifted == 0);
            Bit.Val(ref P, Flags.Negative, Bit.Test(shifted, Flags.Negative));

            return shifted;
        }

        void RolAccumulator()
        {
            A = Rol(A, 2, 1);
        }

        void RolZPage()
        {
            var addr = Ram.ZPage(Ram.Byte(PC + 1));
            var data = Ram.ZPageParam();
            data = Rol(data, 5, 2);
            Ram.WriteByte(addr, data);
        }

        void RolZPageX()
        {
            var addr = Ram.ZPageX(Ram.Byte(PC + 1));
            var data = Ram.Byte(addr);
            data = Rol(data, 6, 2);
            Ram.WriteByte(addr, data);
        }

        void RolAbsolute()
        {
            var addr = Ram.Absolute(Ram.Word(PC + 1));
            var data = Ram.Byte(addr);
            data = Rol(data, 6, 3);
            Ram.WriteByte(addr, data);
        }

        void RolAbsoluteX()
        {
            var addr = Ram.AbsoluteX(Ram.Word(PC + 1));
            var data = Ram.Byte(addr);
            data = Rol(data, 7, 3);
            Ram.WriteByte(addr, data);
        }

        void RorAccumulator()
        {
            A = Ror(A, 2, 1);
        }

        void RorZPage()
        {
            var addr = Ram.ZPage(Ram.Byte(PC + 1));
            var data = Ram.ZPageParam();
            data = Ror(data, 5, 2);
            Ram.WriteByte(addr, data);
        }

        void RorZPageX()
        {
            var addr = Ram.ZPageX(Ram.Byte(PC + 1));
            var data = Ram.Byte(addr);
            data = Ror(data, 6, 2);
            Ram.WriteByte(addr, data);
        }

        void RorAbsolute()
        {
            var addr = Ram.Absolute(Ram.Word(PC + 1));
            var data = Ram.Byte(addr);
            data = Ror(data, 6, 3);
            Ram.WriteByte(addr, data);
        }

        void RorAbsoluteX()
        {
            var addr = Ram.AbsoluteX(Ram.Word(PC + 1));
            var data = Ram.Byte(addr);
            data = Ror(data, 7, 3);
            Ram.WriteByte(addr, data);
        }
        
        #endregion
        
        #region RTI/RTS Returns

        void Rti()
        {
            LogInstruction(0, "RTI");
            P = Ram.PopByte();
            Bit.Set(ref P, Flags.Unused);//It has to be one. Always
            PC = Ram.PopWord(); //Unlike RTS. RTI pulls the correct PC address. No need to increment
            cyclesThisSec += 6;
        }

        void Rts()
        {
            LogInstruction(0, "RTS");
            PC = Ram.PopWord();

            PC++; // JSR pushes the address -1, so when we recover (here) we have to add 1 to make up for that "1" lost
            cyclesThisSec += 6;
        }
        #endregion

        #region Store register

        void StoreRegister(byte value, ushort addr, int cycles, ushort pcIncrease, string mnemonic)
        {
            LogInstruction(pcIncrease - 1, $"{mnemonic} ${addr:X2} = {value:X2}");
            Ram.WriteByte(addr, value);

            PC += pcIncrease;
            cyclesThisSec += cycles;
        }

        void StaZPage() => StoreRegister(A, Ram.ZPage(Ram.Byte(PC + 1)), 3, 2, "STA");
        void StaZPageX() => StoreRegister(A, Ram.ZPageX(Ram.Byte(PC + 1)), 4, 2, "STA");
        void StaAbsolute() => StoreRegister(A, Ram.Absolute(Ram.Word(PC + 1)), 4, 3, "STA");
        void StaAbsoluteX() => StoreRegister(A, Ram.AbsoluteX(Ram.Word(PC + 1)), 5, 3, "STA");
        void StaAbsoluteY() => StoreRegister(A, Ram.AbsoluteY(Ram.Word(PC + 1)), 5, 3, "STA");
        void StaIndirectX() => StoreRegister(A, Ram.IndirectX(Ram.Byte(PC + 1)), 6, 2, "STA");
        void StaIndirectY() => StoreRegister(A, Ram.IndirectY(Ram.Byte(PC + 1)), 6, 2, "STA");
        void StxZPage() => StoreRegister(X, Ram.ZPage(Ram.Byte(PC + 1)), 3, 2, "STX");
        void StxZPageY() => StoreRegister(X, Ram.ZPageY(Ram.Byte(PC + 1)), 4, 2, "STX");
        void StxAbsolute() => StoreRegister(X, Ram.Absolute(Ram.Word(PC + 1)), 4, 3, "STX");
        void StyZPage() => StoreRegister(Y, Ram.ZPage(Ram.Byte(PC + 1)), 3, 2, "STY");
        void StyZPageX() => StoreRegister(Y, Ram.ZPageX(Ram.Byte(PC + 1)), 4, 2, "STY");
        void StyAbsolute() => StoreRegister(Y, Ram.Absolute(Ram.Word(PC + 1)), 4, 3, "STY");

        #endregion

        #region Stack instructions

        void Txs() => TransferRegister(X, ref SP, "TXS");
        void Tsx() => TransferRegister(SP, ref X, "TSX");

        void Pha()
        {
            LogInstruction(0, "PHA");
            Ram.PushByte(A);
            PC++;
            cyclesThisSec += 3;
        }

        void Pla()
        {
            LogInstruction(0, "PLA");
            A = Ram.PopByte();
            Bit.Val(ref P, Flags.Zero, A == 0);
            Bit.Val(ref P, Flags.Negative, Bit.Test(A, Flags.Negative));

            PC++;
            cyclesThisSec += 4;
        }

        void Php()
        {
            LogInstruction(0, "PHP");
            Ram.PushByte(P);

            PC++;
            cyclesThisSec += 3;
        }

        void Plp()
        {
            LogInstruction(0, "PLP");
            P = Ram.PopByte();

            //Bit 5 of P is unused, so clear it. It should always be 1.
            Bit.Set(ref P, 5);
            Bit.Clear(ref P, Flags.Break);

            PC++;
            cyclesThisSec += 4;
        }
        
        #endregion
        
        #region ASO/SLO This opcode ASLs the contents of a memory location and then ORs the result with the accumulator. 
        
        void Aso(ushort addr, int cycles, ushort pcIncrease) {
            LogInstruction(pcIncrease - 1, $"ASO ${addr:X2}");

            //ASL
            var value = Ram.Byte(addr);
            Bit.Val(ref P, Flags.Carry, Bit.Test(value, Flags.Negative));

            var shifted = (byte) (value << 1);
            Ram.WriteByte(addr, shifted);

            //Now the ORA
            A = (byte) (A | shifted);
            //Set the flags
            Bit.Val(ref P, Flags.Zero, A == 0x00);
            Bit.Val(ref P, Flags.Negative, Bit.Test(A, Flags.Negative));

            //Update cycles and pc
            cyclesThisSec += cycles;
            PC += pcIncrease;
        }

        void AsoAbsolute() => Aso(Ram.Absolute(Ram.Word(PC + 1)), 6, 3);
        void AsoAbsoluteX() => Aso(Ram.AbsoluteX(Ram.Word(PC + 1)), 7, 3);
        void AsoAbsoluteY() => Aso(Ram.AbsoluteY(Ram.Word(PC + 1)), 7, 3);
        void AsoZPage() => Aso(Ram.ZPage(Ram.Byte(PC + 1)), 5, 2);
        void AsoZPageX() => Aso(Ram.ZPageX(Ram.Byte(PC + 1)), 6, 2);
        void AsoIndirectX() => Aso(Ram.IndirectX(Ram.Byte(PC + 1)), 8, 2);
        void AsoIndirectY() => Aso(Ram.IndirectY(Ram.Byte(PC + 1)), 8, 2);
        
        #endregion
        
        #region ANC

        /// <summary>
        /// ANC ANDs the contents of the A register with an immediate value and then 
        /// moves bit 7 of A into the Carry flag.  This opcode works basically 
        /// identically to AND #immed. except that the Carry flag is set to the same 
        /// state that the Negative flag is set to.
        /// </summary>
        void Anc()
        {
            AndImmediate();
            Bit.Val(ref P, Flags.Carry, Bit.Test(A, Flags.Negative));
        }
        #endregion
        
        #region RLA

        void Rla(ushort addr, int cycles, ushort pcIncrease)
        {
            LogInstruction(pcIncrease - 1, $"RLA ${addr:X2}");

            var value = Ram.Byte(addr);

            var cachedFlagC = Bit.Test(P, Flags.Carry);
            var cached7 = Bit.Test(value, Flags.Negative);

            Bit.Val(ref P, Flags.Carry, cached7);
            var shifted = (byte) (value << 1);

            Bit.Val(ref shifted, 0, cachedFlagC);
            Ram.WriteByte(addr, shifted);

            A &= shifted;
            Bit.Val(ref P, Flags.Zero, A == 0);
            Bit.Val(ref P, Flags.Negative, Bit.Test(A, Flags.Negative));

            cyclesThisSec += cycles;
            PC += pcIncrease;
        }

        void RlaAbsolute() => Rla(Ram.Absolute(Ram.Word(PC + 1)), 6, 3);
        void RlaAbsoluteX() => Rla(Ram.AbsoluteX(Ram.Word(PC + 1)), 7, 3);
        void RlaAbsoluteY() => Rla(Ram.AbsoluteY(Ram.Word(PC + 1)), 7, 3);
        void RlaZPage() => Rla(Ram.ZPage(Ram.Byte(PC + 1)), 5, 2);
        void RlaZPageX() => Rla(Ram.ZPageX(Ram.Byte(PC + 1)), 6, 2);
        void RlaIndirectX() => Rla(Ram.IndirectX(Ram.Byte(PC + 1)), 8, 2);
        void RlaIndirectY() => Rla(Ram.IndirectY(Ram.Byte(PC + 1)), 8, 2);
        
        #endregion
        
        #region LSE

        void Lse(ushort addr, int cycles, ushort pcIncrease)
        {
            LogInstruction(pcIncrease - 1, $"LSE ${addr:X2}");

            byte data = Ram.Byte(addr);
            data = LsrInternal(data);
            Ram.WriteByte(addr, data);

            EorInternal(data);

            PC += pcIncrease;
            cyclesThisSec += cycles;
        }

        void LseAbsolute() => Lse(Ram.Absolute(Ram.Word(PC + 1)), 6, 3);
        void LseAbsoluteX() => Lse(Ram.AbsoluteX(Ram.Word(PC + 1)), 7, 3);
        void LseAbsoluteY() => Lse(Ram.AbsoluteY(Ram.Word(PC + 1)), 7, 3);
        void LseZPage() => Lse(Ram.ZPage(Ram.Byte(PC + 1)), 5, 2);
        void LseZPageX() => Lse(Ram.ZPageX(Ram.Byte(PC + 1)), 6, 2);
        void LseIndirectX() => Lse(Ram.IndirectX(Ram.Byte(PC + 1)), 8, 2);
        void LseIndirectY() => Lse(Ram.IndirectY(Ram.Byte(PC + 1)), 8, 2);
        
        #endregion
        
        #region ALR

        /// <summary>
        /// ALR This opcode ANDs the contents of the A register with an immediate value and 
        /// then LSRs the result.
        /// </summary>
        void Alr()
        {
            var value = Ram.Byte(PC + 1);
            LogInstruction(1, $"ALR #{value:X2}");
            value &= A;
            Bit.Val(ref P, Flags.Zero, A == 0);
            Bit.Val(ref P, Flags.Negative, Bit.Test(A, Flags.Negative));

            LsrInternal(value);
        }

        #endregion
        
        #region RRA

        void Rra(ushort addr, int cycles, ushort pcIncrease)
        {
            LogInstruction(pcIncrease - 1, $"RRA ${addr:X2}");

            var value = Ram.Byte(addr);
            value =  Rotate(value, RotateDirection.Right);
            Ram.WriteByte(addr, value);
            
            AdcInternal(value);
            
            PC += pcIncrease;
            cyclesThisSec += cycles;
        }
        
        void RraAbsolute() {
            Rra(Ram.Absolute(Ram.Word(PC + 1)), 6, 3);
        }

        void RraAbsoluteX() {
            Rra(Ram.AbsoluteX(Ram.Word(PC + 1)), 7, 3);
        }

        void RraAbsoluteY() {
            Rra(Ram.AbsoluteY(Ram.Word(PC + 1)), 7, 3);
        }

        void RraZPage() {
            Rra(Ram.ZPage(Ram.Byte(PC + 1)), 5, 2);
        }

        void RraZPageX() {
            Rra(Ram.ZPageX(Ram.Byte(PC + 1)), 6, 2);
        }

        void RraIndirectX() {
            Rra(Ram.IndirectX(Ram.Byte(PC + 1)), 8, 2);
        }

        void RraIndirectY() {
            Rra(Ram.IndirectY(Ram.Byte(PC + 1)), 8, 2);
        }
        
        #endregion
        
        #region ARR

        void Arr()
        {
            var value = Ram.Byte(PC + 1);
            LogInstruction(1, $"AAR #{value:X2}");
            
            value &= A;
            Bit.Val(ref P, Flags.Zero, A == 0);
            Bit.Val(ref P, Flags.Negative, Bit.Test(A, Flags.Negative));

            Rotate(value, RotateDirection.Right);
        }
        
        #endregion
        
        #region AXS

        void Axs(ushort addr, int cycles, ushort pcIncrease)
        {
            LogInstruction(1, $"SAX ${addr:X2}");

            var value = (byte) (A & X);
            Ram.WriteByte(addr, value);

            PC += pcIncrease;
            cyclesThisSec += cycles;
        }
        
        void AxsIndirectX() {
            Axs(Ram.IndirectX(Ram.Byte(PC + 1)), 6, 2);
        }

        void AxsZPage() {
            Axs(Ram.ZPage(Ram.Byte(PC + 1)), 3, 2);
        }

        void AxsZPageY() {
            Axs(Ram.ZPageY(Ram.Byte(PC + 1)), 4, 2);
        }

        void AxsAbsolute() {
            Axs(Ram.Absolute(Ram.Word(PC + 1)), 4, 3);
        }

        
        #endregion
        
        #region LAX
        
        void Lax(byte value, int cycles, ushort pcIncrease) 
        {
            LogInstruction(pcIncrease - 1, $"LAX ${value:X2}");
            X = value;
            A = value;

            Bit.Val(ref P, Flags.Zero, value == 0);
            Bit.Val(ref P, Flags.Negative, Bit.Test(value, Flags.Negative));

            PC += pcIncrease;
            cyclesThisSec += cycles;
        }

        void LaxAbsolute()  => Lax(Ram.AbsoluteParam(), 4, 3);
        void LaxAbsoluteY() => Lax(Ram.AbsoluteYParam(), 4, 3);
        void LaxZPage() => Lax(Ram.ZPageParam(), 3, 2);
        void LaxZPageY() => Lax(Ram.ZPageYParam(), 4, 2);
        void LaxIndirectX() => Lax(Ram.IndirectXParam(), 6, 2);
        void LaxIndirectY() => Lax(Ram.IndirectYParam(), 5, 2);

        #endregion
        
        #region DCM
        
        void Dcm(ushort addr, int cycles, ushort pcIncrease) {
            LogInstruction(pcIncrease - 1, $"DCP ${addr:X2}");

            var value = Ram.Byte(addr);
            value--;
            Ram.WriteByte(addr, value);
            
            var temp_result = (byte) (A - value);

            Bit.Val(ref P, Flags.Carry, A >= value);

            //Need to do this since there are some positive numbers that should trigger this flag. i.e. 0x80
            Bit.Val(ref P, Flags.Negative, Bit.Test(temp_result, Flags.Negative));
            Bit.Val(ref P, Flags.Zero, A == value);

            PC += pcIncrease;
            cyclesThisSec += cycles;
        }

        void DcmAbsolute() => Dcm(Ram.Absolute(Ram.Word(PC + 1)), 6, 3);
        void DcmAbsoluteX() => Dcm(Ram.AbsoluteX(Ram.Word(PC + 1)), 7, 3);
        void DcmAbsoluteY() => Dcm(Ram.AbsoluteY(Ram.Word(PC + 1)), 7, 3);
        void DcmZPage() => Dcm(Ram.ZPage(Ram.Byte(PC + 1)), 5, 2);
        void DcmZPageX() => Dcm(Ram.ZPageX(Ram.Byte(PC + 1)), 6, 2);
        void DcmIndirectX() => Dcm(Ram.IndirectX(Ram.Byte(PC + 1)), 8, 2);
        void DcmIndirectY() => Dcm(Ram.IndirectY(Ram.Byte(PC + 1)), 8, 2);
        
        
        #endregion
        
        #region INS
        
        void Ins(ushort addr, int cycles, ushort pcIncrease) {
            LogInstruction(pcIncrease - 1, $"INS ${addr:X2}");

            var value = Ram.Byte(addr);
            value++;
            Ram.WriteByte(addr, value);
            AdcInternal((byte)~value);

            PC += pcIncrease;
            cyclesThisSec += cycles;
        }

        void InsAbsolute() => Ins(Ram.Absolute(Ram.Word(PC + 1)), 6, 3);
        void InsAbsoluteX() => Ins(Ram.AbsoluteX(Ram.Word(PC + 1)), 7, 3);
        void InsAbsoluteY() => Ins(Ram.AbsoluteY(Ram.Word(PC + 1)), 7, 3);
        void InsZPage() => Ins(Ram.ZPage(Ram.Byte(PC + 1)), 5, 2);
        void InsZPageX() => Ins(Ram.ZPageX(Ram.Byte(PC + 1)), 6, 2);
        void InsIndirectX() => Ins(Ram.IndirectX(Ram.Byte(PC + 1)), 8, 2);
        void InsIndirectY() => Ins(Ram.IndirectY(Ram.Byte(PC + 1)), 8, 2);
        
        #endregion
        
        private void LogInstruction(int numParams, string mnemonic)
        {
            var sb = new StringBuilder();
            sb.Append($"{PC:X2} {currentOpcode:X2}  ");

            for (var i = 1; i <= numParams; i++) {
                sb.Append($"{Ram.Byte(PC + i):X2} ");
            }

            //The mnemonic should start at position 16. 
            var padding = 15 - sb.Length;
            sb.Append(string.Empty.PadRight(padding));
            sb.Append(mnemonic);

            padding = 48 - sb.Length;
            sb.Append(string.Empty.PadRight(padding));
            sb.Append($"A:{A:X2} X:{X:X2} Y:{Y:X2} P:{P:X2} SP:{SP:X2} CYC:{cyclesThisSec}");

            Log.Information(sb.ToString());
        }
    }

}
