using OpenCvSharp;
using System;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace ImageBank
{
    public partial class ImgMdf
    {
        public void Compute(BackgroundWorker backgroundworker)
        {
            Contract.Requires(backgroundworker != null);

            AppVars.SuspendEvent.WaitOne(Timeout.Infinite);

            lock (_imglock) {
                if (_imgList.Count == 0) {
                    backgroundworker.ReportProgress(0, "no images");
                    return;
                }
            }

            var nameX = GetNextToCheck();
            if (string.IsNullOrEmpty(nameX)) {
                backgroundworker.ReportProgress(0, $"idle");
                Thread.Sleep(10 * 1000);
                return;
            }

            Img imgX;
            lock (_imglock) {
                if (!_imgList.TryGetValue(nameX, out imgX)) {
                    backgroundworker.ReportProgress(0, $"error getting {nameX}");
                    return;
                }
            }

            if (!File.Exists(imgX.FileName)) {
                Delete(nameX);
                backgroundworker.ReportProgress(0, $"{nameX} deleted");
                return;
            }

            if (imgX.GetDescriptors() == null) {
                if (!Helper.GetImageDataFromFile(
                    imgX.FileName,
                    out _,
#pragma warning disable CA2000 // Dispose objects before losing scope
                out Bitmap bitmap,
#pragma warning restore CA2000 // Dispose objects before losing scope
                out _,
                    out _)) {
                    Delete(nameX);
                    backgroundworker.ReportProgress(0, $"{nameX} deleted");
                    return;
                }

                if (!OrbHelper.Compute(bitmap, out Mat descriptors)) {
                    Delete(nameX);
                    backgroundworker.ReportProgress(0, $"{nameX} deleted");
                    return;
                }

                bitmap.Dispose();

                imgX.SetDescriptors(descriptors);
                imgX.Counter = 0;

                int zerolist;
                lock (_imglock) {
                    zerolist = _imgList.Count(e => e.Value.GetDescriptors() == null);
                }

                backgroundworker.ReportProgress(0, $"({zerolist} left)");
                return;
            }

            int nonzerolist;
            lock (_imglock) {
                nonzerolist = _imgList.Count(e => e.Value.GetDescriptors() != null);
            }

            if (!_flannAvailable || Math.Abs(nonzerolist - _flannNames.Length) > 9999) {
                backgroundworker.ReportProgress(0, "Updating flann...");
                FlannUpdate();
            }

            imgX.NextName = FlannFindNextName(imgX);

            var sb = new StringBuilder();
            sb.Append($"[{Helper.TimeIntervalToString(DateTime.Now.Subtract(imgX.LastCheck))} ago] ");
            sb.Append($"{imgX.Folder:D2}\\{imgX.Name} ");

            imgX.LastCheck = DateTime.Now;

            if (sb.Length > 0) {
                var message = sb.ToString();
                backgroundworker.ReportProgress(0, message);
            }
        }
    }
}