using System;
using System.Diagnostics.Contracts;

namespace ImageBank
{
    public partial class ImgMdf
    {
        private bool SetImgPanles(IProgress<string> progress, int idX, int idY)
        {
            progress.Report(GetPrompt());

            AppVars.ImgPanel[0] = GetImgPanel(idX);
            if (AppVars.ImgPanel[0] == null) {
                Delete(idX);
                progress.Report($"{idX} corrupted, deleted");
                return false;
            }

            AppVars.ImgPanel[1] = GetImgPanel(idY);
            if (AppVars.ImgPanel[1] == null) {
                Delete(idY);
                progress.Report($"{idY} corrupted, deleted");
                return false;
            }

            return true;
        }

        public void Find(IProgress<string> progress)
        {
            Contract.Requires(progress != null);
            int idX;
            int idY;
            do {
                if (!GetPairToCompare(out idX, out idY)) {
                    progress.Report("No images to view");
                    return;
                }
            } while (!SetImgPanles(progress, idX, idY));
        }

        public void Find(int idX, IProgress<string> progress)
        {
            Contract.Requires(progress != null);
            if (!_imgList.TryGetValue(idX, out var imgX)) {
                progress.Report($"error getting {idX}");
                return;
            }

            int idY;
            do {
                FindNext(idX, out var lastid, out var lastchange, out var nextid, out var distance);

                if (lastchange != imgX.LastChange) {
                    imgX.LastChange = lastchange;
                }

                if (lastid != imgX.LastId) {
                    imgX.LastId = lastid;
                }

                if (nextid != imgX.NextId) {
                    imgX.NextId = nextid;
                }

                if (distance != imgX.Distance) {
                    imgX.Distance = distance;
                }

                idY = nextid;
            } while (!SetImgPanles(progress, idX, idY));
        }
    }
}
