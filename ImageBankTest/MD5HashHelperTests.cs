using System;
using ImageBank;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ImageBankTest
{
    [TestClass()]
    public class Md5HashHelperTests
    {
        [TestMethod("Compute")]
        public void ComputeTest()
        {
            var hnull = Md5HashHelper.Compute(null);
            Assert.IsNull(hnull);

            var hempty = Md5HashHelper.Compute(Array.Empty<byte>());
            Assert.IsNotNull(hempty);
            Assert.AreEqual(hempty.Length, 32);
            Assert.AreEqual(hempty, "d41d8cd98f00b204e9800998ecf8427e");

            var a1 = new byte[] { 0x00, 0x01, 0x02, 0x03 };
            var h1a = Md5HashHelper.Compute(a1);
            Assert.AreEqual(h1a.Length, 32);
            Assert.AreEqual(h1a, "37b59afd592725f9305e484a5d7f5168");

            var a2 = new byte[] { 0x00, 0x01, 0x02, 0x04 };
            var h2a = Md5HashHelper.Compute(a2);
            Assert.AreEqual(h2a.Length, 32);
            Assert.AreEqual(h2a, "46c85b65e1a8f4b44d071953c05411c1");
            Assert.IsFalse(h2a.Equals(h1a, StringComparison.OrdinalIgnoreCase));
        }
    }
}