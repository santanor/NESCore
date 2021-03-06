using NESCore;
using NUnit.Framework;

namespace Tests
{
    public class TestMemory
    {
        private NES nes;

        [SetUp]
        public void Setup()
        {
            nes = new NES();
        }

        [Test]
        public void TestByteReadWrite()
        {
            nes.Bus.WriteByte(0, 0x23);
            Assert.AreEqual(0x23, nes.Bus.Byte(0));

            nes.Bus.WriteByte(0x1234, 0x69);
            Assert.AreEqual(0x69, nes.Bus.Byte(0x1234));

            nes.Bus.Ram.Zero();
            Assert.Zero(nes.Bus.Byte(0x23));
            Assert.Zero(nes.Bus.Byte(0x69));
        }

        [Test]
        public void TestWordReadWrite()
        {
            nes.Bus.WriteWord(0x00, 0x2369);
            Assert.AreNotEqual(0x6923, nes.Bus.Word(0x00));
            Assert.AreEqual(0x2369, nes.Bus.Word(0x00));
        }

        [Test]
        public void TestStack()
        {
            nes.Bus.Cpu.PushByte(0x23);
            Assert.AreEqual(nes.Bus.Cpu.PopByte(), 0x23);
            Assert.AreEqual(nes.Bus.Cpu.PopByte(), 0x00);

            //Checking that the stack writes to the correct address in the memory map
            nes.Bus.Cpu.PushByte(0x23);
            Assert.AreEqual(nes.Bus.Byte(0x01FD), 0x23);
            nes.Bus.Cpu.PopByte();// clean it up


            nes.Bus.Cpu.PushWord(0x2369);
            Assert.AreEqual(nes.Bus.Cpu.PopWord(), 0x2369);

            nes.Bus.Cpu.PushWord(0x2369);
            nes.Bus.Cpu.PushByte(0x42);
            Assert.AreEqual(nes.Bus.Cpu.PopWord(), 0x6942);
            Assert.AreEqual(nes.Bus.Cpu.PopByte(), 0x23);

            Assert.AreEqual(nes.Bus.Cpu.PopWord(), 0x0000);
        }

        [Test]
        public void TestIndirectAddressing()
        {
            ushort addr = 0x00;

            nes.Bus.WriteWord(0x70, 0xAC30);
            nes.Bus.Cpu.X = 0x64;
            addr = nes.Bus.Cpu.IndirectX(0xC);
            Assert.AreEqual( 0xAC30, addr);

            nes.Bus.Ram.Zero();
            nes.Bus.WriteWord(0x7E, 0x2074);
            nes.Bus.Cpu.X = 100;
            addr = nes.Bus.Cpu.IndirectX(0x1A);
            Assert.AreEqual(0x2074, addr);

            nes.Bus.Ram.Zero();
            nes.Bus.WriteWord(0x86, 0x4028);
            nes.Bus.Cpu.Y = 0x10;
            addr = nes.Bus.Cpu.IndirectY(0x86);
            Assert.AreEqual(0x4038, addr);
        }

        [Test]
        public void TestZeroPageAddressing()
        {
            ushort addr = 0;

            // zpagex_addr test
            nes.Bus.Cpu.X = 0x60;
            addr = nes.Bus.Cpu.ZPageX(0xC0);
            Assert.AreEqual(0x0020 ,addr);

            // zpagey_addr test
            nes.Bus.Cpu.Y = 0x10;
            addr = nes.Bus.Cpu.ZPageY(0xFB);
            Assert.AreEqual(0x000B, addr);
        }

        [Test]
        public void TestAbsoluteAddressing()
        {
            //First test. Absolute. I know, silly but necessary
            ushort param = 0x6969;
            var addr = nes.Bus.Cpu.Absolute(param);
            Assert.AreEqual(addr, param);

            //Second test. Absolute X
            nes.Bus.Ram.Zero();
            param = 0x6959;
            nes.Bus.Cpu.X = 0x10;
            addr = nes.Bus.Cpu.AbsoluteX(param);
            Assert.AreEqual(addr, 0x6969);

            //Third test. Absolute Y
            nes.Bus.Ram.Zero();
            param = 0x6949;
            nes.Bus.Cpu.Y = 0x20;
            addr = nes.Bus.Cpu.AbsoluteY(param);
            Assert.AreEqual(addr, 0x6969);
        }
    }
}
