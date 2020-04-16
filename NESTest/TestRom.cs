using System.IO;
using System.Linq;
using System.Text;
using NESCore;
using NUnit.Framework;

namespace Tests
{

    /// <summary>
    /// The only way to test that this is actually doing what it's supposed to be doing
    /// is by reading a file that we know how it should look like.
    ///
    /// We'll do Nestest
    /// </summary>
    public class TestRom
    {
        private ROM rom;

        [SetUp]
        public void Setup()
        {
            var (success, romResult) = GetTestROM();
            Assert.True(success);
            Assert.IsNotNull(romResult);

            rom = romResult;
        }

        public static (bool, ROM) GetTestROM()
        {
            return ROM.FromFile("./TestData/nestest/nestest.nes");
        }

        [Test]
        public void Nestest()
        {

            Assert.AreEqual(rom.nesTitle.Length, 4);

            //Only check the ASCII for \n given that the fourth one can be interpreted
            //differently
            Assert.AreEqual(Encoding.ASCII.GetString(rom.nesTitle.Take(3).ToArray()), "NES");

            Assert.AreEqual(rom.numCHRPages, 1);
            Assert.AreEqual(rom.numPRGPages, 1);

            Assert.IsNull(rom.trainer);
            Assert.AreEqual(rom.mapper, 0);

            Assert.AreEqual(rom.prgROM.Length, ROM.PrgPageSize);
            Assert.AreEqual(rom.chrROM.Length, ROM.ChrPageSize);
        }

        [Test]
        public void InvalidFile()
        {
            var (success, romResult) = ROM.FromFile("invalidCrap.nes");
            Assert.IsFalse(success);
            Assert.NotNull(romResult);

            Assert.IsNull(romResult.nesTitle);
        }

    }
}
