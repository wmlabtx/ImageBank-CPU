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
        private int _importcounter = 0;

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

            if (_importcounter >= 100) {
                _importcounter = 0;
                Import(1000, AppVars.BackgroundProgress);
                return;
            }

            _importcounter++;

            var idX = GetNextToCheck();
            FindNext(idX, out var nextid, out var sim);
           
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

            imgX.LastCheck = DateTime.Now;

            var sb = new StringBuilder();
            if (Math.Abs(sim - imgX.Sim) > 0.0001) {
                sb.Append($"i{imgX.Id}: ");
                sb.Append($"{imgX.Sim:F2} ");
                sb.Append($"{char.ConvertFromUtf32(sim > imgX.Sim ? 0x2192 : 0x2193)} ");
                sb.Append($"{sim:F2}");
                imgX.Sim = sim;
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

            if (sb.Length > 0) {
                var message = sb.ToString();
                backgroundworker.ReportProgress(0, message);
            }
        }
    }
}