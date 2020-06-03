using OpenCvSharp;
using System;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;

namespace ImageBank
{
    public partial class ImgMdf
    {
        /*
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

            var idX = GetNextToCheck();
            Img imgX;
            lock (_imglock) {
                if (!_imgList.TryGetValue(idX, out imgX)) {
                    backgroundworker.ReportProgress(0, $"error getting {idX}");
                    return;
                }
            }

            if (!File.Exists(imgX.FileName)) {
                Delete(idX);
                backgroundworker.ReportProgress(0, $"{idX} deleted");
                return;
            }

            var lastmodified = File.GetLastWriteTime(imgX.FileName);
            if (Math.Abs(lastmodified.Subtract(imgX.LastModified).TotalSeconds) > 2.0) {
                if (!Helper.GetImageDataFromFile(
                    imgX.FileName,
                    out byte[] imgdata,
#pragma warning disable CA2000 // Dispose objects before losing scope
                    out Bitmap bitmap,
#pragma warning restore CA2000 // Dispose objects before losing scope
                    out string id,
                    out string message)) {
                    Delete(idX);
                    backgroundworker.ReportProgress(0, $"{idX} deleted");
                    return;
                }

                lock (_imglock) {
                    if (_imgList.ContainsKey(id)) {
                        Helper.DeleteToRecycleBin(imgX.FileName);
                        backgroundworker.ReportProgress(0, $"{idX} deleted");
                        return;
                    }
                }

                if (!OrbHelper.Compute(bitmap, out Mat descriptors)) {
                    return;
                }

                bitmap.Dispose();

                var lastview = GetMinLastView();
                var lastcheck = GetMinLastCheck();
                var img = new Img(
                    id: id,
                    folder: imgX.Folder,
                    lastview: lastview,
                    nextid: id,
                    distance: 256f,
                    lastcheck: lastcheck,
                    lastmodified: lastmodified,
                    descriptors: descriptors,
                    counter: 0);

                Add(img);
                if (!imgX.FileName.Equals(img.FileName, StringComparison.OrdinalIgnoreCase)) {
                    File.WriteAllBytes(img.FileName, imgdata);
                    File.SetLastWriteTime(img.FileName, lastmodified);
                    Delete(imgX.Id);
                }
            }

            if (!FindNext(idX, out var nextid, out var distance)) {
                backgroundworker.ReportProgress(0, $"error getting {idX}");
                return;
            }

            var sb = new StringBuilder();
            if (Math.Abs(distance - imgX.Distance) > 0.0001) {
                if (distance < imgX.Distance) {
                    imgX.Counter = 0;
                }

                sb.Append($"{imgX.Folder}\\{imgX.Id}: ");
                sb.Append($"{imgX.Distance:F2} ");
                sb.Append($"{char.ConvertFromUtf32(distance < imgX.Distance ? 0x2192 : 0x2193)} ");
                sb.Append($"{distance:F2} ");
                imgX.Distance = distance;
                if (!nextid.Equals(imgX.NextId, StringComparison.OrdinalIgnoreCase)) {
                    imgX.NextId = nextid;
                }
            }
            else {
                if (!nextid.Equals(imgX.NextId, StringComparison.OrdinalIgnoreCase)) {
                    sb.Append($"{imgX.Folder}\\{imgX.Id}: ");
                    sb.Append($"{imgX.NextId} ");
                    sb.Append($"{char.ConvertFromUtf32(0x2192)} ");
                    sb.Append($"{nextid}");
                    imgX.NextId = nextid;
                }
            }

            imgX.LastCheck = DateTime.Now;
 
            if (sb.Length > 0) {
                var message = sb.ToString();
                backgroundworker.ReportProgress(0, message);
            }
        }
        */
    }
}