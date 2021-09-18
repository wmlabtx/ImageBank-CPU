using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace ImageBank.Tests
{
    [TestClass()]
    public class EncryptionHelperTests
    {
        [TestMethod()]
        public void TestEncryption()
        {
            var a1 = new byte[] { 0x10, 0x11, 0x12, 0x13 };
            var ea = EncryptionHelper.Encrypt(a1, "01234567");
            Assert.AreNotEqual(a1[0], ea[0]);
            var a2 = EncryptionHelper.Decrypt(ea, "01234567");
            Assert.IsTrue(Enumerable.SequenceEqual(a1, a2));
            var a3 = EncryptionHelper.Decrypt(ea, "01234568");
            Assert.IsNull(a3);
        }
    }
}