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
        private Cartridge cartridge;

        [SetUp]
        public void Setup()
        {
            var c = GetTestCartridge();
            Assert.IsNotNull(c);

            cartridge = c;
        }

        public static Cartridge GetTestCartridge()
        {
            return Cartridge.FromFile("./TestData/nestest/nestest.nes");
        }

        [Test]
        public void Nestest()
        {

            Assert.AreEqual(cartridge.Rom.nesTitle.Length, 4);

            //Only check the ASCII for \n given that the fourth one can be interpreted
            //differently
            Assert.AreEqual(Encoding.ASCII.GetString(cartridge.Rom.nesTitle.Take(3).ToArray()), "NES");

            Assert.AreEqual(cartridge.Rom.numCHRPages, 1);
            Assert.AreEqual(cartridge.Rom.numPRGPages, 1);

            Assert.IsNull(cartridge.Rom.trainer);
            Assert.AreEqual(cartridge.Rom.mapper, 0);

            Assert.AreEqual(cartridge.Rom.prgROM.Length, ROM.PrgPageSize);
            Assert.AreEqual(cartridge.Rom.chrROM.Length, ROM.ChrPageSize);
        }

        [Test]
        public void InvalidFile()
        {
            var c = Cartridge.FromFile("invalidCrap.nes");
            Assert.IsNull(c);
        }

    }
}
