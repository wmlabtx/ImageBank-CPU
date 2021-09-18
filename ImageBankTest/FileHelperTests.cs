﻿using ImageBank;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.IO;

namespace ImageBank.Tests
{
    [TestClass()]
    public class FileHelperTests
    {
        [TestMethod("WriteData_ReadData")]
        public void TestReadWrite()
        {
            const string filename = "test.mzx";
            var a1 = new byte[] { 0x10, 0x11, 0x12, 0x13 };
            FileHelper.WriteData(filename, a1);
            var a2 = FileHelper.ReadData(filename);
            File.Delete(filename);
            Assert.IsTrue(Enumerable.SequenceEqual(a1, a2));
        }

        [TestMethod("HashToName")]
        public void GetNameTest()
        {
            var hash = "d41d8cd98f00b204e9800998ecf8427e";
            var name = FileHelper.HashToName(hash, 1);
            Assert.AreEqual(name, "d8cd98f00b");
        }

        [TestMethod("NameToFileName")]
        public void GetFileNameTest()
        {
            var hash = "d41d8cd98f";
            var filename = FileHelper.NameToFileName(hash);
            var filenameexpected = $"{AppConsts.PathHp}\\d4\\1d8cd98f{AppConsts.MzxExtension}";
            Assert.AreEqual(filename, filenameexpected);
        }
    }
}