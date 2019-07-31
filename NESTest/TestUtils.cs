using System.IO;
using NESCore;
using NUnit.Framework;

namespace Tests
{
    public class TestUtils
    {

        [Test]
        public void TestBitOperations()
        {
            byte val = 0b11100101;
            Assert.False(Bit.Test(val, 1));
            Assert.False(Bit.Test(val, 3));
            Assert.False(Bit.Test(val, 4));

            Assert.True(Bit.Test(val, 0));
            Assert.True(Bit.Test(val, 2));
            Assert.True(Bit.Test(val, 7));

            Assert.False(Bit.Test(val, 8));
            Assert.False(Bit.Test(val, 10));
        }

        [Test]
        public void TestExtensions()
        {
            const short val = 0x2369;
            var bytes = val.ToBytes();
            Assert.AreEqual(bytes[0], 0x69);
            Assert.AreEqual(bytes[1], 0x23);

            var word = bytes.ToWord();
            Assert.AreEqual(val, word);
            Assert.AreNotEqual(word, 0x6923);

            var streamContent = new byte[] {0x23, 0x69, 0x42, 0x32};
            var stream = new MemoryStream(streamContent);
            Assert.AreEqual(stream.Nextbytes(1)[0], 0x23);
            Assert.AreEqual(stream.Nextbytes(2)[1], 0x42);
            Assert.AreEqual(stream.Nextbytes(10).Length, 1);
        }

    }
}
