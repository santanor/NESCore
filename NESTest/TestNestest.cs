using System;
using System.Threading;
using System.Threading.Tasks;
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
        private const int instructionsToRun = 8990;

        [SetUp]
        public void Setup()
        {
            var (success, romResult) = ROM.FromFile("./TestData/nestest/nestest.nes");;
            Assert.True(success);
            Assert.IsNotNull(romResult);

            rom = romResult;

            nes = new NES();
            nes.LoadROM(rom);
            nes.Bus.Cpu.PC = 0xC000;
        }


        [Test]
        public void Nestest()
        {
            for(var i = 0; i <= instructionsToRun; i++)
            {
                nes.Step();
                //A bit over the top to compare it every step, but oh well.....
            }
            
            Assert.Zero(nes.Bus.Byte(0x02));
            Assert.Zero(nes.Bus.Byte(0x03));
        }
    }
}
