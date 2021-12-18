using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace ImageBank.Tests
{
    /*
    [TestClass()]
    public class PHash64Tests
    {
        [TestMethod()]
        public void PHash64Test()
        {
            var p1 = new PHash64();
            p1.SetBit(0);
            p1.SetBit(8);
            p1.SetBit(15);
            p1.SetBit(255);
            var array1 = p1.ToArray();
            Assert.IsNotNull(array1);
            Assert.AreEqual(array1.Length, 32);
            Assert.AreEqual(array1[0], 0x01);
            Assert.AreEqual(array1[1], 0x81);
            Assert.AreEqual(array1[31], 0x80);

            var p2 = new PHash64(array1, 0);
            var array2 = p2.ToArray();
            Assert.IsNotNull(array2);
            Assert.AreEqual(array2.Length, 32);
            Assert.AreEqual(array2[0], 0x01);
            Assert.AreEqual(array2[1], 0x81);
            Assert.AreEqual(array2[31], 0x80);
        }

        [TestMethod()]
        public void HammingDistanceTest()
        {
            var p1 = new PHash64();
            var p2 = new PHash64();
            p2.SetBit(8);
            var d = p1.HammingDistance(p1);
            Assert.AreEqual(d, 0);
            d = p1.HammingDistance(p2);
            Assert.AreEqual(d, 1);
            var buffer = new byte[64];
            for (var i = 32; i < 64; i++) {
                buffer[i] = 0xFF;
            }

            var p3 = new PHash64(buffer, 32);
            d = p2.HammingDistance(p3);
            Assert.AreEqual(d, 255);
            d = p1.HammingDistance(p3);
            Assert.AreEqual(d, 256);
        }
    }
    */
}