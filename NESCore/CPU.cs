using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
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

        public Bus Bus;

        private bool running;

        private Action[] opcodes;

        public CPU(Bus bus)
        {
            Bus = bus;
        }

        public void PowerUp()
        {
            X = 0x00;
            A = 0x00;
            Y = 0x00;
            P = 0x24;
            SP = 0xFD;
            Bus.WriteByte(0x4017, 0x00);

            for (ushort i = 0x4000; i <= 0x4013; i++)
            {
                Bus.WriteByte(i, 0x00);
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
                () => Aso(AddressingModes.IndirectX), //0x03
                Nop, //0x04
                () => Ora(AddressingModes.ZeroPage), //0x05
                () => Bus.WriteByte(ZPage(Bus.Byte(PC + 1)), Asl(AddressingModes.ZeroPage)), //0x06
                () => Aso(AddressingModes.ZeroPage), //0x07
                Php, //0x08
                () => Ora(AddressingModes.Immediate), //0x09
                () => A = Asl(AddressingModes.Accumulator), //0x0A
                Anc, //0x0B
                Nop, //0x0C
                () => Ora(AddressingModes.Absolute), //0x0D
                () => Bus.WriteByte(Bus.Word(PC + 1), Asl(AddressingModes.Absolute)), //0x0E
                () => Aso(AddressingModes.Absolute), //0x0F
                () => TryBranch(Flags.Negative, false, "BPL"), //0x10
                () => Ora(AddressingModes.IndirectY), //0x11
                Halt, //0x12
                () => Aso(AddressingModes.IndirectY), //0x13
                Nop, //0x14
                () => Ora(AddressingModes.ZeroPageX), //0x15
                () => Bus.WriteByte(ZPageX(Bus.Byte(PC + 1)), Asl(AddressingModes.ZeroPageX)), //0x16
                () => Aso(AddressingModes.ZeroPageX), //0x17
                Clc, //0x18
                () => Ora(AddressingModes.AbsoluteY), //0x19
                Nop, //0x1A
                () => Aso(AddressingModes.AbsoluteY), //0x1B
                Nop, //0x1C
                () => Ora(AddressingModes.AbsoluteX), //0x1D
                () => Bus.WriteByte(AbsoluteX(Bus.Word(PC + 1)), Asl(AddressingModes.AbsoluteX)), //0x1E
                () => Aso(AddressingModes.AbsoluteX), //0x1F
                Jsr, //0x20
                () => And(AddressingModes.IndirectX), //0x21
                Halt, //0x22
                () => Rla(AddressingModes.IndirectX), //0x23
                BitZPage, //0x24
                () => And(AddressingModes.ZeroPage), //0x25
                () => Bus.WriteByte(ZPage(Bus.Byte(PC + 1)), Rol(AddressingModes.ZeroPage)), //0x26
                () => Rla(AddressingModes.ZeroPage), //0x27
                Plp, //0x28
                () => And(AddressingModes.Immediate), //0x29
                () => A = Rol(AddressingModes.Accumulator), //0x2A,
                Anc, //0x2B
                BitAbsolute, //0x2C
                () => And(AddressingModes.Absolute), //0x2D
                () => Bus.WriteByte(Bus.Word(PC + 1), Rol(AddressingModes.Absolute)), //0x2E
                () => Rla(AddressingModes.Absolute), //0x2F
                () => TryBranch(Flags.Negative, true, "BMI"), //0x30
                () => And(AddressingModes.IndirectY), //0x31
                Halt, //0x32
                () => Rla(AddressingModes.IndirectY), //0x33
                Nop, //0x34
                () => And(AddressingModes.ZeroPageX), //0x35
                () => Bus.WriteByte(ZPageX(Bus.Byte(PC + 1)), Rol(AddressingModes.ZeroPageX)), //0x36
                () => Rla(AddressingModes.ZeroPageX), //0x37
                Sec, //0x38
                () => And(AddressingModes.AbsoluteY), //0x39
                Nop, //0x3A
                () => Rla(AddressingModes.AbsoluteY), //0x3B
                Nop, //0x3C
                () => And(AddressingModes.AbsoluteX), //0x3D
                () => Bus.WriteByte(AbsoluteX(Bus.Word(PC + 1)), Rol(AddressingModes.AbsoluteX)), //0x3E
                () => Rla(AddressingModes.AbsoluteX), //0x3F
                Rti, //0x40
                () => Eor(AddressingModes.IndirectX), //0x41
                Halt, //0x42
                () => Lse(AddressingModes.IndirectX), //0x43
                Nop, //0x44
                () => Eor(AddressingModes.ZeroPage), //0x45
                () => Bus.WriteByte(ZPage(Bus.Byte(PC + 1)), Lsr(AddressingModes.ZeroPage)), //0x46
                () => Lse(AddressingModes.ZeroPage), //0x47
                Pha, //0x48
                () => Eor(AddressingModes.Immediate), //0x49
                () => A = Lsr(AddressingModes.Accumulator), //0x4A
                Alr, //0x4B
                JmpAbsolute, //0x4C
                () => Eor(AddressingModes.Absolute), //0x4D
                () => Bus.WriteByte(Bus.Word(PC + 1), Lsr(AddressingModes.Absolute)), //0x4E
                () => Lse(AddressingModes.Absolute), //0x4F
                () => TryBranch(Flags.Overflow, false, "BVC"), //0x50
                () => Eor(AddressingModes.IndirectY), //0x51
                Halt, //0x52
                () => Lse(AddressingModes.IndirectY), //0x53
                Nop, //0x54
                () => Eor(AddressingModes.ZeroPageX), //0x55
                () => Bus.WriteByte(ZPageX(Bus.Byte(PC + 1)), Lsr(AddressingModes.ZeroPageX)), //0x56
                () => Lse(AddressingModes.ZeroPageX), //0x57
                Cli, //0x58
                () => Eor(AddressingModes.AbsoluteY), //0x59
                Nop, //0x5A
                () => Lse(AddressingModes.AbsoluteY), //0x5B
                Nop, //0x5C
                () => Eor(AddressingModes.AbsoluteX), //0x5D
                () => Bus.WriteByte(AbsoluteX(Bus.Word(PC + 1)), Lsr(AddressingModes.AbsoluteX)), //0x5E
                () => Lse(AddressingModes.AbsoluteX), //0x5F
                Rts, //0x60
                () => Adc(AddressingModes.IndirectX), //0x61
                Halt, //0x62
                () => Rra(AddressingModes.IndirectX), //0x63
                Nop, //0x64
                () => Adc(AddressingModes.ZeroPage), //0x65
                () => Bus.WriteByte(ZPage(Bus.Byte(PC + 1)), Ror(AddressingModes.ZeroPage)), //0x66
                () => Rra(AddressingModes.ZeroPage), //0x67
                Pla, //0x68
                () => Adc(AddressingModes.Immediate), //0x69
                () => A = Ror(AddressingModes.Accumulator), //0x6A
                Arr, //0x6B
                JmpIndirect, //0x6C
                () => Adc(AddressingModes.Absolute), //0x6D
                () => Bus.WriteByte(Bus.Word(PC + 1), Ror(AddressingModes.Absolute)), //0x6E
                () => Rra(AddressingModes.Absolute), //0x6F
                () => TryBranch(Flags.Overflow, true, "BVS"), //0x70
                () => Adc(AddressingModes.IndirectY), //0x71
                Halt, //0x72
                () => Rra(AddressingModes.IndirectY), //0x73
                Nop, //0x74
                () => Adc(AddressingModes.ZeroPageX), //0x75
                () => Bus.WriteByte(ZPageX(Bus.Byte(PC + 1)), Ror(AddressingModes.ZeroPageX)), //0x76
                () => Rra(AddressingModes.ZeroPageX), //0x77
                Sei, //0x78
                () => Adc(AddressingModes.AbsoluteY), //0x79
                Nop, //0x7A
                () => Rra(AddressingModes.AbsoluteY), //0x7B
                Nop, //0x7C
                () => Adc(AddressingModes.AbsoluteX), //0x7D
                () => Bus.WriteByte(AbsoluteX(Bus.Word(PC + 1)), Ror(AddressingModes.AbsoluteX)), //0x7E
                () => Rra(AddressingModes.AbsoluteX), //0x7F
                Nop, //0x80
                () => StoreRegister(A, AddressingModes.IndirectX, "STA"), //0x81
                Nop, //0x82
                () => Axs(AddressingModes.IndirectX), //0x83
                () => StoreRegister(Y, AddressingModes.ZeroPage, "STY"), //0x84
                () => StoreRegister(A, AddressingModes.ZeroPage, "STA"), //0x85
                () => StoreRegister(X, AddressingModes.ZeroPage, "STX"), //0x86
                () => Axs(AddressingModes.ZeroPage), //0x87
                () => DeltaRegister(ref Y, -1, "DEY"), //0x88
                Nop, //0x89
                () => TransferRegister(X, ref A, "TXA"), //0x8A
                Halt, //0x8B
                () => StoreRegister(Y, AddressingModes.Absolute, "STY"), //0x8C
                () => StoreRegister(A, AddressingModes.Absolute, "STA"), //0x8D
                () => StoreRegister(X, AddressingModes.Absolute, "STX"), //0x8E
                () => Axs(AddressingModes.Absolute), //0x8F
                () => TryBranch(Flags.Carry, false, "BCC"), //0x90
                () => StoreRegister(A, AddressingModes.IndirectY, "STA"), //0x91
                Halt, //0x92
                Halt, //0x93
                () => StoreRegister(Y, AddressingModes.ZeroPageX, "STY"), //0x94
                () => StoreRegister(A, AddressingModes.ZeroPageX, "STA"), //0x95
                () => StoreRegister(X, AddressingModes.ZeroPageY, "STX"), //0x96
                () => Axs(AddressingModes.ZeroPageY), //0x97
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
                () => Lax(AddressingModes.IndirectX), //0xA3
                () => LoadRegister(ref Y, AddressingModes.ZeroPage, "LDY"), //0xA4
                () => LoadRegister(ref A, AddressingModes.ZeroPage, "LDA"), //0xA5
                () => LoadRegister(ref X, AddressingModes.ZeroPage, "LDX"), //0xA6
                () => Lax(AddressingModes.ZeroPage), //0xA7
                () => TransferRegister(A, ref Y, "TAY"), //0xA8
                () => LoadRegister(ref A, AddressingModes.Immediate, "LDA"), //0xA9
                () => TransferRegister(A, ref X, "TAX"), //0xAA
                Halt, //0xAB
                () => LoadRegister(ref Y, AddressingModes.Absolute, "LDY"), //0xAC
                () => LoadRegister(ref A, AddressingModes.Absolute, "LDA"), //0xAD
                () => LoadRegister(ref X, AddressingModes.Absolute, "LDX"), //0xAE
                () => Lax(AddressingModes.Absolute), //0xAF
                () => TryBranch(Flags.Carry, true, "BCS"), //0xB0
                () => LoadRegister(ref A, AddressingModes.IndirectY, "LDA"), //0xB1
                Halt, //0xB2
                () => Lax(AddressingModes.IndirectY), //0xB3
                () => LoadRegister(ref Y, AddressingModes.ZeroPageX, "LDY"), //0xB4
                () => LoadRegister(ref A, AddressingModes.ZeroPageX, "LDA"), //0xB5
                () => LoadRegister(ref X, AddressingModes.ZeroPageY, "LDX"), //0xB6
                () => Lax(AddressingModes.ZeroPageY), //0xB7
                Clv, //0xB8
                () => LoadRegister(ref A, AddressingModes.AbsoluteY, "LDA"), //0xB9
                Tsx, //0xBA
                Halt, //0xBB
                () => LoadRegister(ref Y, AddressingModes.AbsoluteX, "LDY"), //0xBC
                () => LoadRegister(ref A, AddressingModes.AbsoluteX, "LDA"), //0xBD
                () => LoadRegister(ref X, AddressingModes.AbsoluteY, "LDX"), //0xBE
                () => Lax(AddressingModes.AbsoluteY), //0xBF
                () => Cmp(Y, AddressingModes.Immediate, "CPY"), //0xC0
                () => Cmp(A, AddressingModes.IndirectX, "CMP"), //0xC1
                Nop, //0xC2
                () => Dcm(AddressingModes.IndirectX), //0xC3
                () => Cmp(Y, AddressingModes.ZeroPage, "CPY"), //0xC4
                () => Cmp(A, AddressingModes.ZeroPage, "CMP"), //0xC5
                () => DeltaMemory(AddressingModes.ZeroPage, -1, "DEC"), //0xC6
                () => Dcm(AddressingModes.ZeroPage), //0xC7
                () => DeltaRegister(ref Y, 1, "INY"), //0xC8
                () => Cmp(A, AddressingModes.Immediate, "CMP"), //0xC9
                () => DeltaRegister(ref X, -1, "DEX"), //0xCA DEX
                Halt, //0xCB
                () => Cmp(Y, AddressingModes.Absolute, "CPY"), //0xCC
                () => Cmp(A, AddressingModes.Absolute, "CMP"), //0xCD
                () => DeltaMemory(AddressingModes.Absolute, -1, "DEC"), //0xCE
                () => Dcm(AddressingModes.Absolute), //0xCF
                () => TryBranch(Flags.Zero, false, "BNE"), //0xD0
                () => Cmp(A, AddressingModes.IndirectY, "CMP"), //0xD1
                Halt, //0xD2
                () => Dcm(AddressingModes.IndirectY), //0xD3
                Nop, //0xD4
                () => Cmp(A, AddressingModes.ZeroPageX, "CMP"), //0xD5
                () => DeltaMemory(AddressingModes.ZeroPageX, -1, "DEC"), //0xD6
                () => Dcm(AddressingModes.ZeroPageX), //0xD7
                Cld, //0xD8
                () => Cmp(A, AddressingModes.AbsoluteY, "CMP"), //0xD9
                Nop, //0xDA
                () => Dcm(AddressingModes.AbsoluteY), //0xDB
                Nop, //0xDC
                () => Cmp(A, AddressingModes.AbsoluteX, "CMP"), //0xDD
                () => DeltaMemory(AddressingModes.AbsoluteX, -1, "DEC"), //0xDE
                () => Dcm(AddressingModes.AbsoluteX), //0xDF
                () => Cmp(X, AddressingModes.Immediate, "CPX"), //0xE0
                () => Sbc(AddressingModes.IndirectX), //0xE1
                Nop, //0xE2
                () => Ins(AddressingModes.IndirectX), //0xE3
                () => Cmp(X, AddressingModes.ZeroPage, "CPX"), //0xE4
                () => Sbc(AddressingModes.ZeroPage), //0xE5
                () => DeltaMemory(AddressingModes.ZeroPage, 1, "INC"), //0xE6
                () => Ins(AddressingModes.ZeroPage), //0xE7
                () => DeltaRegister(ref X, 1, "INX"), //0xE8
                () => Sbc(AddressingModes.Immediate), //0xE9
                Nop, //0xEA
                () => Sbc(AddressingModes.Immediate, true), //0xEB
                () => Cmp(X, AddressingModes.Absolute, "CPX"), //0xEC
                () => Sbc(AddressingModes.Absolute), //0xED
                () => DeltaMemory(AddressingModes.Absolute, 1, "INC"), //0xEE
                () => Ins(AddressingModes.Absolute), //0xEF
                () => TryBranch(Flags.Zero, true, "BEQ"), //0xF0
                () => Sbc(AddressingModes.IndirectY), //0xF1
                Halt, //0xF2
                () => Ins(AddressingModes.IndirectY), //0xF3
                Nop, //0xF4
                () => Sbc(AddressingModes.ZeroPageX), //0xF5
                () => DeltaMemory(AddressingModes.ZeroPageX, 1, "INC"), //0xF6
                () => Ins(AddressingModes.ZeroPageX), //0xF7
                Sed, //0xF8
                () => Sbc(AddressingModes.AbsoluteY), //0xF9
                Nop, //0xFA
                () => Ins(AddressingModes.AbsoluteY), //0xFB
                Nop, //0xFC
                () => Sbc(AddressingModes.AbsoluteX), //0xFD
                () => DeltaMemory(AddressingModes.AbsoluteX, 1, "INC"), //0xFE
                () => Ins(AddressingModes.AbsoluteX), //0xFF
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
            currentOpcode = Bus.Byte(PC);
            opcodes[currentOpcode].Invoke();
            PC += OpcodeMetadata.Size[currentOpcode];
            cyclesThisSec += OpcodeMetadata.Timings[currentOpcode];
            return cyclesThisSec - cyclesBefore;
        }

        /// <summary>
        /// Invalid Opcode, logs the error to file
        /// </summary>
        private void Invalid()
        {
            Log.Error($"Unkown OPcode: {Bus.Byte(PC):X2}");
        }

        private void Break()
        {
            LogInstruction(0, "BRK");
            Bit.Set(ref P, Flags.Break);
            Bit.Set(ref P, Flags.IRQ);
        }

        /// <summary>
        /// Executes the ORA instruction (OR with Acumulator)
        /// </summary>
        void Ora(AddressingModes mode)
        {
            var value = GetAddressingModeParameter(mode);
            LogInstruction(mode, "ORA");
            
            A = (byte)(A | value);

            Bit.Val(ref P, Flags.Zero, A == 0);
            Bit.Val(ref P, Flags.Negative, Bit.Test(A, Flags.Negative));
        }

        private byte Asl(AddressingModes mode)
        {
            var value = GetAddressingModeParameter(mode);
            
            LogInstruction(mode, "ASL");
            
            Bit.Val(ref P, Flags.Carry, Bit.Test(value, Flags.Negative));

            var shifted = (byte)(value << 1);
            Bit.Val(ref P, Flags.Negative, Bit.Test(shifted, Flags.Negative));
            Bit.Val(ref P, Flags.Zero, shifted == 0);
            
            return shifted;
        }

        void Halt()
        {
            LogInstruction(0, "KILL");
            Stop();
        }

        private void Adc(AddressingModes mode)
        {
            var value = GetAddressingModeParameter(mode);
            LogInstruction(mode, "ADC");

            AdcInternal(value);
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

        void Sbc(AddressingModes mode, bool invalid = false)
        {
            var value = GetAddressingModeParameter(mode);
            LogInstruction(mode, "SBC", invalid);

            AdcInternal((byte) ~value);
        }

        /// <summary>
        /// ANDs the given value with the accumulator 
        /// </summary>
        private void And(AddressingModes mode)
        {
            var value = GetAddressingModeParameter(mode);
            LogInstruction(mode, "AND");
            
            A &= value;
            Bit.Val(ref P, Flags.Zero, A == 0);
            Bit.Val(ref P, Flags.Negative, Bit.Test(A, Flags.Negative));
        }

        private void BIT(byte value)
        {
            var tmp = (byte)(A & value);
            Bit.Val(ref P, Flags.Zero, tmp == 0);
            Bit.Val(ref P, Flags.Overflow, Bit.Test(value, Flags.Overflow));
            Bit.Val(ref P, Flags.Negative, Bit.Test(value, Flags.Negative));
        }

        private void BitZPage()
        {
            var paramAddr = ZPage(Bus.Byte(PC + 1));
            var value = ZPageParam();
            LogInstruction(1, $"BIT ${paramAddr:X2} = {value:X2}");
            BIT(value);
        }

        private void BitAbsolute()
        {
            var paramAddr = Absolute(Bus.Word(PC + 1));
            var value = Bus.Byte(paramAddr);
            LogInstruction(2, $"BIT ${paramAddr:X4} = {value:X2}");
            BIT(value);
        }

        private void TryBranch(Flags flag, bool reqFlagValue, string mnemonic)
        {
            var value = (sbyte)Bus.Byte(PC + 1);
            
            LogInstruction(1, $"{mnemonic} ${(PC + value + 2):X4}");

            if (Bit.Test(P, flag) == reqFlagValue)
            {
                CheckPageCrossed((ushort) (PC + OpcodeMetadata.Timings[currentOpcode] + value), 
                    (ushort)(PC + OpcodeMetadata.Timings[currentOpcode]));
                PC = (ushort)(PC + value);
                cyclesThisSec++;
            }
        }

        private void Cmp(byte register, AddressingModes mode, string mnemonic)
        {
            var value = GetAddressingModeParameter(mode);
            LogInstruction(mode, mnemonic);

            var tmp = (byte)(register - value);

            Bit.Val(ref P, Flags.Carry, register >= value);

            //Need to do this since there are some positive numbers that should trigger this flag. i.e. 0x80
            Bit.Val(ref P, Flags.Negative, Bit.Test(tmp, Flags.Negative));
            Bit.Val(ref P, Flags.Zero, register == value);
        }

        void DeltaMemory(AddressingModes mode, int delta, string mnemonic)
        {
            var addr = GetAddressingModeAddress(mode);
            var value = Bus.Byte(addr);
            LogInstruction(mode, mnemonic);
            value += (byte)delta;
            
            Bus.WriteByte(addr, value);

            Bit.Val(ref P, Flags.Zero, value == 0);
            Bit.Val(ref P, Flags.Negative, Bit.Test(value, Flags.Negative));
        }

        void Eor(AddressingModes mode)
        {
            var value = GetAddressingModeParameter(mode);
            
            LogInstruction(mode, "EOR");
            EorInternal(value);
        }

        void EorInternal(byte value)
        {
            A ^= value;

            Bit.Val(ref P, Flags.Negative, Bit.Test(A, Flags.Negative));
            Bit.Val(ref P, Flags.Zero, A == 0);
        }

        void SetFlagValue(Flags flag, bool isSet, string mnemonic)
        {
            LogInstruction(0, mnemonic);
            Bit.Val(ref P, flag, isSet);
        }

        void Clc() => SetFlagValue(Flags.Carry, false, "CLC");
        void Sec() => SetFlagValue(Flags.Carry, true, "SEC");
        void Cli() => SetFlagValue(Flags.IRQ, false, "CLI");
        void Sei() => SetFlagValue(Flags.IRQ, true, "SEI");
        void Clv() => SetFlagValue(Flags.Overflow, false, "CLV");
        void Cld() => SetFlagValue(Flags.Decimal, false, "CLD");
        void Sed() => SetFlagValue(Flags.Decimal, true, "SED");
        
        void Jmp(ushort addr)
        {
            PC = addr;
        }

        void JmpAbsolute()
        {
            var addr = Bus.Word(PC + 1);
            LogInstruction(2, $"JMP ${addr:X4}");
            Jmp(addr);
        }

        void JmpIndirect()
        {
            var addr = IndirectParam();
            var opcodeParam = Bus.Word(PC + 1);
            LogInstruction(2, $"JMP (${opcodeParam:X4}) = {addr:X4}");
            Jmp(addr);
        }

        void Jsr()
        {
            var cachedPc = (ushort) (PC + 0x02);
            var addr = Absolute(Bus.Word(PC + 1));
            LogInstruction(2, $"JSR ${addr:X2}");
            
            PushWord(cachedPc); // Stores the address of the next opcode minus one
            PC = addr;
        }

        void LoadRegister(ref byte register, AddressingModes mode, string mnemonic)
        {
            LogInstruction(mode, mnemonic);
            var value = GetAddressingModeParameter(mode);

            register = value;

            Bit.Val(ref P, Flags.Zero, register == 0);
            Bit.Val(ref P, Flags.Negative, Bit.Test(register, Flags.Negative));
        }

        byte Lsr(AddressingModes mode)
        {
            //Override pcIncrease and cycle value because LSR has diferent timings
            var value = GetAddressingModeParameter(mode);
            LogInstruction(mode, "LSR");
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

        void Nop()
        {
            switch (Bus.Byte(PC)){
                case 0xEA:
                    LogInstruction(0,"NOP");
                    break;
                case 0x1A: case 0x3A: case 0x5A: case 0x7A: case 0xDA: case 0xFA:
                    LogInstruction(0,"NOP", true);
                    break;

                case 0x80: case 0x82: case 0x89: case 0xC2: case 0xE2:
                    LogInstruction(AddressingModes.Immediate, "NOP", true);
                    break;
                case 0x0C:
                    LogInstruction(AddressingModes.Absolute, "NOP", true);
                    break;
                case 0x1C: case 0x3C: case 0x5C: case 0x7C: case 0xDC: case 0xFC:
                    LogInstruction(AddressingModes.AbsoluteX, "NOP", true);
                    AbsoluteXParam(true);
                    break;
                case 0x04: case 0x44: case 0x64:
                    LogInstruction(AddressingModes.ZeroPage, "NOP", true);
                    break;
                case 0x14: case 0x34: case 0x54:  case 0x74: case 0xD4: case 0xF4:
                    LogInstruction(AddressingModes.ZeroPageX, "NOP", true);
                    break;
            }
        }

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
        }

        void DeltaRegister(ref byte register, int delta, string mnemonic)
        {
            LogInstruction(0, mnemonic);
            register += (byte)delta;
            Bit.Val(ref P, Flags.Zero, register == 0);
            Bit.Val(ref P, Flags.Negative, Bit.Test(register, Flags.Negative));
        }

        byte Rol(AddressingModes mode)
        {
            var value = GetAddressingModeParameter(mode);
            
            LogInstruction(mode, "ROL");
            return Rotate(value, RotateDirection.Left);
        }

        byte Ror(AddressingModes mode)
        {
            var value = GetAddressingModeParameter(mode);
            LogInstruction(mode, "ROR");
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

        void Rti()
        {
            LogInstruction(0, "RTI");
            P = PopByte();
            Bit.Set(ref P, Flags.Unused);//It has to be one. Always
            PC = PopWord(); //Unlike RTS. RTI pulls the correct PC address. No need to increment
        }

        void Rts()
        {
            LogInstruction(0, "RTS");
            PC = PopWord();
        }

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
            var addr = GetAddressingModeAddress(mode);
            LogInstruction(mode, mnemonic);
            Bus.WriteByte(addr, value);
        }

        void Txs() => TransferRegister(X, ref SP, "TXS", false);
        void Tsx() => TransferRegister(SP, ref X, "TSX");

        void Pha()
        {
            LogInstruction(0, "PHA");
            PushByte(A);
        }

        void Pla()
        {
            LogInstruction(0, "PLA");
            A = PopByte();
            Bit.Val(ref P, Flags.Zero, A == 0);
            Bit.Val(ref P, Flags.Negative, Bit.Test(A, Flags.Negative));
        }

        void Php()
        {
            LogInstruction(0, "PHP");
            PushByte((byte) (P | 0x10));
        }

        void Plp()
        {
            LogInstruction(0, "PLP");
            P = PopByte();

            //Bit 5 of P is unused, so clear it. It should always be 1.
            Bit.Set(ref P, 5);
            Bit.Clear(ref P, Flags.Break);
        }

        void Aso(AddressingModes mode)
        {
            var addr = GetAddressingModeAddress(mode);
            LogInstruction(mode, "SLO", true);

            //ASL
            var value = Bus.Byte(addr);
            Bit.Val(ref P, Flags.Carry, Bit.Test(value, Flags.Negative));

            var shifted = (byte) (value << 1);
            Bus.WriteByte(addr, shifted);

            //Now the ORA
            A = (byte) (A | shifted);
            //Set the flags
            Bit.Val(ref P, Flags.Zero, A == 0x00);
            Bit.Val(ref P, Flags.Negative, Bit.Test(A, Flags.Negative));
        }

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
        
        void Rla(AddressingModes mode)
        {
            var addr = GetAddressingModeAddress(mode);
            LogInstruction(mode, "RLA", true);

            var value = Bus.Byte(addr);

            var cachedFlagC = Bit.Test(P, Flags.Carry);
            var cached7 = Bit.Test(value, Flags.Negative);

            Bit.Val(ref P, Flags.Carry, cached7);
            var shifted = (byte) (value << 1);

            Bit.Val(ref shifted, 0, cachedFlagC);
            Bus.WriteByte(addr, shifted);

            A &= shifted;
            Bit.Val(ref P, Flags.Zero, A == 0);
            Bit.Val(ref P, Flags.Negative, Bit.Test(A, Flags.Negative));
        }

        void Lse(AddressingModes mode)
        {
            var addr = GetAddressingModeAddress(mode);
            LogInstruction(mode, "SRE", true);

            var data = Bus.Byte(addr);
            data = LsrInternal(data);
            Bus.WriteByte(addr, data);

            EorInternal(data);
        }

        /// <summary>
        /// ALR This opcode ANDs the contents of the A register with an immediate value and 
        /// then LSRs the result.
        /// </summary>
        void Alr()
        {
            var value = Bus.Byte(PC + 1);
            LogInstruction(1, $"ALR #{value:X2}");
            value &= A;
            Bit.Val(ref P, Flags.Zero, A == 0);
            Bit.Val(ref P, Flags.Negative, Bit.Test(A, Flags.Negative));

            LsrInternal(value);
        }

        void Rra(AddressingModes mode)
        {
            var addr = GetAddressingModeAddress(mode);
            LogInstruction(mode, "RRA", true);

            var value = Bus.Byte(addr);
            value =  Rotate(value, RotateDirection.Right);
            Bus.WriteByte(addr, value);
            
            AdcInternal(value);
        }

        void Arr()
        {
            var value = Bus.Byte(PC + 1);
            LogInstruction(1, $"AAR #{value:X2}");
            
            value &= A;
            Bit.Val(ref P, Flags.Zero, A == 0);
            Bit.Val(ref P, Flags.Negative, Bit.Test(A, Flags.Negative));

            Rotate(value, RotateDirection.Right);
        }

        void Axs(AddressingModes mode)
        {
            var addr = GetAddressingModeAddress(mode);
            LogInstruction(mode, "SAX", true);

            var value = (byte) (A & X);
            Bus.WriteByte(addr, value);
        }
        void Lax(AddressingModes mode)
        {
            LogInstruction(mode, "LAX", true);
            var value = GetAddressingModeParameter(mode);
            X = value;
            A = value;

            Bit.Val(ref P, Flags.Zero, value == 0);
            Bit.Val(ref P, Flags.Negative, Bit.Test(value, Flags.Negative));
        }

        void Dcm(AddressingModes mode)
        {
            var addr = GetAddressingModeAddress(mode);
            LogInstruction(mode, "DCP", true);

            var value = Bus.Byte(addr);
            value--;
            Bus.WriteByte(addr, value);
            
            var temp_result = (byte) (A - value);

            Bit.Val(ref P, Flags.Carry, A >= value);

            //Need to do this since there are some positive numbers that should trigger this flag. i.e. 0x80
            Bit.Val(ref P, Flags.Negative, Bit.Test(temp_result, Flags.Negative));
            Bit.Val(ref P, Flags.Zero, A == value);
        }

        void Ins(AddressingModes mode)
        {
            var addr = GetAddressingModeAddress(mode);
            LogInstruction(mode, "ISB", true);

            var value = Bus.Byte(addr);
            value++;
            Bus.WriteByte(addr, value);
            AdcInternal((byte)~value);
        }
        
        
        /// <summary>
        /// Push a byte on top of the stack
        /// </summary>
        /// <param name="value"></param>
        public void PushByte(byte value)
        {
            var bankPointer = (ushort)(SP + 0x100);
            Bus.WriteByte(bankPointer, value);
            SP -= 1;
        }

        /// <summary>
        /// Push a word on top of the stack, internally the word is flipped to reflect
        /// the correct endian-ess
        /// </summary>
        /// <param name="value"></param>
        public void PushWord(ushort value)
        {
            var bankPointer = (ushort)(SP + 0xFF); // 100 - 1 but in hex -> 99 is 0xFF;
            Bus.WriteWord(bankPointer, value);
            SP -= 2;
        }

        /// <summary>
        /// Pulls the top-most byte from the stack
        /// </summary>
        /// <returns></returns>
        public byte PopByte()
        {
            var value = Bus.Byte((ushort)(SP + 1 + 0x100));
            SP += 1;
            return value;
        }

        /// <summary>
        /// Pops the 2 top-most bytes from the stack and returns them as a word
        /// </summary>
        /// <returns></returns>

        public ushort PopWord()
        {
            var value = Bus.Word((ushort)(SP + 1 + 0x100));
            SP += 2;
            return value;
        }

        /// <summary>
        /// Mockup method, simply returns what its given. It's here for consistency
        /// </summary>
        public ushort Absolute(ushort addr)
        {
            return addr;
        }

        /// <summary>
        /// Absolute X
        /// Returns whatever is entered as a parameter plus register X
        /// </summary>
        public ushort AbsoluteX(ushort addr, bool checkIfPageCrossed = false)
        {
            if (checkIfPageCrossed)
            {
                if ((addr & 0xff00) != ((addr + X) & 0xff00)) {
                    cyclesThisSec++;
                }
            }
            return (ushort)(addr + X);
        }

        /// <summary>
        /// Absolute Y
        /// Returns whatever is entered as a parameter plus register Y
        /// </summary>
        public ushort AbsoluteY(ushort addr)
        {
            return (ushort)(addr + Y);
        }

        public ushort ZPage(ushort addr)
        {
            return (ushort)(addr & 0x00FF);
        }

        /// <summary>
        /// Zero Page X
        /// Gets the content of the parameter, adds X to it and that points
        /// to an address in the zero page where the parameter can be found
        /// </summary>
        public ushort ZPageX(byte addr)
        {
            return (ushort)((addr + X) & 0x00FF);
        }

        /// <summary>
        /// Zero Page Y
        /// Gets the content of the parameter, adds X to it and that points
        /// to an address in the zero page where the parameter can be found
        /// </summary>
        public ushort ZPageY(byte addr)
        {
            return (ushort)((addr + Y) & 0x00FF);
        }

        /// <summary>
        /// Basically the way it works, it gets the value of the parameter, adds the register X to it. That is an
        /// address from which we get one byte, shift it left by 8 bits, read the next position and add t hat to the
        /// shifted value. THAT is the indirectX address of it
        /// </summary>
        /// <param name="addr"></param>
        /// <returns></returns>
        public ushort IndirectX(byte addr)
        {
            addr = (byte)ZPageX(addr);
            //Read a byte from addr and a byte from addr+1. But in both cases wraparound so hence the 0xFF mask
            return (ushort)(Bus.Byte(addr & 0xFF) | Bus.Byte((addr + 1) & 0xFF) << 8);
        }

        public ushort IndirectY(byte addr, bool checkPageCrossed = false)
        {
            var result =(ushort)(Bus.Byte(addr & 0xFF) | Bus.Byte((addr + 1) & 0xFF) << 8);
            
            if (checkPageCrossed)
            {
                if ((result & 0xff00) != ((result + Y) & 0xff00)) {
                    cyclesThisSec++;
                }
                
            }
            return (ushort)(result + Y);
        }

        public byte ZPageXParam() => Bus.Byte(ZPageX(Bus.Byte(PC + 1)));
        public byte ZPageYParam() => Bus.Byte(ZPageY(Bus.Byte(PC + 1)));
        public byte ZPageParam() => Bus.Byte(Bus.Byte(PC + 1) & 0x00FF);
        public byte AbsoluteParam() => Bus.Byte(Absolute(Bus.Word(PC + 1)));

        public byte AbsoluteXParam(bool checkPageCrossed = false)
        {
            var absX = AbsoluteX(Bus.Word(PC + 1), checkPageCrossed);
            var result = Bus.Byte(absX);
            return result;
        }

        public byte AbsoluteYParam(bool checkPageCrossed = false)
        {
            var parameter = Bus.Word(PC + 1);
            var absY = AbsoluteY(parameter);
            var result = Bus.Byte(absY);

            if (checkPageCrossed)
            {
                CheckPageCrossed(parameter, absY);
            }

            return result;
        }

        public byte IndirectXParam() => Bus.Byte(IndirectX(Bus.Byte(PC + 1)));

        public byte IndirectYParam(bool checkPageCrossed = false)
        {
            var parameter = Bus.Byte(PC + 1);
            var indY = IndirectY(parameter, checkPageCrossed);
            var result = Bus.Byte(indY);

            return result;
        }

        public ushort IndirectParam()
        {
            var addr = Bus.Word(PC + 1);
            ushort targetAddr = 0x0000;
            // This is a 6502 bug when instead of reading from $C0FF/$C100 it reads from $C0FF/$C000
            if ((addr & 0xFF) == 0xFF) {
                // Buggy code
                targetAddr = (ushort) ((Bus.Byte(addr & 0xFF00) << 8) + Bus.Byte(addr));
            } else {
                // Normal code
                targetAddr = Bus.Word(addr);
            }

            return targetAddr;
        }


        public void CheckPageCrossed(ushort addr1, ushort addr2)
        {
            if ((addr1 & 0xFF00) != (addr2 & 0xFF00))
            {
                cyclesThisSec++;
            }
        }

        private byte GetAddressingModeParameter(AddressingModes addressingModes)
        {
            switch (addressingModes)
            {
                case AddressingModes.Accumulator:
                    return A;
                case AddressingModes.Immediate:
                    return Bus.Byte(PC + 1);
                case AddressingModes.ZeroPage:
                    return ZPageParam();
                case AddressingModes.ZeroPageX:
                    return ZPageXParam();
                case AddressingModes.ZeroPageY:
                    return ZPageYParam();
                case AddressingModes.Absolute:
                    return AbsoluteParam();
                case AddressingModes.AbsoluteX:
                    return AbsoluteXParam(true);
                case AddressingModes.AbsoluteY:
                    return AbsoluteYParam(true);
                case AddressingModes.IndirectX:
                    return IndirectXParam();
                case AddressingModes.IndirectY:
                    return IndirectYParam(true);
                default:
                    LogInstruction(0, $"Invalid addressing mode ({nameof(addressingModes)})");
                    return 0x00;
            }
        }

        private ushort GetAddressingModeAddress(AddressingModes mode)
        {
            switch (mode)
            {
                case AddressingModes.ZeroPage:
                    return ZPage(Bus.Byte(PC + 1));
                case AddressingModes.ZeroPageX:
                    return ZPageX(Bus.Byte(PC + 1));
                case AddressingModes.ZeroPageY:
                    return ZPageY(Bus.Byte(PC + 1));
                case AddressingModes.Absolute:
                    return Bus.Word(PC + 1);
                case AddressingModes.AbsoluteX:
                    return AbsoluteX(Bus.Word(PC + 1));
                case AddressingModes.Indirect:
                    return IndirectParam();
                case AddressingModes.IndirectX:
                    return IndirectX(Bus.Byte(PC + 1));
                case AddressingModes.IndirectY:
                    return IndirectY(Bus.Byte(PC + 1));
                case AddressingModes.AbsoluteY:
                    return AbsoluteY(Bus.Word(PC + 1));
                default:
                    LogInstruction(0, $"Invalid addressing mode ({nameof(mode)})");
                    return 0x00;
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
                    instruction.AppendFormat("#${0:X2}", Bus.Byte(PC + 1));
                    break;
                case AddressingModes.ZeroPage:
                    numParams = 1;
                    instruction.AppendFormat("${0:X2} = {1:X2}",Bus.Byte(PC + 1), ZPageParam());
                    break;
                case AddressingModes.ZeroPageX:
                    numParams = 1;
                    instruction.AppendFormat("${0:X2},X @ {1:X2} = {2:X2}",Bus.Byte(PC + 1), ZPageX(Bus.Byte(PC + 1)), ZPageXParam());
                    break;
                case AddressingModes.ZeroPageY:
                    numParams = 1;
                    instruction.AppendFormat("${0:X2},Y @ {1:X2} = {2:X2}",Bus.Byte(PC + 1),ZPageY(Bus.Byte(PC + 1)), ZPageYParam());
                    break;
                case AddressingModes.IndirectX:
                    numParams = 1;
                    var indXParam = Bus.Byte(PC + 1);
                    var indXVal = (indXParam + X) & 0xFF;
                    instruction.AppendFormat("(${0:X2},X) @ {1:X2} = {2:X4} = {3:X2}", 
                        indXParam, indXVal, IndirectX(indXParam), IndirectXParam());
                    break;
                case AddressingModes.IndirectY:
                    numParams = 1;
                    var opcodeParam = Bus.Byte(PC + 1);
                    var initialAddr = Bus.Byte(opcodeParam & 0xFF) | Bus.Byte((opcodeParam + 1) & 0xFF) << 8;
                    instruction.AppendFormat("(${0:X2}),Y = {1:X4} @ {2:X4} = {3:X2}", 
                        opcodeParam, initialAddr, IndirectY(opcodeParam), IndirectYParam());
                    break;
                case AddressingModes.Absolute:
                    numParams = 2;
                    instruction.AppendFormat("${0:X4} = {1:X2}", Bus.Word(PC + 1), AbsoluteParam());
                    break;
                case AddressingModes.AbsoluteX: 
                    numParams = 2;
                    instruction.AppendFormat("${0:X4},X @ {1:X4} = {2:X2}", Bus.Word(PC + 1), AbsoluteX(Bus.Word(PC + 1)), AbsoluteXParam());
                    break;
                case AddressingModes.AbsoluteY:
                    numParams = 2;
                    var absYparam = Bus.Word(PC + 1);
                    var absYInitialAddr = (absYparam + Y) & 0xFFFF;
                    instruction.AppendFormat("${0:X4},Y @ {1:X4} = {2:X2}", Bus.Word(PC + 1),absYInitialAddr, AbsoluteYParam());
                    break;
                default:
                    Invalid();
                    break;
            }
            
            LogInstruction(numParams, instruction.ToString(), invalid);
        }
        
        private void LogInstruction(int numParams, string mnemonic, bool invalid = false)
        {
            var sb = new StringBuilder();
            sb.Append($"{PC:X4}  {currentOpcode:X2} ");

            for (var i = 1; i <= numParams; i++) {
                sb.Append($"{Bus.Byte(PC + i):X2} ");
            }

            //The mnemonic should start at position 16 or at 15 if invalid. 
            var padding = Math.Max((invalid ? 15 : 16) - sb.Length, 0);
            sb.Append(string.Empty.PadRight(padding));
            if (invalid)
            {
                sb.Append("*");
            }
            sb.Append(mnemonic);

            padding = Math.Max(48 - sb.Length, 0);
            sb.Append(string.Empty.PadRight(padding));
            sb.Append($"A:{A:X2} X:{X:X2} Y:{Y:X2} P:{P:X2} SP:{SP:X2}");

            sb.Append(" PPU:").Append(Bus.Ppu.CyclesThisFrame.ToString().PadLeft(3));
            sb.Append(",").Append(Bus.Ppu.FrameCount.ToString().PadLeft(3)).Append($" CYC:{cyclesThisSec}");

            Log.Information(sb.ToString());
        }
    }

}
