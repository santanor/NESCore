using NESCore;
using NESCore.Mappers;
using NUnit.Framework;

namespace Tests;

public class TestMappers
{
    private NES nes;

    [SetUp]
    public void Setup()
    {
        nes = new NES();
        var c = TestRom.GetTestCartridge();
        Assert.IsNotNull(c);

        Bus.Cartridge = c;
    }

    [Test]
    public void TestNROM()
    {
        Assert.NotZero(Bus.Byte(NROM.FirstRomPage));

        Assert.NotZero(Bus.Byte(NROM.SecondRomPage));
    }
}