using System;
using System.Linq;

namespace ImageBank
{
    public partial class ImgMdf
    {
        public static void Find(int idX, IProgress<string> progress)
        {
            Img imgX = null;
            do {
                lock (_imglock) {
                    if (_imgList.Count < 2) {
                        progress.Report("No images to view");
                        return;
                    }
                }

                if (idX == 0) {
                    imgX = null;
                    Img[] valid;
                    lock (_imglock) {
                        valid = _imgList
                            .Where(
                                e => e.Value.BestId != 0 &&
                                e.Value.BestId != e.Value.Id &&
                                _imgList.ContainsKey(e.Value.BestId) &&
                                (e.Value.Family == 0 || (e.Value.Family > 0 && e.Value.History.Count == 0)))
                            .Select(e => e.Value)
                            .ToArray();
                    }

                    if (valid.Length == 0) {
                        progress.Report("No images to view");
                        return;
                    }

                    var lv = valid.Min(e => e.LastView);
                    imgX = valid.First(e => e.LastView == lv);
                    idX = imgX.Id;

                    AppVars.CandidateIndex = 0;
                    AppVars.Candidates.Clear();
                    if (!_imgList.TryGetValue(imgX.BestId, out var imgBest)) {
                        Delete(imgX.BestId);
                        progress.Report($"{imgX.BestId} deleted");
                        idX = 0;
                        continue;
                    }

                    AppVars.Candidates.Add(imgBest);
                    var candidates = imgX.Family == 0 ?
                        _imgList.Where(e => e.Key != imgX.Id && e.Key != imgX.BestId && e.Value.Family > 0 && !imgX.History.ContainsKey(e.Key)).OrderBy(e => ImageHelper.Random).Select(e => e.Value).ToArray() :
                        _imgList.Where(e => e.Key != imgX.Id && e.Key != imgX.BestId && e.Value.Family > 0 && !imgX.History.ContainsKey(e.Key) && imgX.Family == e.Value.Family).OrderBy(e => ImageHelper.Random).Select(e => e.Value).ToArray();

                    AppVars.Candidates.AddRange(candidates);
                }

                AppVars.ImgPanel[0] = GetImgPanel(idX);
                if (AppVars.ImgPanel[0] == null) {
                    Delete(idX);
                    progress.Report($"{idX} deleted");
                    idX = 0;
                    continue;
                }

                imgX = AppVars.ImgPanel[0].Img;
                var idY = imgX.BestId;

                AppVars.ImgPanel[1] = GetImgPanel(idY);
                if (AppVars.ImgPanel[1] == null) {
                    Delete(idY);
                    progress.Report($"{idY} deleted");
                    idX = 0;
                    continue;
                }

                break;
            }
            while (true);

            lock (_imglock) {
                var imgcount = _imgList.Count;
                var orphans = _imgList.Count(e => e.Value.Family == 0);
                var families = _imgList.Where(e => e.Value.Family > 0).Select(e => e.Value.Family).Distinct().Count();
                progress.Report($"orphans:{orphans} families:{families} images:{imgcount} distance:{imgX.BestDistance:F1}");
            }
        }

        public static void Find(IProgress<string> progress) => Find(0, progress);

        public static void FindForward(IProgress<string> progress)
        {
            if (AppVars.CandidateIndex + 1 <= AppVars.Candidates.Count - 1) {
                AppVars.CandidateIndex++;
            }

            FindCandidate(progress);
        }

        public static void FindBackward(IProgress<string> progress)
        {
            if (AppVars.CandidateIndex - 1 >= 0) {
                AppVars.CandidateIndex--;
            }

            FindCandidate(progress);
        }

        public static void FindCandidate(IProgress<string> progress)
        {
            var idY = AppVars.Candidates[AppVars.CandidateIndex].Id;
            AppVars.ImgPanel[1] = GetImgPanel(idY);
            if (AppVars.ImgPanel[1] == null) {
                Delete(idY);
                progress.Report($"{idY} deleted");
                AppVars.Candidates.RemoveAt(AppVars.CandidateIndex);
                if (AppVars.CandidateIndex >= AppVars.Candidates.Count) {
                    AppVars.CandidateIndex = AppVars.Candidates.Count - 1;
                }

                return;
            }

            lock (_imglock) {
                var imgcount = _imgList.Count;
                var orphans = _imgList.Count(e => e.Value.Family == 0);
                var families = _imgList.Where(e => e.Value.Family > 0).Select(e => e.Value.Family).Distinct().Count();
                progress.Report($"orphans:{orphans} families:{families} images:{imgcount} position:{AppVars.CandidateIndex}/{AppVars.Candidates.Count}");
            }
        }
    }
}
