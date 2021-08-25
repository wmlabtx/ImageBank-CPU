using ImageBank;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;

namespace ImageBankTest
{
    [TestClass()]
    public class HelperTests
    {
        [TestMethod()]
        public void TestHash()
        {
            var a1 = new byte[] { 0x00, 0x01, 0x02, 0x03 };
            var h1 = Helper.ComputeHash(a1);
            Assert.AreEqual(h1.Length, 32);

            var a2 = new byte[] { 0x00, 0x01, 0x02, 0x04 };
            var h2 = Helper.ComputeHash(a2);
            Assert.IsFalse(h1.Equals(h2, StringComparison.OrdinalIgnoreCase));

            var h3 = Helper.ComputeHash(null);
            Assert.IsNull(h3);

            var h4 = Helper.ComputeHash(Array.Empty<byte>());
            Assert.AreEqual(h4.Length, 32);
        }

        [TestMethod()]
        public void TestEncryption()
        {
            var a1 = new byte[] { 0x10, 0x11, 0x12, 0x13 };
            var ea = Helper.Encrypt(a1, "01234567");
            Assert.AreNotEqual(a1[0], ea[0]);
            var a2 = Helper.Decrypt(ea, "01234567");
            Assert.IsTrue(Enumerable.SequenceEqual(a1, a2));
            var a3 = Helper.Decrypt(ea, "01234568");
            Assert.IsNull(a3);
        }

        [TestMethod()]
        public void TestReadWrite()
        {
            const string filename = "test.mzx";
            var a1 = new byte[] { 0x10, 0x11, 0x12, 0x13 };
            Helper.WriteData(filename, a1);
            var a2 = Helper.ReadData(filename);
            File.Delete(filename);
            Assert.IsTrue(Enumerable.SequenceEqual(a1, a2));
        }

        [TestMethod()]
        public void AddDescriptorTest()
        {
            var d1 = Array.Empty<ulong>();
            var d2 = new ulong[4];
            for (var i = 0; i < 4; i++) {
                d2[i] = 0x1;
            }

            var result = Helper.AddDescriptor(d1, d2);
            Assert.IsTrue(result.Length == 4 && result[0] == 0x1);
            var d3 = new ulong[4];
            for (var i = 0; i < 4; i++) {
                d3[i] = 0xFFFFFFFFFFFFFFFF;
            }

            result = Helper.AddDescriptor(d3, d2);
            Assert.IsTrue(result.Length == 8 && result[0] == 0xFFFFFFFFFFFFFFFF && result[4] == 0x1);

            var d4 = Helper.GetDescriptor(result, 4);
            Assert.IsTrue(d4.Length == 4 && d4[0] == 0x1);
        }
    }
}
