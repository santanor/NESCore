using NESCore;
using NUnit.Framework;

namespace Tests
{
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
            nes.Ram.WriteByte(0, 0x23);
            Assert.AreEqual(0x23, nes.Ram.Byte(0));

            nes.Ram.WriteByte(0x1234, 0x69);
            Assert.AreEqual(0x69, nes.Ram.Byte(0x1234));

            nes.Ram.Zero();
            Assert.Zero(nes.Ram.Byte(0x23));
            Assert.Zero(nes.Ram.Byte(0x69));
        }

        [Test]
        public void TestWordReadWrite()
        {
            nes.Ram.WriteWord(0x00, 0x2369);
            Assert.AreNotEqual(0x6923, nes.Ram.Word(0x00));
            Assert.AreEqual(0x2369, nes.Ram.Word(0x00));
        }

        [Test]
        public void TestStack()
        {
            Assert.Pass();
        }
    }
}
