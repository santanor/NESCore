using NESCore;
using NUnit.Framework;

namespace Tests;

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
        Bus.WriteByte(0, 0x23);
        Assert.AreEqual(0x23, Bus.Byte(0));

        Bus.WriteByte(0x1234, 0x69);
        Assert.AreEqual(0x69, Bus.Byte(0x1234));

        Bus.Ram.Zero();
        Assert.Zero(Bus.Byte(0x23));
        Assert.Zero(Bus.Byte(0x69));
    }

    [Test]
    public void TestWordReadWrite()
    {
        Bus.WriteWord(0x00, 0x2369);
        Assert.AreNotEqual(0x6923, Bus.Word(0x00));
        Assert.AreEqual(0x2369, Bus.Word(0x00));
    }

    [Test]
    public void TestStack()
    {
        Bus.Cpu.PushByte(0x23);
        Assert.AreEqual(Bus.Cpu.PopByte(), 0x23);
        Assert.AreEqual(Bus.Cpu.PopByte(), 0x00);

        //Checking that the stack writes to the correct address in the memory map
        Bus.Cpu.PushByte(0x23);
        Assert.AreEqual(Bus.Byte(0x01FD), 0x23);
        Bus.Cpu.PopByte(); // clean it up


        Bus.Cpu.PushWord(0x2369);
        Assert.AreEqual(Bus.Cpu.PopWord(), 0x2369);

        Bus.Cpu.PushWord(0x2369);
        Bus.Cpu.PushByte(0x42);
        Assert.AreEqual(Bus.Cpu.PopWord(), 0x6942);
        Assert.AreEqual(Bus.Cpu.PopByte(), 0x23);

        Assert.AreEqual(Bus.Cpu.PopWord(), 0x0000);
    }

    [Test]
    public void TestIndirectAddressing()
    {
        ushort addr = 0x00;

        Bus.WriteWord(0x70, 0xAC30);
        Bus.Cpu.X = 0x64;
        addr = Bus.Cpu.IndirectX(0xC);
        Assert.AreEqual(0xAC30, addr);

        Bus.Ram.Zero();
        Bus.WriteWord(0x7E, 0x2074);
        Bus.Cpu.X = 100;
        addr = Bus.Cpu.IndirectX(0x1A);
        Assert.AreEqual(0x2074, addr);

        Bus.Ram.Zero();
        Bus.WriteWord(0x86, 0x4028);
        Bus.Cpu.Y = 0x10;
        addr = Bus.Cpu.IndirectY(0x86);
        Assert.AreEqual(0x4038, addr);
    }

    [Test]
    public void TestZeroPageAddressing()
    {
        ushort addr = 0;

        // zpagex_addr test
        Bus.Cpu.X = 0x60;
        addr = Bus.Cpu.ZPageX(0xC0);
        Assert.AreEqual(0x0020, addr);

        // zpagey_addr test
        Bus.Cpu.Y = 0x10;
        addr = Bus.Cpu.ZPageY(0xFB);
        Assert.AreEqual(0x000B, addr);
    }

    [Test]
    public void TestAbsoluteAddressing()
    {
        //First test. Absolute. I know, silly but necessary
        ushort param = 0x6969;
        var addr = Bus.Cpu.Absolute(param);
        Assert.AreEqual(addr, param);

        //Second test. Absolute X
        Bus.Ram.Zero();
        param = 0x6959;
        Bus.Cpu.X = 0x10;
        addr = Bus.Cpu.AbsoluteX(param);
        Assert.AreEqual(addr, 0x6969);

        //Third test. Absolute Y
        Bus.Ram.Zero();
        param = 0x6949;
        Bus.Cpu.Y = 0x20;
        addr = Bus.Cpu.AbsoluteY(param);
        Assert.AreEqual(addr, 0x6969);
    }
}