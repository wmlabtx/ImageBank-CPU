using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace ImageBank
{
    public partial class ImgMdf
    {
        private static void CalculateVocabularyMinimalDistance()
        {
            byte mind = 255;
            ushort victimindex = AppConsts.VocabularySize - 1;
            for (var i = 0; i < _vocabularyDistances.Length; i++) {
                for (var j = 0; j < _vocabularyDistances[i].Length; j++) {
                    if (_vocabularyDistances[i][j] < mind) {
                        mind = _vocabularyDistances[i][j];
                        victimindex = (ushort)i;
                    }
                }
            }

            _vocabularyMinimalDistance = mind;
            _vocabularyVictimIndex = victimindex;
        }

        private static ushort[] MatchesToKi(DMatch[][] matches)
        {
            var ki = new List<ushort>();
            for (var i = 0; i < matches.Length; i++) {
                if (matches[i].Length < 2) {
                    continue;
                }

                if (float.IsNaN(matches[i][0].Distance)) {
                    continue;
                }

                ki.Add((ushort)matches[i][0].ImgIdx);
            }

            return ki.OrderBy(e => e).ToArray();
        }

        private static ushort[] GetSingleDescriptors(Mat descriptors, BackgroundWorker backgroundworker)
        {
            if (_vocabulary == null) {
                lock (_imglock) {
                    backgroundworker.ReportProgress(0, "Loading vocabulary");
                    _vocabulary = new Mat(AppConsts.VocabularySize, 61, MatType.CV_8U);
                    var buffer = File.ReadAllBytes(AppConsts.FileWords);
                    _vocabulary.SetArray(buffer);
                    backgroundworker.ReportProgress(0, "Calculating vocabulary distances...");
                    _vocabularyDistances = new byte[AppConsts.VocabularySize - 1][];
                    for (var i = 0; i < AppConsts.VocabularySize - 1; i++) {
                        _vocabularyDistances[i] = new byte[AppConsts.VocabularySize - i - 1];
                        using (var mi = _vocabulary.Row(i)) {
                            for (var j = i + 1; j < AppConsts.VocabularySize; j++) {
                                using (var mj = _vocabulary.Row(j)) {
                                    var distance = Cv2.Norm(mi, mj, NormTypes.Hamming);
                                    _vocabularyDistances[i][j - i - 1] = distance > 255 ? (byte)255 : (byte)distance;
                                }
                            }
                        }
                    }

                    CalculateVocabularyMinimalDistance();
                }
            }

            var changed = false;
            do {
                var matches = _bfmatcher.KnnMatch(descriptors, _vocabulary, k: 1);
                var mindistance = (int)matches.Min(e => e[0].Distance);
                if (mindistance < _vocabularyMinimalDistance) {
                    if (changed) {
                        var filebak = Path.ChangeExtension(AppConsts.FileWords, AppConsts.BakExtension);
                        File.Move(AppConsts.FileWords, filebak);
                        _vocabulary.GetArray(out byte[] buffer);
                        File.WriteAllBytes(AppConsts.FileWords, buffer);
                    }

                    return MatchesToKi(matches);
                }

                changed = true;
                for (var i = 0; i < matches.Length; i++) {
                    if (matches[i][0].Distance > _vocabularyMinimalDistance) {
                        var index = matches[i][0].ImgIdx;
                        backgroundworker.ReportProgress(0, $"Replacing word {index}/{_vocabularyVictimIndex}/{_vocabularyMinimalDistance}...");
                        using (var row = descriptors.Row(index)) {
                            for (var j = 0; j < row.Cols; j++) {
                                _vocabulary.At<byte>(_vocabularyVictimIndex, j) = row.At<byte>(0, j);
                            }

                            for (var ib = 0; ib < index; ib++) {
                                using (var mb = _vocabulary.Row(ib)) {
                                    var distance = Cv2.Norm(mb, row, NormTypes.Hamming);
                                    _vocabularyDistances[ib][index] = distance > 255 ? (byte)255 : (byte)distance;
                                }
                            }

                            for (var ia = index + 1; ia < AppConsts.VocabularySize; ia++) {
                                using (var ma = _vocabulary.Row(ia)) {
                                    var distance = Cv2.Norm(row, ma, NormTypes.Hamming);
                                    _vocabularyDistances[index][ia - index - 1] = distance > 255 ? (byte)255 : (byte)distance;
                                }
                            }
                        }

                        foreach (var img in _imgList) {
                            if (Array.BinarySearch(img.Value.Ki[0], _vocabularyVictimIndex) >= 0) {
                                img.Value.SetKi(0, Array.Empty<ushort>());
                            }
                            
                            if (Array.BinarySearch(img.Value.Ki[1], _vocabularyVictimIndex) >= 0) {
                                img.Value.SetKi(1, Array.Empty<ushort>());
                            }
                        }

                        CalculateVocabularyMinimalDistance();
                        break;
                    }
                }
            }
            while (true);
        }

        public static ushort[][] GetKi(Mat[] descriptors, BackgroundWorker backgroundworker)
        {
            var ki = new ushort[2][];
            ki[0] = GetSingleDescriptors(descriptors[0], backgroundworker);
            ki[1] = GetSingleDescriptors(descriptors[0], backgroundworker);
            return ki;
        }
    }
}
