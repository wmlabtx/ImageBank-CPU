using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using ImageBank;

namespace ImageBankTest
{
    [TestClass()]
    public class MetadataExtractorTests
    {
        /*
        [TestMethod()]
        public void ReadMetadata()
        {
            var imagedata = File.ReadAllBytes("000817-01-27.jpg");
            MetadataHelper.GetMetadata(imagedata, out var datetaken, out var metadata);
            Assert.IsTrue(datetaken.HasValue);
            Assert.AreEqual(datetaken.Value.Year, 2008);
            Assert.IsFalse(string.IsNullOrEmpty(metadata));

            imagedata = File.ReadAllBytes("arianna-048-113.jpg");
            MetadataHelper.GetMetadata(imagedata, out datetaken, out metadata);
            Assert.IsFalse(datetaken.HasValue);
            Assert.IsTrue(string.IsNullOrEmpty(metadata));

            imagedata = File.ReadAllBytes("org.jpg");
            MetadataHelper.GetMetadata(imagedata, out datetaken, out metadata);
            Assert.IsFalse(datetaken.HasValue);
            Assert.AreEqual(metadata, string.Empty);

            imagedata = null;
            MetadataHelper.GetMetadata(imagedata, out datetaken, out metadata);
            Assert.IsFalse(datetaken.HasValue);
            Assert.AreEqual(metadata, string.Empty);

            imagedata = new byte[] { 0x00, 0x01, 0x02, 0x03 };
            MetadataHelper.GetMetadata(imagedata, out datetaken, out metadata);
            Assert.IsFalse(datetaken.HasValue);
            Assert.AreEqual(metadata, string.Empty);
        }
        */
    }
}
