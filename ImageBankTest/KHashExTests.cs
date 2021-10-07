using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.IO;

namespace ImageBank.Tests
{
    [TestClass()]
    public class KHashExTests
    {
        [TestMethod()]
        public void HammingDistanceTest()
        {
            var imagedata = File.ReadAllBytes("GEDT2647.JPG");
            var matrix = BitmapHelper.GetMatrix(imagedata);
            Assert.IsNotNull(matrix);
             var p_org = new KHashEx(matrix);
            var p_org_again = new KHashEx(matrix);
            var d_org = p_org.HammingDistance(p_org_again);
            Debug.WriteLine($"d_org = {d_org}");

            imagedata = File.ReadAllBytes("gab_scale.jpg");
            matrix = BitmapHelper.GetMatrix(imagedata);
            Assert.IsNotNull(matrix);
            var p_scale = new KHashEx(matrix);
            var d_scale = p_scale.HammingDistance(p_org);
            Debug.WriteLine($"d_scale = {d_scale}");

            imagedata = File.ReadAllBytes("gab_flip.jpg");
            matrix = BitmapHelper.GetMatrix(imagedata);
            Assert.IsNotNull(matrix);
            var p_flip = new KHashEx(matrix);
            var d_flip = p_flip.HammingDistance(p_org);
            Debug.WriteLine($"d_flip = {d_flip}");

            imagedata = File.ReadAllBytes("gab_logo.jpg");
            matrix = BitmapHelper.GetMatrix(imagedata);
            Assert.IsNotNull(matrix);
            var p_logo = new KHashEx(matrix);
            var d_logo = p_logo.HammingDistance(p_org);
            Debug.WriteLine($"d_logo = {d_logo}");

            imagedata = File.ReadAllBytes("gab_r3.jpg");
            matrix = BitmapHelper.GetMatrix(imagedata);
            Assert.IsNotNull(matrix);
            var p_r3 = new KHashEx(matrix);
            var d_r3 = p_r3.HammingDistance(p_org);
            Debug.WriteLine($"d_r3 = {d_r3}");

            imagedata = File.ReadAllBytes("gab_r10.jpg");
            matrix = BitmapHelper.GetMatrix(imagedata);
            Assert.IsNotNull(matrix);
            var p_r10 = new KHashEx(matrix);
            var d_r10 = p_r10.HammingDistance(p_org);
            Debug.WriteLine($"d_r10 = {d_r10}");

            imagedata = File.ReadAllBytes("gab_r90.jpg");
            matrix = BitmapHelper.GetMatrix(imagedata);
            Assert.IsNotNull(matrix);
            var p_r90 = new KHashEx(matrix);
            var d_r90 = p_r90.HammingDistance(p_org);
            Debug.WriteLine($"d_r90 = {d_r90}");

            imagedata = File.ReadAllBytes("gab_crop.jpg");
            matrix = BitmapHelper.GetMatrix(imagedata);
            Assert.IsNotNull(matrix);
            var p_crop = new KHashEx(matrix);
            var d_crop = p_crop.HammingDistance(p_org);
            Debug.WriteLine($"d_crop = {d_crop}");

            imagedata = File.ReadAllBytes("gab_blur.jpg");
            matrix = BitmapHelper.GetMatrix(imagedata);
            Assert.IsNotNull(matrix);
            var p_blur = new KHashEx(matrix);
            var d_blur = p_blur.HammingDistance(p_org);
            Debug.WriteLine($"d_blur = {d_blur}");

            imagedata = File.ReadAllBytes("gab_face.jpg");
            matrix = BitmapHelper.GetMatrix(imagedata);
            Assert.IsNotNull(matrix);
            var p_face = new KHashEx(matrix);
            var d_face = p_face.HammingDistance(p_org);
            Debug.WriteLine($"d_face = {d_face}");

            imagedata = File.ReadAllBytes("gab_sim1.jpg");
            matrix = BitmapHelper.GetMatrix(imagedata);
            Assert.IsNotNull(matrix);
            var p_sim1 = new KHashEx(matrix);
            var d_sim1 = p_sim1.HammingDistance(p_org);
            Debug.WriteLine($"d_sim1 = {d_sim1}");

            imagedata = File.ReadAllBytes("gab_sim2.jpg");
            matrix = BitmapHelper.GetMatrix(imagedata);
            Assert.IsNotNull(matrix);
            var p_sim2 = new KHashEx(matrix);
            var d_sim2 = p_sim2.HammingDistance(p_org);
            Debug.WriteLine($"d_sim2 = {d_sim2}");

            imagedata = File.ReadAllBytes("gab_nosim1.jpg");
            matrix = BitmapHelper.GetMatrix(imagedata);
            Assert.IsNotNull(matrix);
            var p_nosim1 = new KHashEx(matrix);
            var d_nosim1 = p_nosim1.HammingDistance(p_org);
            Debug.WriteLine($"d_nosim1 = {d_nosim1}");

            imagedata = File.ReadAllBytes("gab_nosim2.jpg");
            matrix = BitmapHelper.GetMatrix(imagedata);
            Assert.IsNotNull(matrix);
            var p_nosim2 = new KHashEx(matrix);
            var d_nosim2 = p_nosim2.HammingDistance(p_org);
            Debug.WriteLine($"d_nosim2 = {d_nosim2}");

            imagedata = File.ReadAllBytes("gab_toside.jpg");
            matrix = BitmapHelper.GetMatrix(imagedata);
            Assert.IsNotNull(matrix);
            var p_toside = new KHashEx(matrix);
            var d_toside = p_toside.HammingDistance(p_org);
            Debug.WriteLine($"d_toside = {d_toside}");
        }
    }
}
