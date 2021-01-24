using System;
using System.Diagnostics;
using ImageBank;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ImageBankTest
{
    [TestClass()]
    public class OrbDescriptorTest
    {
        [TestMethod()]
        public void Performance()
        {
            const int blobsize = 64 * 32;
            var blob = new byte[blobsize];
            var random = new Random(0);
            random.NextBytes(blob);

            var x = Img.BlobToDescriptors(blob);
            var y = Img.BlobToDescriptors(blob);
            Array.Reverse(y);

            var sw = Stopwatch.StartNew();
            var counter = 0;
            var sum = 0f;
            while (sw.ElapsedMilliseconds < 1000) {
                var distance = OrbDescriptor.Distance(x, y);
                counter++;
                sum += distance;
            }

            sw.Stop();
            var avg = sum / counter;
            Debug.Write($"{sw.Elapsed} | {counter} | {avg}");
        }
    }
}
