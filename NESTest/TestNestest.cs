using NESCore;
using NUnit.Framework;

namespace Tests
{
    /// <summary>
    /// Only tests the validity of the instructions, so far it doesn't test how
    /// "cycle accurate" the emulator is
    /// </summary>
    public class TestNestest
    {
        private ROM rom;
        private NES nes;

        [SetUp]
        public void Setup()
        {
            var (success, romResult) = ROM.FromFile("./TestData/nestest.nes");;
            Assert.True(success);
            Assert.IsNotNull(romResult);

            rom = romResult;

            nes = new NES();
            nes.LoadROM(rom);
            nes.Cpu.PC = 0xC000;
        }


        [Test]
        public void Nestest()
        {
            nes.Cpu.Run();
        }

    }
}
