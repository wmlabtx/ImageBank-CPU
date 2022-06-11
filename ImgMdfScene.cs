using System;
using System.Linq;
using System.Security.Cryptography;

namespace ImageBank
{
    public static partial class ImgMdf
    {
        public static int GetSceneSize(int sceneid)
        {
            if (sceneid == 0) {
                return 0;
            }

            var count = 0;
            lock (_imglock) {
                count = _imgList.Count(e => e.Value.SceneId == sceneid);
            }

            return count;
        }

        public static System.Windows.Media.SolidColorBrush GetBrush(int sceneid, bool inv)
        {
            double theta;
            double radius;
            double l;
            var array = BitConverter.GetBytes(sceneid);
            using (var md5 = MD5.Create()) {
                var hashmd5 = md5.ComputeHash(array);
                ulong x = BitConverter.ToUInt64(hashmd5, 2);
                var f = x / (double)ulong.MaxValue;
                if (inv) {
                    f += 0.5;
                    if (f >= 1.0) {
                        f -= 1.0;
                    }
                } 

                theta = f * 360.0;
                x = BitConverter.ToUInt64(hashmd5, 4);
                f = x / (double)ulong.MaxValue;
                if (inv) {
                    f += 0.5;
                    if (f >= 1.0) {
                        f -= 1.0;
                    }
                }

                radius = f * 50.0;
                x = BitConverter.ToUInt64(hashmd5, 6);
                f = x / (double)ulong.MaxValue;
                if (inv) {
                    f += 0.5;
                    if (f >= 1.0) {
                        f -= 1.0;
                    }
                }

                l = f * 30.0 + 60.0;
            }

            var rad = Math.PI * theta / 180.0;
            var lfloat = (float)l;
            var afloat = (float)(radius * Math.Cos(rad));
            var bfloat = (float)(radius * Math.Sin(rad));
            BitmapHelper.ToRGB(lfloat, afloat, bfloat, out byte rbyte, out byte gbyte, out byte bbyte);
            var scb = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(rbyte, gbyte, bbyte));
            return scb;
        }

        public static void CombineScene()
        {
            var imgX = AppVars.ImgPanel[0].Img;
            var imgY = AppVars.ImgPanel[1].Img;
            if (imgX.SceneId == 0 && imgY.SceneId == 0) {
                var sceneid  = AllocateSceneId();
                imgX.SetSceneId(sceneid);
                imgY.SetSceneId(sceneid);
                return;
            }

            if (imgX.SceneId != 0 && imgY.SceneId == 0) {
                imgY.SetSceneId(imgX.SceneId);
                return;
            }

            if (imgX.SceneId == 0 && imgY.SceneId != 0) {
                imgX.SetSceneId(imgY.SceneId);
                return;
            }

            if (imgX.SceneId != imgY.SceneId) {
                lock (_imglock) {
                    var scopeY = _imgList.Where(e => e.Value.SceneId == imgY.SceneId).Select(e => e.Value).ToArray();
                    foreach (var e in scopeY) {
                        e.SetSceneId(imgX.SceneId);
                    }
                }

                return;
            }
        }

        public static void DetachScene(int index)
        {
            var img = AppVars.ImgPanel[index].Img;
            img.SetSceneId(0);
        }
    }
}
