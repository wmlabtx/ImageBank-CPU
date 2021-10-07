using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ImageBank
{
    public class KHashEx
    {
        private readonly PHashEx[] _descriptors;
        private readonly static MSER _mser = MSER.Create();

        public KHashEx(float[][] matrix)
        {
            int numRows = matrix.Length;
            int numCols = matrix[0].Length;

            float fmin = float.MaxValue;
            float fmax = float.MinValue;
            for (int i = 0; i < numRows; i++) {
                for (int j = 0; j < numCols; j++) {
                    if (matrix[i][j] < fmin) {
                        fmin = matrix[i][j];
                    }

                    if (matrix[i][j] > fmax) {
                        fmax = matrix[i][j];
                    }
                }
            }

            using (var matraw = new Mat(numRows, numCols, MatType.CV_8U)) {
                for (int i = 0; i < numRows; i++) {
                    for (int j = 0; j < numCols; j++) {
                        byte val = (byte)(255f * (matrix[i][j] - fmin) / (fmax - fmin));
                        matraw.At<byte>(i, j) = val;
                    }
                }

                var keypoints = _mser.Detect(matraw);
                keypoints = keypoints.OrderByDescending(e => e.Size).Where(e => e.Size >= 64f).ToArray();
                using (var matkeypoints = new Mat()) {
                    Cv2.DrawKeypoints(matraw, keypoints, matkeypoints, null, DrawMatchesFlags.DrawRichKeypoints);
                    matkeypoints.SaveImage("matkeypoints.png");
                }

                var hashes = new List<Tuple<Rect, PHashEx>>();
                foreach (var keypoint in keypoints) {
                    var cx = (int)Math.Floor(keypoint.Pt.X);
                    var cy = (int)Math.Floor(keypoint.Pt.Y);
                    var size = (int)Math.Floor(keypoint.Size);
                    if (cx - size / 2 < 0 || cx + size / 2 >= matraw.Width || cy - size / 2 < 0 || cy + size / 2 >= matraw.Height) {
                        continue;
                    }

                    var x1 = cx - size / 2;
                    var y1 = cy - size / 2;
                    var rect = new Rect(x1, y1, size, size);
                    var isoverlapped = false;
                    for (var i = 0; i < hashes.Count; i++) {
                        if (rect.Left >= hashes[i].Item1.Right || rect.Top >= hashes[i].Item1.Bottom ||
                            rect.Right <= hashes[i].Item1.Left || rect.Bottom <= hashes[i].Item1.Top) {
                            continue;
                        }

                        isoverlapped = true;
                        break;
                    }

                    if (isoverlapped) {
                        continue;
                    }

                    using (Mat mat = matraw.Clone(rect)) {
                        mat.GetArray(out byte[] data);
                        var fmatrix = new float[size][];
                        var offsety = 0;
                        for (var y = 0; y < size; y++) {
                            fmatrix[y] = new float[size];
                            var offsetx = offsety;
                            for (var x = 0; x < size; x++) {
                                fmatrix[y][x] = data[offsetx];
                                offsetx++;
                            }

                            offsety += size;
                        }

                        var phashex = new PHashEx(fmatrix);
                        hashes.Add(new Tuple<Rect, PHashEx>(rect, phashex));
                        mat.SaveImage($"reg{hashes.Count:D2}.png");
                        if (hashes.Count >= 32) {
                            break;
                        }
                    }
                }

                _descriptors = hashes.Select(e => e.Item2).ToArray();
            }
        }

        public int HammingDistance(KHashEx other)
        {
            int mind = 512;
            for (var i = 0; i < _descriptors.Length; i++) {
                for (var j = 0; j < other._descriptors.Length; j++) {
                    var d = _descriptors[i].HammingDistance(other._descriptors[j]);
                    if (d < mind) {
                        mind = d;
                    }
                }
            }

            return mind;
        }
    }
}
