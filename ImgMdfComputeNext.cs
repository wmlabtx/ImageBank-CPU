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
            if (idX <= 0) {
                //Import(AppConsts.MaxImport, AppVars.BackgroundProgress);
                backgroundworker.ReportProgress(0, "idle");
                return;
            }

            FindNext(idX, out var lastid, out var nextid, out var distance);

            Img imgX;
            lock (_imglock) {
                if (!_imgList.TryGetValue(idX, out imgX)) {
                    backgroundworker.ReportProgress(0, $"error getting {idX}");
                    return;
                }
            }

            if (!File.Exists(imgX.File)) {
                Delete(idX);
                backgroundworker.ReportProgress(0, $"{idX} deleted");
                return;
            }

            imgX.LastFind = DateTime.Now;

            var sb = new StringBuilder();
            sb.Append($"i{imgX.Id}: ");
            if (Math.Abs(distance - imgX.Distance) > 0.0001) {
                imgX.LastChange = DateTime.Now;
                sb.Append($"{imgX.Distance:F4} ");
                sb.Append($"{char.ConvertFromUtf32(distance < imgX.Distance ? 0x2192 : 0x2193)} ");
                sb.Append($"{distance:F4}");
                imgX.Distance = distance;
                if (nextid != imgX.NextId) {
                    imgX.NextId = nextid;
                }
            }
            else {
                if (nextid != imgX.NextId) {
                    imgX.LastChange = DateTime.Now;
                    sb.Append($"i{imgX.NextId} ");
                    sb.Append($"{char.ConvertFromUtf32(0x2192)} ");
                    sb.Append($"i{nextid}");
                    imgX.NextId = nextid;
                }
            }

            if (lastid != imgX.LastId) {
                imgX.LastId = lastid;
            }

            var message = sb.ToString();
            backgroundworker.ReportProgress(0, message);
        }
    }
}