using NESCore;
using NUnit.Framework;

namespace Tests;

/// <summary>
///     Only tests the validity of the instructions, so far it doesn't test how
///     "cycle accurate" the emulator is
/// </summary>
public class TestNestest
{
    private const int instructionsToRun = 8990;
    private Cartridge c;
    private NES nes;

    [SetUp]
    public void Setup()
    {
        c = Cartridge.FromFile("./TestData/nestest/nestest.nes");
        Assert.IsNotNull(c);

        nes = new NES();
        Bus.Cartridge = c;
        Bus.Cpu.PC = 0xC000;
    }


    [Test]
    public void Nestest()
    {
        for (var i = 0; i <= instructionsToRun; i++) nes.Step();

        Assert.Zero(Bus.Byte(0x02));
        Assert.Zero(Bus.Byte(0x03));
    }
}