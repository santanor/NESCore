using NESCore;
using NESCore.Mappers;
using NUnit.Framework;

namespace Tests
{
    public class TestMappers
    {
        private NES nes;

        [SetUp]
        public void Setup()
        {
            nes = new NES();
            var (success, rom) = TestRom.GetTestROM();
            Assert.True(success);

            nes.LoadROM(rom);
        }

        [Test]
        public void TestNROM()
        {
            Assert.NotZero(nes.Ram.Byte(NROM.FirstRomPage));

            Assert.NotZero(nes.Ram.Byte(NROM.SecondRomPage));

            //Check that the mirror is implemented correctly (The test ROM has a mirror)
            for (ushort i = 0; i < ROM.PrgPageSize; i++)
            {
                var firstPageByte = nes.Ram.Byte((ushort)(NROM.FirstRomPage + i));
                var secondPageByte = nes.Ram.Byte((ushort)(NROM.FirstRomPage + i));
                Assert.AreEqual(firstPageByte, secondPageByte);
            }
        }
    }
}