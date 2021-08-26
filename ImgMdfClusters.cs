﻿using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace ImageBank
{
    public partial class ImgMdf
    {
        /*
        public static void Clustering()
        {
            AppVars.SuspendEvent.Reset();

            const int MAXDIM = 500;
            var _kaze = KAZE.Create();

            using (var matsum = new Mat()) {
                List<string> list;
                lock (_imglock) {
                    list = _imgList.Select(e => e.Value.Name).Take(32000).ToList();
                }

                var added = 0;
                var dt = DateTime.Now;

                var random = new CryptoRandom();
                while (matsum.Rows < 16 * 1024 * 50) {
                    var index =  random.NextShort(0, (short)(list.Count - 1));
                    var name = list[index];
                    list.RemoveAt(index);
                    var filename = Helper.GetFileName(name);

                    if (DateTime.Now.Subtract(dt).TotalMilliseconds > AppConsts.TimeLapse) {
                        dt = DateTime.Now;
                        ((IProgress<string>)AppVars.Progress).Report($"a:{added}/m:{matsum.Rows / 1024}K");
                    }

                    var imagedata = File.ReadAllBytes(filename);
                    var password = Path.GetFileNameWithoutExtension(filename);
                    var decrypteddata = Helper.Decrypt(imagedata, password);
                    if (decrypteddata == null) {
                        continue;
                    }

                    imagedata = decrypteddata;
                    if (!ImageHelper.GetBitmapFromImageData(imagedata, out Bitmap bitmap)) {
                        continue;
                    }

                    if (bitmap.PixelFormat != PixelFormat.Format24bppRgb) {
                        bitmap = ImageHelper.RepixelBitmap(bitmap);
                    }

                    using (var matsource = bitmap.ToMat())
                    using (var matcolor = new Mat()) {
                        var f = (double)MAXDIM / Math.Max(matsource.Width, matsource.Height);
                        Cv2.Resize(matsource, matcolor, new OpenCvSharp.Size(0, 0), f, f, InterpolationFlags.Area);
                        using (var mat = new Mat()) {
                            Cv2.CvtColor(matcolor, mat, ColorConversionCodes.BGR2GRAY);
                            var keypoints = _kaze.Detect(mat);
                            if (keypoints.Length > 0) {
                                keypoints = keypoints.OrderByDescending(e => e.Response).Take(AppConsts.MaxDescriptors).ToArray();
                                using (var matdescriptors = new Mat()) {
                                    _kaze.Compute(mat, ref keypoints, matdescriptors);
                                    matsum.PushBack(matdescriptors);
                                    added++;
                                    
                                    using (var matkeypoints = new Mat()) {
                                        Cv2.DrawKeypoints(mat, keypoints, matkeypoints, null, DrawMatchesFlags.DrawRichKeypoints);
                                        matkeypoints.SaveImage("keypoints500x500.png");
                                    }
                                    
                                }
                            }
                        }
                    }
                }

                using (var bestlabels = new Mat())
                using (var dictionary = new Mat()) {
                    var tm = new TermCriteria(CriteriaTypes.MaxIter, 3, 1.0);
                    Cv2.Kmeans(matsum, 16 * 1024, bestlabels, tm, 1, KMeansFlags.PpCenters, dictionary);
                    dictionary.GetArray<float>(out var fdata);
                    var data = new byte[fdata.Length * sizeof(float)];
                    Buffer.BlockCopy(fdata, 0, data, 0, data.Length);
                    File.WriteAllBytes(AppConsts.FileKazeClusters, data);
                }
            }

            ((IProgress<string>)AppVars.Progress).Report("Clusterization done");

            AppVars.SuspendEvent.Set();
        }
        */

        /*
        public static void Clustering(string path, int maxadd)
        {
            AppVars.SuspendEvent.Reset();

            using (var matsum = new Mat())
            using (var _kaze = KAZE.Create()) {
                var added = 0;
                var dt = DateTime.Now;
                var folder = string.Empty;

                ((IProgress<string>)AppVars.Progress).Report("clustering...");
                var directoryInfo = new DirectoryInfo(path);
                var fileInfos =
                    directoryInfo.GetFiles("*.*", SearchOption.AllDirectories)
                        .OrderBy(e => e.Length)
                        .ToList();

                for (var x = 0; x < 1000; x++) {
                    var rindex = _random.Next(fileInfos.Count);
                    var fileInfo = fileInfos[rindex];
                    fileInfos.RemoveAt(rindex);
                    var filename = fileInfo.FullName;
                    var extension = Path.GetExtension(filename);
                    if (extension.Equals(AppConsts.CorruptedExtension)) {
                        continue;
                    }

                    var shortfilename = filename.Substring(AppConsts.PathRoot.Length);

                    if (DateTime.Now.Subtract(dt).TotalMilliseconds > AppConsts.TimeLapse) {
                        dt = DateTime.Now;
                        ((IProgress<string>)AppVars.Progress).Report($"{shortfilename} (a:{added}/m:{matsum.Rows})...");
                    }

                    if (!ImageHelper.GetImageDataFromFile(
                        filename,
                        out var imagedata,
                        out var bitmap,
                        out var message)) {
                        ((IProgress<string>)AppVars.Progress).Report($"Corrupted image: {shortfilename}: {message}");
                        File.Move(filename, $"{filename}{AppConsts.CorruptedExtension}");
                        continue;
                    }

                    const int MAXDIM = 1024;
                    using (var matsource = bitmap.ToMat())
                    using (var matcolor = new Mat()) {
                        var f = (double)MAXDIM / Math.Max(matsource.Width, matsource.Height);
                        Cv2.Resize(matsource, matcolor, new Size(0, 0), f, f, InterpolationFlags.Area);
                        using (var mat = new Mat()) {
                            Cv2.CvtColor(matcolor, mat, ColorConversionCodes.BGR2GRAY);
                            var keypoints = _kaze.Detect(mat);
                            if (keypoints.Length > 0) {
                                keypoints = keypoints.OrderByDescending(e => e.Response).Take(AppConsts.MaxDescriptors).ToArray();
                                using (var matdescriptors = new Mat()) {
                                    _kaze.Compute(mat, ref keypoints, matdescriptors);
                                    matsum.PushBack(matdescriptors);
                                    if (matdescriptors.Rows > 0 && keypoints.Length > 0) {
                                        using (var matkeypoints = new Mat()) {
                                            Cv2.DrawKeypoints(mat, keypoints, matkeypoints, null, DrawMatchesFlags.DrawRichKeypoints);
                                            matkeypoints.SaveImage("keypoints.png");
                                        }
                                    }
                                }
                            }
                        }
                    }

                    added++;
                    if (added >= maxadd) {
                        break;
                    }
                }

                using (var bestlabels = new Mat())
                using (var dictionary = new Mat()) {
                    Cv2.Kmeans(matsum, 256, bestlabels, new TermCriteria(CriteriaTypes.Eps, 3, 0.1), 3, KMeansFlags.PpCenters, dictionary);
                    dictionary.GetArray<float>(out var fdata);
                    var data = new byte[fdata.Length * sizeof(float)];
                    Buffer.BlockCopy(fdata, 0, data, 0, data.Length);
                    var clustersfile = Path.Combine(AppConsts.PathRoot, AppConsts.FileKazeClusters);
                    File.WriteAllBytes(clustersfile, data);
                }
            }

            ((IProgress<string>)AppVars.Progress).Report("Clusterization done");

            AppVars.SuspendEvent.Set();
        }
        */
    }
}
