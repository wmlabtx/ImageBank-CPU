using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.IO;

namespace ImageBank.Tests
{
    [TestClass()]
    public class MHashTests
    {
        [TestMethod()]
        public void HammingDistanceTest()
        {
            var imagedata = File.ReadAllBytes("gab_org.jpg");
            var p_org = new MHash(imagedata);
            var p_org_again = new MHash(imagedata);
            var d_org = p_org.ManhattanDistance(p_org_again);
            Debug.WriteLine($"d_org = {d_org:F2}");

            imagedata = File.ReadAllBytes("gab_scale.jpg");
            var p_scale = new MHash(imagedata);
            var d_scale = p_scale.ManhattanDistance(p_org);
            Debug.WriteLine($"d_scale = {d_scale:F2}");

            imagedata = File.ReadAllBytes("gab_flip.jpg");
            var p_flip = new MHash(imagedata);
            var d_flip = p_flip.ManhattanDistance(p_org);
            Debug.WriteLine($"d_flip = {d_flip:F2}");

            imagedata = File.ReadAllBytes("gab_logo.jpg");
            var p_logo = new MHash(imagedata);
            var d_logo = p_logo.ManhattanDistance(p_org);
            Debug.WriteLine($"d_logo = {d_logo:F2}");

            imagedata = File.ReadAllBytes("gab_r3.jpg");
            var p_r3 = new MHash(imagedata);
            var d_r3 = p_r3.ManhattanDistance(p_org);
            Debug.WriteLine($"d_r3 = {d_r3:F2}");

            imagedata = File.ReadAllBytes("gab_r90.jpg");
            var p_r90 = new MHash(imagedata);
            var d_r90 = p_r90.ManhattanDistance(p_org);
            Debug.WriteLine($"d_r90 = {d_r90:F2}");

            imagedata = File.ReadAllBytes("gab_crop.jpg");
            var p_crop = new MHash(imagedata);
            var d_crop = p_crop.ManhattanDistance(p_org);
            Debug.WriteLine($"d_crop = {d_crop:F2}");

            imagedata = File.ReadAllBytes("gab_blur.jpg");
            var p_blur = new MHash(imagedata);
            var d_blur = p_blur.ManhattanDistance(p_org);
            Debug.WriteLine($"d_blur = {d_blur:F2}");

            imagedata = File.ReadAllBytes("gab_face.jpg");
            var p_face = new MHash(imagedata);
            var d_face = p_face.ManhattanDistance(p_org);
            Debug.WriteLine($"d_face = {d_face:F2}");

            imagedata = File.ReadAllBytes("gab_sim1.jpg");
            var p_sim1 = new MHash(imagedata);
            var d_sim1 = p_sim1.ManhattanDistance(p_org);
            Debug.WriteLine($"d_sim1 = {d_sim1:F2}");

            imagedata = File.ReadAllBytes("gab_sim2.jpg");
            var p_sim2 = new MHash(imagedata);
            var d_sim2 = p_sim2.ManhattanDistance(p_org);
            Debug.WriteLine($"d_sim2 = {d_sim2:F2}");

            imagedata = File.ReadAllBytes("gab_nosim1.jpg");
            var p_nosim1 = new MHash(imagedata);
            var d_nosim1 = p_nosim1.ManhattanDistance(p_org);
            Debug.WriteLine($"d_nosim1 = {d_nosim1:F2}");

            imagedata = File.ReadAllBytes("gab_nosim2.jpg");
            var p_nosim2 = new MHash(imagedata);
            var d_nosim2 = p_nosim2.ManhattanDistance(p_org);
            Debug.WriteLine($"d_nosim2 = {d_nosim2:F2}");

            imagedata = File.ReadAllBytes("gab_nosim3.jpg");
            var p_nosim3 = new MHash(imagedata);
            var d_nosim3 = p_nosim3.ManhattanDistance(p_org);
            Debug.WriteLine($"d_nosim3 = {d_nosim3:F2}");
        }
    }
}
