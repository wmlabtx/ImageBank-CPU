using System;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.IO;
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

            var idX = GetNextToCheck();
            if (!FindNext(idX, out var nextid, out var distance)) {
                backgroundworker.ReportProgress(0, $"error getting {idX}");
                return;
            }
           
            Img imgX;
            lock (_imglock) {
                if (!_imgList.TryGetValue(idX, out imgX)) {
                    backgroundworker.ReportProgress(0, $"error getting {idX}");
                    return;
                }
            }

            if (imgX.Format != MagicFormat.Jpeg && imgX.Format != MagicFormat.Png) {
                Delete(idX);
                backgroundworker.ReportProgress(0, $"{idX} deleted");
                return;
            }

            if (!File.Exists(imgX.FileName)) {
                Delete(idX);
                backgroundworker.ReportProgress(0, $"{idX} deleted");
                return;
            }

            var sb = new StringBuilder();
            if (Math.Abs(distance - imgX.Distance) > 0.0001) {
                sb.Append($"i{imgX.Id}: ");
                sb.Append($"{imgX.Distance:F2} ");
                sb.Append($"{char.ConvertFromUtf32(distance < imgX.Distance ? 0x2192 : 0x2193)} ");
                sb.Append($"{distance:F2} ");
                imgX.Distance = distance;
                if (nextid != imgX.NextId) {
                    imgX.NextId = nextid;
                }
            }
            else {
                if (nextid != imgX.NextId) {
                    sb.Append($"i{imgX.Id}: ");
                    sb.Append($"i{imgX.NextId} ");
                    sb.Append($"{char.ConvertFromUtf32(0x2192)} ");
                    sb.Append($"i{nextid}");
                    imgX.NextId = nextid;
                }
            }

            imgX.LastCheck = DateTime.Now;

            if (sb.Length > 0) {
                var message = sb.ToString();
                backgroundworker.ReportProgress(0, message);
            }
        }
    }
}