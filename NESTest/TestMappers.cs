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
            Assert.NotZero(nes.Bus.Byte(NROM.FirstRomPage));

            Assert.NotZero(nes.Bus.Byte(NROM.SecondRomPage));

            //Check that the mirror is implemented correctly (The test ROM has a mirror)
            for (ushort i = 0; i < ROM.PrgPageSize; i++)
            {
                var firstPageByte = nes.Bus.Byte((ushort)(NROM.FirstRomPage + i));
                var secondPageByte = nes.Bus.Byte((ushort)(NROM.FirstRomPage + i));
                Assert.AreEqual(firstPageByte, secondPageByte);
            }
        }
    }
}
