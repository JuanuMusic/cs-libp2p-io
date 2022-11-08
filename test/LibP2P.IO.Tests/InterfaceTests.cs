using System;
using System.Security.Cryptography;
using Moq;
using NUnit.Framework;

namespace LibP2P.IO.Tests
{
    public class InterfaceTests
    {
        private static void GenerateRandomBytes(byte[] data)
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(data);
            }
        }

        [Test]
        public void Closer_Test()
        {
            var closed = false;
            var closer = new Mock<ICloser>();
            closer.Setup(c => c.Close()).Callback(() => closed = true);
            closer.Object.Close();

            Assert.True(closed);
        }

        [Test]
        public void Reader_Test()
        {
            var data = new byte[4096];
            var dataOffset = 0;
            GenerateRandomBytes(data);
            var reader = new Mock<IReader>();
            reader.Setup(r => r.Read(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns<byte[], int, int>((buffer, offset, count) =>
                {
                    count = Math.Min(count, data.Length - dataOffset);
                    Buffer.BlockCopy(data, dataOffset, buffer, offset, count);
                    dataOffset += count;
                    return count;
                });

            var buf = new byte[data.Length];
            var total = 0;
            while (total < data.Length)
            {
                total += reader.Object.Read(buf, total, 512);
            }

            Assert.AreEqual(total, data.Length);
            Assert.AreEqual(dataOffset, data.Length);
            Assert.AreEqual(buf, data);
        }

        [Test]
        public void Writer_Test()
        {
            var data = new byte[4096];
            var dataOffset = 0;
            var writer = new Mock<IWriter>();
            writer.Setup(w => w.Write(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns<byte[], int, int>((buffer, offset, count) =>
                {
                    count = Math.Min(count, data.Length - dataOffset);
                    Buffer.BlockCopy(buffer, offset, data, dataOffset, count);
                    dataOffset += count;
                    return count;
                });

            var buf = new byte[data.Length];
            GenerateRandomBytes(buf);
            var total = 0;
            while (total < data.Length)
            {
                total += writer.Object.Write(buf, total, 512);
            }

            Assert.AreEqual(total, data.Length);
            Assert.AreEqual(dataOffset, data.Length);
            Assert.AreEqual(buf, data);
        }
    }
}
