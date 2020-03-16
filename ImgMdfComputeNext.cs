using System;
using System.ComponentModel;
using System.Diagnostics.Contracts;
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

            float fill;
            lock (_imglock) {
                if (_imgList.Count == 0) {
                    backgroundworker.ReportProgress(0, "no images");
                    return;
                }

                fill = _imgList.Sum(e => e.Value.LastId) * 100f / (_imgList.Count * (float)_id);
            }

            var idX = GetNextToCheck();
            if (idX <= 0) {
                Import(50f, AppVars.BackgroundProgress);
                //backgroundworker.ReportProgress(0, "idle");
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

            var done = imgX.LastId * 100f / _id;
            var donedelta = (lastid * 100f / _id) - done;

            if (!File.Exists(imgX.FileName)) {
                Delete(idX);
                backgroundworker.ReportProgress(0, $"{idX} deleted");
                return;
            }

            var sb = new StringBuilder();
            if (Math.Abs(distance - imgX.Distance) > 0.0001) {
                sb.Append($"f={fill:F2} i{imgX.Id}: ");
                sb.Append($"[{done:F1}%+{donedelta:F1}%] ");
                sb.Append($"{imgX.Distance:F2} ");
                sb.Append($"{char.ConvertFromUtf32(distance < imgX.Distance ? 0x2192 : 0x2193)} ");
                sb.Append($"{distance:F2}");
                imgX.Distance = distance;
                if (nextid != imgX.NextId) {
                    imgX.NextId = nextid;
                }
            }
            else {
                if (nextid != imgX.NextId) {
                    sb.Append($"f={fill:F2} i{imgX.Id}: ");
                    sb.Append($"[{done:F1}%+{donedelta:F1}%] ");
                    sb.Append($"i{imgX.NextId} ");
                    sb.Append($"{char.ConvertFromUtf32(0x2192)} ");
                    sb.Append($"i{nextid}");
                    imgX.NextId = nextid;
                }
            }

            if (lastid != imgX.LastId) {
                imgX.LastId = lastid;
            }

            if (sb.Length > 0) {
                var message = sb.ToString();
                backgroundworker.ReportProgress(0, message);
            }
        }
    }
}