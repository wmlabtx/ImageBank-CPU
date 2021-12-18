using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace ImageBank.Tests
{
    /*
    [TestClass()]
    public class PHashExTests
    {
        [TestMethod()]
        public void PHashExTest()
        {
            var buffer1 = new byte[256];
            for (var i = 0; i < 32; i++) {
                buffer1[i] = 0x01;
            }

            for (var i = 32; i < 64; i++) {
                buffer1[i] = 0x03;
            }

            for (var i = 64; i < 96; i++) {
                buffer1[i] = 0x07;
            }

            for (var i = 96; i < 128; i++) {
                buffer1[i] = 0x0F;
            }

            for (var i = 128; i < 160; i++) {
                buffer1[i] = 0xFF;
            }

            for (var i = 160; i < 192; i++) {
                buffer1[i] = 0x03;
            }

            for (var i = 192; i < 224; i++) {
                buffer1[i] = 0xFE;
            }

            for (var i = 224; i < 256; i++) {
                buffer1[i] = 0xFC;
            }

            var p1 = new PHashEx(buffer1, 0);
            var p2 = new PHashEx(buffer1, 128);
            var d = p1.HammingDistance(p2);
            Assert.AreEqual(d, 0);

            for (var i = 160; i < 192; i++) {
                buffer1[i] = 0x80;
            }

            var p3 = new PHashEx(buffer1, 128);
            d = p1.HammingDistance(p3);
            Assert.AreEqual(d, 64);

            var a1 = p1.ToArray();
            Assert.AreEqual(a1[0], buffer1[0]);
            Assert.AreEqual(a1[127], buffer1[127]);

            var a2 = p2.ToArray();
            Assert.AreEqual(a2[0], buffer1[128]);
            Assert.AreEqual(a2[127], buffer1[255]);
        }

        [TestMethod()]
        public void HammingDistanceTest()
        {
            var imagedata = File.ReadAllBytes("gab_org.jpg");
            var matrix_org = BitmapHelper.GetMatrix(imagedata);
            Assert.IsNotNull(matrix_org);
            var p_org = new PHashEx(matrix_org);
            var d_org = p_org.HammingDistance(p_org);
            Assert.AreEqual(d_org, 0);

            imagedata = File.ReadAllBytes("gab_scale.jpg");
            var matrix_scale = BitmapHelper.GetMatrix(imagedata);
            Assert.IsNotNull(matrix_scale);
            var p_scale = new PHashEx(matrix_scale);
            var d_scale = p_scale.HammingDistance(p_org);
            Assert.AreEqual(d_scale, 6);

            imagedata = File.ReadAllBytes("gab_flip.jpg");
            var matrix_flip = BitmapHelper.GetMatrix(imagedata);
            Assert.IsNotNull(matrix_flip);
            var p_flip = new PHashEx(matrix_flip);
            var d_flip = p_flip.HammingDistance(p_org);
            Assert.AreEqual(d_flip, 18);

            imagedata = File.ReadAllBytes("gab_logo.jpg");
            var matrix_logo = BitmapHelper.GetMatrix(imagedata);
            Assert.IsNotNull(matrix_logo);
            var p_logo = new PHashEx(matrix_logo);
            var d_logo = p_logo.HammingDistance(p_org);
            Assert.AreEqual(d_logo, 22);

            imagedata = File.ReadAllBytes("gab_r3.jpg");
            var matrix_r3 = BitmapHelper.GetMatrix(imagedata);
            Assert.IsNotNull(matrix_r3);
            var p_r3 = new PHashEx(matrix_r3);
            var d_r3 = p_r3.HammingDistance(p_org);
            Assert.AreEqual(d_r3, 60);

            imagedata = File.ReadAllBytes("gab_r90.jpg");
            var matrix_r90 = BitmapHelper.GetMatrix(imagedata);
            Assert.IsNotNull(matrix_r90);
            var p_r90 = new PHashEx(matrix_r90);
            var d_r90 = p_r90.HammingDistance(p_org);
            Assert.AreEqual(d_r90, 14);

            imagedata = File.ReadAllBytes("gab_crop.jpg");
            var matrix_crop = BitmapHelper.GetMatrix(imagedata);
            Assert.IsNotNull(matrix_crop);
            var p_crop = new PHashEx(matrix_crop);
            var d_crop = p_crop.HammingDistance(p_org);
            Assert.AreEqual(d_crop, 102);

            imagedata = File.ReadAllBytes("gab_nosim1.jpg");
            var matrix_nosim = BitmapHelper.GetMatrix(imagedata);
            Assert.IsNotNull(matrix_nosim);
            var p_nosim = new PHashEx(matrix_nosim);
            var d_nosim = p_nosim.HammingDistance(p_org);
            Assert.AreEqual(d_nosim, 110);
        }
    }
    */
}