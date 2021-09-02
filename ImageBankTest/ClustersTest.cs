using ImageBank;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenCvSharp;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace ImageBankTest
{
    [TestClass()]
    public class ClustersTest
    {
        public static readonly ImgMdf Collection = new ImgMdf();
        private static Mat Clusters = new Mat();
        private static readonly BFMatcher _bfmatcher = new BFMatcher(normType: NormTypes.Hamming);
        private static int[] CDistances = new int[0x10000];
        private static int CMinDistance = -1;
        private static int CVictimIndex = -1;

        private static void UpdateClusters()
        {
            var cmatches = _bfmatcher.KnnMatch(Clusters, Clusters, k: 2);
            for (var i = 0; i < cmatches.Length; i++) {
                if (cmatches[i].Length < 2) {
                    continue;
                }

                if (float.IsNaN(cmatches[i][1].Distance)) {
                    continue;
                }

                var d = (int)cmatches[i][1].Distance;
                CDistances[i] = d;
            }

            UpdateDistances();
            Thread.Sleep(10000);
        }

        private static void UpdateDistances()
        {
            CMinDistance = 468;
            for (var i = 0; i < CDistances.Length; i++) {
                if (CDistances[i] < CMinDistance) {
                    CMinDistance = CDistances[i];
                    CVictimIndex = i;
                }
            }
        }

        private static void AddRow(int rowindex, Mat row)
        {
            if (Clusters.Rows < 0x10000) {
                Clusters.PushBack(row);
                return;
            }

            if (CMinDistance < 0) {
                UpdateClusters();
            }

            var matches = _bfmatcher.KnnMatch(row, Clusters, k: 1);
            var mindistance = (int)matches[0][0].Distance;
            if (mindistance > CMinDistance) {
                CDistances[CVictimIndex] = mindistance;
                for (var j = 0; j < row.Cols; j++) {
                    Clusters.At<byte>(CVictimIndex, j) = row.At<byte>(0, j);
                }

                UpdateDistances();
                Debug.WriteLine($"  RowIndex:{rowindex} Distance:{mindistance} MinDistance:{CMinDistance}");
            }
        }

        private static void AddMat(Mat mat)
        {
            for (var i = 0; i < mat.Rows; i++) {
                AddRow(i, mat.Row(i));
            }

            if (Clusters.Rows == 0x10000) {
                //UpdateClusters();
                Clusters.GetArray<byte>(out var buffer);
                File.WriteAllBytes($"akazeclusters{CMinDistance}.dat", buffer);
            }
        }

        [TestMethod()]
        public void BuildClustersTest()
        {
            Debug.WriteLine("Import images...");
            ImgMdf.Import(null);
            Debug.WriteLine("Build clusters...");
            var imagescounter = 0;
            while(ImgMdf._rwList.Count > 0) {
                var fileinfo = ImgMdf._rwList.ElementAt(0);
                ImgMdf._rwList.RemoveAt(0);
                var orgfilename = fileinfo.FullName;
                if (!File.Exists(orgfilename)) {
                    continue;
                }

                var orgextension = Path.GetExtension(orgfilename);
                if (
                    !orgextension.Equals(AppConsts.MzxExtension, StringComparison.OrdinalIgnoreCase) &&
                    !orgextension.Equals(AppConsts.DbxExtension, StringComparison.OrdinalIgnoreCase) &&
                    !orgextension.Equals(AppConsts.DatExtension, StringComparison.OrdinalIgnoreCase) &&
                    !orgextension.Equals(AppConsts.PngExtension, StringComparison.OrdinalIgnoreCase) &&
                    !orgextension.Equals(AppConsts.BmpExtension, StringComparison.OrdinalIgnoreCase) &&
                    !orgextension.Equals(AppConsts.WebpExtension, StringComparison.OrdinalIgnoreCase) &&
                    !orgextension.Equals(AppConsts.JpgExtension, StringComparison.OrdinalIgnoreCase) &&
                    !orgextension.Equals(AppConsts.JpegExtension, StringComparison.OrdinalIgnoreCase)
                    ) {
                    continue;
                }

                var imagedata = File.ReadAllBytes(orgfilename);
                if (imagedata == null || imagedata.Length < 16) {
                    continue;
                }

                if (orgextension.Equals(AppConsts.DatExtension, StringComparison.OrdinalIgnoreCase) ||
                    orgextension.Equals(AppConsts.MzxExtension, StringComparison.OrdinalIgnoreCase)) {
                    var password = Path.GetFileNameWithoutExtension(orgfilename);
                    var decrypteddata = orgextension.Equals(AppConsts.DatExtension, StringComparison.OrdinalIgnoreCase) ?
                        Helper.DecryptDat(imagedata, password) :
                        Helper.Decrypt(imagedata, password);

                    if (decrypteddata != null) {
                        imagedata = decrypteddata;
                    }
                }

                var magicformat = ImageHelper.GetMagicFormat(imagedata);
                if (magicformat == MagicFormat.Jpeg) {
                    if (imagedata[0] != 0xFF || imagedata[1] != 0xD8 || imagedata[imagedata.Length - 2] != 0xFF || imagedata[imagedata.Length - 1] != 0xD9) {
                        continue;
                    }
                }

                if (!ImageHelper.GetBitmapFromImageData(imagedata, out var bitmap)) {
                    continue;
                }

                if (!ImageHelper.GetDescriptors(bitmap, out Mat[] mat, out _)) {
                    bitmap.Dispose();
                    continue;
                }

                bitmap.Dispose();

                Debug.WriteLine($"Adding {orgfilename}");

                AddMat(mat[0]);
                mat[0].Dispose();
                AddMat(mat[1]);
                mat[1].Dispose();

                imagescounter++;

                Debug.WriteLine($"Images:{imagescounter} MinDistance:{CMinDistance} Nodes:{Clusters.Rows}");
            }
        }
    }
}
