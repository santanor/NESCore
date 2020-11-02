using System;
using System.ComponentModel;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Xml.Xsl;
using Serilog;

namespace NESCore
{
    public enum Flags {Carry, Zero, IRQ, Decimal, Break, Unused, Overflow, Negative};
    public enum AddressingModes {Accumulator, Immediate, ZeroPage, ZeroPageX, ZeroPageY, Absolute, AbsoluteX, AbsoluteY, Indirect, IndirectX, IndirectY}

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
        public PPU Ppu;

        private bool running;

        private Action[] opcodes;

        public void PowerUp()
        {
            X = 0x00;
            A = 0x00;
            Y = 0x00;
            P = 0x24;
            SP = 0xFD;
            Ram.WriteByte(0x4017, 0x00);

            for (ushort i = 0x4000; i <= 0x4013; i++)
            {
                Ram.WriteByte(i, 0x00);
            }

            running = true;
            cyclesThisSec = 7;
            
            CreateOpcodeArray();
        }

        private void CreateOpcodeArray()
        {
            opcodes = new Action[]
            {
                Break, //0x00
                () => Ora(AddressingModes.IndirectX), //0x01
                Halt, //0x02
                AsoIndirectX, //0x03
                Nop, //0x04
                () => Ora(AddressingModes.ZeroPage), //0x05
                () => Ram.WriteByte(Ram.ZPage(Ram.Byte(PC + 1)), Asl(AddressingModes.ZeroPage)), //0x06
                AsoZPage, //0x07
                Php, //0x08
                () => Ora(AddressingModes.Immediate), //0x09
                () => A = Asl(AddressingModes.Accumulator), //0x0A
                Anc, //0x0B
                Nop, //0x0C
                () => Ora(AddressingModes.Absolute), //0x0D
                () => Ram.WriteByte(Ram.Word(PC + 1), Asl(AddressingModes.Absolute)), //0x0E
                AsoAbsolute, //0x0F
                () => TryBranch(Flags.Negative, false, "BPL"), //0x10
                () => Ora(AddressingModes.IndirectY), //0x11
                Halt, //0x12
                AsoIndirectY, //0x13
                Nop, //0x14
                () => Ora(AddressingModes.ZeroPageX), //0x15
                () => Ram.WriteByte(Ram.ZPageX(Ram.Byte(PC + 1)), Asl(AddressingModes.ZeroPageX)), //0x16
                AsoZPageX, //0x17
                Clc, //0x18
                () => Ora(AddressingModes.AbsoluteY), //0x19
                Nop, //0x1A
                AsoAbsoluteY, //0x1B
                Nop, //0x1C
                () => Ora(AddressingModes.AbsoluteX), //0x1D
                () => Ram.WriteByte(Ram.AbsoluteX(Ram.Word(PC + 1)), Asl(AddressingModes.AbsoluteX)), //0x1E
                AsoAbsoluteX, //0x1F
                Jsr, //0x20
                () => And(AddressingModes.IndirectX), //0x21
                Halt, //0x22
                RlaIndirectX, //0x23
                BitZPage, //0x24
                () => And(AddressingModes.ZeroPage), //0x25
                () => Ram.WriteByte(Ram.ZPage(Ram.Byte(PC + 1)), Rol(AddressingModes.ZeroPage)), //0x26
                RlaZPage, //0x27
                Plp, //0x28
                () => And(AddressingModes.Immediate), //0x29
                () => A = Rol(AddressingModes.Accumulator), //0x2A,
                Anc, //0x2B
                BitAbsolute, //0x2C
                () => And(AddressingModes.Absolute), //0x2D
                () => Ram.WriteByte(Ram.Word(PC + 1), Rol(AddressingModes.Absolute)), //0x2E
                RlaAbsolute, //0x2F
                () => TryBranch(Flags.Negative, true, "BMI"), //0x30
                () => And(AddressingModes.IndirectY), //0x31
                Halt, //0x32
                RlaIndirectY, //0x33
                Nop, //0x34
                () => And(AddressingModes.ZeroPageX), //0x35
                () => Ram.WriteByte(Ram.ZPageX(Ram.Byte(PC + 1)), Rol(AddressingModes.ZeroPageX)), //0x36
                RlaZPageX, //0x37
                Sec, //0x38
                () => And(AddressingModes.AbsoluteY), //0x39
                Nop, //0x3A
                RlaAbsoluteY, //0x3B
                Nop, //0x3C
                () => And(AddressingModes.AbsoluteX), //0x3D
                () => Ram.WriteByte(Ram.AbsoluteX(Ram.Word(PC + 1)), Rol(AddressingModes.AbsoluteX)), //0x3E
                RlaAbsoluteX, //0x3F
                Rti, //0x40
                () => Eor(AddressingModes.IndirectX), //0x41
                Halt, //0x42
                LseIndirectX, //0x43
                Nop, //0x44
                () => Eor(AddressingModes.ZeroPage), //0x45
                () => Ram.WriteByte(Ram.ZPage(Ram.Byte(PC + 1)), Lsr(AddressingModes.ZeroPage)), //0x46
                LseZPage, //0x47
                Pha, //0x48
                () => Eor(AddressingModes.Immediate), //0x49
                () => A = Lsr(AddressingModes.Accumulator), //0x4A
                Alr, //0x4B
                JmpAbsolute, //0x4C
                () => Eor(AddressingModes.Absolute), //0x4D
                () => Ram.WriteByte(Ram.Word(PC + 1), Lsr(AddressingModes.Absolute)), //0x4E
                LseAbsolute, //0x4F
                () => TryBranch(Flags.Overflow, false, "BVC"), //0x50
                () => Eor(AddressingModes.IndirectY), //0x51
                Halt, //0x52
                LseIndirectY, //0x53
                Nop, //0x54
                () => Eor(AddressingModes.ZeroPageX), //0x55
                () => Ram.WriteByte(Ram.ZPageX(Ram.Byte(PC + 1)), Lsr(AddressingModes.ZeroPageX)), //0x56
                LseZPageX, //0x57
                Cli, //0x58
                () => Eor(AddressingModes.AbsoluteY), //0x59
                Nop, //0x5A
                LseAbsoluteY, //0x5B
                Nop, //0x5C
                () => Eor(AddressingModes.AbsoluteX), //0x5D
                () => Ram.WriteByte(Ram.AbsoluteX(Ram.Word(PC + 1)), Lsr(AddressingModes.AbsoluteX)), //0x5E
                LseAbsoluteX, //0x5F
                Rts, //0x60
                () => Adc(AddressingModes.IndirectX), //0x61
                Halt, //0x62
                RraIndirectX, //0x63
                Nop, //0x64
                () => Adc(AddressingModes.ZeroPage), //0x65
                () => Ram.WriteByte(Ram.ZPage(Ram.Byte(PC + 1)), Ror(AddressingModes.ZeroPage)), //0x66
                RraZPage, //0x67
                Pla, //0x68
                () => Adc(AddressingModes.Immediate), //0x69
                () => A = Ror(AddressingModes.Accumulator), //0x6A
                Arr, //0x6B
                JmpIndirect, //0x6C
                () => Adc(AddressingModes.Absolute), //0x6D
                () => Ram.WriteByte(Ram.Word(PC + 1), Ror(AddressingModes.Absolute)), //0x6E
                RraAbsolute, //0x6F
                () => TryBranch(Flags.Overflow, true, "BVS"), //0x70
                () => Adc(AddressingModes.IndirectY), //0x71
                Halt, //0x72
                RraIndirectY, //0x73
                Nop, //0x74
                () => Adc(AddressingModes.ZeroPageX), //0x75
                () => Ram.WriteByte(Ram.ZPageX(Ram.Byte(PC + 1)), Ror(AddressingModes.ZeroPageX)), //0x76
                RraZPageX, //0x77
                Sei, //0x78
                () => Adc(AddressingModes.AbsoluteY), //0x79
                Nop, //0x7A
                RraAbsoluteY, //0x7B
                Nop, //0x7C
                () => Adc(AddressingModes.AbsoluteX), //0x7D
                () => Ram.WriteByte(Ram.AbsoluteX(Ram.Word(PC + 1)), Ror(AddressingModes.AbsoluteX)), //0x7E
                RraAbsoluteX, //0x7F
                Nop, //0x80
                () => StoreRegister(A, AddressingModes.IndirectX, "STA"), //0x81
                Nop, //0x82
                AxsIndirectX, //0x83
                () => StoreRegister(Y, AddressingModes.ZeroPage, "STY"), //0x84
                () => StoreRegister(A, AddressingModes.ZeroPage, "STA"), //0x85
                () => StoreRegister(X, AddressingModes.ZeroPage, "STX"), //0x86
                AxsZPage, //0x87
                () => DeltaRegister(ref Y, -1, "DEY"), //0x88
                Nop, //0x89
                () => TransferRegister(X, ref A, "TXA"), //0x8A
                Halt, //0x8B
                () => StoreRegister(Y, AddressingModes.Absolute, "STY"), //0x8C
                () => StoreRegister(A, AddressingModes.Absolute, "STA"), //0x8D
                () => StoreRegister(X, AddressingModes.Absolute, "STX"), //0x8E
                AxsAbsolute, //0x8F
                () => TryBranch(Flags.Carry, false, "BCC"), //0x90
                () => StoreRegister(A, AddressingModes.IndirectY, "STA"), //0x91
                Halt, //0x92
                Halt, //0x93
                () => StoreRegister(Y, AddressingModes.ZeroPageX, "STY"), //0x94
                () => StoreRegister(A, AddressingModes.ZeroPageX, "STA"), //0x95
                () => StoreRegister(X, AddressingModes.ZeroPageY, "STX"), //0x96
                AxsZPageY, //0x97
                () => TransferRegister(Y, ref A, "TYA"), //0x98
                () => StoreRegister(A, AddressingModes.AbsoluteY, "STA"), //0x99
                Txs, //0x9A
                Halt, //0x9B
                Halt, //0x9C
                () => StoreRegister(A, AddressingModes.AbsoluteX, "STA"), //0x9D
                Halt, //0x9E
                Halt, //0x9F
                () => LoadRegister(ref Y, AddressingModes.Immediate, "LDY"), //0xA0
                () => LoadRegister(ref A, AddressingModes.IndirectX, "LDA"), //0xA1
                () => LoadRegister(ref X, AddressingModes.Immediate, "LDX"), //0xA2
                LaxIndirectX, //0xA3
                () => LoadRegister(ref Y, AddressingModes.ZeroPage, "LDY"), //0xA4
                () => LoadRegister(ref A, AddressingModes.ZeroPage, "LDA"), //0xA5
                () => LoadRegister(ref X, AddressingModes.ZeroPage, "LDX"), //0xA6
                LaxZPage, //0xA7
                () => TransferRegister(A, ref Y, "TAY"), //0xA8
                () => LoadRegister(ref A, AddressingModes.Immediate, "LDA"), //0xA9
                () => TransferRegister(A, ref X, "TAX"), //0xAA
                Halt, //0xAB
                () => LoadRegister(ref Y, AddressingModes.Absolute, "LDY"), //0xAC
                () => LoadRegister(ref A, AddressingModes.Absolute, "LDA"), //0xAD
                () => LoadRegister(ref X, AddressingModes.Absolute, "LDX"), //0xAE
                LaxAbsolute, //0xAF
                () => TryBranch(Flags.Carry, true, "BCS"), //0xB0
                () => LoadRegister(ref A, AddressingModes.IndirectY, "LDA"), //0xB1
                Halt, //0xB2
                LaxIndirectY, //0xB3
                () => LoadRegister(ref Y, AddressingModes.ZeroPageX, "LDY"), //0xB4
                () => LoadRegister(ref A, AddressingModes.ZeroPageX, "LDA"), //0xB5
                () => LoadRegister(ref X, AddressingModes.ZeroPageY, "LDX"), //0xB6
                LaxZPageY, //0xB7
                Clv, //0xB8
                () => LoadRegister(ref A, AddressingModes.AbsoluteY, "LDA"), //0xB9
                Tsx, //0xBA
                Halt, //0xBB
                () => LoadRegister(ref Y, AddressingModes.AbsoluteX, "LDY"), //0xBC
                () => LoadRegister(ref A, AddressingModes.AbsoluteX, "LDA"), //0xBD
                () => LoadRegister(ref X, AddressingModes.AbsoluteY, "LDX"), //0xBE
                LaxAbsoluteY, //0xBF
                () => Cmp(Y, AddressingModes.Immediate, "CPY"), //0xC0
                () => Cmp(A, AddressingModes.IndirectX, "CMP"), //0xC1
                Nop, //0xC2
                DcmIndirectX, //0xC3
                () => Cmp(Y, AddressingModes.ZeroPage, "CPY"), //0xC4
                () => Cmp(A, AddressingModes.ZeroPage, "CMP"), //0xC5
                () => DeltaMemory(AddressingModes.ZeroPage, -1, "DEC"), //0xC6
                DcmZPage, //0xC7
                () => DeltaRegister(ref Y, 1, "INY"), //0xC8
                () => Cmp(A, AddressingModes.Immediate, "CMP"), //0xC9
                () => DeltaRegister(ref X, -1, "DEX"), //0xCA DEX
                Halt, //0xCB
                () => Cmp(Y, AddressingModes.Absolute, "CPY"), //0xCC
                () => Cmp(A, AddressingModes.Absolute, "CMP"), //0xCD
                () => DeltaMemory(AddressingModes.Absolute, -1, "DEC"), //0xCE
                DcmAbsolute, //0xCF
                () => TryBranch(Flags.Zero, false, "BNE"), //0xD0
                () => Cmp(A, AddressingModes.IndirectY, "CMP"), //0xD1
                Halt, //0xD2
                DcmIndirectY, //0xD3
                Nop, //0xD4
                () => Cmp(A, AddressingModes.ZeroPageX, "CMP"), //0xD5
                () => DeltaMemory(AddressingModes.ZeroPageX, -1, "DEC"), //0xD6
                DcmZPageX, //0xD7
                Cld, //0xD8
                () => Cmp(A, AddressingModes.AbsoluteY, "CMP"), //0xD9
                Nop, //0xDA
                DcmAbsoluteY, //0xDB
                Nop, //0xDC
                () => Cmp(A, AddressingModes.AbsoluteX, "CMP"), //0xDD
                () => DeltaMemory(AddressingModes.AbsoluteX, -1, "DEC"), //0xDE
                DcmAbsoluteX, //0xDF
                () => Cmp(X, AddressingModes.Immediate, "CPX"), //0xE0
                () => Sbc(AddressingModes.IndirectX), //0xE1
                Nop, //0xE2
                InsIndirectX, //0xE3
                () => Cmp(X, AddressingModes.ZeroPage, "CPX"), //0xE4
                () => Sbc(AddressingModes.ZeroPage), //0xE5
                () => DeltaMemory(AddressingModes.ZeroPage, 1, "INC"), //0xE6
                InsZPage, //0xE7
                () => DeltaRegister(ref X, 1, "INX"), //0xE8
                () => Sbc(AddressingModes.Immediate), //0xE9
                Nop, //0xEA
                () => Sbc(AddressingModes.IndirectY), //0xEB
                () => Cmp(X, AddressingModes.Absolute, "CPX"), //0xEC
                () => Sbc(AddressingModes.Absolute), //0xED
                () => DeltaMemory(AddressingModes.Absolute, 1, "INC"), //0xEE
                InsAbsolute, //0xEF
                () => TryBranch(Flags.Zero, true, "BEQ"), //0xF0
                () => Sbc(AddressingModes.IndirectY), //0xF1
                Halt, //0xF2
                InsIndirectY, //0xF3
                Nop, //0xF4
                () => Sbc(AddressingModes.ZeroPageX), //0xF5
                () => DeltaMemory(AddressingModes.ZeroPageX, 1, "INC"), //0xF6
                InsZPageX, //0xF7
                Sed, //0xF8
                () => Sbc(AddressingModes.AbsoluteY), //0xF9
                Nop, //0xFA
                InsAbsoluteY, //0xFB
                Nop, //0xFC
                () => Sbc(AddressingModes.AbsoluteX), //0xFD
                () => DeltaMemory(AddressingModes.AbsoluteX, 1, "INC"), //0xFE
                InsAbsoluteX, //0xFF
            };

        }

        public void Stop() => running = false;

        /// <summary>
        /// Runs a CPU instruction
        /// </summary>
        /// <returns>Cycles it took to run the instruction</returns>
        public int Instruction()
        {
            var cyclesBefore = cyclesThisSec;
            currentOpcode = Ram.Byte(PC);
            opcodes[currentOpcode].Invoke();
            return cyclesThisSec - cyclesBefore;
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

        /// <summary>
        /// Executes the ORA instruction (OR with Acumulator)
        /// </summary>
        void Ora(AddressingModes mode)
        {
            var (value, pcIncrease, cycles) = GetAddressingModeParameter(mode);
            LogInstruction(mode, "ORA");
            
            A = (byte)(A | value);

            Bit.Val(ref P, Flags.Zero, A == 0);
            Bit.Val(ref P, Flags.Negative, Bit.Test(A, Flags.Negative));

            cyclesThisSec += cycles;
            PC += pcIncrease;
        }

        private byte Asl(AddressingModes mode)
        {
            var (value, pcIncrease, cycles) = GetAddressingModeParameter(mode);
            if (mode != AddressingModes.Accumulator)
            {
                cycles += 2;
            }

            if (mode == AddressingModes.AbsoluteX)
            {
                cycles++;
            }
            
            LogInstruction(mode, "ASL");
            
            Bit.Val(ref P, Flags.Carry, Bit.Test(value, Flags.Negative));

            var shifted = (byte)(value << 1);
            Bit.Val(ref P, Flags.Negative, Bit.Test(shifted, Flags.Negative));
            Bit.Val(ref P, Flags.Zero, shifted == 0);
            
            PC += pcIncrease;
            cyclesThisSec += cycles;
            
            return shifted;
        }

        void Halt()
        {
            LogInstruction(0, "KILL");
            Stop();
        }

        private void Adc(AddressingModes mode)
        {
            var (value, pcIncrease, cycles) = GetAddressingModeParameter(mode);
            LogInstruction(mode, "ADC");

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

        void Sbc(AddressingModes mode)
        {
            var (value, pcIncrease, cycles) = GetAddressingModeParameter(mode);
            LogInstruction(mode, "SBC");

            AdcInternal((byte) ~value);
            
            PC += pcIncrease;
            cyclesThisSec += cycles;
        }

        /// <summary>
        /// ANDs the given value with the accumulator 
        /// </summary>
        private void And(AddressingModes mode)
        {
            var (value, pcIncrease, cycles) = GetAddressingModeParameter(mode);
            LogInstruction(mode, "AND");
            
            A &= value;
            Bit.Val(ref P, Flags.Zero, A == 0);
            Bit.Val(ref P, Flags.Negative, Bit.Test(A, Flags.Negative));

            cyclesThisSec += cycles;
            PC += pcIncrease;
        }

        private void BIT(byte value, int cycles, ushort pcIncrease)
        {
            byte tmp = (byte)(A & value);
            Bit.Val(ref P, Flags.Zero, tmp == 0);
            Bit.Val(ref P, Flags.Overflow, Bit.Test(value, Flags.Overflow));
            Bit.Val(ref P, Flags.Negative, Bit.Test(value, Flags.Negative));

            cyclesThisSec += cycles;
            PC += pcIncrease;
        }

        private void BitZPage()
        {
            var paramAddr = Ram.ZPage(Ram.Byte(PC + 1));
            var value = Ram.ZPageParam();
            LogInstruction(1, $"BIT ${paramAddr:X2} = {value:X2}");
            BIT(value, 3, 2);
            
        }

        private void BitAbsolute()
        {
            var paramAddr = Ram.Absolute(Ram.Word(PC + 1));
            var value = Ram.Byte(paramAddr);
            LogInstruction(2, $"BIT ${paramAddr:X4} = {value:X2}");
            BIT(value, 4, 3);
        }

        private void TryBranch(Flags flag, bool reqFlagValue, string mnemonic)
        {
            var value = Ram.Byte(PC + 1);
            LogInstruction(1, $"{mnemonic} ${(PC + value + 2):X4}");
            cyclesThisSec += 2; //This is always constant
            PC += 2;

            if (Bit.Test(P, flag) == reqFlagValue)
            {
                Ram.CheckPageCrossed((ushort) (PC + value), PC);
                PC += value;

                cyclesThisSec++;
            }
        }

        private void Cmp(byte register, AddressingModes mode, string mnemonic)
        {
            var (value, pcIncrease, cycles) = GetAddressingModeParameter(mode);
            LogInstruction(mode, mnemonic);

            var tmp = (byte)(register - value);

            Bit.Val(ref P, Flags.Carry, register >= value);

            //Need to do this since there are some positive numbers that should trigger this flag. i.e. 0x80
            Bit.Val(ref P, Flags.Negative, Bit.Test(tmp, Flags.Negative));
            Bit.Val(ref P, Flags.Zero, register == value);

            PC += pcIncrease;
            cyclesThisSec += cycles;
        }

        void DeltaMemory(AddressingModes mode, int delta, string mnemonic)
        {
            var (memAddr, pcIncrease, cycles) = GetAddressingModeAddress(mode);
            var value = Ram.Byte(memAddr);
            LogInstruction(mode, mnemonic);
            value += (byte)delta;
            
            Ram.WriteByte(memAddr, value);

            Bit.Val(ref P, Flags.Zero, value == 0);
            Bit.Val(ref P, Flags.Negative, Bit.Test(value, Flags.Negative));

            PC += pcIncrease;
            cyclesThisSec += cycles;
        }

        #region EOR bitwise exclusive OR

        void Eor(AddressingModes mode)
        {
            var (value, pcIncrease, cycles) = GetAddressingModeParameter(mode);
            
            LogInstruction(mode, "EOR");
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
            PC = addr;
            cyclesThisSec += cycles;
        }

        void JmpAbsolute()
        {
            var addr = Ram.Word(PC + 1);
            LogInstruction(2, $"JMP ${addr:X4}");
            Jmp(addr, 3);
        }

        void JmpIndirect()
        {
            var addr = Ram.IndirectParam();
            var opcodeParam = Ram.Word(PC + 1);
            LogInstruction(2, $"JMP (${opcodeParam:X4}) = {addr:X4}");
            Jmp(addr, 5);
        }

        void Jsr()
        {
            var cachedPc = (ushort) (PC + 0x02);
            var addr = Ram.Absolute(Ram.Word(PC + 1));
            LogInstruction(2, $"JSR ${addr:X2}");
            
            Ram.PushWord(cachedPc); // Stores the address of the next opcode minus one

            PC = addr;
            cyclesThisSec += 6;
        }
        
        #endregion

        void LoadRegister(ref byte register, AddressingModes mode, string mnemonic)
        {
            LogInstruction(mode, mnemonic);
            var (value, pcIncrease, cycles) = GetAddressingModeParameter(mode);

            register = value;

            Bit.Val(ref P, Flags.Zero, register == 0);
            Bit.Val(ref P, Flags.Negative, Bit.Test(register, Flags.Negative));

            PC += pcIncrease;
            cyclesThisSec += cycles;
        }
        
        #region LSR Logical Shift Right

        byte Lsr(AddressingModes mode)
        {
            //Override pcIncrease and cycle value because LSR has diferent timings
            var (value, pcIncrease, cycles) = GetAddressingModeParameter(mode);
            if (mode != AddressingModes.Accumulator)
            {
                cycles += 2;
            }

            if (mode == AddressingModes.AbsoluteX)
            {
                cycles++;
            }
            
            LogInstruction(mode, "LSR");

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

        #endregion
        
        #region NOP

        void Nop()
        {
            switch (Ram.Byte(PC)){
                case 0xEA:
                    LogInstruction(0,"NOP");
                    PC++;
                    cyclesThisSec += 2;
                    break;
                case 0x1A: case 0x3A: case 0x5A: case 0x7A: case 0xDA: case 0xFA:
                    LogInstruction(0,"NOP", true);
                    PC++;
                    cyclesThisSec += 2;
                    break;

                case 0x80: case 0x82: case 0x89: case 0xC2: case 0xE2:
                    LogInstruction(AddressingModes.Immediate, "NOP", true);
                    PC += 2;
                    cyclesThisSec += 2;
                    break;
                case 0x0C:
                    LogInstruction(AddressingModes.Absolute, "NOP", true);
                    PC += 3;
                    cyclesThisSec += 4;
                    break;
                case 0x1C: case 0x3C: case 0x5C: case 0x7C: case 0xDC: case 0xFC:
                    LogInstruction(AddressingModes.AbsoluteX, "NOP", true);
                    PC += 3;
                    Ram.AbsoluteXParam(true);
                    cyclesThisSec += 4;
                    break;
                case 0x04: case 0x44: case 0x64:
                    LogInstruction(AddressingModes.ZeroPage, "NOP", true);
                    PC += 2;
                    cyclesThisSec += 3;
                    break;
                case 0x14: case 0x34: case 0x54:  case 0x74: case 0xD4: case 0xF4:
                    LogInstruction(AddressingModes.ZeroPageX, "NOP", true);
                    PC += 2;
                    cyclesThisSec += 4;
                    break;
            }
        }
        #endregion
        
        #region Register Instructions

        // ReSharper disable once RedundantAssignment
        void TransferRegister(byte source, ref byte destination, string mnemonic, bool updateFlags = true)
        {
            LogInstruction(0, mnemonic);
            
            destination = source;
            if (updateFlags)
            {
                Bit.Val(ref P, Flags.Zero, destination == 0);
                Bit.Val(ref P, Flags.Negative, Bit.Test(destination, Flags.Negative));    
            }
            
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

        #endregion
        
        #region ROR/ROL Rotate region

        byte Rol(AddressingModes mode)
        {
            var (value, pcIncrease, cycles) = GetAddressingModeParameter(mode);
            if (mode != AddressingModes.Accumulator)
            {
                cycles += 2;
            }
            
            if (mode == AddressingModes.AbsoluteX)
            {
                cycles++;
            }
            
            LogInstruction(mode, "ROL");
            
            PC += pcIncrease;
            cyclesThisSec += cycles;
            return Rotate(value, RotateDirection.Left);
        }

        byte Ror(AddressingModes mode)
        {
            var (value, pcIncrease, cycles) = GetAddressingModeParameter(mode);
            if (mode != AddressingModes.Accumulator)
            {
                cycles += 2;
            }

            if (mode == AddressingModes.AbsoluteX)
            {
                cycles++;
            }
            
            LogInstruction(mode, "ROR");

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

        /// <summary>
        /// Stores the given register value in the specified address
        /// </summary>
        /// <param name="value"></param>
        /// <param name="addr"></param>
        /// <param name="cycles"></param>
        /// <param name="pcIncrease"></param>
        /// <param name="mnemonic">Opcode mnemonic to log the instruction. If null is provided then this method
        /// won't log the instruction. It assumes that the caller wants to create a bespoke log for the specific call</param>
        void StoreRegister(byte value, AddressingModes mode,[AllowNull]string mnemonic)
        {
            var (addr, pcIncrease, cycles) = GetAddressingModeAddress(mode);
            LogInstruction(mode, mnemonic);
            
            Ram.WriteByte(addr, value);

            switch (mode)
            {
                case AddressingModes.ZeroPage: case AddressingModes.ZeroPageX: case AddressingModes.Absolute:
                case AddressingModes.AbsoluteX:
                    cycles -= 2;
                    break;
            }
            
            PC += pcIncrease;
            cyclesThisSec += cycles;
        }

        #region Stack instructions

        void Txs() => TransferRegister(X, ref SP, "TXS", false);
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
            Ram.PushByte((byte) (P | 0x10));

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
            And(AddressingModes.Immediate);
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

        private (byte, ushort, ushort) GetAddressingModeParameter(AddressingModes addressingModes)
        {
            switch (addressingModes)
            {
                case AddressingModes.Accumulator:
                    return (A, 1, 2);
                case AddressingModes.Immediate:
                    return (Ram.Byte(PC + 1), 2, 2);
                case AddressingModes.ZeroPage:
                    return (Ram.ZPageParam(), 2, 3);
                case AddressingModes.ZeroPageX:
                    return (Ram.ZPageXParam(), 2, 4);
                case AddressingModes.ZeroPageY:
                    return (Ram.ZPageYParam(), 2, 4);
                case AddressingModes.Absolute:
                    return (Ram.AbsoluteParam(), 3, 4);
                case AddressingModes.AbsoluteX:
                    return (Ram.AbsoluteXParam(true), 3, 4);
                case AddressingModes.AbsoluteY:
                    return (Ram.AbsoluteYParam(true), 3, 4);
                case AddressingModes.IndirectX:
                    return (Ram.IndirectXParam(), 2, 6);
                case AddressingModes.IndirectY:
                    return (Ram.IndirectYParam(true), 2, 5);
                default:
                    throw new ArgumentOutOfRangeException(nameof(addressingModes), addressingModes, null);
            }
        }

        private (ushort, ushort, ushort) GetAddressingModeAddress(AddressingModes mode)
        {
            switch (mode)
            {
                case AddressingModes.ZeroPage:
                    return (Ram.ZPage(Ram.Byte(PC + 1)), 2, 5);
                case AddressingModes.ZeroPageX:
                    return (Ram.ZPageX(Ram.Byte(PC + 1)), 2, 6);
                case AddressingModes.ZeroPageY:
                    return (Ram.ZPageY(Ram.Byte(PC + 1)), 2, 4);
                case AddressingModes.Absolute:
                    return (Ram.Word(PC + 1), 3, 6);
                case AddressingModes.AbsoluteX:
                    return (Ram.AbsoluteX(Ram.Word(PC + 1)), 3, 7);
                case AddressingModes.Indirect:
                    return (Ram.IndirectParam(),0, 5);
                case AddressingModes.IndirectX:
                    return (Ram.IndirectX(Ram.Byte(PC + 1)), 2, 6);
                case AddressingModes.IndirectY:
                    return (Ram.IndirectY(Ram.Byte(PC + 1)), 2, 6);
                case AddressingModes.AbsoluteY:
                    return (Ram.AbsoluteY(Ram.Word(PC + 1)), 3, 5);
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }
        }

            private void LogInstruction(AddressingModes mode, string mnemonic, bool invalid = false)
        {
            var numParams = 0;
            var instruction = new StringBuilder(mnemonic).Append(" ");
            switch (mode)
            {
                case AddressingModes.Accumulator:
                    instruction.Append("A");
                    break;
                case AddressingModes.Immediate:
                    numParams = 1;
                    instruction.AppendFormat("#${0:X2}", Ram.Byte(PC + 1));
                    break;
                case AddressingModes.ZeroPage:
                    numParams = 1;
                    instruction.AppendFormat("${0:X2} = {1:X2}",Ram.Byte(PC + 1), Ram.ZPageParam());
                    break;
                case AddressingModes.ZeroPageX:
                    numParams = 1;
                    instruction.AppendFormat("${0:X2},X @ {1:X2} = {2:X2}",Ram.Byte(PC + 1), Ram.ZPageX(Ram.Byte(PC + 1)), Ram.ZPageXParam());
                    break;
                case AddressingModes.ZeroPageY:
                    numParams = 1;
                    instruction.AppendFormat("${0:X2},Y @ {1:X2} = {2:X2}",Ram.Byte(PC + 1),Ram.ZPageY(Ram.Byte(PC + 1)), Ram.ZPageYParam());
                    break;
                case AddressingModes.IndirectX:
                    numParams = 1;
                    var indXParam = Ram.Byte(PC + 1);
                    var indXVal = (indXParam + X) & 0xFF;
                    instruction.AppendFormat("(${0:X2},X) @ {1:X2} = {2:X4} = {3:X2}", 
                        indXParam, indXVal, Ram.IndirectX(indXParam), Ram.IndirectXParam());
                    break;
                case AddressingModes.IndirectY:
                    numParams = 1;
                    var opcodeParam = Ram.Byte(PC + 1);
                    var initialAddr = Ram.Byte(opcodeParam & 0xFF) | Ram.Byte((opcodeParam + 1) & 0xFF) << 8;
                    instruction.AppendFormat("(${0:X2}),Y = {1:X4} @ {2:X4} = {3:X2}", 
                        opcodeParam, initialAddr, Ram.IndirectY(opcodeParam), Ram.IndirectYParam());
                    break;
                case AddressingModes.Absolute:
                    numParams = 2;
                    instruction.AppendFormat("${0:X4} = {1:X2}", Ram.Word(PC + 1), Ram.AbsoluteParam());
                    break;
                case AddressingModes.AbsoluteX: 
                    numParams = 2;
                    instruction.AppendFormat("${0:X4},X @ {1:X4} = {2:X2}", Ram.Word(PC + 1), Ram.AbsoluteX(Ram.Word(PC + 1)), Ram.AbsoluteXParam());
                    break;
                case AddressingModes.AbsoluteY:
                    numParams = 2;
                    var absYparam = Ram.Word(PC + 1);
                    var absYInitialAddr = (absYparam + Y) & 0xFFFF;
                    instruction.AppendFormat("${0:X4},Y @ {1:X4} = {2:X2}", Ram.Word(PC + 1),absYInitialAddr, Ram.AbsoluteYParam());
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }
            
            LogInstruction(numParams, instruction.ToString(), invalid);
        }
        
        private void LogInstruction(int numParams, string mnemonic, bool invalid = false)
        {
            var sb = new StringBuilder();
            sb.Append($"{PC:X4}  {currentOpcode:X2} ");

            for (var i = 1; i <= numParams; i++) {
                sb.Append($"{Ram.Byte(PC + i):X2} ");
            }

            //The mnemonic should start at position 16 or at 15 if invalid. 
            var padding = Math.Max((invalid ? 15 : 16) - sb.Length, 0);
            sb.Append(string.Empty.PadRight(padding));
            if (invalid)
            {
                sb.Append("*");
            }
            sb.Append(mnemonic);

            padding = 48 - sb.Length;
            sb.Append(string.Empty.PadRight(padding));
            sb.Append($"A:{A:X2} X:{X:X2} Y:{Y:X2} P:{P:X2} SP:{SP:X2}");

            sb.Append(" PPU:").Append(Ppu.CyclesThisFrame.ToString().PadLeft(3));
            sb.Append(",").Append(Ppu.FrameCount.ToString().PadLeft(3)).Append($" CYC:{cyclesThisSec}");

            Log.Information(sb.ToString());
        }
    }

}
