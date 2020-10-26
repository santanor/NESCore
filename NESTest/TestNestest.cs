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

        [SetUp]
        public void Setup()
        {
            var (success, romResult) = ROM.FromFile("./TestData/nestest/nestest.nes");;
            Assert.True(success);
            Assert.IsNotNull(romResult);

            rom = romResult;

            nes = new NES();
            nes.LoadROM(rom);
            nes.Cpu.PC = 0xC000;
            
        }


        [Test]
        public async Task Nestest()
        {
            var cancellationToken = new CancellationToken();
            var _ = Task.Run(nes.Run, cancellationToken);
            await Task.Delay(TimeSpan.FromSeconds(3), CancellationToken.None);
            
            nes.Stop();
            
            //Now read the value on byte 002h
            // 000h - tests completed successfully

            var testResult = nes.Ram.Byte(0x01);
            Assert.Zero(testResult);
        }

    }
}
