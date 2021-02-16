using System;
using System.IO;
using System.Linq;

namespace ImageBank
{
    public static class DescriptorFactory
    {
        /*
        private static readonly SIFT _sift = SIFT.Create();

        public static void CollectDescriptors()
        {
            var icounter = 0;
            var dcounter = 0;
            var dt = DateTime.Now;
            var random = new Random(0);

            ((IProgress<string>)AppVars.Progress).Report("importing...");
            var directoryInfo = new DirectoryInfo(AppConsts.PathHp);
            var fileInfos = directoryInfo.GetFiles("*.*", SearchOption.AllDirectories).ToList();
            using (var writer = new BinaryWriter(File.Open(AppConsts.FileDescriptrors, FileMode.Create)))
            {
                while (fileInfos.Count > 0 && dcounter < 1000000)
                {
                    var index = random.Next(fileInfos.Count);
                    var filename = fileInfos[index].FullName;
                    fileInfos.RemoveAt(index);
                    var name = Path.GetFileNameWithoutExtension(filename);

                    if (DateTime.Now.Subtract(dt).TotalMilliseconds > AppConsts.TimeLapse)
                    {
                        dt = DateTime.Now;
                        ((IProgress<string>)AppVars.Progress).Report($"images:{icounter} descriptors:{dcounter}");
                    }

                    if (!ImageHelper.GetImageDataFromFile(
                        filename,
                        out var imagedata,
                        out var bitmap,
                        out var message))
                    {
                        ((IProgress<string>)AppVars.Progress).Report($"Corrupted image: {name}: {message}");
                        continue;
                    }

                    if (!ImageHelper.ComputeDescriptors(bitmap, out var fblob))
                    {
                        message = "not enough descriptors";
                        ((IProgress<string>)AppVars.Progress).Report($"Corrupted image: {name}: {message}");
                        continue;
                    }

                    var blob = new byte[fblob.Length * sizeof(float)];
                    Buffer.BlockCopy(fblob, 0, blob, 0, blob.Length);
                    writer.Write(blob, 0, blob.Length);
                    icounter++;
                    dcounter += fblob.Length / _sift.DescriptorSize;
                }
            }
        }

        public static void FindWords()
        {
            ((IProgress<string>)AppVars.Progress).Report("Training...");
            var dt = DateTime.Now;
            const int scopedescriptors = 1000 * 1000;
            var length = scopedescriptors * _sift.DescriptorSize * sizeof(float);
            using (var reader = new BinaryReader(File.Open(AppConsts.FileDescriptrors, FileMode.Open)))
            {
                var blob = reader.ReadBytes(length);
                var fblob = new float[length / sizeof(float)];
                Buffer.BlockCopy(blob, 0, fblob, 0, length);
                using (var mat = new Mat(scopedescriptors, _sift.DescriptorSize, MatType.CV_32F))
                {
                    mat.SetArray(fblob);
                    using (var bestlabels = new Mat())
                    using (var clusters = new Mat())
                    {
                        Cv2.Kmeans(
                            mat,
                            AppConsts.MaxWords,
                            bestlabels,
                            new TermCriteria(CriteriaType.Eps, 10, 0.0001),
                            10,
                            KMeansFlags.PpCenters,
                            clusters);

                        clusters.GetArray(out float[] fclusters);
                        blob = new byte[fclusters.Length * sizeof(float)];
                        Buffer.BlockCopy(fclusters, 0, blob, 0, blob.Length);
                        File.WriteAllBytes(AppConsts.FileClusters, blob);
                    }
                }
            }

            var secs = DateTime.Now.Subtract(dt).TotalSeconds;
            ((IProgress<string>)AppVars.Progress).Report($"Clustering finished ({scopedescriptors / 1024}K) - {secs}s");
        }
        */
    }
}
